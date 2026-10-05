using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
namespace Synkoz {
 // Android records locally. USB carries commands, two timing anchors, and the finalized file.
 // No periodic clock resync, retransmission or musical recovery is allowed here.
 public sealed class Android936 : IDisposable {
  public static string Adb {get{return Path.Combine(Engine.Root,"Dependencies","platform-tools","adb.exe");}}
  public readonly string Serial,Session=Guid.NewGuid().ToString("N");readonly string token;int port;bool ownsForward=true;
  public double Offset,Rtt;public Dictionary<string,object> StartStatus,EndStatus;public Task StopTask;
  public static string Protect(string code){return string.IsNullOrWhiteSpace(code)?"":Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(code.Trim()),null,DataProtectionScope.CurrentUser));}
  public static string Unprotect(string code){if(string.IsNullOrEmpty(code))return "";try{return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(code),null,DataProtectionScope.CurrentUser));}catch{throw new Exception("Ressaisir le code USB Android sur ce compte Windows.");}}
  public static List<Device> Devices(){var list=new List<Device>();if(!File.Exists(Adb))return list;string output=Engine.Run(Adb,"devices -l",10000);foreach(string line in output.Split('\n')){var parts=line.Trim().Split(new[]{' ','\t'},StringSplitOptions.RemoveEmptyEntries);if(parts.Length<2||parts[1]!="device")continue;string model=parts.FirstOrDefault(x=>x.StartsWith("model:"));list.Add(new Device{Name="Android "+(model==null?parts[0]:model.Substring(6).Replace('_',' ')),Id="android:"+parts[0],PhoneSerial=parts[0],Video=true});}return list;}
  public Android936(Track track){Serial=track.Video.PhoneSerial;token="";string value=Engine.Run(Adb,"-s "+Engine.Q(Serial)+" forward tcp:0 tcp:18765",10000).Trim();if(!int.TryParse(value,out port)||port<1)throw new Exception("Redirection USB Android indisponible.");try{token=Convert.ToString(Call("/pair")["token"]);if(string.IsNullOrWhiteSpace(token))throw new Exception("Association USB vide.");}catch{Dispose();throw;}}
  internal Android936(int testPort,string testToken){port=testPort;token=testToken;Serial="synthetic";ownsForward=false;}
  HttpWebRequest Request(string path,object body=null,int timeout=20000){var r=(HttpWebRequest)WebRequest.Create("http://127.0.0.1:"+port+path);r.Proxy=null;r.Timeout=timeout;r.ReadWriteTimeout=30000;r.KeepAlive=false;r.Headers["Authorization"]="Bearer "+token;if(body!=null){r.Method="POST";r.ContentType="application/json";var bytes=Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(body));r.ContentLength=bytes.Length;using(var s=r.GetRequestStream())s.Write(bytes,0,bytes.Length);}return r;}
  public Dictionary<string,object> Call(string path,object body=null,int timeout=20000){try{using(var r=Request(path,body,timeout).GetResponse())using(var s=new StreamReader(r.GetResponseStream(),Encoding.UTF8))return new JavaScriptSerializer{MaxJsonLength=4*1024*1024}.Deserialize<Dictionary<string,object>>(s.ReadToEnd());}catch(WebException e){string message="Application Prise USB inaccessible : vérifier câble, autorisation USB Android et Prise USB V9.36.";if(e.Response!=null)using(var r=e.Response)using(var s=new StreamReader(r.GetResponseStream())){try{var j=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(s.ReadToEnd());message=Convert.ToString(j["error"]);}catch{}}throw new Exception(message);}}
  public static double Number(Dictionary<string,object> d,string k){object v;return d!=null&&d.TryGetValue(k,out v)?Convert.ToDouble(v,Engine.Inv):0;}
  public static Dictionary<string,object> Timing(Dictionary<string,object> d){object v;return d!=null&&d.TryGetValue("timing",out v)?v as Dictionary<string,object>:null;}
  public void Measure(){Rtt=double.PositiveInfinity;for(int i=0;i<5;i++){double before=Engine.Epoch();var c=Call("/clock");double after=Engine.Epoch(),rtt=after-before;if(rtt<Rtt){Rtt=rtt;Offset=(before+after)/2-Number(c,"monoNs")/1e9;}}}
  public static double Origin(Dictionary<string,object> status,double offset){var t=Timing(status);double frame=Number(t,"firstSensorNs"),callback=Number(t,"firstCallbackNs");object value;bool realtime=t!=null&&t.TryGetValue("sensorRealtime",out value)&&Convert.ToBoolean(value);return (realtime&&frame>0?frame:callback>0?callback:Number(t,"recordStartNs"))/1e9+offset;}
  public void Start(){Measure();StartStatus=Call("/command",new{action="start",session=Session,startedLocal=DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff")});var limit=Stopwatch.StartNew();while(Number(Timing(StartStatus),"firstCallbackNs")==0&&limit.ElapsedMilliseconds<2500){System.Threading.Thread.Sleep(50);StartStatus=Call("/status");}if(Number(Timing(StartStatus),"firstCallbackNs")==0||Convert.ToString(StartStatus["state"])!="recording")throw new Exception("Android n'a pas démarré la prise.");}
  public void Marker(string kind,uint teeUs,uint tick,double epoch){Call("/marker",new{session=Session,kind=kind,teeUs=teeUs,tick=tick,pcEpoch=epoch,phoneMonoNs=(long)((epoch-Offset)*1e9)});}
  public void Stop(){EndStatus=Call("/command",new{action="stop",session=Session});}
  public void Download(string file){var hash=Call("/recording/"+Session+"/sha256",null,3600000);string part=file+".part";using(var r=Request("/recording/"+Session).GetResponse())using(var s=r.GetResponseStream())using(var f=File.Create(part))s.CopyTo(f);string actual;using(var f=File.OpenRead(part))using(var sha=SHA256.Create())actual=BitConverter.ToString(sha.ComputeHash(f)).Replace("-","").ToLowerInvariant();if(new FileInfo(part).Length!=(long)Number(hash,"size")||actual!=Convert.ToString(hash["sha256"]))throw new Exception("Copie Android incomplète : original conservé sur le téléphone.");File.Move(part,file);}
  public Bitmap Preview(){var j=Call("/preview");using(var s=new MemoryStream(Convert.FromBase64String((string)j["jpeg"])))using(var b=new Bitmap(s))return new Bitmap(b);}
  public void Dispose(){int p=port;port=0;if(p>0&&ownsForward)try{Engine.Run(Adb,"-s "+Engine.Q(Serial)+" forward --remove tcp:"+p,5000);}catch{}}
 }
 public static partial class Engine {
  public static bool IsPhone(Track t){return t.Video!=null&&!string.IsNullOrEmpty(t.Video.PhoneSerial);}
  public static bool HasAudio(Track t){return t.Audio!=null||IsPhone(t);}
  public static Capture PhoneStart936(Track t,string folder,int index,bool preview){
   var c=new Capture{Track=t,PreviewOnly=preview,EpochBase=Epoch(),File=preview?null:Path.Combine(folder,"Piste_"+index.ToString("00")+"_"+Sources936.FileLabel(t)+"_brut.mp4")};
   c.Phone=new Android936(t);
   if(preview){c.PreviewTask=Task.Run(async()=>{try{while(!c.StopRequested){var image=c.Phone.Preview();lock(c.Gate){if(c.Preview!=null)c.Preview.Dispose();c.Preview=image;}await Task.Delay(Engine.Direct939?33:400);}}catch(Exception ex){if(!c.StopRequested){c.Failed=true;c.Failure=ex.Message;}}});return c;}
   try{c.Phone.Start();c.VideoOrigin=c.AudioOrigin=Android936.Origin(c.Phone.StartStatus,c.Phone.Offset);Store934.Save(c.File+".timing.json",new{session=c.Phone.Session,phoneToPcEpoch=c.Phone.Offset,minRtt=c.Phone.Rtt,start=c.Phone.StartStatus,quality="estimated; optical/audio calibration required"});return c;}
   catch{try{c.Phone.Stop();}catch{}c.Phone.Dispose();throw;}
  }
  public static void PhoneRequestStop936(Capture c){if(c.StopRequested)return;c.StopRequested=true;if(!c.PreviewOnly)c.Phone.StopTask=Task.Run(()=>c.Phone.Stop());}
  public static void PhoneStop936(Capture c){if(c.Closed)return;try{PhoneRequestStop936(c);if(c.PreviewOnly){if(c.PreviewTask!=null)c.PreviewTask.Wait(25000);}else{if(c.Phone.StopTask!=null)c.Phone.StopTask.GetAwaiter().GetResult();c.Phone.Download(c.File);var p=ProbeMedia(c.File);c.Valid=p.Video&&p.Duration>0;c.Duration=p.Duration;Store934.Save(c.File+".timing.json",new{session=c.Phone.Session,phoneToPcEpoch=c.Phone.Offset,minRtt=c.Phone.Rtt,start=c.Phone.StartStatus,end=c.Phone.EndStatus,quality="estimated; optical/audio calibration required"});}}catch(Exception ex){c.Failed=true;c.Failure=ex.Message;c.Errors.AppendLine(ex.Message);}finally{c.Closed=true;c.Phone.Dispose();if(c.File!=null)File.WriteAllText(c.File+".log",c.Errors.ToString());}}
  public static async Task WaitReady936(Capture c,int timeoutMs=10000){if(c.Failed||c.Closed)return;var timer=Stopwatch.StartNew();while(timer.ElapsedMilliseconds<timeoutMs){if(Exited(c))break;if((c.Track.Video==null||!double.IsNaN(c.VideoOrigin))&&(!HasAudio(c.Track)||!double.IsNaN(c.AudioOrigin)))return;if(Exited(c))break;await Task.Delay(25);}c.Failed=true;c.Failure="Pas de premières données dans le délai de démarrage ; autres pistes conservées.";RequestStop(c);}
 }
}
