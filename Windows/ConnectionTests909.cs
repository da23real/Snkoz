using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
namespace UrbanKaoz.DisplayRecorder {
 internal static class ConnectionTests909 {
  static object Get(MainForm f,string name){return typeof(MainForm).GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(f);}
  static void Wait(Func<bool> done,int timeout){var w=Stopwatch.StartNew();while(!done()){Application.DoEvents();Thread.Sleep(10);if(w.ElapsedMilliseconds>timeout)throw new Exception("Connection test timeout");}}
  internal static void Run(string folder,string port){
   Directory.CreateDirectory(folder);
   using(var f=new MainForm()){
    f.Show();Application.DoEvents();f.LiveConnect(port,"");
    Wait(()=>f.surface.Connected&&f.state.Song.Synced,15000);
    var w=Stopwatch.StartNew();while(w.ElapsedMilliseconds<30000){Application.DoEvents();Thread.Sleep(10);if(!f.surface.Connected)throw new Exception("Connection lost during 30s: "+((Label)Get(f,"status")).Text);}
    var automatic=(CheckBox)Get(f,"autoConnect909");automatic.Checked=true;
    typeof(MainForm).GetMethod("Fault",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(f,new object[]{"test reprise contrôlée"});
    w.Restart();Wait(()=>f.surface.Connected&&f.state.Song.Synced,15000);long elapsed=w.ElapsedMilliseconds;
    if(elapsed<950)throw new Exception("Reconnect attempted too soon");
    ((Button)Get(f,"connect")).PerformClick();if(automatic.Checked||f.surface.Connected)throw new Exception("Manual disconnect ignored");
    w.Restart();while(w.ElapsedMilliseconds<1600){Application.DoEvents();Thread.Sleep(10);}
    if(f.surface.Connected||(bool)Get(f,"connecting909"))throw new Exception("Unwanted reconnect after manual stop");
    File.WriteAllText(Path.Combine(folder,"connection_live.txt"),"PASS: real handshake + Song sync; stable 30 seconds; automatic reconnect and fresh sync in "+elapsed+" ms; manual disconnect disables retry. No musical command, SD write or restore.\n");
   }
  }
 }
}
