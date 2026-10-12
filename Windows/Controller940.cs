using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
namespace Synkoz {
 public sealed partial class MontagePanel {
  public Action<bool> ControllerRequested940;public Action<string> SendController940;
  readonly CheckBox controllerBox940=new CheckBox{Text="Contrôleur",AutoSize=true};bool controllerUpdating940,controllerConfirmed940;
  public bool ControllerWanted940{get{return controllerBox940.Checked;}}
  readonly HashSet<MediaBlock> selection940=new HashSet<MediaBlock>();readonly Stack<BlockUndo944> undo940=new Stack<BlockUndo944>();
  readonly Label cursorLabel940=new Label{AutoSize=true};readonly NumericUpDown zoom940=new Synkoz.ScrollNumberR8{Minimum=0,Maximum=1,Value=0,Width=60};readonly HScrollBar pan940=new HScrollBar{Minimum=0,Maximum=1099,LargeChange=100,Width=200};
  readonly int[] leds940=Enumerable.Repeat(-1,16).ToArray();double cursor940,start940;int previewConfirm940=-1;string previewKey940="";bool panUpdating940;
  double Span940{get{return Current==null?1:TimelineZoomR11.Span(Current,(int)zoom940.Value,start940);}}
  void InitController940(FlowLayoutPanel bar){InitEdit941(bar);bar.Height=100;bar.AutoScroll=true;bar.Controls.Add(controllerBox940);controllerBox940.CheckedChanged+=delegate{if(controllerUpdating940)return;controllerConfirmed940=false;status.Text=controllerBox940.Checked?"Contrôleur : attente de confirmation Teensy…":"Contrôleur désactivé.";if(ControllerRequested940!=null)ControllerRequested940(controllerBox940.Checked);};
   bar.Controls.Add(new Label{Text="Zoom ×",AutoSize=true,Margin=new Padding(4,8,0,0)});bar.Controls.Add(zoom940);zoom940.ValueChanged+=delegate{start940=Math.Max(0,Math.Min(Current==null?0:TimelineZoomR11.LastStart(Current,(int)zoom940.Value),cursor940-Span940/2));DeferViewR11();RefreshNavigation940();};bar.Controls.Add(pan940);pan940.ValueChanged+=delegate{if(panUpdating940||Current==null)return;start940=TimelineZoomR11.LastStart(Current,(int)zoom940.Value)*pan940.Value/1000;DeferViewR11();InvalidateTimelines940();};bar.Controls.Add(cursorLabel940);
   var cut=new Button{Text="Couper au curseur",AutoSize=true};cut.Click+=delegate{Cut940();};bar.Controls.Add(cut);var mute=new Button{Text="Mute sélection",AutoSize=true};mute.Click+=delegate{MuteSelection940();};bar.Controls.Add(mute);
  }
  public void ControllerMode940(bool on){controllerUpdating940=true;controllerBox940.Checked=on;controllerUpdating940=false;controllerConfirmed940=on;for(int i=0;i<16;i++)leds940[i]=-1;status.Text=on?"Contrôleur actif : OA curseur/coupe · Néos sélection/preview/mute/FX · J1 annuler":"Contrôleur arrêté.";if(on)SendLeds940();}
  public bool ControllerPacket940(string packet){if(!packet.StartsWith("CTL940,"))return false;var p=packet.Split(',');int id,value;if(p.Length!=4||!int.TryParse(p[2],out id)||!int.TryParse(p[3],out value))return true;if(p[1]=="MODE"){ControllerMode940(id==1);return true;}if(!controllerConfirmed940||!controllerBox940.Checked||Current==null||!Visible)return true;
   try{if(WindowControllerR12.Dispatch(p[1],id,value,FindForm()))return true;
    if(p[1]=="CUR")SetCursor940(cursor940+value/30.0);
    else if(p[1]=="CUT")Cut940();else if(p[1]=="SELECT")selectMode941.Checked=!selectMode941.Checked;else if(p[1]=="COLUMN")SelectColumn942();else if(p[1]=="ALL")SelectAll942();else if(p[1]=="KY"&&(id==14||id==15))MoveSelection942(value,id==15);else if(p[1]=="JOY"&&joysticks942.Checked&&(id==0||id==1))MoveSelection942(value,id==1);
    else if(p[1]=="UNDO")Undo940();
    else if(p[1]=="TRAN"){var music=TimelineMusicR11.Get(Current);if(id==3)SetCursor940(0);else if(id==4||id==5)SetExactCursor942(music.Available?music.Time(Edit941.AdjacentBar(music.Beat(cursor940),id==4?-1:1)):cursor940+(id==4?-1:1));}
    else if(p[1]=="NEO"&&id>=0&&id<Current.Tracks.Count){var b=BlockAt940(id);if(value==0){if(b==null){status.Text="Aucun bloc sous le curseur sur cette piste.";return true;}if(!selection940.Add(b))selection940.Remove(b);previewConfirm940=-1;RefreshNavigation940();}
     else if(value==2){if(selection940.Count>0)MuteSelection940();else if(b!=null)Targets942(new[]{b},MuteSelection940);}
     else if(value==3)FxSelection940(id,b);
     else if(value==1)PreviewSelection940(id,b);
    }
   }catch(Exception ex){status.Text=ex.Message;Synkoz.ErrorR8.Show(this,ex.Message,"Erreur");}return true;
  }
  MediaBlock BlockAt940(int track){return Current==null?null:Current.Blocks.FirstOrDefault(b=>b.Track==track&&cursor940>=b.Start&&cursor940<b.Start+b.Duration);}
  void SetCursor940(double time){SetExactCursor942(Math.Round(time*30)/30);}
  void SetExactCursor942(double time){if(Current==null)return;cursor940=Math.Max(0,Math.Min(TimelineZoomR11.End(Current),time));if(cursor940<start940)start940=cursor940;if(cursor940>start940+Span940)start940=cursor940-Span940*.8;previewConfirm940=-1;RefreshNavigation940();}
  void RefreshNavigation940(){if(Current!=null){int steps=TimelineZoomR11.Steps(Current);zoom940.Maximum=steps;zoomA01.Maximum=steps;start940=Math.Max(0,Math.Min(TimelineZoomR11.LastStart(Current,(int)zoom940.Value),start940));panUpdating940=true;double last=TimelineZoomR11.LastStart(Current,(int)zoom940.Value);pan940.Value=last>0?(int)Math.Round(start940/last*1000):0;zoomLabelR11.Text=zoom940.Value==0?Language945.T("Tout + 8 mesures"):TimelineZoomR11.Bars(Current,(int)zoom940.Value)+Language945.T(" mesures");tipsA01.SetToolTip(zoomA01,TimelineMusicR11.Get(Current).Available?zoomLabelR11.Text:zoomLabelR11.Text+" (repère 120 BPM, sans horloge MIDI)");panUpdating940=false;var music=TimelineMusicR11.Get(Current);cursorLabel940.Text="Curseur "+MusicalTimeR11.Position(Current,cursor940)+" · image "+Math.Round(cursor940*30);}PositionDisplay944();InvalidateTimelines940();SendLeds940();}
  void InvalidateTimelines940(){interactionR6=DateTime.UtcNow;repaintR6=true;}
  void PaintTimelinesR6(){if(rulerA01!=null)rulerA01.Invalidate();rows.Invalidate();foreach(Control row in rows.Controls)row.Invalidate();foreach(var c in Descendants945(rows))if(c is BlocksView||c is MidiCompact942)c.Invalidate();}
  void SendLeds940(){if(!controllerConfirmed940||SendController940==null)return;for(int t=0;t<16;t++){int state=0;if(Current!=null&&t<Current.Tracks.Count&&Current.Blocks.Any(b=>b.Track==t)){state=1;if(selection940.Any(b=>b.Track==t))state|=2;var under=BlockAt940(t);if(under!=null&&Mute940.At(Current,t,cursor940))state|=4;if(previewConfirm940==t)state|=8;}if(leds940[t]!=state){leds940[t]=state;SendController940("CLED940,"+t+","+state);}}}
  void Remember940(bool tracks=false){if(Current==null)return;var copy=new BlockUndo944(Current,tracks);if(undo940.Count>=2){var previous=undo940.Peek();undo940.Clear();undo940.Push(previous);}undo940.Push(copy);}
  void Undo940(){selectMode941.Checked=false;selection940.Clear();if(undo940.Count==0){RefreshNavigation940();status.Text="Rien à annuler dans ce montage — sélection quittée.";return;}undo940.Pop().Restore(Current);Current.Save();Render();RefreshNavigation940();status.Text="Modification du montage annulée.";}
  void Cut940(){if(Current==null||occupied())return;var targets=Current.Blocks.Where(b=>MontageTbo15.Is(Current,b));var blocks=targets.Where(b=>cursor940>b.Start+1e-8&&cursor940<b.Start+b.Duration-1e-8).ToArray();var tracks=blocks.Select(b=>b.Track).Distinct().ToArray();if(blocks.Length==0){status.Text="Sélectionner une piste et placer le curseur à l’intérieur d’un bloc.";return;}Remember940();foreach(var b in blocks)BlockRules939.Split(Current,b,cursor940);selection940.Clear();foreach(var b in Current.Blocks.Where(b=>tracks.Contains(b.Track)&&Math.Abs(b.Start-cursor940)<.001))selection940.Add(b);Current.Save();Render();RefreshNavigation940();status.Text=blocks.Length+" piste(s) coupée(s).";}
  void MuteSelection940(){if(Current==null||occupied()||selection940.Count==0)return;bool mute=!selection940.All(b=>b.Muted940);foreach(var b in selection940)b.Muted940=mute;Current.Save();RefreshBlocks944(selection940.Select(b=>b.Track));status.Text=mute?"Sélection entièrement mutée — noir et silence à l’export.":"Sélection entièrement réactivée.";}
  void FxSelection940(int track,MediaBlock under){if(selection940.Count>1){status.Text="FX refusé : sélectionner un seul bloc.";return;}var b=selection940.FirstOrDefault()??under;if(b==null)return;if(Current.Tracks[b.Track].Video==null){status.Text="Cette piste ne contient pas de vidéo.";return;}EditBlock939(b.Track,b);}
  void PreviewSelection940(int track,MediaBlock under){if(selection940.Count>1){status.Text="Preview : choisir un seul bloc.";return;}var b=selection940.FirstOrDefault()??under;if(b==null)return;string key=Current.Blocks.IndexOf(b)+":"+cursor940.ToString("F6");if(previewConfirm940!=track||previewKey940!=key){previewConfirm940=track;previewKey940=key;status.Text="Rappuyer sur la même touche Preview pour confirmer.";SendLeds940();return;}previewConfirm940=-1;SendLeds940();if(preview936==null||preview936.IsDisposed)preview936=new Preview936();preview936.Show();preview936.Activate();preview936.CutAtR14=time=>{var target=Current.Blocks.FirstOrDefault(x=>x.Track==b.Track&&x.Video==b.Video&&time>x.Start+1e-8&&time<x.Start+x.Duration-1e-8);if(target==null)throw new Exception("Choisir une position à l’intérieur du bloc.");SetExactCursor942(time);Targets942(new[]{target},Cut940);};preview936.Render(Current,b.Track,b.Start,b.Duration,Current.Tracks[b.Track].Fx.PreviewHeight941,true);}
 }
}

