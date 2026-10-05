using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using UrbanKaoz.DisplayRecorder;
namespace Synkoz {
 public sealed class CameraMode935 {
  public string Size,Format;public double Min,Max;
  public override string ToString(){return Size+" / "+Format+" / "+Min.ToString(Engine.Inv)+"–"+Max.ToString(Engine.Inv)+" fps";}
 }
 public static partial class Engine {
  public static Func<IEnumerable<string>> ScreenSnapshot935;
  public static string CameraOptions935(Track t){
   if(t.Video==null||t.Video.Desktop||!string.IsNullOrEmpty(t.Video.TextSource)||string.IsNullOrEmpty(t.CameraSize))return "";
   return "-video_size "+Q(t.CameraSize)+" -framerate "+(t.CameraFps>0?t.CameraFps:t.Fps).ToString(Inv)+" "+(string.IsNullOrEmpty(t.CameraFormat)?"":t.CameraFormat.StartsWith("pixel:")?"-pixel_format "+Q(t.CameraFormat.Substring(6))+" ":"-vcodec "+Q(t.CameraFormat)+" ");
  }
  public static List<CameraMode935> ParseModes935(string text){
   var result=new List<CameraMode935>();
   foreach(string line in text.Split('\n')){var m=Regex.Match(line,@"(?:vcodec=(\w+)|pixel_format=(\w+)).*?min s=(\d+x\d+) fps=([\d.]+) max s=(\d+x\d+) fps=([\d.]+)");
    if(!m.Success||m.Groups[3].Value!=m.Groups[5].Value)continue;
    var item=new CameraMode935{Format=m.Groups[1].Success?m.Groups[1].Value:"pixel:"+m.Groups[2].Value,Size=m.Groups[3].Value,Min=double.Parse(m.Groups[4].Value,Inv),Max=double.Parse(m.Groups[6].Value,Inv)};
    if(!result.Any(x=>x.ToString()==item.ToString()))result.Add(item);
   }return result;
  }
  public static List<CameraMode935> Modes935(Device device){using(var p=Create(FF,"-hide_banner -list_options true -f dshow -i "+Q("video="+device.Id))){p.Start();var error=p.StandardError.ReadToEndAsync();var output=p.StandardOutput.ReadToEndAsync();if(!p.WaitForExit(15000)){p.Kill();throw new Exception("Caméra sans réponse.");}Task.WaitAll(error,output);return ParseModes935(error.Result);}}
  public static Capture TextStart935(Track t,string folder,int index,bool preview){

   if(t.Audio!=null)throw new Exception("Placer le microphone sur une piste audio distincte.");
   double now=Epoch();var c=new Capture{Track=t,PreviewOnly=preview,EpochBase=now,VideoOrigin=now,TextState935=new CaptureState(),TextDirty935=true};if(ScreenSnapshot935!=null)foreach(var line in ScreenSnapshot935())c.TextState935.Apply(line);if(preview)return c;c.File=Path.Combine(folder,"Piste_"+index.ToString("00")+"_"+Sources936.FileLabel(t)+"_brut.avi");
   c.TextLog=new StreamWriter(Path.ChangeExtension(c.File,"ukd"),false,new UTF8Encoding(false));
   c.TextLog.WriteLine("# Snkoz V9.36 LCD changes");
   if(ScreenSnapshot935!=null)foreach(var line in ScreenSnapshot935())c.TextLog.WriteLine("0000000000|"+line);
   return c;
  }
  internal static Bitmap TextImage935(Capture c){return c.Track.Video.TextSource=="EP"?LcdRenderer.RenderSingle(c.TextState935.Screens.Lines[0],20,640,240,DisplaySkin.RealLcd,true):LcdRenderer.RenderStrip(c.TextState935.Screens,1280,120,DisplaySkin.RealLcd,true);}
  public static void TextPacket935(Capture c,string packet){if(c.TextState935==null||!packet.StartsWith("UKD1|"))return;lock(c.Gate){if(c.TextLog!=null)c.TextLog.WriteLine(((long)Math.Max(0,(Epoch()-c.EpochBase)*1000)).ToString("D10")+"|"+packet);if(c.TextState935.Apply(packet))c.TextDirty935=true;}}
  public static void TextStop935(Capture c){
   if(c.Closed)return;if(c.PreviewOnly){c.Closed=true;return;}c.Duration=Math.Max(0.001,(c.StopRequested?c.TextEnd935:Epoch())-c.EpochBase);
   lock(c.Gate){if(c.TextLog!=null){c.TextLog.WriteLine(((long)(c.Duration*1000)).ToString("D10")+"|END831");c.TextLog.Dispose();c.TextLog=null;}}
   c.Closed=true;
   var state=new CaptureState();bool ep=c.Track.Video.TextSource=="EP";int fps=Math.Max(1,Math.Min(60,c.Track.Fps));
   using(var events=Recording.Events(Path.ChangeExtension(c.File,"ukd")).GetEnumerator())using(var avi=new AviWriter(c.File,ep?640:1280,ep?240:120,fps)){
    bool next=events.MoveNext();long total=(long)Math.Ceiling(c.Duration*fps);
    for(long i=0;i<total;i++){while(next&&events.Current.Time<=i*1000/fps){state.Apply(events.Current.Packet);next=events.MoveNext();}
     using(var frame=ep?LcdRenderer.RenderSingle(state.Screens.Lines[0],20,640,240,DisplaySkin.RealLcd,true):LcdRenderer.RenderStrip(state.Screens,1280,120,DisplaySkin.RealLcd,true))avi.AddFrame(frame);
    }
   }c.Valid=true;
  }
 }
 public sealed class Setup:Form {
  public Track Result;Track original936;TextBox shortName936=new TextBox{MaxLength=40};ComboBox video=new ComboBox{FormattingEnabled=true},audio=new ComboBox{FormattingEnabled=true},format=new ComboBox{FormattingEnabled=true},encoder=new ComboBox{FormattingEnabled=true},modes=new ComboBox{FormattingEnabled=true};NumericUpDown fps=new NumericUpDown{Minimum=1,Maximum=240,Value=30,DecimalPlaces=3};TextBox phoneCode=new TextBox{UseSystemPasswordChar=true};Label phoneLabel=new Label{Text="Code USB Android",AutoSize=true};Label status=new Label{AutoSize=false};Button refresh,inspect,test,ok;bool working;
  public Setup(Track t,List<Device> devices,Action<List<Device>> refreshed=null){
   original936=t;Text="Sources — Snkoz";ClientSize=new Size(760,510);StartPosition=FormStartPosition.CenterParent;
   string[] names={"Vidéo / écran","Audio USB","Format","Traitement","Mode caméra","FPS"};Control[] fields={video,audio,format,encoder,modes,fps};
   for(int i=0;i<fields.Length;i++){Controls.Add(new Label{Text=names[i],Location=new Point(12,20+i*45),AutoSize=true});fields[i].SetBounds(130,15+i*45,610,30);Controls.Add(fields[i]);var box=fields[i] as ComboBox;if(box!=null)box.DropDownStyle=ComboBoxStyle.DropDownList;}
   fps.Value=(decimal)Math.Max(1,Math.Min(240,t.CameraFps>0?t.CameraFps:t.Fps));
   refresh=new Button{Text="Actualiser sources",AutoSize=true,Location=new Point(15,295)};inspect=new Button{Text="Modes disponibles",AutoSize=true,Location=new Point(180,295)};test=new Button{Text="Tester 5 secondes",AutoSize=true,Location=new Point(345,295)};ok=new Button{Text="Valider",Location=new Point(640,465)};
   Controls.AddRange(new Control[]{refresh,inspect,test,ok});status.SetBounds(15,335,725,65);Controls.Add(status);phoneLabel.Location=new Point(15,418);phoneCode.SetBounds(150,410,320,28);Controls.Add(new Label{Text="Nom court",Location=new Point(15,418),AutoSize=true});shortName936.SetBounds(150,410,320,28);shortName936.Text=t.ShortName;Controls.Add(shortName936);
   Fill(devices,t.Video,t.Audio);modes.Items.Add("Automatique (caméra)");modes.SelectedIndex=0;
   if(!string.IsNullOrEmpty(t.CameraSize)){modes.Items.Add(new CameraMode935{Size=t.CameraSize,Format=t.CameraFormat,Min=t.Fps,Max=t.Fps});modes.SelectedIndex=1;}
   Action update=()=>{var d=video.SelectedItem as Device;bool phone=d!=null&&!string.IsNullOrEmpty(d.PhoneSerial);bool text=d!=null&&!string.IsNullOrEmpty(d.TextSource);format.Items.Clear();format.Items.AddRange(d==null?new object[]{"WAV","MP3"}:new object[]{"MP4"});format.SelectedItem=t.Format;if(format.SelectedIndex<0)format.SelectedIndex=0;encoder.Items.Clear();encoder.Items.AddRange(text?new object[]{"Décodage texte"}:phone?new object[]{"H.264 — téléphone"}:new object[]{"x264 — logiciel","NVENC — NVIDIA"});encoder.SelectedIndex=!text&&!phone&&t.Encoder=="nvenc"?1:0;audio.Enabled=!text&&!phone;if(text||phone)audio.SelectedIndex=0;phoneCode.Visible=phoneLabel.Visible=phone;if(phone)status.Text="Vidéo + micro du téléphone. Résolution réglée dans Prise USB ; original conservé sur Android.";modes.Enabled=inspect.Enabled=d!=null&&!d.Desktop&&!text&&!phone;test.Enabled=phone||inspect.Enabled;test.Text=phone?"Vérifier connexion":"Tester 5 secondes";fps.Enabled=text||modes.SelectedItem is CameraMode935;};
   video.SelectedIndexChanged+=delegate{modes.SelectedIndex=0;update();};modes.SelectedIndexChanged+=delegate{var cm=modes.SelectedItem as CameraMode935;if(cm!=null)fps.Value=Math.Max(fps.Minimum,Math.Min(fps.Maximum,Math.Max((decimal)cm.Min,Math.Min((decimal)cm.Max,fps.Value))));fps.Enabled=video.SelectedItem is Device&&!string.IsNullOrEmpty(((Device)video.SelectedItem).TextSource)||modes.SelectedItem is CameraMode935;};update();
   refresh.Click+=async(s,e)=>{await Work(async()=>{var v=video.SelectedItem as Device;var a=audio.SelectedItem as Device;var list=await Task.Run(()=>Engine.Devices());if(refreshed!=null)refreshed(list);Fill(list,v,a);status.Text="Sources actualisées.";});};
   inspect.Click+=async(s,e)=>{var d=video.SelectedItem as Device;if(d==null)return;await Work(async()=>{var list=await Task.Run(()=>Engine.Modes935(d));modes.Items.Clear();modes.Items.Add("Automatique (caméra)");foreach(var m in list)modes.Items.Add(m);modes.SelectedIndex=0;status.Text=list.Count+" modes annoncés. Choisir résolution/format, puis FPS dans la plage indiquée. Un essai confirme l'ouverture.";});};
   test.Click+=async(s,e)=>{await Work(async()=>{Track selected=Selection();var result=await Task.Run(()=>{if(!Engine.IsPhone(selected))return CameraTest935.Run(selected);using(var link=new Android936(selected)){link.Measure();var state=link.Call("/status");return "Prise USB : "+Convert.ToString(state["state"])+" ; RTT minimal "+(link.Rtt*1000).ToString("F1")+" ms. Aucun enregistrement lancé.";}});status.Text=result;});};
   ok.Click+=delegate{try{Result=Selection();DialogResult=DialogResult.OK;}catch(Exception ex){status.Text=ex.Message;}};
   FormClosing+=delegate(object sender,FormClosingEventArgs e){if(working)e.Cancel=true;};
  }
  void Fill(List<Device> list,Device v,Device a){video.Items.Clear();audio.Items.Clear();video.Items.Add("Aucune");audio.Items.Add("Aucune");foreach(var d in list)(d.Video?video:audio).Items.Add(d);video.Items.Add(new Device{Name="EP-UK",Id="text:EP",Video=true,TextSource="EP"});video.Items.Add(new Device{Name="E4-UK",Id="text:ABCD",Video=true,TextSource="ABCD"});if(v!=null&&!video.Items.OfType<Device>().Any(d=>d.Id==v.Id))video.Items.Add(v);if(a!=null&&!audio.Items.OfType<Device>().Any(d=>d.Id==a.Id))audio.Items.Add(a);video.SelectedIndex=audio.SelectedIndex=0;foreach(var d in video.Items.OfType<Device>())if(v!=null&&d.Id==v.Id)video.SelectedItem=d;foreach(var d in audio.Items.OfType<Device>())if(a!=null&&d.Id==a.Id)audio.SelectedItem=d;}
  Track Selection(){var d=video.SelectedItem as Device;var m=modes.SelectedItem as CameraMode935;if(m!=null&&(fps.Value<(decimal)m.Min||fps.Value>(decimal)m.Max))throw new Exception("FPS hors de la plage annoncée.");return new Track{Video=d,Audio=audio.SelectedItem as Device,Format=(string)format.SelectedItem,Encoder=encoder.SelectedIndex==1?"nvenc":"x264",Fps=Math.Min(60,(int)fps.Value),CameraFps=m==null?0:(double)fps.Value,CameraSize=m==null?"":m.Size,CameraFormat=m==null?"":m.Format,ShortName=shortName936.Text.Trim(),Fx=original936.Fx};}
  async Task Work(Func<Task> action){if(working)return;working=true;foreach(Control c in Controls)if(!(c is Label))c.Enabled=false;status.Text="Opération en cours…";try{await action();}catch(Exception ex){status.Text=ex.Message;}finally{working=false;foreach(Control c in Controls)if(!(c is Label))c.Enabled=true;var d=video.SelectedItem as Device;bool phone=d!=null&&!string.IsNullOrEmpty(d.PhoneSerial);bool text=d!=null&&!string.IsNullOrEmpty(d.TextSource);audio.Enabled=!text&&!phone;modes.Enabled=inspect.Enabled=d!=null&&!d.Desktop&&!text&&!phone;test.Enabled=phone||inspect.Enabled;test.Text=phone?"Vérifier connexion":"Tester 5 secondes";fps.Enabled=text||modes.SelectedItem is CameraMode935;}}
 }
 public static class CameraTest935 {
  public static string Run(Track t){
   if(t.Video==null||t.Video.Desktop||!string.IsNullOrEmpty(t.Video.TextSource))throw new Exception("Choisir une caméra USB.");
   string folder=Path.Combine(Engine.Root,"Analyses","CameraTests",DateTime.Now.ToString("yyyyMMdd_HHmmss")+"_"+Guid.NewGuid().ToString("N").Substring(0,6));Directory.CreateDirectory(folder);
   // Bounded user-triggered source test, no Tee commands and no periodic probe.
   string args="-hide_banner -f dshow -rtbufsize 256M "+Engine.CameraOptions935(t)+"-i "+Engine.Q("video="+t.Video.Id)+" -t 5 -an -vf showinfo -fps_mode passthrough -f null -";
   using(var p=Engine.Create(Engine.FF,args)){p.Start();var error=p.StandardError.ReadToEndAsync();var output=p.StandardOutput.ReadToEndAsync();if(!p.WaitForExit(15000)){p.Kill();p.WaitForExit();throw new Exception("Test arrêté après 15 secondes sans fin normale.");}Task.WaitAll(error,output);string log=error.Result;File.WriteAllText(Path.Combine(folder,"test.log"),log);if(p.ExitCode!=0)throw new Exception("Mode refusé ou erreur de capture. Journal : "+folder);
    var times=Regex.Matches(log,@"\bn:\s*\d+\s+pts:.*?pts_time:([\d.Ee+\-]+)").Cast<Match>().Select(m=>double.Parse(m.Groups[1].Value,Engine.Inv)).ToArray();double rate=times.Length>1?(times.Length-1)/(times.Last()-times.First()):0;double gap=0;for(int i=1;i<times.Length;i++)gap=Math.Max(gap,times[i]-times[i-1]);
    return "Demandé : "+(string.IsNullOrEmpty(t.CameraSize)?"automatique":t.CameraSize+" / "+(t.CameraFps>0?t.CameraFps:t.Fps).ToString(Engine.Inv)+" fps")+". Reçu : "+rate.ToString("F1",Engine.Inv)+" fps ; écart maximal "+(gap*1000).ToString("F0")+" ms. "+(log.Contains("frame dropped")?"Pertes signalées.":"Pas de perte signalée (ne garantit pas zéro perte).")+" Journal : "+folder;
   }
  }
 }
}
