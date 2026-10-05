using System;using System.Linq;using System.Collections.Generic;using System.Windows.Forms;
namespace Synkoz {
 public static class MidiBlocks945 {
  public static void Ensure(Montage m){if(m.Version>=9450||m.Tracks.Any(t=>t.Midi945))return;if(m.Notes.Count+m.CC.Count+m.Programs940.Count+m.Beats.Count==0)return;
   // Freeze the old inferred edit once. From here MIDI has its own source and blocks.
   if(m.Version<9450&&m.Blocks.Any(b=>!b.Imported942)){var old=MidiProjection942.Build(m);m.Notes=old.Notes;m.CC=old.CC;m.Programs940=old.Programs940;m.Triggers=old.Triggers;}
   int track=m.Tracks.Count;m.Tracks.Add(new Track{Midi945=true,ShortName="MIDI"});if(m.Duration>0)m.Blocks.Add(new MediaBlock{Track=track,Start=0,Duration=m.Duration,File=""});m.Version=9450;
  }
  public static MediaBlock[] Maps(Montage m){return m.Blocks.Where(b=>b.Track>=0&&b.Track<m.Tracks.Count&&m.Tracks[b.Track].Midi945&&!b.Muted940&&!m.Tracks[b.Track].Muted940).ToArray();}
  public static void CloseGap(Montage m,IEnumerable<MediaBlock> selection){var selected=Edit941.Expand(m,selection);if(selected.Length==0)throw new Exception("Select the blocks to the right of the gap.");double gap=double.PositiveInfinity;foreach(var group in selected.GroupBy(b=>b.Track)){double first=group.Min(b=>b.Start);double previous=m.Blocks.Where(b=>b.Track==group.Key&&!selected.Contains(b)&&b.Start+b.Duration<=first+1e-8).Select(b=>b.Start+b.Duration).DefaultIfEmpty(0).Max();gap=Math.Min(gap,first-previous);}if(gap<=1e-8||double.IsInfinity(gap))throw new Exception("No common gap before the selected blocks.");Edit941.Move(m,selected,-gap);}
 }
 public sealed partial class MontagePanel {
  static IEnumerable<Control> Descendants945(Control root){foreach(Control c in root.Controls){yield return c;foreach(var child in Descendants945(c))yield return child;}}
  void CloseGap945(){if(Current==null||occupied())return;MidiBlocks945.CloseGap(Current,selection940);Changed941("Gap closed on the selected tracks.");}
  void EditTrack945(int track){if(Current==null||occupied()||Current.Tracks[track].Video==null)return;var source=Current;var local=BlockRules939.Events(source,new MediaBlock{Track=track,Duration=source.Duration});local.Save939=()=>{source.Tracks[track].Fx=UiR2.Copy(local.Tracks[track].Fx);source.Save();};using(var form=new EffectsForm936(local,track,(effect,measures)=>{local.Save();var window=PreviewWindow940.ForTrack(source,track,measures,effect);var editor=Application.OpenForms.OfType<EffectsForm936>().FirstOrDefault();if(editor!=null)editor.RenderPreview942(source,track,window.Item1,window.Item2);}))form.ShowDialog(FindForm());Render();}
 }
}
