using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Script.Serialization;

namespace Synkoz {
 public sealed class LatencySampleA01 {
  public string Id,Take,Conditions,Date;public double DelayMs,ResolutionMs;public int Event;public double MidiTime;
 }
 public sealed class LatencyDeviceA01 {
  public string Key="",Name="",Stream="audio";public List<LatencySampleA01> Samples=new List<LatencySampleA01>();
  public List<string> Diagnostics=new List<string>();
  [ScriptIgnore]public double RecommendedMinimum {get{return Samples.Min(s=>s.DelayMs-(Stream=="video"?s.ResolutionMs:0));}}
  [ScriptIgnore]public double Resolution {get{return Samples.Count==0?0:Samples.Max(s=>s.ResolutionMs);}}
  [ScriptIgnore]public double Minimum {get{return Samples.Min(s=>s.DelayMs);}}
  [ScriptIgnore]public double Maximum {get{return Samples.Max(s=>s.DelayMs);}}
  [ScriptIgnore]public double Median {get{var a=Samples.Select(s=>s.DelayMs).OrderBy(x=>x).ToArray();return (a[(a.Length-1)/2]+a[a.Length/2])/2;}}
  [ScriptIgnore]public int Takes {get{return Samples.Select(s=>s.Take).Distinct(StringComparer.OrdinalIgnoreCase).Count();}}
 }
 public sealed class LatencySettingsA01 {
  public double ToleranceMs=5,MarginMs=0,SearchMs=250;public int Repetitions=10,Channel=1,Note=60,UkTrack=0;public string Conditions="";
  public List<LatencyDeviceA01> Devices=new List<LatencyDeviceA01>();
  public void Normalize(){ToleranceMs=Finite(ToleranceMs)?Math.Max(0,Math.Min(1000,ToleranceMs)):5;MarginMs=Finite(MarginMs)?Math.Max(0,Math.Min(100,MarginMs)):0;SearchMs=Finite(SearchMs)?Math.Max(10,Math.Min(1000,SearchMs)):250;UkTrack=Math.Max(0,Math.Min(16,UkTrack));Repetitions=Math.Max(1,Math.Min(100,Repetitions));Channel=Math.Max(1,Math.Min(16,Channel));Note=Math.Max(0,Math.Min(127,Note));if(Devices==null)Devices=new List<LatencyDeviceA01>();Devices.RemoveAll(d=>d==null||string.IsNullOrEmpty(d.Key));foreach(var d in Devices){if(d.Stream!="video")d.Stream="audio";if(d.Diagnostics==null)d.Diagnostics=new List<string>();if(d.Samples==null)d.Samples=new List<LatencySampleA01>();d.Samples.RemoveAll(s=>s==null||string.IsNullOrEmpty(s.Id)||!Finite(s.DelayMs)||Math.Abs(s.DelayMs)>2000||!Finite(s.ResolutionMs)||s.ResolutionMs<0||s.ResolutionMs>2000);d.Samples=d.Samples.GroupBy(s=>s.Id).Select(g=>g.First()).ToList();}}
  static bool Finite(double n){return !double.IsNaN(n)&&!double.IsInfinity(n);}
  public void Merge(LatencySettingsA01 other){if(other==null)return;Normalize();other.Normalize();foreach(var d in other.Devices){var target=Devices.FirstOrDefault(x=>x.Key==d.Key&&x.Stream==d.Stream);if(target==null){Devices.Add(UiR2.Copy(d));continue;}var ids=new HashSet<string>(target.Samples.Select(s=>s.Id));target.Samples.AddRange(d.Samples.Where(s=>ids.Add(s.Id)).Select(s=>UiR2.Copy(s)));target.Diagnostics=target.Diagnostics.Concat(d.Diagnostics).Distinct().ToList();}}
 }
 public sealed class LatencyStampA01 {
  public string Key="";public double MinimumMs,MaximumMs,ToleranceMs=5,MarginMs;public int Takes;public bool NeedsReview,Validated,VideoProxy;
 }
 public static class LatencyAnalysisA01 {
  public static string Key(Track t){return new JavaScriptSerializer().Serialize(new[]{t.Video==null?"":t.Video.Id??t.Video.PhoneSerial??t.Video.Name,t.Audio==null?"":t.Audio.Id??t.Audio.Name,t.Video==null?"":t.Video.PhoneSerial??"",t.CameraSize??"",t.CameraFormat??"",t.CameraFps.ToString("R",Engine.Inv),t.Fps.ToString(),t.Encoder??"",t.Format??""});}
  public static bool Pending(Track t){return t.LatencyA01!=null&&!t.LatencyA01.Validated&&(t.LatencyA01.NeedsReview||t.LatencyA01.Key!=Key(t));}
  public static string Conditions(Montage m,string user){return m.Tracks.Count(t=>!t.Midi945&&(t.Video!=null||t.Audio!=null))+" source(s) enregistrée(s) : "+string.Join(" / ",m.Tracks.Where(t=>!t.Midi945).Select(t=>Sources936.Label(t)+" ["+Key(t)+"]"))+" ; "+user;}
  // Independent raw streams: no corrections, FX, mixing or montage frame rounding.
  public static LatencySettingsA01 Analyze(Montage m,LatencySettingsA01 settings,CancellationToken token,Action<string> progress){
   string invalid=m.Triggers==null||!m.Triggers.Any(n=>n.Channel==settings.Channel-1&&n.Note==settings.Note)?"Aucune note MIDI de test sur le canal et la note choisis.":m.Warnings.Any(w=>w.IndexOf("manquants",StringComparison.OrdinalIgnoreCase)>=0)?"Repères MIDI incomplets : prise ignorée.":null;
   var output=new LatencySettingsA01();string take=Path.GetFullPath(m.Folder),conditions=Conditions(m,string.IsNullOrEmpty(m.LatencyConditionsA01)?settings.Conditions:m.LatencyConditionsA01);
   for(int track=0;track<m.Tracks.Count;track++){
    token.ThrowIfCancellationRequested();var t=m.Tracks[track];if(t.Midi945)continue;
    foreach(bool video in new[]{false,true}){
    if(video?t.Video==null:!Engine.HasAudio(t))continue;
    var device=new LatencyDeviceA01{Key=Key(t),Name=Sources936.Label(t),Stream=video?"video":"audio"};output.Devices.Add(device);int eventNumber=0;
    Action<string> reject=reason=>{string message=take+" | "+device.Stream+" | "+reason;device.Diagnostics.Add(message);progress(device.Name+" : "+reason);};
    if(invalid!=null){reject(invalid);continue;}
    foreach(var b in m.Blocks.Where(b=>b.Track==track&&(video?b.Video:b.Audio))){
     double? rawZero=video?b.RawVideoZero:b.RawAudioZero;
     if(!rawZero.HasValue||string.IsNullOrEmpty(b.RawFile)){reject("Origine brute absente, bloc ignoré.");continue;}
     var events=m.Triggers.Where(n=>n.Channel==settings.Channel-1&&n.Note==settings.Note&&n.Time>=b.Start&&n.Time<b.Start+b.Duration).Select(n=>n.Time).Distinct().OrderBy(x=>x).ToArray();if(m.LatencyTestA01)events=events.Take(4).ToArray();if(events.Length==0)continue;
     double gap=events.Zip(events.Skip(1),(a,z)=>z-a).DefaultIfEmpty(.5).Min();double after=Math.Min(settings.SearchMs/1000,Math.Max(.01,gap*.65));
     string path=Store934.MediaPath(m.Folder,b.RawFile);double zero=rawZero.Value;
     foreach(double time in events){
      token.ThrowIfCancellationRequested();int ordinal=++eventNumber;double source=time-b.Start+zero;double before=video?.15:.05,start=source-before,length=before+after;
      if(start<0||start+length>b.RawDuration){reject("Note "+ordinal+" : bord de fichier, marge insuffisante.");continue;}
      progress(device.Name+" · "+device.Stream+" · note "+ordinal+" — analyse locale…");
      try{
       double? onset;double resolution=0;
       if(video){var found=VideoLatencyA01.Onset(path,source,start,length,token);onset=found==null?(double?)null:found.Item1;resolution=found==null?0:found.Item2*1000;}
       else{var raw=CalibrationMediaR5.Pipe("-ss "+start.ToString("F9",Engine.Inv)+" -i "+Engine.Q(path)+" -t "+length.ToString("F9",Engine.Inv)+" -vn -ar 48000 -ac 2 -f f32le pipe:1",token);onset=Onset(raw,before,after);}
       if(!onset.HasValue){reject("Note "+ordinal+" : "+(video?"allumage absent ou ambigu":"attaque sonore absente ou ambiguë"));continue;}
       device.Samples.Add(new LatencySampleA01{Id=take+"|"+track+"|"+b.Segment+"|"+time.ToString("R",Engine.Inv),Take=take,Date=DateTime.UtcNow.ToString("o"),Conditions=conditions,Event=ordinal,MidiTime=time,DelayMs=onset.Value*1000,ResolutionMs=resolution});
      }catch(OperationCanceledException){throw;}catch(Exception ex){reject("Note "+ordinal+" : "+ex.Message);}
     }
    }
    if(device.Samples.Count==0)reject("Aucune mesure exploitable pour ce flux dans cette prise.");
    }
   }
   return output;
  }
  static double? Onset(byte[] raw,double before,double after){
   // 1 ms energy bins, channels kept separate before taking the maximum (no phase cancellation).
   int frames=raw.Length/8,bins=frames/48;if(bins<20)return null;float[] samples=new float[frames*2];Buffer.BlockCopy(raw,0,samples,0,samples.Length*4);double[] envelope=new double[bins];
   for(int b=0;b<bins;b++){double left=0,right=0;for(int i=0;i<48;i++){double l=samples[(b*48+i)*2],r=samples[(b*48+i)*2+1];left+=l*l;right+=r*r;}envelope[b]=Math.Sqrt(Math.Max(left,right)/48);}
   var quiet=envelope.Take(Math.Min(30,bins)).OrderBy(x=>x).ToArray();double noise=quiet[quiet.Length/2],peak=envelope.Max();if(peak<.005||peak<noise*5)return null;
   double high=Math.Max(noise*4,peak*.12),low=Math.Max(noise*2,high*.45);int previousQuiet=0;
   for(int i=3;i<bins-2;i++){if(envelope[i]<low)previousQuiet++;else {if(previousQuiet>=3&&envelope[i]>=high&&envelope[i+1]>=high&&envelope[i+2]>=high){double delay=i*.001-before;if(delay<=-.04||delay>=after-.004)return null;return delay;}if(envelope[i]>=high)previousQuiet=0;}}
   return null;
  }
 }
}
