using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
namespace Synkoz {
 public static class Edit941 {
  public static double AdjacentBar(double beat,int direction){double b=beat/4;return (direction>0?Math.Floor(b+1e-7)+1:Math.Ceiling(b-1e-7)-1)*4;}
  public static MediaBlock[] Expand(Montage m,IEnumerable<MediaBlock> selected){var a=selected.Where(m.Blocks.Contains).ToArray();var links=new HashSet<string>(a.Where(b=>!string.IsNullOrEmpty(b.Link941)).Select(b=>b.Link941));return m.Blocks.Where(b=>a.Contains(b)||links.Contains(b.Link941)).ToArray();}
  public static void Move(Montage m,IEnumerable<MediaBlock> selected,double delta){var blocks=Expand(m,selected);if(blocks.Length==0)throw new Exception("Sélectionner des blocs.");if(blocks.Any(b=>b.Start+delta<-.0001))throw new Exception("Déplacement avant le début du montage.");foreach(var b in blocks)foreach(var other in m.Blocks.Where(x=>x.Track==b.Track&&!blocks.Contains(x)))if(b.Start+delta<other.Start+other.Duration-1e-7&&b.Start+delta+b.Duration>other.Start+1e-7)throw new Exception("Déplacement refusé : des blocs se chevaucheraient.");foreach(var b in blocks){b.Start+=delta;b.EventOffset941-=delta;}m.Duration=m.Blocks.Select(b=>b.Start+b.Duration).DefaultIfEmpty(0).Max();}
  public static void RemoveRange(Montage m,double start,double end,bool ripple){if(start<0||end<=start||end>m.Duration+.0001)throw new Exception("Définir une tranche valide.");foreach(double edge in new[]{end,start})foreach(var b in m.Blocks.Where(b=>MontageTbo15.Is(m,b)&&edge>b.Start+1e-7&&edge<b.Start+b.Duration-1e-7).ToArray())BlockRules939.Split(m,b,edge);var removed=m.Blocks.Where(b=>MontageTbo15.Is(m,b)&&b.Start>=start-1e-7&&b.Start<end-1e-7).ToArray();m.Blocks.RemoveAll(b=>removed.Contains(b));if(m.OrderR5!=null)m.OrderR5.RemoveAll(n=>removed.Any(b=>b.NumberR5==n));else m.OrderR5=m.Blocks.Where(b=>MontageTbo15.Is(m,b)).OrderBy(b=>b.Start).Select(b=>b.NumberR5).ToList();}
  public static void Separate(Montage m,IEnumerable<MediaBlock> selection){var selected=Expand(m,selection);foreach(var b in selected)b.Link941="";foreach(var group in selected.Where(b=>b.Video&&b.Audio).GroupBy(b=>b.Track).ToArray()){var original=m.Tracks[group.Key];var track=UiR2.Copy(original);track.SourceKey15=Guid.NewGuid().ToString("N");track.ShortName=Sources936.Label(track)+" — audio";track.Video=null;if(track.Audio==null)track.Audio=new Device{Name="Audio extrait"};int index=m.Tracks.Count;m.Tracks.Add(track);foreach(var b in group){var a=UiR2.Copy(b);a.Track=index;a.Video=false;a.Fx939=null;b.Audio=false;m.Blocks.Add(a);}if(!m.Blocks.Any(b=>b.Track==group.Key&&b.Audio)){original.Audio=null;if(m.ReferenceAudio==group.Key)m.ReferenceAudio=index;foreach(var choice in m.PreviewAudio939.PreviewAudio.Where(c=>c.Track==group.Key))choice.Track=index;foreach(var fx in m.Tracks.Select(t=>t.Fx).Concat(m.Blocks.Select(b=>b.Fx939)).Where(f=>f!=null&&f.Graph15!=null).Distinct())foreach(var n in fx.Graph15.Nodes.Where(n=>n.Kind==GraphKind15.Audio&&n.MediaTrack==original.SourceKey15))n.MediaTrack=track.SourceKey15;}}}
  public static void Link(Montage m,IEnumerable<MediaBlock> selection){var a=Expand(m,selection);if(a.Any(b=>MontageTbo15.Is(m,b)))throw new Exception("Les blocs Montage restent indépendants.");if(a.Length<2)throw new Exception("Sélectionner au moins deux blocs à réunir.");string id=Guid.NewGuid().ToString("N");foreach(var b in a)b.Link941=id;}
 }
 public sealed partial class MontagePanel {
  readonly CheckBox selectMode941=new CheckBox{Text="Select",Appearance=Appearance.Button,AutoSize=true};
  void InitEdit941(FlowLayoutPanel bar){bar.Controls.Add(selectMode941);var export=new Button{Text="Export montage",AutoSize=true};export.Click+=delegate{if(Current!=null&&!occupied())ExportMontage941();};bar.Controls.Add(export);}
  void Changed941(string message){QueueThumbsR6();Current.Save();Render();RefreshNavigation940();status.Text=message;}
 }
}


