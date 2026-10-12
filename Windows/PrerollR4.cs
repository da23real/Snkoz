using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Synkoz {
 public static class PrerollR4 {
  static bool Finite(double n){return !double.IsNaN(n)&&!double.IsInfinity(n);}
  public static void Attach(Montage m,MediaBlock b,Capture c,double music){
   if(!c.Valid||string.IsNullOrEmpty(c.File)||!File.Exists(c.File)||!Finite(music)||!Finite(c.Duration)||c.Duration<=0)return;
   string root=Path.GetFullPath(m.Folder).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar,file=Path.GetFullPath(c.File);
   if(!file.StartsWith(root,StringComparison.OrdinalIgnoreCase))throw new Exception("Brut hors du dossier de prise.");
   b.RawFile=file.Substring(root.Length);b.RawDuration=c.Duration;
   if(b.Video&&Finite(c.VideoOrigin))b.RawVideoZero=music-c.VideoOrigin;
   if(b.Audio&&Finite(c.AudioOrigin))b.RawAudioZero=music-c.AudioOrigin;
  }
  // Independent stream-to-musical mappings; subsequent segment prerolls never overlap kept music.
  public static IEnumerable<MediaBlock> Blocks(Montage m,int track,bool video){
   foreach(var b in m.Blocks.Where(x=>x.Track==track&&(video?x.Video:x.Audio)).OrderBy(x=>x.Start)){
    double? zero=video?b.RawVideoZero:b.RawAudioZero;
    bool raw=zero.HasValue&&Finite(zero.Value)&&Finite(b.RawDuration)&&b.RawDuration>0&&!string.IsNullOrEmpty(b.RawFile)&&File.Exists(Store934.MediaPath(m.Folder,b.RawFile));
    if(!raw){yield return new MediaBlock{Muted940=b.Muted940,TransparentR7=b.TransparentR7,Imported942=b.Imported942,Sprite942=b.Sprite942,Fps942=b.Fps942,X942=b.X942,Y942=b.Y942,Size942=b.Size942,KeepProportionsR4=b.KeepProportionsR4,HeightR4=b.HeightR4,Track=b.Track,Segment=b.Segment,File=b.File,Start=b.Start,Duration=b.Duration,Video=b.Video,Audio=b.Audio,Issue=b.Issue,RawVideoZero=b.MediaOffset939,RawAudioZero=b.MediaOffset939};continue;}
    double begin=b.Start-zero.Value,start=b.Start==0?begin:Math.Max(b.Start,begin),end=Math.Min(b.Start+b.Duration,begin+b.RawDuration);
    if(end<=start)continue;
    yield return new MediaBlock{Muted940=b.Muted940,TransparentR7=b.TransparentR7,Imported942=b.Imported942,Sprite942=b.Sprite942,Fps942=b.Fps942,X942=b.X942,Y942=b.Y942,Size942=b.Size942,KeepProportionsR4=b.KeepProportionsR4,HeightR4=b.HeightR4,Track=b.Track,Segment=b.Segment,File=b.RawFile,Start=start,Duration=end-start,Video=b.Video,Audio=b.Audio,RawVideoZero=video?(double?)(start-begin):null,RawAudioZero=video?null:(double?)(start-begin),Issue=b.Issue};
   }
  }
  public static double Minimum(Montage m){double minimum=0;for(int i=0;i<m.Tracks.Count;i++){foreach(bool video in new[]{true,false}){if(video?m.Tracks[i].Video==null:!Engine.HasAudio(m.Tracks[i]))continue;foreach(var b in Blocks(m,i,video))minimum=Math.Min(minimum,b.Start);}}return Math.Ceiling(minimum*30-1e-8)/30;}
 }
}

