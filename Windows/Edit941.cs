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
  public static void RemoveRange(Montage m,double start,double end,bool ripple){if(start<0||end<=start||end>m.Duration+.0001)throw new Exception("Définir une tranche valide avec Début / Fin.");foreach(double edge in new[]{end,start})foreach(var b in m.Blocks.Where(b=>edge>b.Start+1e-7&&edge<b.Start+b.Duration-1e-7).ToArray())BlockRules939.Split(m,b,edge);m.Blocks.RemoveAll(b=>b.Start>=start-1e-7&&b.Start<end-1e-7);if(ripple){foreach(var b in m.Blocks.Where(b=>b.Start>=end-1e-7)){b.Start-=end-start;b.EventOffset941+=end-start;}}m.Duration=m.Blocks.Select(b=>b.Start+b.Duration).DefaultIfEmpty(0).Max();}
  public static void Separate(Montage m,IEnumerable<MediaBlock> selection){var selected=Expand(m,selection);foreach(var b in selected)b.Link941="";foreach(var group in selected.Where(b=>b.Video&&b.Audio).GroupBy(b=>b.Track).ToArray()){var track=UiR2.Copy(m.Tracks[group.Key]);track.ShortName=Sources936.Label(track)+" — audio";track.Video=null;if(track.Audio==null)track.Audio=new Device{Name="Audio extrait"};int index=m.Tracks.Count;m.Tracks.Add(track);foreach(var b in group){var a=UiR2.Copy(b);a.Track=index;a.Video=false;a.Fx939=null;b.Audio=false;m.Blocks.Add(a);}}}
  public static void Link(Montage m,IEnumerable<MediaBlock> selection){var a=Expand(m,selection);if(a.Length<2)throw new Exception("Sélectionner au moins deux blocs à réunir.");string id=Guid.NewGuid().ToString("N");foreach(var b in a)b.Link941=id;}
 }
 public sealed partial class MontagePanel {
  readonly CheckBox selectMode941=new CheckBox{Text="Select",Appearance=Appearance.Button,AutoSize=true};double rangeStart941,rangeEnd941;
  void InitEdit941(FlowLayoutPanel bar){bar.Controls.Add(selectMode941);Action<string,Action> add=(name,act)=>{var b=new Button{Text=name,AutoSize=true};b.Click+=delegate{try{if(Current==null||occupied())return;act();}catch(Exception ex){status.Text=ex.Message;}};bar.Controls.Add(b);};
   add("Tout après curseur",SelectAfter941);add("Déplacer au curseur",()=>{if(selection940.Count==0)return;double delta=cursor940-selection940.Min(b=>b.Start);Edit941.Move(Current,selection940,delta);Changed941("Sélection déplacée ; synchronisation conservée.");});
   add("Réunir A/V",()=>{Edit941.Link(Current,selection940);Changed941("Blocs liés pour déplacement et découpe ; mutes indépendants.");});add("Séparer A/V",()=>{Edit941.Separate(Current,selection940);Changed941("Audio et vidéo indépendants.");});
   add("Début tranche",()=>{rangeStart941=cursor940;status.Text="Début de tranche : "+cursor940.ToString("F3")+" s";});add("Fin tranche",()=>{rangeEnd941=cursor940;status.Text="Tranche : "+rangeStart941.ToString("F3")+" → "+rangeEnd941.ToString("F3")+" s";});
   add("Supprimer tranche",()=>RemoveRange941(false));add("Supprimer + refermer",()=>RemoveRange941(true));add("Export montage",ExportMontage941);
  }
  void SelectAfter941(){if(Current==null)return;selection940.Clear();foreach(var b in Current.Blocks.Where(b=>b.Start>=cursor940-1e-7))selection940.Add(b);RefreshNavigation940();status.Text=selection940.Count+" blocs sélectionnés après le curseur ; blocs traversants conservés.";}
  void RemoveRange941(bool ripple){Remember940();Edit941.RemoveRange(Current,rangeStart941,rangeEnd941,ripple);selection940.Clear();Changed941(ripple?"Tranche supprimée et toutes les pistes rapprochées.":"Tranche supprimée ; espace conservé.");}
  void Changed941(string message){Current.Save();Render();RefreshNavigation940();status.Text=message;}
 }
}
