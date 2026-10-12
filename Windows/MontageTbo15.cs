using System;using System.Collections.Generic;using System.Linq;
namespace Synkoz {
 public static class MontageTbo15 {
  public static bool Is(Montage m,MediaBlock b){return b!=null&&b.Track>=0&&b.Track<m.Tracks.Count&&m.Tracks[b.Track].MontageTbo15;}
  public static int Ensure(Montage m){int at=m.Tracks.FindIndex(t=>t.MontageTbo15);if(at>=0)return at;if(m.Duration<=0)return -1;at=m.Tracks.Count;m.Tracks.Add(new Track{MontageTbo15=true,ShortName="Montage",Video=new Device{Name="Montage",Video=true},Fx=new TrackFx936{Graphic=true}});var fx=new TrackFx936{Graphic=true,Graph15=GraphRules15.Empty()};m.Blocks.Add(new MediaBlock{Track=at,Start=0,Duration=m.Duration,Video=true,TransparentR7=true,Fx939=fx});return at;}
  public static Montage Events(Montage m,MediaBlock b){var s=m.Tracks.Any(t=>t.Midi945)?MidiProjection942.Build(m):new Montage{Duration=m.Duration,Beats=m.Beats,Notes=m.Notes,Triggers=m.Triggers,CC=m.CC,Programs940=m.Programs940,PreviewAudio939=m.PreviewAudio939,ReferenceAudio=m.ReferenceAudio};s.Folder=m.Folder;s.Blocks=m.Blocks;s.Tracks=m.Tracks.Select(t=>UiR2.Copy(t)).ToList();s.GraphColors15=m.GraphColors15;s.ConditionsR9=m.ConditionsR9;return s;}
  public static bool HasTeensy(Montage m){return m.Triggers.Any(n=>n.Track940>=0&&n.Track940<16)||m.Notes.Any(n=>n.Track940>=0&&n.Track940<16)||m.CC.Any(n=>n.Track940>=0&&n.Track940<16);}
 }
}

