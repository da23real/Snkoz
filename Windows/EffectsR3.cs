using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
namespace Synkoz {
 public enum LoopR3 { Aucune,Retour_début,Aller_retour }
 public enum MaskSideR3 { Intérieur,Extérieur,Deux_côtés }
 public sealed class ZoneR3 {public double Start,End;}
 public sealed class AudioChoiceR3 {public int Track;public double GainDb;}
 public static class MaskR3 {
  public static byte[] Shape(byte[] mask,Effect936 e){if(mask==null)throw new Exception("Masque absent.");const int n=256;var inside=new bool[n*n];for(int i=0;i<inside.Length;i++)inside[i]=mask[i]>=128;
   float[] di=Distance(inside,false),doo=Distance(inside,true);double max=1;for(int i=0;i<di.Length;i++)if(inside[i]&&di[i]<10000)max=Math.Max(max,di[i]);
   double depth=Math.Max(.01,max*e.MaskDepth/100),soft=Math.Max(.5,depth*e.MaskSoftness/100);var output=new byte[n*n];int dx=(int)Math.Round(e.MaskX*2.56),dy=(int)Math.Round(e.MaskY*2.56);
   for(int y=0;y<n;y++)for(int x=0;x<n;x++){int sx=x-dx,sy=y-dy;if(sx<0||sx>=n||sy<0||sy>=n)continue;int i=sy*n+sx;double a=mask[i]/255.0,v=0;
    if(e.MaskSide!=MaskSideR3.Extérieur){double inward=e.MaskDepth>=100?1:Math.Max(0,Math.Min(1,(depth-di[i])/soft+.5));v=a*inward;}
    if(e.MaskSide!=MaskSideR3.Intérieur){double outward=Math.Max(0,Math.Min(1,(depth-doo[i])/soft+.5));v=Math.Max(v,(1-a)*outward);}
    if(e.ContourBandsA01>1&&v>0){double d=inside[i]?di[i]:doo[i];double band=d/Math.Max(.01,depth)*e.ContourBandsA01;v*=((band%1)<.5?1:0);}
    if(e.MaskDepth<=0)v=0;output[y*n+x]=(byte)Math.Round(255*v);
   }return output;
  }
  static float[] Distance(bool[] input,bool target){const int n=256;var d=new float[n*n];for(int y=0;y<n;y++)for(int x=0;x<n;x++){int i=y*n+x;d[i]=input[i]==target?0:(!target&&(x==0||x==255||y==0||y==255)?1:10000);}
   for(int y=0;y<n;y++)for(int x=0;x<n;x++){int i=y*n+x;if(x>0)d[i]=Math.Min(d[i],d[i-1]+1);if(y>0){d[i]=Math.Min(d[i],d[i-n]+1);if(x>0)d[i]=Math.Min(d[i],d[i-n-1]+1.4142f);if(x<n-1)d[i]=Math.Min(d[i],d[i-n+1]+1.4142f);}}
   for(int y=n-1;y>=0;y--)for(int x=n-1;x>=0;x--){int i=y*n+x;if(x<n-1)d[i]=Math.Min(d[i],d[i+1]+1);if(y<n-1){d[i]=Math.Min(d[i],d[i+n]+1);if(x>0)d[i]=Math.Min(d[i],d[i+n-1]+1.4142f);if(x<n-1)d[i]=Math.Min(d[i],d[i+n+1]+1.4142f);}}return d;
  }
 }
 public static class ChromaticR3 {
  public static unsafe void PreserveAlpha(Bitmap source,Bitmap result){var rect=new Rectangle(0,0,source.Width,source.Height);var a=source.LockBits(rect,ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);var b=result.LockBits(rect,ImageLockMode.ReadWrite,PixelFormat.Format32bppArgb);try{for(int y=0;y<source.Height;y++){byte* p=(byte*)a.Scan0+y*a.Stride,q=(byte*)b.Scan0+y*b.Stride;for(int x=0;x<source.Width;x++)q[x*4+3]=p[x*4+3];}}finally{source.UnlockBits(a);result.UnlockBits(b);}}
  internal static unsafe Bitmap Apply(Bitmap image,EffectRun936 run,double t,double amount){int w=image.Width,h=image.Height;var fx=run.Fx;var a=Color.FromArgb(fx.ColorA);var b=Color.FromArgb(fx.ColorB);double power=Math.Max(0,Math.Min(1,Math.Abs(amount)/100)),angle=fx.Angle*Math.PI/180,phase=run.Phase(t);
   if(fx.Kind==EffectKind936.Cycle_couleurs){double hue=((run.MusicalBeat(t)*360/fx.CycleBeats+fx.Angle)%360+360)%360;double q=hue/60;int k=(int)q;double f=q-k;int up=(int)(255*f),down=255-up;a=k==0?Color.FromArgb(255,up,0):k==1?Color.FromArgb(down,255,0):k==2?Color.FromArgb(0,255,up):k==3?Color.FromArgb(0,down,255):k==4?Color.FromArgb(up,0,255):Color.FromArgb(255,0,down);}
   var bits=image.LockBits(new Rectangle(0,0,w,h),ImageLockMode.ReadWrite,PixelFormat.Format32bppArgb);try{for(int y=0;y<h;y++){byte* row=(byte*)bits.Scan0+y*bits.Stride;for(int x=0;x<w;x++){byte* p=row+x*4;if(p[3]==0)continue;double mix=power;
     if(fx.Sweep){double vx=fx.ToX-fx.FromX,vy=fx.ToY-fx.FromY,den=vx*vx+vy*vy;double position=den<.0001?0:((x*100.0/Math.Max(1,w-1)-fx.FromX)*vx+(y*100.0/Math.Max(1,h-1)-fx.FromY)*vy)/den;double sweep=fx.Trigger==TriggerKind936.CC?power:phase;mix*=Math.Max(0,1-Math.Abs(position-sweep)/.15);}
     double ratio=Math.Max(0,Math.Min(1,.5+(x/(double)Math.Max(1,w-1)-.5)*Math.Cos(angle)+(y/(double)Math.Max(1,h-1)-.5)*Math.Sin(angle)));
     for(int c=0;c<3;c++){double ca=c==0?a.B:c==1?a.G:a.R,cb=c==0?b.B:c==1?b.G:b.R,target=ca;
      if(fx.Kind==EffectKind936.Dégradé)target=ca*(1-ratio)+cb*ratio;else if(fx.Kind==EffectKind936.Inversion)target=255-p[c];else if(fx.Kind==EffectKind936.Postérisation){int levels=Math.Max(2,16-(int)Math.Round(power*14));target=Math.Round(p[c]/255.0*(levels-1))*255/(levels-1);}
      p[c]=(byte)Math.Max(0,Math.Min(255,Math.Round(p[c]*(1-mix)+target*mix)));
     }
   }}}finally{image.UnlockBits(bits);}return image;
  }
  public static unsafe void Mix(Bitmap before,Bitmap after,double strength){var rect=new Rectangle(0,0,before.Width,before.Height);var a=before.LockBits(rect,ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);var b=after.LockBits(rect,ImageLockMode.ReadWrite,PixelFormat.Format32bppArgb);try{for(int y=0;y<before.Height;y++){byte* p=(byte*)a.Scan0+y*a.Stride,q=(byte*)b.Scan0+y*b.Stride;for(int x=0;x<before.Width;x++){int i=x*4;double aa=p[i+3]*(1-strength),ab=q[i+3]*strength,total=aa+ab;for(int c=0;c<3;c++)q[i+c]=total<.001?(byte)0:(byte)Math.Round((p[i+c]*aa+q[i+c]*ab)/total);q[i+3]=(byte)Math.Round(total);}}}finally{before.UnlockBits(a);after.UnlockBits(b);}}
 }
 public sealed class ProjectR3 {public int Version=100232;public string MediaRoot;public Montage Montage;}
 public sealed class PresetR3 {public int Version=100232;public List<Effect936> Effects=new List<Effect936>();public Dictionary<string,string> Assets=new Dictionary<string,string>();}
 public static class FilesR3 {
  public static void SaveProject(Montage m,string path){Store934.Save(path,new ProjectR3{Version=100232,MediaRoot=Path.GetFullPath(m.Folder),Montage=m});m.ProjectFile=path;}
  public static Montage LoadProject(string path){return ProjectReadR8.Load(path); }
  public static void SavePreset(Montage m,IEnumerable<Effect936> effects,string path){var p=new PresetR3{Version=100232,Effects=UiR2.Copy(effects.ToList())};foreach(var e in p.Effects){if(e.Kind==EffectKind936.Texte&&!string.IsNullOrEmpty(e.TextR12.FontFileR2)){string font=e.TextR12.FontFileR2;if(!p.Assets.ContainsKey(font))p.Assets[font]=Convert.ToBase64String(File.ReadAllBytes(font));}if(!string.IsNullOrWhiteSpace(e.WaitingR20.Asset))p.Assets[e.WaitingR20.Asset]=Convert.ToBase64String(File.ReadAllBytes(Store934.MediaPath(m.Folder,e.WaitingR20.Asset)));BankPresetsR16.Pack(e.BanksR16,p.Assets,m.Folder);e.Validate();AnimationRules938.ValidateAsset(m,e);if(e.Kind==EffectKind936.Animation&&!string.IsNullOrWhiteSpace(e.Asset)&&!p.Assets.ContainsKey(e.Asset))p.Assets[e.Asset]=Convert.ToBase64String(File.ReadAllBytes(Store934.MediaPath(m.Folder,e.Asset)));}Store934.Save(path,p);}
  public static List<Effect936> LoadPreset(Montage m,string path){var p=Store934.Load<PresetR3>(path);if(p.Version!=100218&&p.Version!=100219&&p.Version!=100220&&p.Version!=100221&&p.Version!=100222&&p.Version!=100223&&p.Version!=100224&&p.Version!=100225&&p.Version!=100226&&p.Version!=100227&&p.Version!=100228&&p.Version!=100229&&p.Version!=100230&&p.Version!=100231&&p.Version!=100232)throw new Exception("Preset FX incompatible.");foreach(var e in p.Effects){e.Validate();BankPresetsR16.Unpack(e.BanksR16,p.Assets,Path.Combine(ProjectStorageR11.Resources(m),"Banks"));}var mapping=new Dictionary<string,string>();foreach(var e in p.Effects.Where(e=>e.Kind==EffectKind936.Animation&&!string.IsNullOrWhiteSpace(e.Asset))){if(!p.Assets.ContainsKey(e.Asset))throw new Exception("Animation absente du preset.");if(!mapping.ContainsKey(e.Asset)){string ext=Path.GetExtension(e.Asset).ToLowerInvariant();if(ext!=".png"&&ext!=".gif"&&ext!=".mp4")throw new Exception("Format animation invalide.");string relative=Path.Combine(ProjectStorageR11.Resources(m),Guid.NewGuid().ToString("N")+ext);File.WriteAllBytes(Store934.MediaPath(m.Folder,relative),Convert.FromBase64String(p.Assets[e.Asset]));mapping[e.Asset]=relative;}e.Asset=mapping[e.Asset];}foreach(var e in p.Effects.Where(x=>x.Kind==EffectKind936.Texte&&!string.IsNullOrEmpty(x.TextR12.FontFileR2))){string old=e.TextR12.FontFileR2;if(!p.Assets.ContainsKey(old))throw new Exception("Police absente du preset.");string ext=Path.GetExtension(old).ToLowerInvariant();if(ext!=".ttf"&&ext!=".otf")throw new Exception("Police incompatible.");string dir=Path.Combine(ProjectStorageR11.Resources(m),"Fonts");Directory.CreateDirectory(dir);string dest=Path.Combine(dir,Guid.NewGuid().ToString("N")+ext);File.WriteAllBytes(dest,Convert.FromBase64String(p.Assets[old]));PersonalFontsR2.Family(dest);e.TextR12.FontFileR2=dest;}foreach(var effect in p.Effects.Where(e=>!string.IsNullOrWhiteSpace(e.WaitingR20.Asset))){string data;if(!p.Assets.TryGetValue(effect.WaitingR20.Asset,out data))throw new Exception("Animation d’attente absente du preset.");string dest=Path.Combine(ProjectStorageR11.Resources(m),"Attente_"+Guid.NewGuid().ToString("N")+".gif");File.WriteAllBytes(dest,Convert.FromBase64String(data));effect.WaitingR20.Asset=dest;}var ids=p.Effects.ToDictionary(e=>e.IdR8,e=>Guid.NewGuid().ToString("N"));foreach(var e in p.Effects){if(!string.IsNullOrEmpty(e.ParentR8))e.ParentR8=ids.ContainsKey(e.ParentR8)?ids[e.ParentR8]:null;e.IdR8=ids[e.IdR8];}return p.Effects;}
 }
}



