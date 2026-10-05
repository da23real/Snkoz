using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Synkoz;
namespace UrbanKaoz.DisplayRecorder {
 internal static class Tests935 {
  static int count;static void Check(bool b,string text){count++;if(!b)throw new Exception(text);}
  internal static void Run(string folder){
   Directory.CreateDirectory(folder);
   var modes=Engine.ParseModes935("[dshow] vcodec=mjpeg min s=1920x1080 fps=5 max s=1920x1080 fps=30\n[dshow] pixel_format=yuyv422 min s=640x480 fps=10 max s=640x480 fps=60\n");
   Check(modes.Count==2&&modes[0].Min==5&&modes[1].Format=="pixel:yuyv422","camera capability parsing");
   var camera=new Track{Video=new Device{Video=true,Id="usb"},Audio=new Device{Id="mic"},CameraSize="1920x1080",CameraFormat="mjpeg",Fps=15};
   var args=Engine.Arguments(camera,"capture.mp4",false);Check(args.Contains("-framerate 15")&&args.IndexOf("-framerate 15")<args.IndexOf("-i "),"FPS requested on input");Check(args.Contains("video=usb:audio=mic")&&args.Contains("-vcodec \"mjpeg\""),"shared input and source format retained");
   camera.CameraFormat="pixel:yuyv422";Check(Engine.CameraOptions935(camera).Contains("-pixel_format"),"raw camera format");
   Store934.Save(Path.Combine(folder,"config.synkozconfig"),camera);var back=Store934.Load<Track>(Path.Combine(folder,"config.synkozconfig"));Check(back.Fps==15&&back.CameraSize==camera.CameraSize&&back.CameraFormat==camera.CameraFormat,"source mode config persisted");
   Engine.ScreenSnapshot935=()=>new[]{"UKD1|L|EP|0|EP INITIAL","UKD1|L|A|0|BAND INITIAL"};
   for(int i=0;i<2;i++){
    string source=i==0?"EP":"ABCD";var track=new Track{Video=new Device{Video=true,Id="text:"+source,TextSource=source},Fps=10};
    var c=Engine.Start(track,folder,i+1);Check(c.Process==null&&c.TextLog!=null,"text starts without encoder process");c.EpochBase-=1;c.VideoOrigin=c.EpochBase;
    Engine.TextPacket935(c,"UKD1|L|EP|1|EP CHANGE");Engine.RequestStop(c);double end=c.TextEnd935;Engine.Stop(c);
    Check(c.Valid&&c.Closed&&File.Exists(Path.ChangeExtension(c.File,"ukd")),"text log and reconstruction retained");var info=Engine.ProbeMedia(c.File);Check(info.Width==(i==0?640:1280)&&info.Height==(i==0?240:120),"EP and strip separate dimensions");
    Engine.FinalizeSegment(c,c.VideoOrigin,end);Check(Path.GetExtension(c.File)==".mp4"&&Engine.ProbeMedia(c.File).Video,"text enters montage as standard video");c.Clear();
   }
   using(var setup=new Setup(new Track(),new List<Device>())){setup.ShowInTaskbar=false;setup.Show();Application.DoEvents();var video=(ComboBox)typeof(Setup).GetField("video",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(setup);Check(video.Items.OfType<Device>().Count()==2,"exactly two LCD sources");video.SelectedIndex=1;Application.DoEvents();var codec=(ComboBox)typeof(Setup).GetField("encoder",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(setup);Check(codec.Items.Count==1&&(string)codec.SelectedItem=="Décodage texte","LCD codec chooser replaced");using(var bitmap=new Bitmap(setup.Width,setup.Height)){setup.DrawToBitmap(bitmap,new Rectangle(Point.Empty,bitmap.Size));bitmap.Save(Path.Combine(folder,"Sources.png"));}setup.Close();}
   File.WriteAllText(Path.Combine(folder,"tests935.txt"),"PASS "+count+" source and LCD assertions; no hardware capture.");
  }
 }
}
