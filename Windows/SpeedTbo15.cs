using System;using System.Collections.Generic;using System.Linq;using System.Windows.Forms;
namespace Synkoz {
 public sealed partial class Effect936 {public double SpeedBaseTbo15=1;public Binding15 SpeedTbo15=new Binding15{Source=-1,Min=0,Max=4};}
 // Integral in musical beats, piecewise constant at source events; binary search on every frame.
 internal sealed class SpeedIntegralTbo15 {
  readonly Music936 clock;readonly double baseline;readonly double[] times,beats,areas,speeds;
  public SpeedIntegralTbo15(Effect936 fx,Montage m,List<GraphEvent15> input=null,Montage source=null){clock=new Music936(source??m);baseline=fx.SpeedBaseTbo15;var bank=fx.EmbeddedSources15;var events=input!=null&&fx.SpeedTbo15.MidiR4!=null?input.Where(fx.SpeedTbo15.MidiR4.Matches).GroupBy(e=>e.Time).Select(g=>g.Last()).ToArray():fx.SpeedTbo15.Source>=0&&fx.SpeedTbo15.Source<bank.Count?GraphEvents15.Source(bank[fx.SpeedTbo15.Source],m,"speed").Where(e=>e.Kind!="NoteOff").GroupBy(e=>e.Time).Select(g=>g.Last()).ToArray():new GraphEvent15[0];times=events.Select(e=>e.Time).ToArray();beats=times.Select(Beat).ToArray();speeds=events.Select(e=>Math.Max(0,fx.SpeedTbo15.MidiR4!=null?MidiR4.Map(fx.SpeedTbo15,e,baseline):fx.SpeedTbo15.Min+(fx.SpeedTbo15.Max-fx.SpeedTbo15.Min)*TboSignal15.Normalize(e))).ToArray();areas=new double[times.Length];for(int i=0;i<times.Length;i++)areas[i]=i==0?beats[i]*baseline:areas[i-1]+(beats[i]-beats[i-1])*speeds[i-1];}
  double Beat(double t){return clock.Available?clock.Beat(t):t*2;}
  double Area(double t){int lo=0,hi=times.Length;while(lo<hi){int mid=(lo+hi)/2;if(times[mid]<=t)lo=mid+1;else hi=mid;}return lo==0?Beat(t)*baseline:areas[lo-1]+(Beat(t)-beats[lo-1])*speeds[lo-1];}
  public double Age(double begin,double end){return Math.Max(0,Area(end)-Area(begin));}
 }
 public sealed partial class EffectsForm936 {
  void SpeedFieldsTbo15(Effect936 e){if(System.IO.Path.GetExtension(e.Asset).Equals(".mp4",StringComparison.OrdinalIgnoreCase)){editor.Controls.Add(new Label{AutoSize=true,Text="Vitesse MP4 fixe."});return;}Number("Vitesse de l’animation (×)",e.SpeedBaseTbo15,0,16,v=>e.SpeedBaseTbo15=v,3);BindingFields15("Vitesse (×)",e.SpeedTbo15);editor.Controls.Add(new Label{AutoSize=true,Text="Vitesse intégrée dans le temps : pas de saut à chaque variation MIDI.\nCycle par notes conserve son avance d’une image par événement."});}
 }
}

