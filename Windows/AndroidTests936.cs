using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Synkoz;
namespace UrbanKaoz.DisplayRecorder {
 internal static class AndroidTests936 {
  internal static void Run(string folder){
   Directory.CreateDirectory(folder);string raw=Path.Combine(folder,"fixture.mp4");Engine.Run(Engine.FF,"-hide_banner -y -f lavfi -i testsrc=size=160x120:rate=30:duration=2 -f lavfi -i sine=frequency=440:duration=2 -c:v libx264 -preset ultrafast -pix_fmt yuv420p -c:a aac -shortest "+Engine.Q(raw));
   byte[] media=File.ReadAllBytes(raw);string hash;using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(media)).Replace("-","").ToLowerInvariant();
   var reserve=new TcpListener(IPAddress.Loopback,0);reserve.Start();int port=((IPEndPoint)reserve.LocalEndpoint).Port;reserve.Stop();
   using(var listener=new HttpListener()){listener.Prefixes.Add("http://127.0.0.1:"+port+"/");listener.Start();bool started=false,stopped=false,marked=false;int clocks=0;var json=new JavaScriptSerializer();
    var server=Task.Run(()=>{try{while(listener.IsListening){var ctx=listener.GetContext();object response=null;string path=ctx.Request.Url.AbsolutePath;byte[] output=null;
     if(ctx.Request.Headers["Authorization"]!="Bearer test-only"){ctx.Response.StatusCode=401;response=new{error="unauthorized"};}
     else if(path=="/clock"){clocks++;response=new{monoNs=10000000000L,wallMs=0};}
     else if(path=="/command"){Dictionary<string,object> body;using(var sr=new StreamReader(ctx.Request.InputStream))body=json.Deserialize<Dictionary<string,object>>(sr.ReadToEnd());if((string)body["action"]=="start")started=true;else stopped=true;response=new{state=stopped?"stopped":"recording",timing=new{recordStartNs=10000000000L,firstSensorNs=10000000000L,firstCallbackNs=10001000000L,sensorRealtime=true}};}
     else if(path=="/marker"){using(var sr=new StreamReader(ctx.Request.InputStream)){var body=json.Deserialize<Dictionary<string,object>>(sr.ReadToEnd());marked=(string)body["kind"]=="music"&&Convert.ToInt32(body["tick"])==1536&&Convert.ToInt64(body["teeUs"])==1234;}response=new{ok=true};}
     else if(path.EndsWith("/sha256"))response=new{sha256=hash,size=media.Length};
     else if(path.StartsWith("/recording/"))output=media;
     else{ctx.Response.StatusCode=404;response=new{error="unknown"};}
     if(output==null)output=Encoding.UTF8.GetBytes(json.Serialize(response));ctx.Response.ContentLength64=output.Length;ctx.Response.OutputStream.Write(output,0,output.Length);ctx.Response.Close();
    }}catch(HttpListenerException){}catch(ObjectDisposedException){}});
    try{
     var track=new Track{Video=new Device{Name="Android synthetic",PhoneSerial="synthetic",Video=true},Format="MP4"};var phone=new Android936(port,"test-only");phone.Start();phone.Marker("music",1234,1536,Android936.Origin(phone.StartStatus,phone.Offset)+0.5);
     string dest=Path.Combine(folder,"Piste_01_Android_brut.mp4");var capture=new Capture{Track=track,Phone=phone,File=dest,VideoOrigin=100,AudioOrigin=100};Engine.RequestStop(capture);Engine.Stop(capture);
     if(!capture.Valid||!capture.Closed||capture.Failed||!started||!stopped||!marked||clocks!=5||File.ReadAllBytes(dest).Length!=media.Length)throw new Exception("Android synthetic start/anchor/stop/hash transfer failed: "+capture.Failure);
     Engine.FinalizeSegment(capture,100.5,101.2);var info=Engine.ProbeMedia(capture.File);if(!info.Video||!info.Audio||Math.Abs(info.Duration-0.7)>0.06||!File.Exists(dest))throw new Exception("Android synthetic trim failed");
     File.WriteAllText(Path.Combine(folder,"android_synthetic.txt"),"PASS authenticated commands, 5 startup clock samples, original Tee music anchor, immediate stop request, SHA256 transfer, local-mic MP4 trim (0.7s), raw retained. No phone recording.");
    }finally{listener.Stop();server.GetAwaiter().GetResult();}
   }
  }
 }
}
