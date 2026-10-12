using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
namespace Synkoz {
 // All positions are on the fixed 30 fps montage grid, never repeated rounded milliseconds.
 public static class CalibrationTimeR5 {
  public static double Seconds(long frame){return frame/30.0;}
  public static long Frame(double time){return (long)Math.Round(time*30,MidpointRounding.AwayFromZero);}
  public static double Musical(Music936 music,double anchor,double beats){return music.Time(music.Beat(anchor)+beats)-anchor;}
  public static double Beats(Music936 music,double anchor,double seconds){return music.Beat(anchor+seconds)-music.Beat(anchor);}
  public static void Set(TrackFx936 fx,bool video,double seconds){
   if(double.IsNaN(seconds)||double.IsInfinity(seconds)||Math.Abs(seconds)>3600)throw new Exception("Décalage maximal : ± 3600 secondes.");
   // Materialize the OTHER legacy stream before changing one stream: old coupled offsets migrate losslessly.
   if(!fx.VideoDelayR5.HasValue)fx.VideoDelayR5=fx.Shift;
   if(!fx.AudioDelayR5.HasValue)fx.AudioDelayR5=fx.AudioShift;
   if(video)fx.VideoDelayR5=seconds;else fx.AudioDelayR5=seconds;
  }
 }
 public sealed class FrameCacheR5 {
  public long First;public byte[][] Frames;public const int Width=480,Height=270;
  public Bitmap Get(long frame){int index=(int)(frame-First);if(index<0||index>=Frames.Length)return null;
   var b=new Bitmap(Width,Height,PixelFormat.Format32bppArgb);var data=b.LockBits(new Rectangle(0,0,Width,Height),ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);
   try{Marshal.Copy(Frames[index],0,data.Scan0,Frames[index].Length);}finally{b.UnlockBits(data);}return b;
  }
 }
 public sealed class WaveR5 {public const int Rate=12000;public float[] Samples;public double Origin,Shift;public int Channels=2;}
 public static class CalibrationMediaR5 {
  static string F(double value){return value.ToString("F9",Engine.Inv);}
  public static byte[] Pipe(string args,CancellationToken token){
   token.ThrowIfCancellationRequested();using(var p=Engine.Create(Engine.FF,"-hide_banner -loglevel error "+args)){
    p.Start();var errors=p.StandardError.ReadToEndAsync();using(token.Register(()=>{try{p.Kill();}catch{}}))using(var data=new MemoryStream()){
     p.StandardOutput.BaseStream.CopyTo(data);p.WaitForExit();token.ThrowIfCancellationRequested();if(p.ExitCode!=0)throw new Exception(errors.GetAwaiter().GetResult());return data.ToArray();
    }
   }
  }
  static string Work(Montage model){string dir=Path.Combine(ProjectStorageR11.Cache(model,"Calage"),Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);return dir;}
  static void Clean(string dir){try{Directory.Delete(dir,true);}catch{}}
  public static FrameCacheR5 Video(Montage snapshot,int track,long first,int count,CancellationToken token){
   string dir=Work(snapshot);try{
    string input=Path.Combine(dir,"video.mkv");double origin=CalibrationTimeR5.Seconds(first),length=count/30.0;
    RenderRangeR3.Prepare(snapshot,track,input,origin,length,270,token,false,true);
    string rotation=OrientationR2.Filter(snapshot.Tracks[track]).TrimStart(',');
    string filter=(rotation.Length>0?rotation+",":"")+"scale=480:270:force_original_aspect_ratio=decrease,pad=480:270:(ow-iw)/2:(oh-ih)/2,setsar=1";
    byte[] raw=Pipe("-i "+Engine.Q(input)+" -an -vf "+Engine.Q(filter)+" -frames:v "+count+" -f rawvideo -pix_fmt bgra pipe:1",token);
    int size=FrameCacheR5.Width*FrameCacheR5.Height*4;if(raw.Length%size!=0||raw.Length/size!=count)throw new Exception("Extrait vidéo incomplet.");
    var result=new FrameCacheR5{First=first,Frames=new byte[count][]};for(int i=0;i<count;i++){result.Frames[i]=new byte[size];Buffer.BlockCopy(raw,i*size,result.Frames[i],0,size);}return result;
   }finally{Clean(dir);}
  }
  public static WaveR5 Audio(Montage snapshot,int track,double origin,double length,CancellationToken token){
   string dir=Work(snapshot);try{
    // Calibration displays recorded samples, before EQ, compressor and other audio effects.
    snapshot.Tracks[track].Fx.Audio=new AudioFxR2();string input=Path.Combine(dir,"audio.mkv");
    RenderRangeR3.Prepare(snapshot,track,input,origin,length,0,token,true);
    byte[] raw=Pipe("-i "+Engine.Q(input)+" -vn -ar "+WaveR5.Rate+" -ac 2 -f f32le pipe:1",token);
    var samples=new float[raw.Length/4];Buffer.BlockCopy(raw,0,samples,0,samples.Length*4);
    return new WaveR5{Origin=origin,Shift=snapshot.Tracks[track].Fx.AudioShift,Samples=samples};
   }finally{Clean(dir);}
  }
  // Two separate channels prevent opposite-phase stereo from disappearing through mono averaging.
  public static void Peaks(WaveR5 wave,double a,double b,int channel,out float min,out float max){
   min=max=0;int start=Math.Max(0,(int)Math.Floor((a-wave.Origin)*WaveR5.Rate)),end=Math.Min(wave.Samples.Length/2,(int)Math.Ceiling((b-wave.Origin)*WaveR5.Rate));
   for(int i=start;i<end;i++){float v=wave.Samples[i*2+channel];if(v<min)min=v;if(v>max)max=v;}
  }
  public static MemoryStream Sound(WaveR5 wave,double origin,double length){
   var stream=new MemoryStream();var writer=new BinaryWriter(stream);int count=(int)Math.Round(length*WaveR5.Rate),bytes=count*4;
   writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+bytes);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)2);writer.Write(WaveR5.Rate);writer.Write(WaveR5.Rate*4);writer.Write((short)4);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(bytes);
   int first=(int)Math.Round((origin-wave.Origin)*WaveR5.Rate);
   for(int i=0;i<count;i++)for(int c=0;c<2;c++){int index=(first+i)*2+c;float value=index>=0&&index<wave.Samples.Length?wave.Samples[index]:0;writer.Write((short)Math.Round(Math.Max(-1,Math.Min(1,value))*32767));}
   writer.Flush();stream.Position=0;return stream;
  }
 }
}
