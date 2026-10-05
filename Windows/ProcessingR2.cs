using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Synkoz {
 public static class OrientationR2 {
  public static string Filter(Track t){return Filter(Degrees(t))+(t.Fx.MirrorX939?",hflip":"")+(t.Fx.MirrorY939?",vflip":"");}
  public static int Degrees(Track t){return ((t.Fx.Rotation)%360+360)%360;}
  public static string Filter(int rotation){return rotation==90?",transpose=clock":rotation==180?",hflip,vflip":rotation==270?",transpose=cclock":"";}
 }
 public static class FoldersR2 {
  public static string Get(string kind){var p=Preferences936.Load();string dir=kind=="animation"?p.LastAnimationFolder:kind=="export"?p.LastExportFolder:kind=="audio"?p.LastAudioPresetFolder:kind=="fx"?p.LastFxPresetFolder:kind=="project"?p.LastProjectFolder:p.LastMontageFolder;return !string.IsNullOrEmpty(dir)&&Directory.Exists(dir)?dir:kind=="montage"?p.DefaultTakeFolder:Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);}
  public static void Set(string kind,string dir){var p=Preferences936.Load();if(kind=="animation")p.LastAnimationFolder=dir;else if(kind=="export")p.LastExportFolder=dir;else if(kind=="audio")p.LastAudioPresetFolder=dir;else if(kind=="fx")p.LastFxPresetFolder=dir;else if(kind=="project")p.LastProjectFolder=dir;else p.LastMontageFolder=dir;p.Save();}
 }
 public static class TriggerR2 {
  public static string NoteName(int n){string[] names={"C","C♯","D","D♯","E","F","F♯","G","G♯","A","A♯","B"};return names[n%12]+(n/12-1)+" — "+n;}
  public static double First(Montage m,Effect936 e){if(m.DirectMode939)return 0;
   if(e.Trigger==TriggerKind936.Timestamp){e.Validate();var clock=new Music936(m);if(!clock.Available)throw new Exception("Repères musicaux absents pour le Timestamp.");double at=clock.Time(AnimationRules938.TimestampBeat(e));if(at<0||at>=m.Duration)throw new Exception("Timestamp en dehors de la prise.");return at;}
   if(e.Trigger==TriggerKind936.CC){var cc=m.CC.Where(x=>x.Channel==e.Channel-1&&x.Number==e.CC).OrderBy(x=>x.Time).FirstOrDefault();if(cc!=null)return cc.Time;}
   else if(e.Trigger==TriggerKind936.Début_mesure){var music=new Music936(m);if(music.Available){double t=music.Time(Math.Ceiling(Math.Max(0,music.Beat(0))/4)*4);if(t<m.Duration)return Math.Max(0,t);}}
   else {var events=m.Triggers.Count>0?m.Triggers:m.Notes.Select(n=>new Trigger936{Time=n.Start,Note=n.Note,Channel=n.Channel}).ToList();var note=events.Where(n=>n.Channel==e.Channel-1&&(e.Note<0||n.Note==e.Note)).OrderBy(n=>n.Time).FirstOrDefault();if(note!=null)return note.Time;}
   throw new Exception("Aucun déclenchement trouvé pour ces réglages. Choisir un canal et un déclencheur présents dans la prise.");
  }
  public static Tuple<double,double> Window(Montage m,Effect936 e,int measures){double first=First(m,e);var music=new Music936(m);if(!music.Available)throw new Exception("Repères musicaux absents : durée en mesures indisponible.");double advance=AnimationRules938.Advance(e);double start=Math.Max(PrerollR4.Minimum(m),music.Time(music.Beat(first)-advance-2));start=Math.Max(PrerollR4.Minimum(m),Math.Floor(start*30)/30);double end=Math.Min(m.Duration,music.Time(music.Beat(start)+Math.Max(4*Math.Max(1,Math.Min(32,measures)),advance+3)));return Tuple.Create(start,Math.Max(1.0/30,end-start));}
 }
 public sealed class BodyMaskR2:IDisposable {
  readonly Process worker;readonly CancellationToken cancel;readonly IDisposable registration;readonly StringBuilder errors=new StringBuilder();
  public BodyMaskR2(CancellationToken cancellation){cancel=cancellation;string root=Path.Combine(Engine.Root,"Dependencies","BodyMask");string python=Path.Combine(root,"python","python.exe");if(!File.Exists(python)||!File.Exists(Path.Combine(root,"selfie_segmenter.tflite")))throw new Exception("Dépendances du masque corporel absentes. Réinstaller la livraison complète.");
   worker=Engine.Create(python,"-u "+Engine.Q(Path.Combine(root,"segment.py")));worker.ErrorDataReceived+=(s,e)=>{if(e.Data!=null)lock(errors){if(errors.Length<4096)errors.AppendLine(e.Data);}};
   worker.Start();worker.BeginErrorReadLine();registration=cancel.Register(()=>{try{worker.Kill();}catch{}});
   try{var hello=Read(8,60000);if(Encoding.ASCII.GetString(hello)!="SKMASK2\n")throw new Exception("Protocole du masque corporel invalide.");}catch{Dispose();throw;}
  }
  byte[] Read(int length,int timeout){var task=Task.Run(()=>{var bytes=new byte[length];int at=0;while(at<length){int n=worker.StandardOutput.BaseStream.Read(bytes,at,length-at);if(n==0){string detail;lock(errors)detail=errors.ToString();throw new Exception("Analyse corporelle interrompue. "+detail);}at+=n;}return bytes;});if(!task.Wait(timeout,cancel)){try{worker.Kill();}catch{}throw new Exception("Analyse corporelle sans réponse.");}return task.GetAwaiter().GetResult();}
  public byte[] Mask(Bitmap image){cancel.ThrowIfCancellationRequested();byte[] rgb=new byte[256*256*3];using(var small=new Bitmap(256,256,PixelFormat.Format24bppRgb)){using(var g=Graphics.FromImage(small)){g.InterpolationMode=InterpolationMode.HighQualityBilinear;g.DrawImage(image,new Rectangle(0,0,256,256));}var bits=small.LockBits(new Rectangle(0,0,256,256),ImageLockMode.ReadOnly,PixelFormat.Format24bppRgb);try{System.Runtime.InteropServices.Marshal.Copy(bits.Scan0,rgb,0,rgb.Length);}finally{small.UnlockBits(bits);}}for(int i=0;i<rgb.Length;i+=3){byte b=rgb[i];rgb[i]=rgb[i+2];rgb[i+2]=b;}worker.StandardInput.BaseStream.Write(rgb,0,rgb.Length);worker.StandardInput.BaseStream.Flush();return Read(256*256,20000);}
  public void Dispose(){if(registration!=null)registration.Dispose();if(worker!=null){try{worker.StandardInput.Close();if(!worker.WaitForExit(1500))worker.Kill();}catch{}worker.Dispose();}}
 }
 public static class PixelsR2 {
  public static Bitmap Filter(Bitmap source,EffectKind936 kind,double amount){int w=source.Width,h=source.Height;int radius=Math.Max(1,(int)Math.Min(48,Math.Abs(amount)/3));
   if(kind==EffectKind936.Pixelisation){int block=Math.Max(1,(int)Math.Min(256,Math.Abs(amount)));using(var small=new Bitmap(Math.Max(1,w/block),Math.Max(1,h/block))){using(var g=Graphics.FromImage(small)){g.InterpolationMode=InterpolationMode.HighQualityBilinear;g.DrawImage(source,new Rectangle(0,0,small.Width,small.Height));}var result=new Bitmap(w,h,PixelFormat.Format32bppArgb);using(var g=Graphics.FromImage(result)){g.CompositingMode=CompositingMode.SourceCopy;g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;g.DrawImage(small,new Rectangle(0,0,w,h));}ChromaticR3.PreserveAlpha(source,result);source.Dispose();return result;}}
   var rect=new Rectangle(0,0,w,h);byte[] input=new byte[w*h*4],temp=new byte[input.Length],output=new byte[input.Length];var bits=source.LockBits(rect,ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);try{System.Runtime.InteropServices.Marshal.Copy(bits.Scan0,input,0,input.Length);}finally{source.UnlockBits(bits);}
   // Sliding box blur: O(pixels), independent of radius. No retained image history.
   for(int y=0;y<h;y++)for(int c=0;c<4;c++){int sum=0;for(int dx=-radius;dx<=radius;dx++)sum+=input[(y*w+Math.Max(0,Math.Min(w-1,dx)))*4+c];for(int x=0;x<w;x++){temp[(y*w+x)*4+c]=(byte)(sum/(radius*2+1));sum-=input[(y*w+Math.Max(0,x-radius))*4+c];sum+=input[(y*w+Math.Min(w-1,x+radius+1))*4+c];}}
   for(int x=0;x<w;x++)for(int c=0;c<4;c++){int sum=0;for(int dy=-radius;dy<=radius;dy++)sum+=temp[(Math.Max(0,Math.Min(h-1,dy))*w+x)*4+c];for(int y=0;y<h;y++){output[(y*w+x)*4+c]=(byte)(sum/(radius*2+1));sum-=temp[(Math.Max(0,y-radius)*w+x)*4+c];sum+=temp[(Math.Min(h-1,y+radius+1)*w+x)*4+c];}}
   for(int i=3;i<output.Length;i+=4)output[i]=input[i];
   bits=source.LockBits(rect,ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);try{System.Runtime.InteropServices.Marshal.Copy(output,0,bits.Scan0,output.Length);}finally{source.UnlockBits(bits);}return source;
  }
  public static unsafe void Blend(Bitmap before,Bitmap after,byte[] mask){if(mask==null)throw new Exception("Masque corporel absent ; rendu interrompu.");int w=before.Width,h=before.Height;var rect=new Rectangle(0,0,w,h);var a=before.LockBits(rect,ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);var b=after.LockBits(rect,ImageLockMode.ReadWrite,PixelFormat.Format32bppArgb);try{for(int y=0;y<h;y++){double my=y*255.0/Math.Max(1,h-1);int iy=(int)my,jy=Math.Min(255,iy+1);double fy=my-iy;byte* p=(byte*)a.Scan0+y*a.Stride,q=(byte*)b.Scan0+y*b.Stride;for(int x=0;x<w;x++){double mx=x*255.0/Math.Max(1,w-1);int ix=(int)mx,jx=Math.Min(255,ix+1);double fx=mx-ix;double alpha=((mask[iy*256+ix]*(1-fx)+mask[iy*256+jx]*fx)*(1-fy)+(mask[jy*256+ix]*(1-fx)+mask[jy*256+jx]*fx)*fy)/255;for(int c=0;c<4;c++){int at=x*4+c;q[at]=(byte)Math.Round(p[at]*(1-alpha)+q[at]*alpha);}}}}finally{before.UnlockBits(a);after.UnlockBits(b);}}
 }
}
