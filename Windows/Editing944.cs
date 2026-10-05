using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
namespace Synkoz {
 // Only structural block edits are retained, never MIDI events, media or track settings.
 sealed class BlockUndo944 {
  readonly List<MediaBlock> blocks;readonly double duration;
  readonly TrackUndoA01 tracks;
  public BlockUndo944(Montage m,bool includeTracks=false){blocks=m.Blocks.Select(b=>UiR2.Copy(b)).ToList();duration=m.Duration;if(includeTracks)tracks=new TrackUndoA01(m);}
  public void Restore(Montage m){if(tracks!=null)tracks.Restore(m);foreach(var b in blocks){var same=m.Blocks.Where(x=>x.Track==b.Track&&x.File==b.File&&Math.Abs(x.EventOffset941-b.EventOffset941)<1e-7&&x.Start<b.Start+b.Duration-1e-7&&x.Start+x.Duration>b.Start+1e-7).ToArray();if(same.Length>0&&same.All(x=>x.Muted940==same[0].Muted940))b.Muted940=same[0].Muted940;}m.Blocks=blocks;m.Duration=duration;}
 }
 public sealed partial class MontagePanel {
  readonly TextBox musicalPosition944=new TextBox{Width=100,Text="1:1:0"};
  void RefreshBlocks944(IEnumerable<int> tracks=null){var selected=tracks==null?null:new HashSet<int>(tracks);foreach(var view in Descendants945(rows).OfType<BlocksView>())if(selected==null||selected.Contains(view.Track))view.Invalidate();SendLeds940();}
  void InitPosition944(FlowLayoutPanel bar){var group=new FlowLayoutPanel{AutoSize=true,WrapContents=false,Margin=Padding.Empty};group.Controls.Add(new Label{Text="Mesure:temps:quart",AutoSize=true,Margin=new Padding(3,7,3,0)});group.Controls.Add(musicalPosition944);var go=new Button{Text="Aller",AutoSize=true};go.Click+=delegate{ApplyPosition944();};group.Controls.Add(go);musicalPosition944.KeyDown+=delegate(object s,KeyEventArgs e){if(e.KeyCode==Keys.Enter){ApplyPosition944();e.Handled=e.SuppressKeyPress=true;}};bar.Controls.Add(group);rows.MouseDown+=delegate(object s,MouseEventArgs e){if(e.Button!=MouseButtons.Left||Current==null)return;var row=rows.Controls.Cast<Control>().FirstOrDefault();if(row==null)return;var view=row.Controls.OfType<BlocksView>().FirstOrDefault();if(view==null)return;int left=row.Left+view.Left;if(e.X>=left&&e.X<=left+view.Width){view.Focus();SetCursor940(start940+(e.X-left)*Span940/Math.Max(1,view.Width));}};}
  void ApplyPosition944(){if(Current==null||occupied())return;var music=new Music936(Current);if(!music.Available){status.Text="Repères MIDI absents.";return;}var parts=musicalPosition944.Text.Trim().Split(':');int measure,beat,quarter;if(parts.Length!=3||!int.TryParse(parts[0],out measure)||!int.TryParse(parts[1],out beat)||!int.TryParse(parts[2],out quarter)||measure<1||beat<1||beat>4||quarter<0||quarter>3){status.Text="Position : mesure ≥ 1, temps 1 à 4, quart 0 à 3. Exemple : 8:2:3.";return;}double time=music.Time((measure-1)*4.0+beat-1+quarter/4.0);SetExactCursor942(Math.Max(0,Math.Min(Current.Duration,time)));}
  void PositionDisplay944(){PositionDisplayA01();if(Current==null)return;var music=new Music936(Current);if(!music.Available)return;double beat=Math.Max(0,music.Beat(cursor940));long quarter=(long)Math.Floor(beat*4+1e-7);string text=(quarter/16+1)+":"+(quarter/4%4+1)+":"+(quarter%4);cursorLabel940.Text="Position "+text;if(!musicalPosition944.Focused)musicalPosition944.Text=text;}
  void BetweenTracks944(Control row,int left,int width){row.Paint+=delegate(object sender,PaintEventArgs paint){if(Current!=null)TimelineView940.Grid(paint.Graphics,Font,Current,new Rectangle(left,0,width,row.Height),start940,Span940,false);};row.MouseDown+=delegate(object s,MouseEventArgs e){if(e.Button==MouseButtons.Left&&e.X>=left&&e.X<=left+width){var view=row.Controls.OfType<BlocksView>().FirstOrDefault();if(view!=null)view.Focus();SetCursor940(start940+(e.X-left)*Span940/Math.Max(1,width));}};}
 }
}
