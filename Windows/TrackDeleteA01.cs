using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
namespace Synkoz {
 // Track deletion is occasional. Retain source lists by reference, never clone MIDI data.
 sealed class TrackUndoA01 {
  readonly List<Track> tracks;readonly int reference;
  readonly List<AudioChoiceR3> audio;
  readonly List<MidiSpan> notes;readonly List<Cc936> cc;
  readonly List<Program940> programs;readonly List<Trigger936> triggers;
  public TrackUndoA01(Montage m){tracks=m.Tracks.ToList();reference=m.ReferenceAudio;audio=m.PreviewAudio939.PreviewAudio.Select(x=>UiR2.Copy(x)).ToList();notes=m.Notes;cc=m.CC;programs=m.Programs940;triggers=m.Triggers;}
  public void Restore(Montage m){foreach(var block in m.Blocks){block.Track=block.Track>=0&&block.Track<m.Tracks.Count?tracks.IndexOf(m.Tracks[block.Track]):-1;}m.Tracks=tracks;m.ReferenceAudio=reference;m.PreviewAudio939.PreviewAudio=audio;m.Notes=notes;m.CC=cc;m.Programs940=programs;m.Triggers=triggers;}
 }
 public sealed partial class MontagePanel {
  void TrackMenuA01(ToolStripMenuItem menu){menu.DropDownItems.Clear();if(Current==null||Current.Tracks.Count==0){ItemA01(menu.DropDownItems,"Aucune piste").Enabled=false;return;}for(int i=0;i<Current.Tracks.Count;i++){int index=i;var track=Current.Tracks[i];ItemA01(menu.DropDownItems,(i+1)+" — "+(track.Midi945?"MIDI":Sources936.Label(track)),()=>DeleteTrackA01(index));}}
  void DeleteTrackA01(int index){if(Current==null||occupied()||index<0||index>=Current.Tracks.Count)return;Remember940(true);bool midi=Current.Tracks[index].Midi945;
   Current.Blocks.RemoveAll(b=>b.Track==index);Current.Tracks.RemoveAt(index);foreach(var b in Current.Blocks)if(b.Track>index)b.Track--;
   if(Current.ReferenceAudio==index)Current.ReferenceAudio=-1;else if(Current.ReferenceAudio>index)Current.ReferenceAudio--;
   Current.PreviewAudio939.PreviewAudio.RemoveAll(a=>a.Track==index);foreach(var a in Current.PreviewAudio939.PreviewAudio)if(a.Track>index)a.Track--;
   if(midi){Current.Notes=new List<MidiSpan>();Current.CC=new List<Cc936>();Current.Programs940=new List<Program940>();Current.Triggers=new List<Trigger936>();}
   selection940.Clear();clipboardA01=null;clipboardSourceA01=null;previewConfirm940=-1;
   Current.Duration=Current.Blocks.Select(b=>b.Start+b.Duration).DefaultIfEmpty(0).Max();cursor940=Math.Min(cursor940,Current.Duration);
   Changed941(Language945.T("Piste supprimée."));
  }
 }
}
