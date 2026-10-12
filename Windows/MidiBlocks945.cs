using System;using System.Linq;using System.Collections.Generic;using System.Windows.Forms;
namespace Synkoz {
 public static class MidiBlocks945 {
  public static void Ensure(Montage m){if(m.Tracks.Any(t=>t.Midi945))return;if(m.Notes.Count+m.CC.Count+m.Programs940.Count+m.Triggers.Count+(m.Version<9450?m.Beats.Count:0)==0)return;
   // Freeze the old inferred edit once. From here MIDI has its own source and blocks.
   if(m.Version<9450&&m.Blocks.Any(b=>!b.Imported942)){var old=MidiProjection942.Build(m);m.Notes=old.Notes;m.CC=old.CC;m.Programs940=old.Programs940;m.Triggers=old.Triggers;}
   int track=m.Tracks.Count;m.Tracks.Add(new Track{Midi945=true,ShortName="MIDI"});
   // Recover the same source-to-edit mappings used before the MIDI lane existed.
   var maps=m.Version>=9450?m.Blocks.Where(b=>!b.Imported942&&!MontageTbo15.Is(m,b)).GroupBy(b=>new{b.Start,b.Duration,b.EventOffset941}).Select(g=>g.First()).ToArray():new MediaBlock[0];
   if(maps.Length>0){foreach(var b in maps)m.Blocks.Add(new MediaBlock{Track=track,Start=b.Start,Duration=b.Duration,EventOffset941=b.EventOffset941,File=""});}else if(m.Duration>0)m.Blocks.Add(new MediaBlock{Track=track,Start=0,Duration=m.Duration,File=""});m.Version=Math.Max(m.Version,9450);
  }
  public static void CaptureSegment(Montage m,double start,double duration){
   if(duration<=0)return;int track=m.Tracks.FindIndex(t=>t.Midi945&&t.MidiDataR8==null);if(track<0){track=m.Tracks.Count;m.Tracks.Add(new Track{Midi945=true,ShortName="MIDI"});}
   m.Blocks.Add(new MediaBlock{Track=track,Start=start,Duration=duration,File=""});
  }
  public static MediaBlock[] Maps(Montage m){return m.Blocks.Where(b=>b.Track>=0&&b.Track<m.Tracks.Count&&m.Tracks[b.Track].Midi945&&!m.Tracks[b.Track].MidiArchiveR9&&!b.Muted940&&!m.Tracks[b.Track].Muted940).ToArray();}
  public static bool CanCloseR9(Montage m,IEnumerable<MediaBlock> selection){var selected=Edit941.Expand(m,selection);if(selected.Length==0)return false;return selected.GroupBy(b=>b.Track).All(g=>g.Min(b=>b.Start)-m.Blocks.Where(b=>b.Track==g.Key&&!selected.Contains(b)&&b.Start+b.Duration<=g.Min(x=>x.Start)+1e-8).Select(b=>b.Start+b.Duration).DefaultIfEmpty(0).Max()>1e-8);}
  public static void CloseGap(Montage m,IEnumerable<MediaBlock> selection){var selected=Edit941.Expand(m,selection);if(selected.Length==0)throw new Exception("Select the blocks to the right of the gap.");double gap=double.PositiveInfinity;foreach(var group in selected.GroupBy(b=>b.Track)){double first=group.Min(b=>b.Start);double previous=m.Blocks.Where(b=>b.Track==group.Key&&!selected.Contains(b)&&b.Start+b.Duration<=first+1e-8).Select(b=>b.Start+b.Duration).DefaultIfEmpty(0).Max();gap=Math.Min(gap,first-previous);}if(gap<=1e-8||double.IsInfinity(gap))throw new Exception("Aucun espace commun à combler avant ces blocs.");Edit941.Move(m,selected,-gap);}
 }
 public sealed partial class MontagePanel {
  static IEnumerable<Control> Descendants945(Control root){foreach(Control c in root.Controls){yield return c;foreach(var child in Descendants945(c))yield return child;}}
 }
}

