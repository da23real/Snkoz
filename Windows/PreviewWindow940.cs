using System;
using System.Collections.Generic;
using System.Linq;
namespace Synkoz {
 public static class PreviewWindow940 {
  public static Tuple<double,double> ForBlock(Montage m,int track,MediaBlock b,int measures,Effect936 requested=null){m=BlockRules939.Events(m,b);var clock=new Music936(m);double end=b.Start+b.Duration,first=double.PositiveInfinity,advance=0;var fx=b.Fx939??m.Tracks[track].Fx;
   foreach(var e in (requested==null?fx.Effects:new List<Effect936>{requested}).Where(e=>e.Enabled)){IEnumerable<double> candidates;
    if(e.Trigger==TriggerKind936.CC)candidates=m.CC.Where(c=>c.Channel==e.Channel-1&&c.Number==e.CC).Select(c=>c.Time);
    else if(e.Trigger==TriggerKind936.Timestamp)candidates=clock.Available?new[]{clock.Time(AnimationRules938.TimestampBeat(e))}:new double[0];
    else if(e.Trigger==TriggerKind936.Début_mesure)candidates=clock.Available?new[]{clock.Time(Math.Ceiling(clock.Beat(b.Start)/4-1e-9)*4)}:new double[0];
    else candidates=(m.Triggers.Any(n=>n.Channel==e.Channel-1&&(e.Note<0||e.Note==n.Note))?m.Triggers:m.Notes.Select(n=>new Trigger936{Time=n.Start,Channel=n.Channel,Note=n.Note}).ToList()).Where(n=>n.Channel==e.Channel-1&&(e.Note<0||e.Note==n.Note)).Select(n=>n.Time);
    double at=candidates.Where(t=>t>=b.Start-1e-8&&t<end).Select(t=>Math.Max(b.Start,t+e.OffsetMs941/1000)).Where(t=>t<end).DefaultIfEmpty(double.PositiveInfinity).Min();if(at<first){first=at;advance=System.IO.Path.GetExtension(e.Asset).Equals(".mp4",StringComparison.OrdinalIgnoreCase)&&clock.Available?clock.Beat(at)-clock.Beat(at-(e.SyncImage938-e.StartImage938)/15.0):AnimationRules938.Advance(e);}
   }
   if(double.IsInfinity(first))throw new InvalidOperationException("Aucun déclenchement des effets actifs dans ce bloc. Les événements des autres blocs sont ignorés.");
   double start=clock.Available?clock.Time(clock.Beat(first)-2-advance):first-1;start=Math.Max(b.Start,Math.Floor(start*30)/30);double finish=clock.Available?clock.Time(clock.Beat(start)+Math.Max(measures*4,advance+3)):start+Math.Max(2,measures*2);finish=Math.Min(end,Math.Max(first+1.0/30,finish));return Tuple.Create(start,finish-start);
  }
  public static Tuple<double,double> ForTrack(Montage m,int track,int measures,Effect936 requested=null){foreach(var b in m.Blocks.Where(b=>b.Track==track).OrderBy(b=>b.Start)){try{return ForBlock(m,track,b,measures,requested);}catch(InvalidOperationException){}}throw new InvalidOperationException("Aucun déclenchement des effets actifs sur cette piste.");}
 }
}
