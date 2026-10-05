using System;
using System.IO;
using System.Linq;
using System.Threading;
namespace Synkoz {
 public sealed class PartialDone942:Exception {public PartialDone942(string path):base("Partie rendue conservée : "+path){}}
 public sealed class PartialState942 {public volatile bool Keep;public double Seconds;public bool Composite;}
 public static class PartialExport942 {
  [ThreadStatic] public static PartialState942 State;
  public static bool Keeping{get{return State!=null&&State.Keep;}}
  public static void Run(PartialState942 state,Action action){State=state;try{action();}finally{State=null;}}
  public static string Name(string output){return Path.Combine(Path.GetDirectoryName(output),Path.GetFileNameWithoutExtension(output)+"_partiel_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+"_"+Guid.NewGuid().ToString("N").Substring(0,6)+Path.GetExtension(output));}
  public static void Save(string rendered,string output,double seconds){string target=Name(output);try{RenderRangeR3.Run("-v error -y -i "+Engine.Q(rendered)+" -t "+seconds.ToString("F9",Engine.Inv)+" -c copy "+(Path.GetExtension(target).Equals(".mp4",StringComparison.OrdinalIgnoreCase)?"-movflags +faststart ":"")+Engine.Q(target),CancellationToken.None);if(Engine.ProbeMedia(target).Duration<=0)throw new Exception("Partie rendue vide.");}catch{if(File.Exists(target))File.Delete(target);throw;}throw new PartialDone942(target);}
  public static void Composite(Montage m,string work,string output){var parts=Directory.GetFiles(work,"part*.mp4").Where(p=>{try{return Engine.ProbeMedia(p).Duration>0;}catch{return false;}}).OrderBy(p=>int.Parse(Path.GetFileNameWithoutExtension(p).Substring(4))).ToArray();if(parts.Length==0)throw new OperationCanceledException("Aucun segment terminé à conserver.");double duration=parts.Sum(p=>Engine.ProbeMedia(p).Duration);string list=Path.Combine(work,"partial.txt"),video=Path.Combine(work,"partial_video.mp4");File.WriteAllLines(list,parts.Select(p=>"file '"+Path.GetFileName(p)+"'"));RenderRangeR3.Run("-v error -y -f concat -safe 0 -i "+Engine.Q(list)+" -c copy "+Engine.Q(video),CancellationToken.None);var tracks=Enumerable.Range(0,m.Tracks.Count).Where(i=>!m.Tracks[i].Muted940&&m.Blocks.Any(b=>b.Track==i&&b.Audio)).ToArray();string args="-v error -y -i "+Engine.Q(video)+" ";for(int j=0;j<tracks.Length;j++){string path=Path.Combine(work,"partial_audio"+j+".mkv");RenderRangeR3.Prepare(m,tracks[j],path,0,duration,0,CancellationToken.None,true);args+="-i "+Engine.Q(path)+" ";}string mixed=Path.Combine(work,"partial_mixed.mp4");if(tracks.Length>0)args+="-filter_complex "+Engine.Q(string.Join("",Enumerable.Range(1,tracks.Length).Select(i=>"["+i+":a]"))+"amix=inputs="+tracks.Length+":normalize=0:duration=longest,alimiter=limit=0.98:level=false:latency=true[a]")+" -map 0:v -map [a] -c:a aac -b:a 192k ";else args+="-map 0:v -an ";RenderRangeR3.Run(args+"-c:v copy -t "+duration.ToString("F9",Engine.Inv)+" "+Engine.Q(mixed),CancellationToken.None);Save(mixed,output,duration);}
 }
}
