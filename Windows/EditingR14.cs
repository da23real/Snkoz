using System;using System.Linq;using System.Drawing;using System.Windows.Forms;using System.Collections.Generic;
namespace Synkoz {
 static class HeaderR14 {
  public static void Install(Form form,FlowLayoutPanel buttons,MontagePanel montage){
   var menu=montage.menuA01;if(menu==null)return;var header=new TableLayoutPanel{Dock=DockStyle.Top,Height=40,ColumnCount=2,RowCount=1,Margin=Padding.Empty,Padding=Padding.Empty};header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));menu.Parent.Controls.Remove(menu);menu.Dock=DockStyle.None;menu.Anchor=AnchorStyles.Left;menu.Margin=Padding.Empty;header.Controls.Add(menu,0,0);buttons.Dock=DockStyle.Fill;buttons.WrapContents=false;buttons.Margin=Padding.Empty;buttons.Padding=Padding.Empty;header.Controls.Add(buttons,1,0);form.Controls.Add(header);header.SendToBack();menu.Visible=true;
  }
 }
 public sealed partial class EffectsForm936 {
  readonly NumericUpDown startR14=new ScrollNumberR8{Minimum=1,Maximum=1000000,Value=1,Width=65};
  readonly CheckBox detectR14=new CheckBox{Text="Détecter",AutoSize=true};
  public Action<double> CutPreviewR14;
  public int PreviewStartR14{get{return (int)startR14.Value;}set{startR14.Value=Math.Max(startR14.Minimum,Math.Min(startR14.Maximum,value));}}
  public bool PreviewDetectR14{get{return detectR14.Checked;}}
  void InitRangeR14(FlowLayoutPanel actions){actions.Controls.Add(new Label{Text="Départ mesure",AutoSize=true,Margin=new Padding(3,8,0,0)});actions.Controls.Add(startR14);actions.Controls.Add(detectR14);if(embedded942!=null)embedded942.CutAtR14=delegate(double time){if(CutPreviewR14==null)return;if(!Apply())return;CutPreviewR14(time);Close();};}
  internal int[] AvailableCcR14(Effect936 e){if(!model.AssistanceR20.Midi)return Enumerable.Range(0,128).ToArray();return model.CC.Where(c=>((e.Channel==0&&e.ChannelsR9==0)||EventsR9.Channel(e,c.Channel,c.Track940))&&c.Number>=0&&c.Number<128&&c.Value>=0).Select(c=>c.Number).Distinct().OrderBy(n=>n).ToArray();}
  void CcChoiceR14(string label,Effect936 e,int value,Action<int> set){var numbers=AvailableCcR14(e);var options=(!model.AssistanceR20.Midi?Enumerable.Range(0,128):model.CC.Where(c=>c.Value>=0).Select(c=>c.Number)).Concat(new[]{value}).Distinct().OrderBy(n=>n).Cast<object>().ToArray();var combo=Combo(options,value);combo.Format+=delegate(object sender,ListControlConvertEventArgs args){if(args.ListItem is int)args.Value=PatchLabelsR2.Label(model,e,(int)args.ListItem);};((ChoiceComboR15)combo).Allowed=x=>numbers.Contains((int)x);((ChoiceComboR15)combo).Accepted=()=>{if(label=="Numéro CC"||label=="Numéro CC déclencheur"){ccChosenR15.Add(e.IdR8);ccValueChosenR15.Remove(e.IdR8);triggerChosenR15.Add(e.IdR8);}};combo.Width=190;combo.Enabled=numbers.Length>0;combo.SelectedIndexChanged+=delegate{if(combo.SelectedItem is int){set((int)combo.SelectedItem);QueueRefresh938(list.SelectedIndex);}};Field(label,combo);if(!numbers.Contains(value))combo.BackColor=SystemColors.Control;}
  internal static void InheritR14(Effect936 child,Effect936 parent){child.Channel=parent.Channel;child.ChannelsR9=parent.ChannelsR9;double x=parent.Kind==EffectKind936.Texte?parent.TextR12.X:parent.X,y=parent.Kind==EffectKind936.Texte?parent.TextR12.Y:parent.Y;child.X=child.ZoomX=child.FromX=child.ToX=child.ZoneXr8=Math.Max(0,Math.Min(100,x));child.Y=child.ZoomY=child.FromY=child.ToY=child.ZoneYr8=Math.Max(0,Math.Min(100,y));child.CenterPosition939=true;}
 }
 public static class PreviewRangeR14 {
  public static Tuple<double,double> ForBlock(Montage original,int track,MediaBlock block,int measure,int count,bool detect,Effect936 requested){
   var music=new Music936(original);if(!music.Available)return Tuple.Create(block.Start,Math.Min(block.Duration,Math.Max(1,count)*2.0));
   double lower=Math.Max(block.Start,music.Time(music.Beat(block.Start)+(Math.Max(1,measure)-1)*4.0)),end=block.Start+block.Duration,start=lower;
   if(lower>=end-1e-8)throw new InvalidOperationException("La mesure de départ est après la fin du bloc.");
   if(detect){var model=BlockRules939.Events(original,block);double first=double.PositiveInfinity;var effects=requested==null?(block.Fx939??original.Tracks[track].Fx).Effects:new List<Effect936>{requested};
    foreach(var fx in effects.Where(e=>e.Enabled)){IEnumerable<double> times;
     if(fx.Trigger==TriggerKind936.CC)times=model.CC.Where(c=>!c.StateOnlyR9&&c.Value>=0&&EventsR9.Channel(fx,c.Channel,c.Track940)&&(c.Number==fx.CC||(fx.Kind==EffectKind936.Texte&&fx.TextR12.AllCC))).Select(c=>c.Time);
     else if(fx.Trigger==TriggerKind936.Timestamp)times=new[]{music.Time(music.Beat(block.Start)+AnimationRules938.TimestampBeat(fx))};
     else if(fx.Trigger==TriggerKind936.Début_mesure)times=new[]{music.Time(Math.Ceiling(music.Beat(lower)/4-1e-9)*4)};
     else times=EventsR9.Notes(model,fx).Select(n=>n.Time);
     first=Math.Min(first,times.Where(t=>t>=lower-1e-8&&t<end).DefaultIfEmpty(double.PositiveInfinity).Min());
    }
    if(double.IsInfinity(first))throw new InvalidOperationException("Aucun déclenchement à partir de cette mesure dans le bloc.");start=Math.Max(lower,music.Time(Math.Floor(music.Beat(first)/4+1e-9)*4));
   }
   double finish=Math.Min(end,music.Time(music.Beat(start)+Math.Max(1,count)*4));return Tuple.Create(start,finish-start);
  }
 }
 public sealed partial class Preview936 {
  readonly Button cutR14=new Button{Text="Couper le bloc ici",AutoSize=true,Enabled=false};public Action<double> CutAtR14;
  public double TimelinePositionR14{get{return timelineOrigin+(player==null?duration*seek.Value/10000.0:player.Position);}}
  void InitCutR14(FlowLayoutPanel buttons){buttons.Controls.Add(cutR14);cutR14.Click+=delegate{ControlPlayer(delegate{player.Pause();});try{if(CutAtR14!=null)CutAtR14(TimelinePositionR14);}catch(Exception ex){status.Text=ex.Message;Synkoz.ErrorR8.Show(this,ex.Message,"Erreur");}};}
 }
}

