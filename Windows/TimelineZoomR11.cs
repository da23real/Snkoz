using System;
using System.Linq;
namespace Synkoz {
 public static class TimelineZoomR11 {
  public static double Beat(Montage m,double time){var c=TimelineMusicR11.Get(m);return c.Available?c.Beat(time):time*2;}
  public static double Time(Montage m,double beat){var c=TimelineMusicR11.Get(m);return c.Available?c.Time(beat):beat/2;}
  public static double End(Montage m){double end=Math.Max(m.Duration,m.Blocks.Select(b=>b.Start+b.Duration).DefaultIfEmpty(0).Max());return Math.Max(4,Time(m,Beat(m,end)+32));}
  public static int Steps(Montage m){double bars=(Beat(m,End(m))-Beat(m,0))/4;return Math.Max(1,(int)Math.Ceiling(Math.Log(Math.Max(2,bars)/2,2)));}
  public static double Bars(Montage m,int level){return Math.Pow(2,Math.Max(0,Steps(m)-level)+1);}
  public static double Span(Montage m,int level,double start){return level<=0?End(m):Math.Max(.001,Time(m,Beat(m,start)+Bars(m,level)*4)-start);}
  public static double LastStart(Montage m,int level){return level<=0?0:Math.Max(0,Time(m,Beat(m,End(m))-Bars(m,level)*4));}
 }
}
