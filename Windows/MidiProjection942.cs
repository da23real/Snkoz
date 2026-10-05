using System;
using System.Linq;
using System.Collections.Generic;
namespace Synkoz {
 public static class MidiProjection942 {
  // A take's MIDI is shared by camera tracks. Deduplicate equal source-to-edit mappings,
  // but retain distinct mappings when media blocks have deliberately diverged.
  public static Montage Build(Montage m){var result=new Montage{Duration=m.Duration,Beats=m.Beats};var maps=m.Tracks.Any(t=>t.Midi945)?MidiBlocks945.Maps(m):m.Blocks.Where(b=>!b.Imported942).GroupBy(b=>b.Start.ToString("R")+":"+b.Duration.ToString("R")+":"+b.EventOffset941.ToString("R")).Select(g=>g.First());
   foreach(var b in maps){double a=b.Start+b.EventOffset941,z=a+b.Duration;
    foreach(var n in m.Notes.Where(n=>n.Start<z&&n.Start+n.Duration>a)){double s=Math.Max(a,n.Start),e=Math.Min(z,n.Start+n.Duration);result.Notes.Add(new MidiSpan{Start=s-b.EventOffset941,Duration=e-s,Channel=n.Channel,Track940=n.Track940,Note=n.Note,Velocity=n.Velocity});}
    foreach(var c in m.CC.Where(c=>c.Time>=a&&c.Time<z))result.CC.Add(new Cc936{Time=c.Time-b.EventOffset941,Channel=c.Channel,Track940=c.Track940,Number=c.Number,Value=c.Value});
    foreach(var t in m.Triggers.Where(t=>t.Time>=a&&t.Time<z)){var copy=UiR2.Copy(t);copy.Time-=b.EventOffset941;result.Triggers.Add(copy);}foreach(var p in m.Programs940.Where(p=>p.Time>=a&&p.Time<z))result.Programs940.Add(new Program940{Time=p.Time-b.EventOffset941,Channel=p.Channel,Track940=p.Track940,Value=p.Value});
   }
   if(!m.Tracks.Any(t=>t.Midi945)){result.Notes=result.Notes.GroupBy(n=>new{n.Start,n.Duration,n.Channel,n.Track940,n.Note,n.Velocity}).Select(g=>g.First()).ToList();result.CC=result.CC.GroupBy(c=>new{c.Time,c.Channel,c.Track940,c.Number,c.Value}).Select(g=>g.First()).ToList();result.Programs940=result.Programs940.GroupBy(p=>new{p.Time,p.Channel,p.Track940,p.Value}).Select(g=>g.First()).ToList();}return result;
  }
 }
}
