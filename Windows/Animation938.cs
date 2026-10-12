using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
namespace Synkoz {
 public sealed class AnimationAsset938:IDisposable {
  readonly VideoAnimation941 video941;readonly Image image;readonly bool sprite;public readonly int Count,Width,Height;
  public AnimationAsset938(string path){if(Path.GetExtension(path).Equals(".mp4",StringComparison.OrdinalIgnoreCase)){video941=new VideoAnimation941(path);Count=video941.Frames.Count;Width=video941.Width;Height=video941.Height;return;}image=Image.FromFile(path);try{sprite=Path.GetExtension(path).Equals(".png",StringComparison.OrdinalIgnoreCase);if(sprite){if(image.Width%4!=0||image.Height%2!=0)throw new Exception("PNG : grille régulière 4 × 2 requise.");Count=8;Width=image.Width/4;Height=image.Height/2;}else{Count=image.GetFrameCount(FrameDimension.Time);Width=image.Width;Height=image.Height;}}catch{image.Dispose();throw;}}
  public Bitmap Frame(int index){if(index<0||index>=Count)throw new Exception("Image absente de cette animation.");if(video941!=null)return new Bitmap(video941.Frames[index]);if(!sprite)image.SelectActiveFrame(FrameDimension.Time,index);var b=new Bitmap(Width,Height,PixelFormat.Format32bppArgb);using(var g=Graphics.FromImage(b)){g.CompositingMode=CompositingMode.SourceCopy;g.DrawImage(image,new Rectangle(0,0,Width,Height),sprite?new Rectangle(index%4*Width,index/4*Height,Width,Height):new Rectangle(0,0,Width,Height),GraphicsUnit.Pixel);}return b;}
  public void Dispose(){if(video941!=null)video941.Dispose();if(image!=null)image.Dispose();}
 }
 public static class AnimationRules938 {
  public static double TimestampBeat(Effect936 e){return ((double)e.TimestampMeasure938-1)*4+e.TimestampTime938-1+(e.TimestampTickR11>=0?e.TimestampTickR11/96.0:e.TimestampQuarter938/4.0);}
  public static double Advance(Effect936 e){return !e.ImagesR7.Enabled&&e.Kind==EffectKind936.Animation&&e.Trigger!=TriggerKind936.CC&&e.NoteImages939==0?(e.SyncImage938-e.StartImage938)*Music936.Length(e.FrameLength):0;}
  public static void ValidateFrames(Effect936 e,int count){e.ImagesR7.ValidateFrames(count);if(e.ImagesR7.Enabled)return;if(e.Trigger==TriggerKind936.CC)return;if(e.StartImage938>count||e.SyncImage938>count)throw new Exception("Les images de départ et de repère doivent exister dans l’animation (1 à "+count+").");if(e.SyncImage938<e.StartImage938)throw new Exception("L’image repère doit être égale ou postérieure à l’image de départ.");}
  public static void ValidateAsset(Montage m,Effect936 e){if(e.Kind!=EffectKind936.Animation||(!e.BanksR16.Enabled&&string.IsNullOrWhiteSpace(e.Asset)))return;if(e.BanksR16.Enabled){using(var banks=new BankRunR16(e.BanksR16,m.Folder)){}return;}using(var asset=new AnimationAsset938(Store934.MediaPath(m.Folder,e.Asset)))ValidateFrames(e,asset.Count);}
  public static string Summary(Effect936 e){if(e.Trigger==TriggerKind936.CC)return "CC : toutes les images, repères temporels inactifs.";double advance=Advance(e);return "Départ : image "+e.StartImage938+" · Au déclenchement : image "+e.SyncImage938+"\nAnticipation : "+advance.ToString("0.####")+" temps (noires)."+(!e.Gate&&advance>=Music936.Length(e.Length)?"\nAttention : durée trop courte pour atteindre l’image repère.":"");}
 }
 // Sparse event anchors in musical beats. Binary lookup also supports preview seeks backwards.
 sealed class AnimationTiming938 {internal Func<double,double,double> SpeedAgeTbo15;
  struct Cue {public double Beat,End;}
  readonly bool video941;readonly Effect936 fx;readonly Music936 music;readonly Cue[] cues;readonly double advance;readonly int count;
  public AnimationTiming938(Effect936 effect,Montage m,Music936 clock,int frames){fx=effect;video941=Path.GetExtension(fx.Asset).Equals(".mp4",StringComparison.OrdinalIgnoreCase);music=clock;count=frames;advance=AnimationRules938.Advance(fx);AnimationRules938.ValidateFrames(fx,count);
   if(fx.Trigger==TriggerKind936.Timestamp){cues=new[]{new Cue{Beat=clock.Beat(m.EditorOriginTbo15)+AnimationRules938.TimestampBeat(fx)}};return;}
   if(fx.Trigger==TriggerKind936.Début_mesure){cues=new Cue[0];return;}
   var notes=EventsR9.Notes(m,fx);
   var spans=m.Notes.Where(n=>EventsR9.Channel(fx,n.Channel,n.Track940)&&(fx.Note<0||n.Note==fx.Note)).OrderBy(n=>n.Start).ToArray();var result=new List<Cue>();
   foreach(var note in notes){double end=double.PositiveInfinity;if(fx.Gate){int lo=0,hi=spans.Length;while(lo<hi){int mid=(lo+hi)/2;if(spans[mid].Start<note.Time-.002)lo=mid+1;else hi=mid;}MidiSpan matched=null;for(int i=lo;i<spans.Length&&spans[i].Start<=note.Time+.002;i++)if(spans[i].Channel==note.Channel&&spans[i].Note==note.Note&&(note.Track940<0||spans[i].Track940==note.Track940)){matched=spans[i];break;}if(matched==null)continue;end=music.Beat(matched.Start+matched.Duration);}
    result.Add(new Cue{Beat=music.Beat(note.Time),End=end});
   }cues=result.ToArray();
  }
  bool WindowR9(double time,out double start,out double finish){start=finish=0;double beat=music.Beat(time),anchor,end=double.PositiveInfinity;double leadSeconds=(fx.SyncImage938-fx.StartImage938)/15.0;Func<double,double> begin=b=>video941?music.Time(b)-leadSeconds:music.Time(b-advance);
   if(fx.Trigger==TriggerKind936.Début_mesure){anchor=Math.Floor(((video941?music.Beat(time+leadSeconds):beat+advance)+1e-9)/4)*4;if(anchor<0)return false;}
   else{int lo=0,hi=cues.Length;while(lo<hi){int mid=(lo+hi)/2;if(begin(cues[mid].Beat)<=time+1e-9)lo=mid+1;else hi=mid;}if(lo==0)return false;anchor=cues[lo-1].Beat;end=cues[lo-1].End;}
   start=begin(anchor);double startBeat=music.Beat(start);finish=fx.Gate?music.Time(end):video941&&fx.Loop==LoopR3.Aucune?double.PositiveInfinity:music.Time(startBeat+Music936.Length(fx.Length));
   if(fx.Loop==LoopR3.Aucune&&(SpeedAgeTbo15==null||video941))finish=Math.Min(finish,video941?start+(count-fx.StartImage938+1)/15.0:music.Time(startBeat+(count-fx.StartImage938+1)*Music936.Length(fx.FrameLength)));
   return time>=start-1e-9&&time<finish-1e-9;
  }
  public double OpacityR9(double time){double start,end;if(!WindowR9(time,out start,out end)||end<=start)return 0;double p=(time-start)/(end-start),attack=fx.Attack/100,release=fx.Release/100;return Math.Max(0,Math.Min(1,attack>0&&p<attack?p/attack:release>0&&p>1-release?(1-p)/release:1));}
  public int FrameAt(double time){double begin,end;if(!WindowR9(time,out begin,out end))return -1;double age=SpeedAgeTbo15==null?music.Beat(time)-music.Beat(begin):SpeedAgeTbo15(begin,time);long raw=(long)Math.Floor(Math.Max(0,video941?(time-begin)*15:age/Music936.Length(fx.FrameLength))+1e-9);int start=fx.StartImage938-1,length=count-start;
   if(fx.Loop==LoopR3.Aucune)return raw<length?start+(int)raw:-1;
   if(fx.Loop==LoopR3.Retour_début)return start+(int)(raw%length);
   if(length==1)return start;long cycle=2L*(length-1),position=raw%cycle;return start+(int)(position<length?position:cycle-position);
  }
 }
}

