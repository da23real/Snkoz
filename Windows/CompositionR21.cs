using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
namespace Synkoz {
 public sealed class CompositionStepR21 { public double Start; public List<GraphNode15> Sources=new List<GraphNode15>(); }
 public sealed class CompositionSettingsR21 {
  public List<CompositionStepR21> Steps=new List<CompositionStepR21>{new CompositionStepR21()};
  public bool ThumbnailsLeft; public int PreviewHeight=144;
  public CompositionStepR21 At(double time){return Steps.LastOrDefault(s=>s.Start<=time+1e-9)??Steps[0];}
  public void Validate(){if(Steps==null||Steps.Count==0||Steps.Count>10000||Steps[0].Start!=0)throw new Exception("Composition : début absent.");double last=-1;foreach(var step in Steps){if(double.IsNaN(step.Start)||double.IsInfinity(step.Start)||step.Start<=last||step.Sources==null||step.Sources.Count>64)throw new Exception("Composition : étapes invalides.");last=step.Start;foreach(var n in step.Sources){if(n==null||n.Kind!=GraphKind15.Video||n.CompositionR21!=null)throw new Exception("Source de composition invalide.");GraphRules15.ValidateSourceR21(n);}}}
  public void CutStart(double delta){var first=UiR2.Copy(At(delta));first.Start=0;var rest=Steps.Where(s=>s.Start>delta+1e-9).Select(s=>UiR2.Copy(s)).ToList();foreach(var step in rest)step.Start-=delta;Steps=new List<CompositionStepR21>{first};Steps.AddRange(rest);}
  public void Fix(double time){time=Math.Round(time*30)/30;if(time<=Steps.Last().Start+1e-8)throw new Exception("Choisis une image après le début de l’étape actuelle.");Steps.Add(new CompositionStepR21{Start=time,Sources=UiR2.Copy(Steps.Last().Sources)});}
 }
 public sealed class CompositionPresetR21 {public int Version=1;public List<GraphNode15> Sources=new List<GraphNode15>();}
 public sealed partial class GraphNode15 {
  public CompositionSettingsR21 CompositionR21;
  public bool MirrorXR21,MirrorYR21;
  public string MaskShapeR21="Aucun";public double MaskXR21,MaskYR21,MaskWidthR21=100,MaskHeightR21=100;
  public List<PointF> MaskPointsR21=new List<PointF>();
 }
 public static partial class GraphRules15 {internal static void ValidateSourceR21(GraphNode15 n){ValidateR4(n);if(new[]{n.MaskXR21,n.MaskYR21,n.MaskWidthR21,n.MaskHeightR21}.Any(x=>double.IsNaN(x)||double.IsInfinity(x))||n.MaskWidthR21<=0||n.MaskHeightR21<=0||n.MaskPointsR21==null||n.MaskPointsR21.Count>256||n.MaskPointsR21.Any(p=>float.IsNaN(p.X)||float.IsNaN(p.Y)||float.IsInfinity(p.X)||float.IsInfinity(p.Y)))throw new Exception("Masque de composition invalide.");}}
 internal static class CompositionRenderR21 {
  internal static Bitmap Render(CompositionSettingsR21 settings,double time,int width,int height,Func<GraphNode15,Bitmap> read){var result=new Bitmap(width,height);try{using(var g=Graphics.FromImage(result)){foreach(var source in settings.At(time).Sources){if(source.MutedR4||source.ImageOpacityR6<=0)continue;using(var image=read(source))using(var attributes=new System.Drawing.Imaging.ImageAttributes()){var matrix=new System.Drawing.Imaging.ColorMatrix();matrix.Matrix33=(float)Math.Max(0,Math.Min(1,source.ImageOpacityR6/100));attributes.SetColorMatrix(matrix);g.DrawImage(image,new Rectangle(0,0,width,height),0,0,width,height,GraphicsUnit.Pixel,attributes);}}}return result;}catch{result.Dispose();throw;}}
  internal static Bitmap Mask(Bitmap image,GraphNode15 n){if(n.MirrorXR21)image.RotateFlip(RotateFlipType.RotateNoneFlipX);if(n.MirrorYR21)image.RotateFlip(RotateFlipType.RotateNoneFlipY);if(n.MaskShapeR21=="Aucun")return image;var result=new Bitmap(image.Width,image.Height);using(var g=Graphics.FromImage(result))using(var path=new GraphicsPath()){var r=new RectangleF((float)(image.Width*n.MaskXR21/100),(float)(image.Height*n.MaskYR21/100),(float)(image.Width*n.MaskWidthR21/100),(float)(image.Height*n.MaskHeightR21/100));if(n.MaskShapeR21=="Ellipse")path.AddEllipse(r);else if(n.MaskShapeR21=="Polygone"&&n.MaskPointsR21.Count>=3)path.AddPolygon(n.MaskPointsR21.Select(p=>new PointF(p.X*image.Width/100,p.Y*image.Height/100)).ToArray());else path.AddRectangle(r);g.SetClip(path);g.DrawImageUnscaled(image,0,0);}image.Dispose();return result;}
 }
 internal sealed partial class GraphRun15 {
  Bitmap CompositionFrameR21(GraphNode15 n,Bitmap background,double time){if(n.MutedR4||!ConditionR20.At(n.ConditionR20,conditionStreamsR20[n.Id],time))return Blank(background.Width,background.Height);var settings=n.CompositionR21;var step=settings.At(Math.Max(0,time-blockOriginR19));var current=new CompositionSettingsR21{Steps=new List<CompositionStepR21>{new CompositionStepR21{Sources=step.Sources.Select(source=>SourceLookR12.At(source,events[n.Id],model,time)).ToList()}}};return CompositionRenderR21.Render(current,0,background.Width,background.Height,source=>SourceFrameTbo15(source,background,time));}
 }
}
