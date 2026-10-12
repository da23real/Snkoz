using System;
using System.Drawing;
namespace Synkoz {
 public sealed partial class Effect936 {
  public int DelayShape07,DelaySlices07=8,DelayFilter07;public double DelayDiameter07=60;public bool DelayImages07;
 }
 public static class DelayPlacement07 {
  public static PointF Position(int shape,int repeat,int delay,double diameter,int slices){
   if(shape==0||repeat<=0||delay<=0)return new PointF(50,50);
   slices=Math.Max(1,slices);double phase=((repeat-1)/(double)slices+(delay-1)/4.0)%1;
   double radius=Math.Max(0,Math.Min(100,diameter))/2;
   if(shape==3)radius*=Math.Min(1,repeat/(double)slices);
   double x=Math.Cos(phase*Math.PI*2-Math.PI/2),y=Math.Sin(phase*Math.PI*2-Math.PI/2);
   if(shape==2){double scale=Math.Max(Math.Abs(x),Math.Abs(y));x/=scale;y/=scale;}
   return new PointF((float)(50+radius*x),(float)(50+radius*y));
  }
 }
 sealed partial class EffectRun936 {
  public PointF DelayPosition07(double time){if(Fx.Trigger!=TriggerKind936.Note)return new PointF(50,50);double age=Age(time-Fx.OffsetMs941/1000);if(ni<0||age<0||(!Fx.Gate&&age>=Music936.Length(Fx.Length)))return new PointF(50,50);var n=notes[ni];return DelayPlacement07.Position(Fx.DelayShape07,n.Repeat07,n.Delay07,Fx.DelayDiameter07,Fx.DelaySlices07);}
 }
 public sealed partial class EffectsForm936 {
  void DelayFields07(Effect936 e){
   if(e.Kind!=EffectKind936.Animation&&e.Kind!=EffectKind936.Placement_2D)return;
   var modes=new object[]{"Fixe","Cercle","Carré","Spirale"};var shape=Combo(modes,modes[Math.Max(0,Math.Min(3,e.DelayShape07))]);shape.SelectedIndexChanged+=delegate{e.DelayShape07=shape.SelectedIndex;QueueRefresh938(list.SelectedIndex);};Field("Placement 2D",shape);
   if(e.DelayShape07!=0){Number("Diamètre (%)",e.DelayDiameter07,0,100,v=>e.DelayDiameter07=v,1);Number("Nombre de positions",e.DelaySlices07,1,128,v=>e.DelaySlices07=(int)v,0);Number("Delay (0 = tous)",e.DelayFilter07,0,4,v=>e.DelayFilter07=(int)v,0);}
   if(e.Kind==EffectKind936.Animation&&e.Trigger==TriggerKind936.Note)Check("Répétition → image suivante",e.DelayImages07,v=>{e.DelayImages07=v;QueueRefresh938(list.SelectedIndex);});
  }
 }
}
