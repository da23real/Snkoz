using System;
using System.Collections.Generic;
using System.IO;
namespace Synkoz {
 public sealed class CaptureConfig936 {public string MicroA01;public bool MicroOnTakeA01=true;public LatencySettingsA01 LatencyA01=new LatencySettingsA01();public List<Track> Tracks=new List<Track>();public string TakeFolder="";}
 public sealed class Preferences936 {public GraphColors15 GraphColors15=new GraphColors15();public static event Action ChangedR2;public static void NotifyR2(){if(ChangedR2!=null)ChangedR2();}
  public bool SmartPresetsR19=true,MidiHelpR19=true;public string PersonalFontsR2="",PatchFolderR2="";public bool DeletePhoneR8;public string LastTakeR8;
  public string DefaultTakeFolder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),"Snkoz","Recordings");
  public string LastAnimationFolder="",LastExportFolder="",LastMontageFolder="",LastAudioPresetFolder="";public int PreviewMeasures=2,PreviewHeight=540;public string LastFxPresetFolder="",LastProjectFolder="";
  public string LastConfigFolder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Snkoz","Config");
  public static string FilePath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Snkoz","preferences.json");
  public static Preferences936 Load(){try{return Store934.Load<Preferences936>(FilePath)??new Preferences936();}catch{return new Preferences936();}}
  public void Save(){Store934.Save(FilePath,this);}
 }
 public static class Sources936 {
  public static string Label(Track t){if(!string.IsNullOrWhiteSpace(t.ShortName))return t.ShortName.Trim();if(t.Video!=null){if(t.Video.TextSource=="EP")return "EP-UK";if(t.Video.TextSource=="ABCD")return "E4-UK";return t.Video.Name;}return t.Audio==null?"Sans source":t.Audio.Name;}
  public static string FileLabel(Track t){string name=Label(t)??"Source";foreach(char c in Path.GetInvalidFileNameChars())name=name.Replace(c,'_');name=name.Trim().TrimEnd('.');if(name.Length==0)name="Source";return name.Length>65?name.Substring(0,65):name;}
 }
}

