using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Synkoz;
namespace UrbanKaoz.DisplayRecorder {
internal static class SynkozTests933 {
 static int count;static void Check(bool ok,string name){count++;if(!ok)throw new Exception("933 "+name);}
 static object Field(object o,string name){return o.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(o);}
 internal static void Run(string folder){Directory.CreateDirectory(folder);var t=new Track{Video=new Device{Name="Synthetic",Id="test",Video=true},Audio=new Device{Name="Sine",Id="sine"}};
 string raw=Path.Combine(folder,"Piste_01_Synthetic_brut.mp4");string args="-hide_banner -y -f lavfi -i testsrc=size=160x120:rate=30:duration=2 -f lavfi -i sine=frequency=440:sample_rate=48000:duration=2 -filter_complex \"[0:v]showinfo,setpts=PTS-STARTPTS,split=2[v][p];[p]fps=5,scale=800:600[preview];[1:a]ashowinfo,asetpts=PTS-STARTPTS[a]\" -map [v] -map [a] -c:v libx264 -preset ultrafast -pix_fmt yuv420p -c:a aac "+Engine.Q(raw)+" -map [preview] -an -c:v rawvideo -pix_fmt bgr24 -f rawvideo pipe:1";
 var c=Engine.Start(t,folder,1,false,args);if(!c.Process.WaitForExit(15000)){Engine.Stop(c);throw new Exception("Synthetic encoding timeout");}Engine.Stop(c);Check(c.Valid&&c.Duration>1.9,"synthetic audio/video encode");Check(!double.IsNaN(c.VideoOrigin)&&!double.IsNaN(c.AudioOrigin),"both source timestamp origins parsed");Check(Math.Abs(c.VideoOrigin-c.AudioOrigin)<0.002,"shared synthetic origins");using(var preview=c.Image())Check(preview!=null&&preview.Width==800&&preview.Height==600,"same-source preview stream");Engine.Finalize(c,c.VideoOrigin+0.5);Check(c.Valid&&c.Duration>1.4&&c.Duration<1.6,"half-second audio/video trim");Check(File.Exists(raw)&&c.File!=raw,"raw retained for latency analysis");c.Clear();
 var desktop=new Track{Video=new Device{Name="Screen",Id="Screen",Video=true,Desktop=true,Bounds=new Rectangle(-1920,0,1920,1080)}};string a=Engine.Arguments(desktop,"screen.mp4",false,1000);Check(a.Contains("gdigrab")&&a.Contains("-offset_x -1920")&&a.Contains("-itsoffset -1000.000000"),"desktop and common epoch arguments");Check(Engine.NewTake(folder,new DateTime(2026,10,2,12,0,0))!=Engine.NewTake(folder,new DateTime(2026,10,2,12,0,0)),"take collisions");
 using(var f=new MainForm()){f.ShowInTaskbar=false;f.Opacity=0;f.Show();Application.DoEvents();f.TestPacket("LCD831|UKD1|L|EP|0|STA END S");foreach(string id in new[]{"EP","A","B","C","D"})for(int r=0;r<(id=="EP"?4:2);r++)f.TestPacket("LCD831|UKD1|L|"+id+"|"+r+"|TEST "+id+r);Check((int)Field(f,"rowCount")==12,"Picran twelve lines through app parser");f.TestPacket("SK1,MODE,1");var capture=(CapturePanel)Field(f,"capture933");Check(capture.EnabledOnTee,"mode requires Tee acknowledgement");f.TestPacket("SK1,MODE,0");Check(!capture.EnabledOnTee,"mode disable acknowledgement");f.LiveImage(Path.Combine(folder,"Snkoz.png"));((TabControl)Field(f,"tabs933")).SelectedIndex=1;Application.DoEvents();f.LiveImage(Path.Combine(folder,"Capture.png"));f.Close();Application.DoEvents();}
 File.WriteAllText(Path.Combine(folder,"tests933.txt"),"PASS "+count+" checks; synthetic media only, no cameras, audio devices, transport or SD session.");
 }
 internal static void Lcd(string port,string folder){Directory.CreateDirectory(folder);using(var f=new MainForm()){f.ShowInTaskbar=false;f.Opacity=0;f.Show();Application.DoEvents();((ComboBox)Field(f,"lcdPorts")).Text=port;typeof(MainForm).GetMethod("ConnectLcd",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(f,null);var timer=Stopwatch.StartNew();while((int)Field(f,"rowCount")<12&&timer.ElapsedMilliseconds<5000){Application.DoEvents();Thread.Sleep(10);}Check((int)Field(f,"rowCount")==12,"live Picran 12/12");f.LiveImage(Path.Combine(folder,"Picran_live.png"));File.WriteAllLines(Path.Combine(folder,"Picran_live.txt"),f.state.Screens.Lines[0]);File.AppendAllText(Path.Combine(folder,"Picran_live.txt"),"\r\nPASS 12/12 LCD lines through Snkoz V9.33; Tee connection closed.\r\n");f.Close();Application.DoEvents();}}
}
}
