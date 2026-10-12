using System;
namespace UrbanKaoz.DisplayRecorder {
 internal sealed class Envelope07 {
  bool held; long began,released; double releaseLevel,level;
  public void Reset(){held=false;began=released=0;releaseLevel=level=0;}
  public double Tick(long now,bool gate,int attack,int decay,double sustain,int release){
   sustain=Math.Max(0,Math.Min(1,sustain));
   if(gate&&!held){held=true;began=now;}
   if(!gate&&held){held=false;released=now;releaseLevel=level;}
   if(held){double age=now-began;level=attack>0&&age<attack?age/attack:decay>0&&age<attack+decay?1-(1-sustain)*(age-attack)/decay:sustain;}
   else level=release>0?releaseLevel*Math.Max(0,1-(now-released)/(double)release):0;
   return level;
  }
 }
}
