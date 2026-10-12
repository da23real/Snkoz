using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
namespace Synkoz {
 static class ZoneMaskR8 {
  public static byte[] Make(Effect936 e){if(e.ZoneFreeR20&&e.ZonePointsR20.Count>=3)return ZoneFreeR20.Mask(e);var mask=new byte[256*256];double rx=e.ZoneWr8/2,ry=e.ZoneHr8/2,soft=e.FeatherR8/100;for(int y=0;y<256;y++)for(int x=0;x<256;x++){double dx=Math.Abs(((x+.5)*100/256-e.ZoneXr8)/rx),dy=Math.Abs(((y+.5)*100/256-e.ZoneYr8)/ry);double distance=e.EllipseR8?Math.Sqrt(dx*dx+dy*dy):Math.Max(dx,dy);double a=soft<=0?(distance<=1?1:0):Math.Max(0,Math.Min(1,(1-distance)/soft));a=a*a*(3-2*a);mask[y*256+x]=(byte)Math.Round(255*(e.InvertR8?1-a:a));}return mask;}
 }
 static class BranchRenderR8 {
  public static void Validate(List<Effect936> effects){if(effects.Select(e=>e.IdR8).Distinct().Count()!=effects.Count)throw new Exception("Identifiants FX dupliqués.");foreach(var e in effects.Where(e=>!string.IsNullOrEmpty(e.ParentR8))){var parent=effects.FirstOrDefault(x=>x.IdR8==e.ParentR8);if(parent==null||(parent.Kind!=EffectKind936.Animation&&parent.Kind!=EffectKind936.Texte)||!string.IsNullOrEmpty(parent.ParentR8)||(e.Kind==EffectKind936.Animation||e.Kind==EffectKind936.Texte))throw new Exception("Branche de texte ou d’animation invalide.");if(e.BodyMask)throw new Exception("Utiliser la zone d’action sur un effet d’animation ; le masque corporel analyse la vidéo.");}}
  internal static Bitmap Apply(Bitmap source,EffectRun936 run,IEnumerable<EffectRun936> runs,double time){if(run.Fx.Kind!=EffectKind936.Animation&&run.Fx.Kind!=EffectKind936.Texte)return FxRender936.Apply(source,run,time);var children=runs.Where(r=>r.Fx.ParentR8==run.Fx.IdR8).ToArray();run.OpacityOffset15=run.Fx.Kind==EffectKind936.Animation?children.Where(c=>c.Fx.Kind==EffectKind936.Opacité).Sum(c=>c.Value(time)):0;if(children.Length==0)return FxRender936.Apply(source,run,time);Bitmap layer=new Bitmap(source.Width,source.Height,PixelFormat.Format32bppArgb);try{layer=FxRender936.Apply(layer,run,time);double strength=1;foreach(var child in children){if(run.Fx.Kind==EffectKind936.Animation&&child.Fx.Kind==EffectKind936.Opacité)continue;if(child.Fx.Kind==EffectKind936.Mute){strength*=1-Math.Max(0,Math.Min(100,child.Value(time)))/100;continue;}if(strength<=0||!child.Active(time))continue;if(strength>=1)layer=FxRender936.Apply(layer,child,time);else using(var before=(Bitmap)layer.Clone()){layer=FxRender936.Apply(layer,child,time);ChromaticR3.Mix(before,layer,strength);}}using(var g=Graphics.FromImage(source))g.DrawImageUnscaled(layer,0,0);return source;}finally{layer.Dispose();}}
 }
 public sealed partial class EffectsForm936 {
  void AddEffectR8(){var selected=list.SelectedItem as Effect936;string parent=selected==null?null:(selected.Kind==EffectKind936.Animation||selected.Kind==EffectKind936.Texte)?selected.IdR8:selected.ParentR8;var e=new Effect936{ParentR8=parent};var source=draft.Effects.FirstOrDefault(x=>x.IdR8==parent);if(source!=null){InheritR14(e,source);channelsChosenR15.Add(e.IdR8);}draft.Effects.Add(e);RefreshList(draft.Effects.IndexOf(e));}
  void ZoneFieldsR8(Effect936 e){if(e.Kind==EffectKind936.Mute)return;Check("Limiter à une zone",e.ZoneR8,v=>{e.ZoneR8=v;QueueRefresh938(list.SelectedIndex);});if(!e.ZoneR8)return;ZoneVisualR19(e);Check("Inverser la zone",e.InvertR8,v=>e.InvertR8=v);Number("Adoucir les bords (%)",e.FeatherR8,0,100,v=>e.FeatherR8=v);editor.Controls.Add(new Label{AutoSize=true,Text="Zone relative à l’image entière ; adoucissement progressif vers l’intérieur du bord."});}
 }
}
