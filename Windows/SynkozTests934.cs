using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Synkoz;
namespace UrbanKaoz.DisplayRecorder {
 internal static class SynkozTests934 {
  static int checks;static void Check(bool value,string text){checks++;if(!value)throw new Exception("934: "+text);}
  static object Field(object o,string n){return o.GetType().GetField(n,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(o);}
  static void Set(object o,string n,object value){o.GetType().GetField(n,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(o,value);}
  internal static void Nvenc(string folder){
   Directory.CreateDirectory(folder);string raw=Path.Combine(folder,"Piste_01_Synthetic_brut.mp4");var t=new Track{Encoder="nvenc",Video=new Device{Name="Synthetic",Video=true},Audio=new Device{Name="Sine"}};
   string args="-hide_banner -y -f lavfi -i testsrc=size=160x120:rate=30:duration=2 -f lavfi -i sine=frequency=440:sample_rate=48000:duration=2 -filter_complex \"[0:v]showinfo,setpts=PTS-STARTPTS,split=2[v][p];[p]fps=5,scale=800:600[preview];[1:a]ashowinfo,asetpts=PTS-STARTPTS[a]\" -map [v] -map [a] -c:v h264_nvenc -preset p1 -pix_fmt yuv420p -c:a aac "+Engine.Q(raw)+" -map [preview] -an -c:v rawvideo -pix_fmt bgr24 -f rawvideo pipe:1";
   var c=Engine.Start(t,folder,1,false,args);Check(c.Process.WaitForExit(20000),"NVENC capture completes");Engine.Stop(c);Check(c.Valid,"NVENC media valid");Engine.FinalizeSegment(c,c.VideoOrigin+0.5,c.VideoOrigin+1.5);Check(Math.Abs(Engine.ProbeMedia(c.File).Duration-1)<0.06,"NVENC exact trim");c.Clear();
   var m=new Montage{Folder=folder,Duration=1,Tracks=new List<Track>{t},Blocks=new List<MediaBlock>{new MediaBlock{Track=0,File=Path.GetFileName(c.File),Duration=1,Audio=true,Video=true}}};
   Engine.Export(m,0,Path.Combine(folder,"Export_nvenc.mp4"));var info=Engine.ProbeMedia(Path.Combine(folder,"Export_nvenc.mp4"));Check(info.Video&&info.Audio&&Math.Abs(info.Duration-1)<0.06,"NVENC video/audio export");File.WriteAllText(Path.Combine(folder,"nvenc934.txt"),"PASS "+checks+" synthetic NVENC checks, driver unchanged.");
  }
  internal static void Run(string folder){
   Directory.CreateDirectory(folder);
   var camera=new Device{Video=true,Id="camera"};var mic=new Device{Id="micro"};
   string combined=Engine.Arguments(new Track{Video=camera,Audio=mic},"test.mp4",false);
   Check(combined.Contains("video=camera:audio=micro")&&!combined.Contains("[1:a]")&&combined.Contains("[0:a]"),"camera and microphone share DirectShow input");
   Check(Engine.Arguments(new Track{Video=camera},"test.mp4",false).Contains("video=camera")&&!Engine.Arguments(new Track{Video=camera},"test.mp4",false).Contains("ashowinfo"),"camera alone needs no audio input");
   Check(Engine.Arguments(new Track{Audio=mic},"test.wav",false).Contains("audio=micro"),"audio alone retained");
   Check(!Engine.Arguments(new Track{Video=camera,Audio=mic},null,true).Contains("audio=micro"),"preview does not open microphone");
   Check(Engine.Arguments(new Track{Video=camera,Audio=mic,Encoder="nvenc"},"test.mp4",false).Contains("video=camera:audio=micro"),"NVENC shares same source opening");
   using(var standalone=new CapturePanel(()=>false,()=>{throw new Exception("unexpected connect");},s=>{throw new Exception("unexpected command");})){
    var field=typeof(CapturePanel).GetField("clockLog",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
    var musicField=typeof(CapturePanel).GetField("music",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
    var rtt=typeof(CapturePanel).GetField("bestRtt",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
    using(var writer=new StreamWriter(new MemoryStream())){field.SetValue(standalone,writer);rtt.SetValue(standalone,0.0);standalone.Packet("SK1,E,1,1000,1536,2,0");Check(double.IsNaN((double)musicField.GetValue(standalone)),"standalone ignores musical start even with calibrated Tee");field.SetValue(standalone,null);}
   }
   var found=Engine.ParseDevices("[dshow] \"DV20 USB CAMERA\" (video)\n[dshow] Alternative name \"dv20-real-id\"\n[dshow] \"OBS Virtual Camera\" (none)\n[dshow] Alternative name \"obs-wrong-id\"\n");
   Check(found.Count==1&&found[0].Id=="dv20-real-id","untyped device cannot overwrite preceding camera identity");
   var marks=new[]{new ClockMark{Tick=1536,Time=100},new ClockMark{Tick=3072,Time=102},new ClockMark{Tick=4608,Time=104}};
   Check(Cuts934.LastComplete(marks,100,103.5,1536).Time==102,"pause removes partial measure");
   Check(Cuts934.LastComplete(marks,100,101,1536)==null,"pause before first full measure adds no segment");
   Check(Cuts934.LastComplete(marks,100,104,1536).Tick==4608,"exact boundary kept");
   var ev=new[]{new NoteEvent{Time=99,On=true,Note=60,Velocity=90},new NoteEvent{Time=100.5,Note=60},new NoteEvent{Time=101,On=true,Note=64,Velocity=100},new NoteEvent{Time=103,Note=64}};
   var notes=Cuts934.Notes(ev,100,102,8);Check(notes.Count==2&&notes[0].Start==8&&notes[0].Duration==0.5,"notes clipped at beginning");Check(notes[1].Start==9&&notes[1].Duration==1,"notes clipped at end and appended");
   string raw=Path.Combine(folder,"Piste_01_Synthetic_brut.mp4");var track=new Track{Video=new Device{Name="Synthetic",Id="lavfi",Video=true},Audio=new Device{Name="Sine",Id="sine"}};
   string args="-hide_banner -y -f lavfi -i testsrc=size=160x120:rate=30:duration=3 -f lavfi -i sine=frequency=440:sample_rate=48000:duration=3 -filter_complex \"[0:v]showinfo,setpts=PTS-STARTPTS,split=2[v][p];[p]fps=5,scale=800:600[preview];[1:a]ashowinfo,asetpts=PTS-STARTPTS[a]\" -map [v] -map [a] -c:v libx264 -preset ultrafast -pix_fmt yuv420p -c:a aac "+Engine.Q(raw)+" -map [preview] -an -c:v rawvideo -pix_fmt bgr24 -f rawvideo pipe:1";
   var c=Engine.Start(track,folder,1,false,args);Check(c.Process.WaitForExit(15000),"synthetic media completes");Engine.Stop(c);Check(c.Valid,"raw is usable");Engine.FinalizeSegment(c,c.VideoOrigin+0.5,c.VideoOrigin+2.5);var p=Engine.ProbeMedia(c.File);Check(Math.Abs(p.Duration-2)<0.06&&p.Video&&p.Audio,"both ends trimmed to two seconds");Check(File.Exists(raw),"raw retained");c.Clear();
   var audio=new Track{Audio=new Device{Name="Audio",Id="audio"},Format="WAV"};string wav=Path.Combine(folder,"audio.wav");Engine.Run(Engine.FF,"-hide_banner -y -f lavfi -i sine=frequency=880:duration=2 -c:a pcm_s16le "+Engine.Q(wav));
   var m=new Montage{Folder=folder,Duration=2.5,Tracks=new List<Track>{track,audio},Notes=Cuts934.Notes(ev,100,102,0)};
   m.Blocks.Add(new MediaBlock{Track=0,Segment=1,Start=0,Duration=1,File=Path.GetFileName(c.File),Video=true,Audio=true});
   m.Blocks.Add(new MediaBlock{Track=0,Segment=2,Start=1,Duration=0.5,Issue="Camera disconnected",File="",Video=true,Audio=true});
   m.Blocks.Add(new MediaBlock{Track=0,Segment=3,Start=1.5,Duration=1,File=Path.GetFileName(c.File),Video=true,Audio=true});
   m.Blocks.Add(new MediaBlock{Track=1,Segment=1,Start=0,Duration=1,File="audio.wav",Audio=true});
   m.Blocks.Add(new MediaBlock{Track=1,Segment=3,Start=1.5,Duration=1,File="audio.wav",Audio=true});
   m.Save();var loaded=Montage.Load(folder);Check(loaded.Blocks.Count==5&&loaded.Notes.Count==2,"whole folder montage round trip");
   Engine.Export(m,0,Path.Combine(folder,"Export_video.mp4"));var vp=Engine.ProbeMedia(Path.Combine(folder,"Export_video.mp4"));Check(vp.Video&&vp.Audio&&Math.Abs(vp.Duration-2.5)<0.08,"video/audio export preserves missing interval");
   Engine.Export(m,1,Path.Combine(folder,"Export_audio.wav"));var ap=Engine.ProbeMedia(Path.Combine(folder,"Export_audio.wav"));Check(ap.Audio&&!ap.Video&&Math.Abs(ap.Duration-2.5)<0.002,"audio export sample duration and gap");
   audio.Format="MP3";Engine.Export(m,1,Path.Combine(folder,"Export_audio.mp3"));Check(Engine.ProbeMedia(Path.Combine(folder,"Export_audio.mp3")).Audio,"MP3 export");
   track.Encoder="nvenc";string config=Path.Combine(folder,"Sources.synkozconfig");Store934.Save(config,m.Tracks);var restored=Store934.Load<List<Track>>(config);Check(restored[0].Encoder=="nvenc"&&restored[0].Video.Id=="lavfi"&&restored[1].Audio.Id=="audio","source config and per-track encoder persistence");
   Check(Engine.Arguments(track,"out.mp4",false).Contains("h264_nvenc"),"NVENC selection used");Check(Engine.Arguments(new Track{Video=track.Video,Audio=track.Audio},"out.mp4",false).Contains("[0:a]"),"audio and video share DirectShow input");
   using(var form=new MainForm()){
    form.ShowInTaskbar=false;form.Opacity=0;form.Show();Application.DoEvents();var tabs=(TabControl)Field(form,"tabs933");Check(tabs.TabPages.Count==3&&tabs.TabPages[1].Text=="Prise"&&tabs.TabPages[2].Text=="Montage","separate tabs");
    var capture=(CapturePanel)Field(form,"capture933");tabs.SelectedIndex=1;Application.DoEvents();for(int i=0;i<5;i++)((Button)Field(capture,"add")).PerformClick();var rows=(FlowLayoutPanel)Field(capture,"rows");Check(rows.Controls.Count==7&&rows.Height>230,"seven tracks with expanded capture area");
    var failed=new Capture{Track=new Track(),Process=Engine.Create(Engine.FF,"-version")};failed.Process.Start();failed.Process.WaitForExit(10000);
    ((List<Capture>)Field(capture,"active")).Add(failed);Set(capture,"recording",true);capture.GetType().GetMethod("CheckSources",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(capture,null);
    Check((bool)Field(capture,"recording")&&failed.Failed,"source failure never stops recording");Set(capture,"recording",false);Engine.Stop(failed);((List<Capture>)Field(capture,"active")).Clear();
    form.LiveImage(Path.Combine(folder,"Prise.png"));capture.MontageView.ShowMontage(m);tabs.SelectedIndex=2;Application.DoEvents();form.LiveImage(Path.Combine(folder,"Montage.png"));form.Close();Application.DoEvents();
   }
   File.WriteAllText(Path.Combine(folder,"tests934.txt"),"PASS "+checks+" checks; synthetic sources only. No camera/audio hardware, musical transport, or SD session used.");
  }
 }
}
