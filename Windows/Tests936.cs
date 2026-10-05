using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Synkoz;
namespace UrbanKaoz.DisplayRecorder {
 internal static class Tests936 {
  static int count;static void Check(bool ok,string message){count++;if(!ok)throw new Exception(message);}
  static object Field(object o,string n){return o.GetType().GetField(n,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(o);}
  static void Set(object o,string n,object v){o.GetType().GetField(n,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(o,v);}
  static void Wait(Task task){while(!task.IsCompleted){Application.DoEvents();System.Threading.Thread.Sleep(5);}task.GetAwaiter().GetResult();}
  internal static void AndroidCheck(string folder){
   Directory.CreateDirectory(folder);string serial=Environment.GetEnvironmentVariable("SNKOZ_TEST_SERIAL"),code=Environment.GetEnvironmentVariable("SNKOZ_TEST_CODE");
   var device=Android936.Devices().Single(d=>d.PhoneSerial==serial);var track=new Track{Video=device,PhoneCode=Android936.Protect(code),Format="MP4"};
   using(var link=new Android936(track)){
    var status=link.Call("/status");if(Convert.ToString(status["state"])!="stopped")throw new Exception("Téléphone occupé ; aucun test lancé.");
    link.Measure();File.WriteAllText(Path.Combine(folder,"android_connection.txt"),"Version="+Convert.ToString(status["version"])+"\r\nState="+status["state"]+"\r\nReady="+status["ready"]+"\r\nMinimum RTT ms="+(link.Rtt*1000).ToString("F3",Engine.Inv)+"\r\nAuthenticated USB/status/5 clock samples PASS. No recording or media download.\r\n");
    Store934.Save(Path.Combine(folder,"Y5_Prise_USB.synkozconfig"),new CaptureConfig936{TakeFolder=Preferences936.Load().DefaultTakeFolder,Tracks=new List<Track>{track}});
   }
  }
  internal static void Run(string root){
   Directory.CreateDirectory(root);string previous=Preferences936.FilePath;Preferences936.FilePath=Path.Combine(root,"preferences.json");
   try{
    var pref=Preferences936.Load();Check(pref.LastConfigFolder==@"C:\Arduino\GPT\Projects\Synkoz\Config","config fallback");pref.DefaultTakeFolder=Path.Combine(root,"Default");pref.Save();
    var conf=new CaptureConfig936{TakeFolder=Path.Combine(root,"Other"),Tracks=new List<Track>{new Track{Video=new Device{Name="Camera / 1",Video=true},CameraFps=29.97,PhoneCode=Android936.Protect("test-only")}}};
    Store934.Save(Path.Combine(root,"sources.synkozconfig"),conf);var loaded=Store934.Load<CaptureConfig936>(Path.Combine(root,"sources.synkozconfig"));Check(loaded.TakeFolder==conf.TakeFolder&&loaded.Tracks[0].CameraFps==29.97,"config includes folder and fractional FPS");Check(!File.ReadAllText(Path.Combine(root,"sources.synkozconfig")).Contains("test-only"),"USB code protected");Check(Android936.Unprotect(loaded.Tracks[0].PhoneCode)=="test-only","USB code DPAPI roundtrip");Check(Preferences936.Load().DefaultTakeFolder==pref.DefaultTakeFolder,"config does not alter default folder");
    Check(Sources936.FileLabel(conf.Tracks[0])=="Camera _ 1","safe camera filenames");Check(Sources936.FileLabel(new Track{Video=new Device{TextSource="EP"}})=="EP-UK"&&Sources936.FileLabel(new Track{Video=new Device{TextSource="ABCD"}})=="E4-UK","LCD names");
    var phone=new Track{Video=new Device{Name="Android",PhoneSerial="test",Video=true}};Check(Engine.IsPhone(phone)&&Engine.HasAudio(phone),"Android includes local microphone");
    var timing=new Dictionary<string,object>{{"firstSensorNs",2000000000L},{"firstCallbackNs",2200000000L},{"recordStartNs",1900000000L},{"sensorRealtime",true}};var status=new Dictionary<string,object>{{"timing",timing}};Check(Math.Abs(Android936.Origin(status,100)-102)<1e-8,"sensor monotonic clock mapping");timing["sensorRealtime"]=false;Check(Math.Abs(Android936.Origin(status,100)-102.2)<1e-8,"unknown sensor clock uses receipt estimate");
    using(var setup=new Setup(phone,new List<Device>{phone.Video})){var encoder=(ComboBox)Field(setup,"encoder");Check(encoder.Items.Count==1&&((string)encoder.SelectedItem).Contains("téléphone"),"Android encoding displayed");Check(!((ComboBox)Field(setup,"audio")).Enabled,"no unrelated PC mic on phone source");Check(((Button)Field(setup,"test")).Text=="Vérifier connexion","Android connection check distinct from camera recording test");}
    // Exercise the actual Finish path, with a synthetic audio file and one failed source.
    string fixture=Path.Combine(root,"fixture.wav");Engine.Run(Engine.FF,"-hide_banner -y -f lavfi -i sine=frequency=440:duration=3 -c:a pcm_s16le "+Engine.Q(fixture));
    for(int mode=0;mode<2;mode++){
     string folder=Path.Combine(root,mode==0?"StopShort":"PauseShort");Directory.CreateDirectory(folder);
     var t=new Track{Audio=new Device{Name="Synthetic"}};string raw=Path.Combine(folder,"Piste_01_Synthetic_brut.wav");File.Copy(fixture,raw,true);double now=Engine.Epoch();
     using(var panel=new CapturePanel(()=>false,()=>{},s=>{throw new Exception("No hardware command expected");})){
      var tracks=(List<Track>)Field(panel,"tracks");tracks.Clear();tracks.Add(t);var missing=new Track{Video=new Device{Name="Missing",Video=true}};tracks.Add(missing);
      var captures=(List<Capture>)Field(panel,"active");captures.Add(new Capture{Track=t,File=raw,Valid=true,Closed=true,Duration=3,AudioOrigin=now-1});captures.Add(new Capture{Track=missing,Closed=true,Failed=true,Failure="Unavailable"});
      var montage=new Montage{Folder=folder,Tracks=new List<Track>(tracks)};Set(panel,"project",montage);Set(panel,"folder",folder);Set(panel,"segment",1);Set(panel,"music",now-0.3);Set(panel,"musicTick",(uint)1536);Set(panel,"segmentBegan",now-1);((CheckBox)Field(panel,"startTee")).Checked=true;Set(panel,"recording",true);
      var method=typeof(CapturePanel).GetMethod("Finish",BindingFlags.NonPublic|BindingFlags.Instance,null,new[]{typeof(bool)},null);Wait((Task)method.Invoke(panel,new object[]{mode==1}));
      if(mode==0){Check(montage.Duration>0.29&&montage.Duration<2,"Stop keeps sub-bar interval");Check(montage.Blocks.Count==2&&File.Exists(Store934.MediaPath(folder,montage.Blocks[0].File)),"failed source does not block good trim");Check(!string.IsNullOrEmpty(montage.Blocks[1].Issue),"failed source remains explicit");}else Check(montage.Duration==0&&montage.Blocks.Count==0,"Pause still discards incomplete first bar");Check(File.Exists(raw),"raw preserved");
     }
    }
    File.WriteAllText(Path.Combine(root,"tests935r2.txt"),"PASS "+count+" assertions: actual Stop/Pause finalization, failure isolation, paths, Android timing mapping and UI. Synthetic media only.");
   }finally{Preferences936.FilePath=previous;}
  }
 }
}
