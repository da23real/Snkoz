using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Reflection;

namespace UrbanKaoz.DisplayRecorder {
  internal static class Program {
    [DllImport("user32.dll")]static extern bool SetProcessDPIAware();
    [STAThread]static int Main(string[] args){
      SetProcessDPIAware();Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
      try{
        if(args.Length==2&&args[0]=="--android-synthetic"){AndroidTests936.Run(args[1]);return 0;}
        if(args.Length==2&&args[0]=="--android-check"){Tests936.AndroidCheck(args[1]);return 0;}
        if(args.Length==2&&args[0]=="--revision-test"){Tests936.Run(args[1]);return 0;}
        if(args.Length==2&&args[0]=="--sources-test"){Tests935.Run(args[1]);return 0;}
        if(args.Length==2&&args[0]=="--nvenc-test"){SynkozTests934.Nvenc(args[1]);return 0;}
        if(args.Length==2&&args[0]=="--montage-test"){SynkozTests934.Run(args[1]);return 0;}
        if(args.Length==2&&args[0]=="--synkoz-test"){SynkozTests933.Run(args[1]);return 0;}
        if(args.Length==3&&args[0]=="--lcd-check"){SynkozTests933.Lcd(args[1],args[2]);return 0;}
        if(args.Length==2&&args[0]=="--audio-settings-test"){AudioTests918.Run(args[1]);return 0;}
        if(args.Length==2&&args[0]=="--numeric-test"){NumericTests917.Run(args[1]);return 0;}
        if(args.Length==3&&args[0]=="--connection-test"){ConnectionTests909.Run(args[1],args[2]);return 0;}
        if(args.Length>1&&args[0]=="--sd-test"){SdTests905.Run(args[1]);return 0;}
        if(args.Length>1&&args[0]=="--self-test"){Tests.Run(args[1]);return 0;}
        if(args.Length==4&&args[0]=="--live-test"){Tests.Live(args[1],args[2],args[3]);return 0;}
        Synkoz.Language945.Start();Application.Run(new MainForm());return 0;
      }catch(Exception ex){string p=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"error831.txt");File.WriteAllText(p,ex.ToString());if(args.Length==0)MessageBox.Show(ex.Message,"Snkoz");return 1;}
    }
  }
  internal static class Tests {
    static int checks;
    static void Check(bool ok,string what){checks++;if(!ok)throw new Exception("Test "+checks+" : "+what);}
    internal static void Run(string folder){
      var gate=new AudioGate904();
      Check(!gate.Tick(0,true,100,800,120),"audio attack waits");
      Check(!gate.Tick(50,false,100,800,120),"short noise rejected");
      Check(!gate.Tick(100,true,100,800,120),"attack restarts");
      Check(gate.Tick(200,true,100,800,120),"first note at attack end");
      Check(!gate.Tick(201,true,100,800,120),"no duplicate note");
      Check(!gate.Tick(300,false,100,800,120)&&gate.Active,"release keeps envelope active");
      Check(gate.Tick(700,false,100,800,120),"notes continue during release");
      Check(!gate.Tick(1000,true,100,800,120)&&gate.Active,"return cancels release without new attack");
      Check(gate.Tick(1200,true,100,800,120),"cadence preserved after return");
      Check(!gate.Tick(1300,false,100,800,120),"second release begins");
      Check(gate.Tick(1700,false,100,800,120),"release second beat");
      Check(!gate.Tick(2100,false,100,800,120)&&!gate.Active,"release ends before next note");
      gate.Reset();Check(gate.Tick(0,true,0,0,115),"zero attack");
      Check(!gate.Tick(521,true,0,0,115)&&gate.Tick(522,true,0,0,115),"115 BPM fallback cadence");
      Check(gate.Tick(10000,true,0,0,115)&&!gate.Tick(10001,true,0,0,115),"no catchup burst after stall");
      Check(!gate.Tick(10002,false,0,0,115)&&!gate.Active,"zero release stops immediately");
      gate.Reset();Check(gate.Tick(0,true,0,0,double.NaN)&&gate.Tick(522,true,0,0,115),"invalid tempo uses fallback");
      Directory.CreateDirectory(folder);
      using(var audio=new AudioMidiForm(()=>115,(k,c,n,v)=>{},()=>false)){audio.Show();Application.DoEvents();using(var preview=new Bitmap(audio.Width,audio.Height)){audio.DrawToBitmap(preview,new Rectangle(Point.Empty,preview.Size));preview.Save(Path.Combine(folder,"audio_notes.png"));}audio.Close();}
      var markedState=new ScreenState();Check(UkdProtocol.Apply("UKD1|L|B|1|%U3384 096 000 000",markedState),"quarter marker protocol");
      Check(markedState.Lines[2][1][0]==(char)0xE133,"quarter marker retained");
      markedState.Lines[2][0]="GA+ TIM PBN ---";
      using(var preview=LcdRenderer.RenderSingle(markedState.Lines[2],16,640,100,DisplaySkin.RealLcd,true))preview.Save(Path.Combine(folder,"gate_quarter937.png"));
      var inverseState=new ScreenState();Check(UkdProtocol.Apply("UKD1|L|EP|0|%I3120   %I312/4",inverseState),"inverse LCD protocol");
      Check(inverseState.Lines[0][0][0]==(char)0xE031,"inverse LCD character preserved");
      inverseState.Lines[0][1]="X01  1    0001 0016";inverseState.Lines[0][2]="PARA WRITE FX   ENV";inverseState.Lines[0][3]="TRACK PATCH --  LFO";
      using(var preview=LcdRenderer.RenderSingle(inverseState.Lines[0],20,640,240,DisplaySkin.RealLcd,true))preview.Save(Path.Combine(folder,"ep_inverse.png"));
      var s=new CaptureState();
      Check(s.Apply("UKD1|L|EP|0|ONE%7CTWO%25"),"LCD escaping");Check(s.Screens.Lines[0][0].StartsWith("ONE|TWO%"),"LCD decode");
      Check(!s.Apply("UKD1|L|EP|7|bad"),"invalid row");Check(!s.Apply("SONG,CLIP,bad"),"truncated Song");
      Check(!s.Apply("VIEW831,NaN,9,30,0,800,200"),"NaN view");Check(!s.Apply("PC1,LED,8,0"),"invalid LED");
      Check(Surface.PicoPin(26)=="patte 31 (GP26)"&&Surface.PicoPin(28)=="patte 34 (GP28)","ADC GPIO physical pins");
      Check(ObsWatcher.Authentication("supersecretpassword","lM1GncleQOaCu9lT1yeUZhFYnqhsLLP1G5lAGo3ixaI=","+IxH4CnCiqpX1rM9scsNynZzbOe4KhDeYcTNS3PDaeY=")=="1Ct943GAT+6YQUUX47Ia/ncufilbe6+oD6lY+5kaCu4=","OBS authentication retained");
      s.Apply("SONG,BEGIN,2");s.Apply("SONG,CLIP,0,2,3,16,0,32");s.Apply("SONG,ZL,1,2,0,1536,96,60,90");s.Apply("SONG,STATE,1,0,0,1,1,4,11500,0,0,0");s.Apply("SONG,END");
      Check(s.Song.Synced&&s.Song.Clips.Count==1&&s.Song.Zones.Count==1,"Song synchronization");
      var copy=new CaptureState();foreach(var p in s.Snapshot())copy.Apply(p);Check(copy.Song.Clips[0].Position==16&&copy.Song.Zones[1].A==60&&copy.HasSong,"snapshot preserves arrangement");
      string path=Path.Combine(folder,"capture_test.ukd");using(var log=new StreamWriter(path,false,new UTF8Encoding(false))){log.WriteLine("# UKDLOG2");foreach(var p in s.Snapshot())log.WriteLine("0000000000|"+p);log.WriteLine("0000000250|UKD1|L|EP|0|AFTER");log.WriteLine("0000001500|END831");}
      Check(Recording.Duration(path)==1500,"quiet tail retains stop duration");Recording.Export(path,Path.Combine(folder,"capture_test"),10,DisplaySkin.Futuristic,true,delegate{},()=>false);
      foreach(var p in Recording.Paths(Path.Combine(folder,"capture_test"))){var bytes=File.ReadAllBytes(p);Check(Encoding.ASCII.GetString(bytes,0,4)=="RIFF","AVI RIFF");int at=Find(bytes,"avih");Check(at>=0&&BitConverter.ToInt32(bytes,at+8+16)==15,"all videos have exactly 15 frames");}
      using(var surface=new Surface()){int pads=0,ky=0,roy=0;foreach(var c in surface.Items){if(c.Kind=="NEO")pads++;if(c.Kind=="KB")ky++;if(c.Kind=="RB")roy++;Check(c.Bounds.Left>=0&&c.Bounds.Right<=1240&&c.Bounds.Bottom<=312,"control within 720p canvas: "+c.Label);Check(c.Wiring.Length>20,"wiring present");}Check(pads==64&&ky==16&&roy==4,"complete pads/encoders");}
      var sent=new List<string>();using(var surface=new Surface{Size=new Size(1240,312),Connected=true,Send=sent.Add}){
        surface.CreateControl();Mouse(surface,"OnMouseDown",MouseButtons.Left,15,185,0);Mouse(surface,"OnMouseUp",MouseButtons.Left,15,185,0);
        Check(sent.Count==2&&sent[0]=="NEO,0,1"&&sent[1]=="NEO,0,0","pad press/release routed");sent.Clear();
        Mouse(surface,"OnMouseDown",MouseButtons.Left,30,70,0);Mouse(surface,"OnMouseUp",MouseButtons.Left,30,70,0);typeof(Surface).GetMethod("FlushClick917",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(surface,null);Check(sent.Count==2&&sent[0]=="KB,0,1"&&sent[1]=="KB,0,0","encoder click");sent.Clear();
        Mouse(surface,"OnMouseWheel",MouseButtons.None,110,70,-120);Check(sent.Count==1&&sent[0]=="KT,1,-1","encoder wheel");sent.Clear();
        Mouse(surface,"OnMouseDown",MouseButtons.Left,30,70,0);Mouse(surface,"OnMouseMove",MouseButtons.Left,30,52,0);Mouse(surface,"OnMouseUp",MouseButtons.Left,30,52,0);Check(sent.Count==1&&sent[0]=="KT,0,3","encoder drag does not generate click");sent.Clear();
        Mouse(surface,"OnMouseDown",MouseButtons.Left,15,185,0);surface.Capture=false;Check(sent.Count==2&&sent[1]=="NEO,0,0","lost mouse capture releases pad");sent.Clear();
        surface.Connected=false;Mouse(surface,"OnMouseDown",MouseButtons.Left,15,185,0);Mouse(surface,"OnMouseUp",MouseButtons.Left,15,185,0);Check(sent.Count==0,"offline commands blocked");
      }
      using(var f=new MainForm()){f.ShowInTaskbar=false;f.Opacity=0;f.Show();Application.DoEvents();f.PerformLayout();Check(f.Width<=1280&&f.Height<=680,"window fits 1280x720 with taskbar");
        for(int i=0;i<6;i++){f.RenderPreview(Path.Combine(folder,"skin_"+i+".png"),i);Check(f.surface.Height>=280&&f.timeline.Height>=130,"both views fit");}
      }
      using(var f=new MainForm()){
        Check(f.AudioTempo904()==115,"audio offline tempo 115");
        sent.Clear();f.TestWriter=sent.Add;f.TestPacket("PC1,HELLO,V9.05,1");
        f.TestPacket("SONG,STATE,0,0,0,0,1,4,13250,1,0,0");Check(f.AudioTempo904()==132.5,"audio uses connected Tee tempo");
        f.TestCommand("HC,24,1");f.TestCommand("HC,24,0");Check(sent.Count==1&&sent[0]=="PC1,1,HC,24,1","one command in flight");
        f.TestPacket("PC1,ACK,999,OK");Check(sent.Count==1,"unrelated ACK ignored");
        f.TestPacket("PC1,ACK,1,OK");Check(sent.Count==2&&sent[1]=="PC1,2,HC,24,0","release ordered after press ACK");
        f.TestPacket("PC1,ACK,1,OK");Check(sent.Count==2,"old ACK ignored");
        typeof(MainForm).GetField("sentAt",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(f,-10000L);f.TestTick();Check(sent[2]==sent[1],"retry keeps same sequence and packet");
        f.TestPacket("PC1,ACK,2,OK");f.TestPacket("PC1,ACK,3,INVALID");
        // A rejected pending command closes the session and disables every input.
        f.TestCommand("KT,0,1");int lastSequence=int.Parse(sent[sent.Count-1].Split(',')[1]);f.TestPacket("PC1,ACK,"+lastSequence+",INVALID");Check(!f.surface.Connected,"rejection closes unsafe session");
      }
      using(var f=new MainForm()){
        bool refused=false;try{f.SendUsbMessage901("NOTE",1,60,100);}catch(IOException){refused=true;}Check(refused,"USB note refuses disconnected Tee");
        sent.Clear();f.TestWriter=sent.Add;f.TestPacket("PC1,HELLO,V9.05,1");
        f.SendUsbMessage901("NOTE",1,60,100);f.SendUsbMessage901("OFF",1,60,0);f.SendUsbMessage901("CC",16,7,100);
        Check(sent.Count==1&&sent[0]=="PC1,1,NOTE,1,60,100","USB note uses serial command queue");
        f.TestPacket("PC1,ACK,1,OK");Check(sent[1]=="PC1,2,OFF,1,60,0","USB note off ordered");
        f.TestPacket("PC1,ACK,2,OK");Check(sent[2]=="PC1,3,CC,16,7,100","USB CC transport preserved");
        refused=false;try{f.SendUsbMessage901("NOTE",17,60,100);}catch(ArgumentException){refused=true;}Check(refused,"USB invalid channel rejected");
      }
      string peer909;
      foreach(string v in new[]{"V9.01","V9.08","V9.18","V12.42"})Check(MainForm.Compatible909("PC1,HELLO,"+v+",1",out peer909)&&peer909==v,"delivery independent handshake "+v);
      foreach(string p in new[]{"PC1,HELLO,V9.18,2","PC1,HELLO,V9.18,10","PC1,HELLO,,1","PC1,HELLO,V9.18,1,junk"})Check(!MainForm.Compatible909(p,out peer909),"reject malformed or incompatible protocol");
      using(var f=new MainForm()){
        var lamp=(Label)Get(f,"audioLamp909");var automatic=(CheckBox)Get(f,"autoConnect909");
        Check(!automatic.Checked,"auto connect off by default");
        sent.Clear();f.TestWriter=sent.Add;f.TestPacket("PC1,HELLO,V99.42,1");Check(f.surface.Connected,"future firmware accepted by actual receiver");
        f.TestCommand("PING");f.SendUsbMessage901("NOTE",1,60,100);
        Check(lamp.ForeColor==Color.DimGray,"queued note does not light indicator");
        f.TestPacket("PC1,ACK,1,OK");Check(lamp.ForeColor==Color.LimeGreen,"note write lights without its ACK");
        Set(f,"audioUntil909",1L);f.TestTick();Check(lamp.ForeColor==Color.DimGray,"note pulse expires");
        f.TestPacket("PC1,ACK,2,OK");f.SendUsbMessage901("OFF",1,60,0);Check(lamp.ForeColor==Color.DimGray,"note off does not pulse");
        automatic.Checked=true;typeof(MainForm).GetMethod("Fault",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(f,new object[]{"test disconnect"});Check(automatic.Checked&&!f.surface.Connected,"fault retains reconnect choice and disables commands");
        Check((long)Get(f,"nextConnect909")>0,"reconnect is delayed");automatic.Checked=false;
        f.TestPacket("PC1,HELLO,V99.42,2");Check(!f.surface.Connected,"unsupported protocol actually rejected");
      }
      PauseTests(folder);
      File.WriteAllText(Path.Combine(folder,"tests_windows.txt"),checks+" PASS\nProtocol, snapshot, quiet tail, 3 MJPEG AVI, control geometry, 6 skins.\n");
    }
    static void Set(MainForm f,string field,object value){typeof(MainForm).GetField(field,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(f,value);}
    static object Get(MainForm f,string field){return typeof(MainForm).GetField(field,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(f);}
    static void PauseTests(string folder){
      string dir=Path.Combine(folder,"pause-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
      using(var f=new MainForm()){
        Set(f,"ready",true);Set(f,"rowCount",12);Set(f,"rows",new bool[]{true,true,true,true,true,true,true,true,true,true,true,true});Set(f,"obsFolder",dir);Set(f,"obsMidi",true);f.state.Song.Synced=true;int midi=0;f.TestMidiStart=()=>midi++;
        f.ObsRecording("OBS_WEBSOCKET_OUTPUT_RESUMED");Check(Get(f,"log")==null&&midi==0,"resume without take ignored");
        f.ObsRecording("OBS_WEBSOCKET_OUTPUT_STARTING");Check(Get(f,"log")==null,"starting is not started");
        f.ObsRecording("OBS_WEBSOCKET_OUTPUT_STARTED");var writer=Get(f,"log");Check(writer!=null&&midi==1,"start captures and sends MIDI once");
        f.ObsRecording("OBS_WEBSOCKET_OUTPUT_STARTED");Check(Get(f,"log")==writer&&midi==1,"duplicate start ignored");
        f.TestPacket("SONG,CLIP,42,0,0,0,0,16");System.Threading.Thread.Sleep(120);
        f.ObsRecording("OBS_WEBSOCKET_OUTPUT_PAUSED");var clock=(System.Diagnostics.Stopwatch)Get(f,"recordingClock");long time=clock.ElapsedMilliseconds;
        f.TestPacket("UKD1|L|EP|0|PAUSED_TRANSIENT");f.state.Song.Clips.Clear();f.state.Screens.Lines[0][0]="RESUMED_STATE";
        System.Threading.Thread.Sleep(220);f.ObsRecording("OBS_WEBSOCKET_OUTPUT_PAUSED");Check(clock.ElapsedMilliseconds==time&&!clock.IsRunning,"duplicate pause keeps clock frozen");
        Check(Get(f,"log")==writer,"pause keeps original journal");
        f.ObsRecording("OBS_WEBSOCKET_OUTPUT_RESUMED");Check(clock.IsRunning&&midi==1&&Get(f,"log")==writer,"resume keeps take without MIDI Start");
        f.ObsRecording("OBS_WEBSOCKET_OUTPUT_RESUMED");Check(midi==1,"duplicate resume never sends MIDI");System.Threading.Thread.Sleep(120);
        f.ObsRecording("OBS_WEBSOCKET_OUTPUT_PAUSED");long end=clock.ElapsedMilliseconds;System.Threading.Thread.Sleep(100);
        f.ObsRecording("OBS_WEBSOCKET_OUTPUT_STOPPING");Check(Get(f,"log")!=null,"stopping does not close early");
        f.ObsRecording("OBS_WEBSOCKET_OUTPUT_STOPPED");Check(Get(f,"log")==null,"stop while paused finalizes");
        string[] files=Directory.GetFiles(dir,"*.ukd");Check(files.Length==1,"all pauses in one take");
        Check(Recording.Duration(files[0])==end,"end excludes both pauses");string text=File.ReadAllText(files[0]);Check(!text.Contains("PAUSED_TRANSIENT")&&text.Contains("RESUMED_STATE"),"pause drops transient changes and resume snapshots state");
        var replay=new CaptureState();foreach(var e in Recording.Events(files[0]))replay.Apply(e.Packet);Check(replay.Song.Clips.Count==0,"resume snapshot preserves deletions");
        Recording.Export(files[0],Path.Combine(dir,"pause"),10,DisplaySkin.Futuristic,true,delegate{},()=>false);
        foreach(string file in Recording.Paths(Path.Combine(dir,"pause"))){byte[] bytes=File.ReadAllBytes(file);int at=Find(bytes,"avih");Check(BitConverter.ToInt32(bytes,at+24)==(int)Math.Ceiling(end/100.0),"video frame count excludes pause");}
        f.ObsRecording("OBS_WEBSOCKET_OUTPUT_RESUMED");Check(Directory.GetFiles(dir,"*.ukd").Length==1&&Get(f,"log")==null,"late resume cannot create a new take");
        f.LiveStart(Path.Combine(dir,"manual.ukd"));f.ObsRecording("OBS_WEBSOCKET_OUTPUT_STARTED");f.ObsRecording("OBS_WEBSOCKET_OUTPUT_PAUSED");Check(!(bool)Get(f,"capturePaused"),"OBS does not pause manual capture");f.ObsRecording("OBS_WEBSOCKET_OUTPUT_STOPPED");Check(Get(f,"log")!=null,"OBS does not close manual capture");f.LiveStop();
      }
    }
    static void Mouse(Control control,string method,MouseButtons button,int x,int y,int delta){control.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(control,new object[]{new MouseEventArgs(button,1,x,y,delta)});}
    static int Find(byte[] data,string token){byte[] find=Encoding.ASCII.GetBytes(token);for(int i=0;i<=data.Length-find.Length;i++){bool yes=true;for(int j=0;j<find.Length;j++)if(data[i+j]!=find[j]){yes=false;break;}if(yes)return i;}return -1;}
    static void WaitUntil(Func<bool> condition,int milliseconds,string reason){var watch=System.Diagnostics.Stopwatch.StartNew();while(!condition()&&watch.ElapsedMilliseconds<milliseconds){Application.DoEvents();System.Threading.Thread.Sleep(10);}Check(condition(),reason);}
    internal static void Live(string tee,string lcd,string folder){
      Directory.CreateDirectory(folder);using(var form=new MainForm{ShowInTaskbar=false,Opacity=0}){
        form.Show();Application.DoEvents();form.LiveConnect(tee,lcd);WaitUntil(()=>form.LiveReady,18000,"real Song + 12 LCD rows + PC handshake");
        Check(!form.state.Song.Playing,"device stopped before non-musical control test");
        int before=LastValue(form.state.Screens.Lines[1][1]);Check(before>0&&before<120,"velocity test has room in both directions");string log=Path.Combine(folder,"capture_live.ukd");form.LiveStart(log);
        form.TestCommand("KT,3,1");WaitUntil(()=>LastValue(form.state.Screens.Lines[1][1])>before,4500,"remote KY4 changes velocity on device");
        form.TestCommand("KT,3,-1");WaitUntil(()=>LastValue(form.state.Screens.Lines[1][1])==before,4500,"remote KY4 restores initial velocity");
        form.TestCommand("MASK,HC,47,1");form.TestCommand("MASK,HC,47,0");
        var until=DateTime.UtcNow.AddSeconds(1);while(DateTime.UtcNow<until){Application.DoEvents();System.Threading.Thread.Sleep(10);}
        Check(form.LiveReady,"session remains healthy after command/mask ACKs");
        form.LiveStop();form.LiveImage(Path.Combine(folder,"live_1280.png"));form.Close();
        Recording.Export(log,Path.Combine(folder,"capture_live"),10,DisplaySkin.Futuristic,true,delegate{},()=>false);
        File.WriteAllText(Path.Combine(folder,"tests_live.txt"),checks+" PASS\nTeensy "+tee+", Picran "+lcd+"; 12 real LCD rows, Song END, LED telemetry, acknowledged encoder commands; velocity "+before+" restored; 3 live AVI exported.\nNo transport, pad notes, SD save or restore requested.\n");
      }
    }
    static int LastValue(string line){var v=line.Trim().Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries);int value;return v.Length>0&&int.TryParse(v[v.Length-1],out value)?value:-1;}
  }
}





