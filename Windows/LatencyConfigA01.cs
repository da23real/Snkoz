using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Synkoz {
 public sealed partial class CapturePanel {
  void SaveCapturePreferencesA01(){var current=Preferences936.Load();current.DefaultTakeFolder=prefs936.DefaultTakeFolder;current.LastConfigFolder=prefs936.LastConfigFolder;current.Save();}
  async Task LoadConfig(bool append=false){
   if(Busy||paused||DirectOpen939)return;string path;
   Directory.CreateDirectory(prefs936.LastConfigFolder);
   using(var d=new OpenFileDialog{AutoUpgradeEnabled=true,InitialDirectory=prefs936.LastConfigFolder,Filter="Configuration Snkoz|*.synkozconfig"}){if(d.ShowDialog()!=DialogResult.OK)return;path=d.FileName;}
   try{
    var json=File.ReadAllText(path).TrimStart();var config=json.StartsWith("[")?new CaptureConfig936{Tracks=Store934.Load<List<Track>>(path),TakeFolder=takeFolder936}:Store934.Load<CaptureConfig936>(path);
    if(config==null||config.Tracks==null||config.Tracks.Exists(t=>t==null))throw new Exception("Configuration invalide.");
    var incoming=config.LatencyA01??new LatencySettingsA01();incoming.Normalize();string destination=append||string.IsNullOrWhiteSpace(config.TakeFolder)?takeFolder936:Path.GetFullPath(config.TakeFolder);
    await StopPreviews();devices=await Task.Run(()=>Engine.Devices());
    if(!append){microOnTakeA01.Checked=config.MicroOnTakeA01;if(WriteMicroA01!=null)WriteMicroA01(config.MicroA01);tracks.Clear();latencyA01=incoming;defaultFolder936.Checked=false;takeFolder936=destination;prefs936.DefaultTakeFolder=destination;takeLabel936.Text=takeFolder936;}else latencyA01.Merge(incoming);
    foreach(var t in config.Tracks){if(t.Fx==null)t.Fx=new TrackFx936();if(append&&tracks.Any(x=>LatencyAnalysisA01.Key(x)==LatencyAnalysisA01.Key(t)))continue;t.Video=Resolve(t.Video);t.Audio=Resolve(t.Audio);tracks.Add(t);}
    prefs936.LastConfigFolder=Path.GetDirectoryName(path);SaveCapturePreferencesA01();Render();status.Text=append?"Configuration ajoutée ; appareils identiques conservés, mesures fusionnées sans doublons.":"Configuration chargée ; dossier de prises mémorisé.";await StartPreviews();
   }catch(Exception ex){status.Text=ex.Message;Synkoz.ErrorR8.Show(ex.Message,"Configuration Snkoz");}
  }
 }
}
