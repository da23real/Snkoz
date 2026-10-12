using System;
using System.IO;
using System.Linq;
using System.Threading;
namespace Synkoz {
 public static class Mute940 {
  public static bool VideoR21(Montage m,int track,double time){return m.Tracks[track].MuteVideoR21||At(m,track,time);} public static bool At(Montage m,int track,double time){return m.Tracks[track].Muted940||m.Blocks.Any(b=>b.Track==track&&b.Muted940&&time>=b.Start&&time<b.Start+b.Duration);}
  public static void Audio(Montage m,int track,string output,double origin,double length,CancellationToken token){var blocks=m.Blocks.Where(b=>b.Track==track&&b.Muted940&&b.Start+b.Duration>origin&&b.Start<origin+length).ToArray();if(!m.Tracks[track].Muted940&&!m.Tracks[track].MuteAudioR21&&blocks.Length==0)return;var info=Engine.ProbeMedia(output);if(!info.Audio)return;string expression=(m.Tracks[track].Muted940||m.Tracks[track].MuteAudioR21)?"1":string.Join("+",blocks.Select(b=>"gte(t,"+Math.Max(0,b.Start-origin).ToString("F9",Engine.Inv)+")*lt(t,"+Math.Min(length,b.Start+b.Duration-origin).ToString("F9",Engine.Inv)+")"));string temp=Path.Combine(Path.GetDirectoryName(output),Guid.NewGuid().ToString("N")+".mkv");try{RenderRangeR3.Run("-v error -y -i "+Engine.Q(output)+" "+(info.Video?"-map 0:v -c:v copy ":"")+"-map 0:a -af "+Engine.Q("volume=0:enable='"+expression+"'")+" -c:a pcm_s16le "+Engine.Q(temp),token);File.Delete(output);File.Move(temp,output);}finally{if(File.Exists(temp))File.Delete(temp);}}
 }
 public sealed class Program940 { public int Track940=-1;public double Time;public int Channel,Value;}
}
