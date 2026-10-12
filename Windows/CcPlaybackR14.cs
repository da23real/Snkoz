using System;using System.Linq;using System.Collections.Generic;
namespace Synkoz {
 public sealed partial class Effect936 { public double CcUpdateFpsR14,CcSmoothingMsR14; public double? CcSmoothingBeatsR18; public bool ResetFrameZeroR14; }
 // Immutable, causal evaluation. Random seeks and sequential rendering use the same anchors.
 internal sealed class CcPlaybackR14 {
  readonly List<GraphEvent15> events;readonly Dictionary<GraphEvent15,double> anchors=new Dictionary<GraphEvent15,double>();readonly double tau,fps;readonly Func<double,double> coordinate;
  public CcPlaybackR14(IEnumerable<GraphEvent15> input,Effect936 fx,Music936 music=null){coordinate=fx.Kind!=EffectKind936.Texte&&fx.CcSmoothingBeatsR18.HasValue?(Func<double,double>)(t=>music!=null&&music.Available?music.Beat(t):t*2):t=>t;events=input.OrderBy(x=>x.Time).ToList();tau=fx.Kind!=EffectKind936.Texte&&fx.CcSmoothingBeatsR18.HasValue?Math.Max(0,fx.CcSmoothingBeatsR18.Value):Math.Max(0,fx.CcSmoothingMsR14)/1000;fps=Math.Max(0,fx.CcUpdateFpsR14);var previous=new Dictionary<string,GraphEvent15>();foreach(var e in events.Where(x=>x.Kind=="CC")){string key=e.Origin+"/"+e.Channel+"/"+e.Track+"/"+e.CC+"/"+e.Bits;GraphEvent15 p;anchors[e]=previous.TryGetValue(key,out p)&&tau>0?p.Value+(anchors[p]-p.Value)*Math.Exp(-(coordinate(e.Time)-coordinate(p.Time))/tau):e.Value;previous[key]=e;}}
  public double Time(double time){return fps>0?Math.Floor(time*fps+1e-8)/fps:time;}
  public GraphEvent15 At(double time){var raw=GraphEvents15.Latest(events,time);if(raw==null||raw.Kind!="CC")return raw;double t=Time(time);var e=GraphEvents15.Latest(events,t);if(e==null||e.Kind!="CC"||tau<=0)return e;var value=e.Copy();value.Value=e.Value+(anchors[e]-e.Value)*Math.Exp(-Math.Max(0,coordinate(t)-coordinate(e.Time))/tau);return value;}
 }
 internal sealed partial class EffectRun936 {
  readonly Dictionary<List<GraphEvent15>,CcPlaybackR14> ccReadersR14=new Dictionary<List<GraphEvent15>,CcPlaybackR14>();
  internal GraphEvent15 CcAtR14(List<GraphEvent15> data,double time){CcPlaybackR14 reader;if(!ccReadersR14.TryGetValue(data,out reader))ccReadersR14[data]=reader=new CcPlaybackR14(data,Fx,music);return reader.At(time);}
 }
}
