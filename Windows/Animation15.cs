using System;using System.Linq;using System.Windows.Forms;
namespace Synkoz {
 public sealed partial class Effect936 {
  public int CycleMode15,ResetMeasures15=4;public double Opacity15=100;
  public void Validate15(){if(CcSmoothingBeatsR18.HasValue&&(double.IsNaN(CcSmoothingBeatsR18.Value)||double.IsInfinity(CcSmoothingBeatsR18.Value)||CcSmoothingBeatsR18<0||CcSmoothingBeatsR18>16))throw new Exception("Lissage musical hors limites.");if(new[]{CcUpdateFpsR14,CcSmoothingMsR14}.Any(v=>double.IsNaN(v)||double.IsInfinity(v))||CcUpdateFpsR14<0||CcUpdateFpsR14>240||CcSmoothingMsR14<0||CcSmoothingMsR14>5000)throw new Exception("Cadence ou lissage CC hors limites.");if(double.IsNaN(SpeedBaseTbo15)||double.IsInfinity(SpeedBaseTbo15)||SpeedBaseTbo15<0||SpeedBaseTbo15>16)throw new Exception("Vitesse invalide.");if(CycleMode15<0||CycleMode15>2||ResetMeasures15<0||ResetMeasures15>1024||double.IsNaN(Opacity15)||Opacity15<0||Opacity15>100)throw new Exception("Cycle ou opacité invalide.");}
 }
 public static class Cycle15 {
  public static int Index(int count,long step,int mode){if(count<=0||step<0)return -1;if(mode==1)return step<count?(int)step:-1;if(mode==0)return (int)(step%count);if(count==1)return 0;long period=2L*(count-1),p=step%period;return (int)(p<count?p:period-p);}
 }
 sealed partial class EffectRun936 {
  int CycleFrame15(double time){int end=Upper15(time);if(end==0&&!Fx.ResetFrameZeroR14)return -1;int begin=0;if(Fx.ResetMeasures15>0){double beat=music.Beat(time),period=Fx.ResetMeasures15*4.0;double start=music.Time(Math.Floor((beat+1e-9)/period)*period);int lo=0,hi=end;while(lo<hi){int mid=(lo+hi)/2;if(notes[mid].Time<start-1e-9)lo=mid+1;else hi=mid;}begin=lo;}int frame=Cycle15.Index(Frames-Fx.StartImage938+1,end-begin-(Fx.ResetFrameZeroR14?0:1),Fx.CycleMode15);return frame<0?-1:Fx.StartImage938-1+frame;}
  int Upper15(double time){int lo=0,hi=notes.Length;while(lo<hi){int mid=(lo+hi)/2;if(notes[mid].Time<=time+1e-9)lo=mid+1;else hi=mid;}return lo;}
  internal double OpacityOffset15;
  double AnimationOpacity15(double time){return Math.Max(0,Math.Min(100,Fx.Opacity15+OpacityOffset15))/100;}
 }
 public sealed partial class EffectsForm936 {
  void CycleFields15(Effect936 e){Check("Afficher la première image au retour de mesure",e.ResetFrameZeroR14,v=>e.ResetFrameZeroR14=v);var mode=Combo(new object[]{"En boucle","Une passe","Aller-retour"},new[]{"En boucle","Une passe","Aller-retour"}[e.CycleMode15]);mode.SelectedIndexChanged+=delegate{e.CycleMode15=mode.SelectedIndex;};Field("Parcours du cycle",mode);Number("Réinitialiser toutes les mesures (0 = jamais)",e.ResetMeasures15,0,1024,v=>e.ResetMeasures15=(int)v,0);editor.Controls.Add(new Label{AutoSize=true,Text="Chaque Note On avance d’une image, même pour la même hauteur. Une passe devient invisible après la dernière image. La réinitialisation suit la grille des mesures."});}
 }
}

