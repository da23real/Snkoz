using System;using System.Linq;using System.Collections.Generic;using System.Windows.Forms;
namespace Synkoz {
 public static class MidiFixR10 {
  // R4-R9 generated selectors constrained both channel and inferred track. In channel mode
  // snapshots carry track IDs but subsequent merged CCs do not. Respect the source's chosen axis.
  public static MidiSelectR4 Resolve(MidiSelectR4 selected,Graph15 graph){if(selected==null)return null;var s=UiR2.Copy(selected);var source=graph.Nodes.FirstOrDefault(n=>n.Id==s.Origin&&n.Kind==GraphKind15.Trigger);if(source!=null&&source.Source<0&&source.ListenVirtual==0&&!source.MidiR4.ByTrack&&s.Channel>=0)s.Track=-1;return s;}
  public static void ResolveBindings(Effect936 e,Graph15 graph){foreach(var b in new[]{e.Amplitude15,e.OpacityR4,e.Radius15,e.Phase15,e.SpeedTbo15,e.MoveXR19,e.MoveYR19})b.MidiR4=Resolve(b.MidiR4,graph);}
 }
 public sealed class CcRangeR10 {
  public double Min,Max,Scale;public int Count;
  public static CcRangeR10 Detect(IEnumerable<GraphEvent15> input,MidiSelectR4 select){if(select==null||select.Kind!="CC")return null;var data=input.Where(select.Matches).Where(e=>e.Value>=0).ToArray();if(data.Length==0)return null;double scale=data.Any(e=>e.Bits==64)?1:Math.Pow(2,data.Max(e=>e.Bits))-1;return new CcRangeR10{Min=data.Min(e=>TboSignal15.Normalize(e))*scale,Max=data.Max(e=>TboSignal15.Normalize(e))*scale,Scale=scale,Count=data.Length};}
  public void Fit(Binding15 b){b.InputMinR4=Min/Scale;b.InputMaxR4=Max>Min?Max/Scale:(Min+1)/Scale;b.ClampR4=true;}
 }
 public sealed partial class EffectsForm936 {
  void ThresholdFieldsR10(GraphNode15 n){var range=CcRangeR10.Detect(IncomingR4(n),MidiFixR10.Resolve(n.ActivationR4,draft.Graph15));if(range==null){editor.Controls.Add(new Label{AutoSize=true,Text="CC absent : seuil mémorisé conservé."});return;}double level=n.TriggerLevelR4*range.Scale,rearm=n.RearmR4*range.Scale;if(level<range.Min||level>range.Max||rearm<range.Min||rearm>range.Max){editor.Controls.Add(new Label{AutoSize=true,Tag="keepR10",Text="Seuil mémorisé "+level.ToString("0.##")+" / réarmement "+rearm.ToString("0.##")+" hors plage détectée "+range.Min.ToString("0.##")+"–"+range.Max.ToString("0.##")});var fit=new Button{Text="Adapter les seuils à la plage CC",AutoSize=true};fit.Click+=delegate{n.TriggerLevelR4=(range.Min+(range.Max-range.Min)*.75)/range.Scale;n.RearmR4=(range.Min+(range.Max-range.Min)*.25)/range.Scale;QueueRefresh938(list.SelectedIndex);};editor.Controls.Add(fit);}Number("Seuil CC reçu",Math.Max(range.Min,Math.Min(range.Max,level)),(decimal)range.Min,(decimal)range.Max,v=>n.TriggerLevelR4=v/range.Scale);Number("Réarmement sous CC",Math.Max(range.Min,Math.Min(range.Max,rearm)),(decimal)range.Min,(decimal)range.Max,v=>n.RearmR4=v/range.Scale);}
  void CcBindingR10(string name,Binding15 b){var incoming=IncomingR4(editingGraph15);var selected=MidiFixR10.Resolve(b.MidiR4,draft.Graph15);var range=CcRangeR10.Detect(incoming,selected);
   if(range==null){editor.Controls.Add(new Label{AutoSize=true,Text="CC absent de ce bloc : affectation conservée."});return;}
   editor.Controls.Add(new Label{AutoSize=true,Text="CC détecté : "+range.Min.ToString("0.###")+" → "+range.Max.ToString("0.###")+" · "+range.Count+" messages"});
   double a=b.InputMinR4*range.Scale,z=b.InputMaxR4*range.Scale;bool legacy=a<range.Min||a>range.Max||z<range.Min||z>range.Max;
   if(legacy){var fit=new Button{Text="Utiliser la plage CC détectée",AutoSize=true};fit.Click+=delegate{range.Fit(b);QueueRefresh938(list.SelectedIndex);};editor.Controls.Add(fit);editor.Controls.Add(new Label{AutoSize=true,Text="Ancienne plage conservée : "+a.ToString("0.###")+" → "+z.ToString("0.###")});}
   if(range.Max>range.Min){Number("CC reçu minimum",Math.Max(range.Min,Math.Min(range.Max,a)),(decimal)range.Min,(decimal)range.Max,v=>{if(v>=b.InputMaxR4*range.Scale){QueueRefresh938(list.SelectedIndex);return;}b.InputMinR4=v/range.Scale;b.ClampR4=true;});Number("CC reçu maximum",Math.Max(range.Min,Math.Min(range.Max,z)),(decimal)range.Min,(decimal)range.Max,v=>{if(v<=b.InputMinR4*range.Scale){QueueRefresh938(list.SelectedIndex);return;}b.InputMaxR4=v/range.Scale;b.ClampR4=true;});}
   else editor.Controls.Add(new Label{AutoSize=true,Text="CC constant : aucune variation dans ce bloc."});
   var effect=list.SelectedItem as Effect936;double lo=name=="Amplitude"&&effect!=null?BoundsR3.Min(effect.Kind):0,hi=name=="Amplitude"&&effect!=null?BoundsR3.Max(effect.Kind):100;Number(name+" au minimum CC",b.Min,(decimal)Math.Min(lo,b.Min),(decimal)Math.Max(hi,b.Min),v=>b.Min=v);Number(name+" au maximum CC",b.Max,(decimal)Math.Min(lo,b.Max),(decimal)Math.Max(hi,b.Max),v=>b.Max=v);if(b.GainR4!=1)Number(name+" multiplicateur conservé",b.GainR4,-10000,10000,v=>b.GainR4=v,4);
   var invert=new Button{Text="Inverser le sens de l’effet",AutoSize=true};invert.Click+=delegate{double old=b.Min;b.Min=b.Max;b.Max=old;QueueRefresh938(list.SelectedIndex);};editor.Controls.Add(invert);
  }
 }
}
