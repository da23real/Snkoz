using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Synkoz {
 public sealed class Cc936 { public int Track940=-1; public double Time; public int Channel,Number,Value; }
 public sealed class Trigger936 { public int Track940=-1; public double Time; public int Channel,Note; }
 public sealed class Beat936 { public double Time,Beat; }
 public enum EffectKind936 { Luminosité,Contraste,Saturation,Teinte,Opacité,Zoom,Animation,Flou,Pixelisation,Déplacement_X,Déplacement_Y,Colorisation,Dégradé,Cycle_couleurs,Inversion,Postérisation,Mute }
 public enum TriggerKind936 { Note,CC,Début_mesure,Timestamp }
 public enum Length936 { Octuple_croche,Quadruple_croche,Double_croche,Croche,Noire,Blanche,Mesure }
 public sealed class TrackFx936 {
  public int PreviewMeasures941=2,PreviewHeight941=240;public bool MirrorX939,MirrorY939; public int Rotation=0;public bool PhoneReference=false;public AudioFxR2 Audio=new AudioFxR2();public bool ImageMode=false,VideoOnly=false; public int Frames=0;public decimal Milliseconds=0;
  public bool Graphic=false;public int CanvasWidth=1920,CanvasHeight=1080;
  public bool UseZones=false;public List<ZoneR3> Zones=new List<ZoneR3>();
  public int PreviewAudioMode=0;public List<AudioChoiceR3> PreviewAudio=new List<AudioChoiceR3>();
  public List<Effect936> Effects=new List<Effect936>();
  public double? VideoDelayR5,AudioDelayR5;
  [ScriptIgnore] public double LegacyShift {get{return ImageMode?Frames/30.0:(double)Milliseconds/1000;}}
  [ScriptIgnore] public double Shift {get{return VideoDelayR5??LegacyShift;}}
  [ScriptIgnore] public double AudioShift {get{return AudioDelayR5??(VideoOnly?0:LegacyShift);}}
 }
 public sealed class Effect936 {
  public double OffsetMs941{get;set;}public bool PixelArt941{get;set;}public int StartImage938{get;set;}public int SyncImage938{get;set;}public int TimestampMeasure938{get;set;}public int TimestampTime938{get;set;}public int TimestampQuarter938{get;set;}
  public int DirectProgram939{get;set;}public int NoteImages939{get;set;}public int CenterNote939{get;set;}public int CenterImage939{get;set;}public int OtherImage939{get;set;}public int[] NoteMap939{get;set;}
  public double CcTail939{get;set;}public bool CenterPosition939{get;set;}
  public bool BodyMask{get;set;}
  public bool Gate{get;set;}public LoopR3 Loop{get;set;}
  public double ZoomX{get;set;}public double ZoomY{get;set;}
  public MaskSideR3 MaskSide{get;set;}public double MaskDepth{get;set;}public double MaskSoftness{get;set;}public double MaskX{get;set;}public double MaskY{get;set;}
  public double CycleBeats{get;set;}public int ColorA{get;set;}public int ColorB{get;set;}public double Angle{get;set;}
  public bool Sweep{get;set;}public double FromX{get;set;}public double FromY{get;set;}public double ToX{get;set;}public double ToY{get;set;}

  public bool CenterMapping{get;set;}public double CcCenter{get;set;}public double EffectCenter{get;set;}public double CcGain{get;set;}

  [Category("1 — Effet"),DisplayName("Actif")] public bool Enabled{get;set;}
  [Category("1 — Effet"),DisplayName("Effet")] public EffectKind936 Kind{get;set;}
  [Category("2 — Déclenchement"),DisplayName("Commande")] public TriggerKind936 Trigger{get;set;}
  [Category("2 — Déclenchement"),DisplayName("Canal"),Description("Canal MIDI 1 à 16. Aucune sélection de port.")] public int Channel{get;set;}
  [Category("2 — Déclenchement"),DisplayName("Note"),Description("-1 = toutes les notes ; sinon 0 à 127.")] public int Note{get;set;}
  [Category("2 — Déclenchement"),DisplayName("Numéro CC")] public int CC{get;set;}
  [Category("3 — Note"),DisplayName("Amplitude"),Description("Écart à la valeur initiale : pourcentage ; teinte en degrés. Opacité : 0 = inchangée, -100 = invisible. Zoom : +50 = 150 %.")] public double Amount{get;set;}
  [Category("3 — Note"),DisplayName("Durée musicale")] public Length936 Length{get;set;}
  [Category("3 — Note"),DisplayName("Attaque (%)"),Description("Part de la durée totale, de 0 à 100. 0 = immédiat. Attaque + retour <= 100.")] public double Attack{get;set;}
  [Category("3 — Note"),DisplayName("Retour (%)")] public double Release{get;set;}
  [Category("4 — CC"),DisplayName("Valeur à CC 0")] public double At0{get;set;}
  [Category("4 — CC"),DisplayName("Valeur à CC 127")] public double At127{get;set;}
  [Category("5 — Animation"),DisplayName("Fichier"),Description("PNG transparent : 4 colonnes × 2 lignes ; ou GIF animé. Importer avec le bouton de la fenêtre FX.")] public string Asset{get;set;}
  [Category("5 — Animation"),DisplayName("Durée par image")] public Length936 FrameLength{get;set;}
  [Category("5 — Animation"),DisplayName("Largeur (%)"),Description("Pourcentage de la largeur de la vidéo. Proportions conservées.")] public double Size{get;set;}
  [Category("5 — Animation"),DisplayName("Position X (%)"),Description("0 = bord gauche ; 100 = bord droit.")] public double X{get;set;}
  [Category("5 — Animation"),DisplayName("Position Y (%)"),Description("0 = haut ; 100 = bas.")] public double Y{get;set;}
  public Effect936(){DirectProgram939=-1;CcTail939=0.5;CenterNote939=60;CenterImage939=1;NoteMap939=new int[12];StartImage938=SyncImage938=TimestampMeasure938=TimestampTime938=1;CycleBeats=4;ZoomX=ZoomY=50;MaskDepth=100;MaskSoftness=10;ColorA=Color.Cyan.ToArgb();ColorB=Color.Magenta.ToArgb();ToX=100;FromY=ToY=50;Enabled=true;CcCenter=64;CcGain=1;Channel=1;Note=-1;Amount=50;At127=50;Length=Length936.Croche;FrameLength=Length936.Double_croche;Size=30;X=Y=50;Asset="";}
  public override string ToString(){return (Enabled?"● ":"○ ")+Kind+(Trigger==TriggerKind936.Note||Trigger==TriggerKind936.CC?" · Ch "+Channel:"")+" · "+(Trigger==TriggerKind936.Timestamp?"Timestamp "+TimestampMeasure938+"·"+TimestampTime938+"·"+TimestampQuarter938:Trigger==TriggerKind936.Début_mesure?"début mesure":Trigger==TriggerKind936.CC?"CC "+CC:Note<0?"toutes notes":"note "+Note);}
  public void Validate(){
   if(NoteImages939<0||NoteImages939>2||CenterNote939<0||CenterNote939>127||CenterImage939<1||OtherImage939<0||NoteMap939==null||NoteMap939.Length!=12||NoteMap939.Any(n=>n<0))throw new Exception("Association note/image invalide.");
   if(StartImage938<1||SyncImage938<1||TimestampMeasure938<1||TimestampTime938<1||TimestampTime938>4||TimestampQuarter938<0||TimestampQuarter938>3)throw new Exception("Repères d’animation ou position musicale invalides.");
   if(Trigger==TriggerKind936.Timestamp&&Kind!=EffectKind936.Animation)throw new Exception("Timestamp est réservé aux animations.");
   if(Kind==EffectKind936.Animation&&Trigger!=TriggerKind936.CC&&SyncImage938<StartImage938)throw new Exception("L’image repère doit suivre ou égaler le départ.");
   if(CcTail939<0||CcTail939>3600||double.IsNaN(CcTail939)||double.IsInfinity(CcTail939))throw new Exception("Prolongement CC : 0 à 3600 secondes.");
   double[] finite={OffsetMs941,Amount,Attack,Release,At0,At127,Size,X,Y,CcCenter,EffectCenter,CcGain,ZoomX,ZoomY,MaskDepth,MaskSoftness,MaskX,MaskY,CycleBeats,Angle,FromX,FromY,ToX,ToY};if(finite.Any(n=>double.IsNaN(n)||double.IsInfinity(n)))throw new Exception("Valeur numérique invalide.");
   if(Channel<1||Channel>16||Note< -1||Note>127||CC<0||CC>127)throw new Exception("Canal : 1..16 ; note : -1 ou 0..127 ; CC : 0..127.");
   if(!Enum.IsDefined(typeof(EffectKind936),Kind)||!Enum.IsDefined(typeof(TriggerKind936),Trigger)||!Enum.IsDefined(typeof(Length936),Length)||!Enum.IsDefined(typeof(Length936),FrameLength))throw new Exception("Type d’effet ou durée inconnus.");
   if(Attack<0||Release<0||Attack+Release>100)throw new Exception("Attaque et retour positifs, somme maximale 100 % de la durée.");
   if(Size<=0||Size>400||X< -999||X>999||Y< -999||Y>999)throw new Exception("Animation : largeur 0..400 %, position −999..999 %.");
   if(CycleBeats<.0625||CycleBeats>64)throw new Exception("Cycle : 1/64 à 16 mesures.");
   if(ZoomX<0||ZoomX>100||ZoomY<0||ZoomY>100||MaskDepth<0||MaskDepth>100||MaskSoftness<0||MaskSoftness>100||Math.Abs(MaskX)>100||Math.Abs(MaskY)>100)throw new Exception("Masque/centre hors limites.");
   if(!Enum.IsDefined(typeof(LoopR3),Loop)||!Enum.IsDefined(typeof(MaskSideR3),MaskSide))throw new Exception("Boucle/contour invalide.");
   if(Gate&&Trigger!=TriggerKind936.Note)throw new Exception("Gate nécessite un déclencheur Note.");
   if(CcCenter<0||CcCenter>127)throw new Exception("Centre CC : 0..127.");
   if(Kind==EffectKind936.Animation&&string.IsNullOrWhiteSpace(Asset))throw new Exception("Importer le PNG 4 × 2 ou le GIF de cette animation.");
  }
 }
 public sealed class Music936 {
  readonly Beat936[] marks;
  public Music936(Montage m){marks=m.Beats.OrderBy(x=>x.Time).GroupBy(x=>x.Time).Select(g=>g.Last()).ToArray();}
  public bool Available{get{return marks.Length>=2;}}
  public double Time(double beat){if(!Available)throw new Exception("Repères musicaux absents.");int lo=0,hi=marks.Length-1;while(lo+1<hi){int mid=(lo+hi)/2;if(marks[mid].Beat<=beat)lo=mid;else hi=mid;}var a=marks[lo];var b=marks[hi];if(b.Beat<=a.Beat)throw new Exception("Repères musicaux non croissants.");return a.Time+(beat-a.Beat)*(b.Time-a.Time)/(b.Beat-a.Beat);}
  public double Beat(double time){if(!Available)throw new Exception("Repères musicaux absents : enregistrer une prise synchronisée avec Tee pour les effets par note.");int lo=0,hi=marks.Length-1;while(lo+1<hi){int mid=(lo+hi)/2;if(marks[mid].Time<=time)lo=mid;else hi=mid;}var a=marks[lo];var b=marks[hi];return a.Beat+(time-a.Time)*(b.Beat-a.Beat)/(b.Time-a.Time);}
  public static double Length(Length936 length){return new[]{0.0625,0.125,0.25,0.5,1.0,2.0,4.0}[(int)length];}
 }
 // Event lists only. Rendering walks sparse MIDI events, never constructs state for every musical tick.
 sealed class EffectRun936:IDisposable {
  public readonly Effect936 Fx;readonly Music936 music;readonly Trigger936[] notes;readonly Cc936[] cc;readonly MidiSpan[] spans;int ni=-1,ci=-1;
  readonly Dictionary<string,Bitmap> scaled941=new Dictionary<string,Bitmap>();long cacheBytes941;readonly AnimationTiming938 animation938;VideoAnimation941 video941;Image asset;FrameDimension dimension;public int Frames;readonly bool sprite;readonly int fw,fh;
  public EffectRun936(Effect936 fx,Montage m,Music936 clock){Fx=fx;music=clock;fx.Validate();spans=m.Notes.Where(n=>n.Channel==fx.Channel-1&&(fx.Note<0||n.Note==fx.Note)).OrderBy(n=>n.Start).ToArray();
   notes=m.Triggers.Where(n=>n.Channel==fx.Channel-1&&(fx.Note<0||n.Note==fx.Note)).OrderBy(n=>n.Time).ToArray();
   if(notes.Length==0)notes=m.Notes.Where(n=>n.Channel==fx.Channel-1&&(fx.Note<0||n.Note==fx.Note)).Select(n=>new Trigger936{Time=n.Start,Note=n.Note,Channel=n.Channel}).OrderBy(n=>n.Time).ToArray();
   cc=m.CC.Where(c=>c.Channel==fx.Channel-1&&c.Number==fx.CC).OrderBy(c=>c.Time).ToArray();
   if(fx.Trigger!=TriggerKind936.CC&&!music.Available)throw new Exception("Cette prise ne contient pas les repères musicaux requis pour les effets par note.");
   if(fx.Kind==EffectKind936.Animation){string path=Store934.MediaPath(m.Folder,fx.Asset);if(Path.GetExtension(path).Equals(".mp4",StringComparison.OrdinalIgnoreCase)){video941=new VideoAnimation941(path);asset=new Bitmap(video941.Frames[0]);}else asset=Image.FromFile(path);sprite=Path.GetExtension(path).Equals(".png",StringComparison.OrdinalIgnoreCase);
    if(sprite){if(asset.Width%4!=0||asset.Height%2!=0){asset.Dispose();throw new Exception("La sprite sheet PNG doit être divisible en 4 colonnes × 2 lignes.");}Frames=8;fw=asset.Width/4;fh=asset.Height/2;}
    else {dimension=FrameDimension.Time;Frames=video941==null?asset.GetFrameCount(dimension):video941.Frames.Count;fw=asset.Width;fh=asset.Height;}
    try{if(fx.Trigger!=TriggerKind936.CC)animation938=new AnimationTiming938(fx,m,music,Frames);}catch{asset.Dispose();if(video941!=null)video941.Dispose();throw;}
   }
  }
  public double Age(double t){while(ni+1<notes.Length&&notes[ni+1].Time<=t)ni++;if(Fx.Trigger==TriggerKind936.Début_mesure){double beat=music.Beat(t);return beat<0?-1:beat-Math.Floor(beat/4)*4;}if(ni<0)return -1;if(Fx.Gate){var span=spans.FirstOrDefault(n=>n.Note==notes[ni].Note&&Math.Abs(n.Start-notes[ni].Time)<.002);if(span==null||t>=span.Start+span.Duration)return -1;}return music.Beat(t)-music.Beat(notes[ni].Time);}
  public double Value(double time){double t=time-Fx.OffsetMs941/1000;if(Fx.Trigger==TriggerKind936.CC){while(ci+1<cc.Length&&cc[ci+1].Time<=t)ci++;return ci<0||t>cc[ci].Time+Fx.CcTail939?0:Fx.CenterMapping?Fx.EffectCenter+(cc[ci].Value-Fx.CcCenter)*Fx.CcGain:Fx.At0+(Fx.At127-Fx.At0)*cc[ci].Value/127.0;}
   double age=Age(t),length=Music936.Length(Fx.Length);if(Fx.Gate&&ni>=0){var span=spans.FirstOrDefault(n=>n.Note==notes[ni].Note&&Math.Abs(n.Start-notes[ni].Time)<.002);if(span!=null)length=music.Beat(span.Start+span.Duration)-music.Beat(span.Start);}if(age<0||age>=length)return 0;double attack=length*Fx.Attack/100,release=length*Fx.Release/100;
   double envelope=attack>0&&age<attack?age/attack:release>0&&age>length-release?(length-age)/release:1;return Fx.Amount*envelope;
  }
  public double MusicalBeat(double t){return music.Available?music.Beat(t):t*2;}
  public double Phase(double t){double a=Age(t-Fx.OffsetMs941/1000);return a<0?0:Math.Max(0,Math.Min(1,a/Music936.Length(Fx.Length)));}
  public int FrameAt(double time){double t=time-Fx.OffsetMs941/1000;if(Fx.Trigger==TriggerKind936.Note&&Fx.NoteImages939!=0){double age=Age(t);if(age<0||(!Fx.Gate&&age>=Music936.Length(Fx.Length)))return -1;int n=notes[ni].Note;int image=Fx.NoteImages939==1?Fx.CenterImage939+n-Fx.CenterNote939:(Fx.NoteMap939[n%12]>0?Fx.NoteMap939[n%12]:Fx.OtherImage939);return image>=1&&image<=Frames?image-1:-1;}if(Fx.Trigger==TriggerKind936.CC){Value(time);return ci<0||t>cc[ci].Time+Fx.CcTail939?-1:(int)Math.Round(Math.Max(0,Math.Min(100,Value(time)))*(Frames-1)/100);}
   return animation938.FrameAt(t);
  }
  public bool Active(double t){return Fx.Kind==EffectKind936.Animation?FrameAt(t)>=0:Math.Abs(Value(t))>.000001;}
  public void Overlay(Graphics g,int width,int height,double t){int frame=FrameAt(t);if(frame<0)return;
   Image currentAsset=video941!=null?video941.Frames[frame]:asset;float w=(float)(width*Fx.Size/100),h=w*fh/fw;var dest=new RectangleF((float)(Fx.CenterPosition939?width*Fx.X/100-w/2:(width-w)*Fx.X/100),(float)(Fx.CenterPosition939?height*Fx.Y/100-h/2:(height-h)*Fx.Y/100),w,h);
   var src=sprite?new Rectangle((frame%4)*fw,(frame/4)*fh,fw,fh):new Rectangle(0,0,fw,fh);int cw=Math.Max(1,(int)Math.Round(w)),ch=Math.Max(1,(int)Math.Round(h));string key=frame+":"+cw+":"+ch+":"+Fx.PixelArt941;Bitmap cached;if(!scaled941.TryGetValue(key,out cached)){if(video941==null&&!sprite)asset.SelectActiveFrame(dimension,frame);long bytes=(long)cw*ch*4;if(bytes>32*1024*1024){g.DrawImage(currentAsset,dest,src,GraphicsUnit.Pixel);return;}if(cacheBytes941+bytes>32*1024*1024){foreach(var entry in scaled941.Values)entry.Dispose();scaled941.Clear();cacheBytes941=0;}cached=new Bitmap(cw,ch,PixelFormat.Format32bppArgb);using(var cg=Graphics.FromImage(cached)){cg.CompositingMode=CompositingMode.SourceCopy;cg.InterpolationMode=Fx.PixelArt941?InterpolationMode.NearestNeighbor:InterpolationMode.HighQualityBicubic;cg.PixelOffsetMode=PixelOffsetMode.Half;cg.DrawImage(currentAsset,new Rectangle(0,0,cw,ch),src,GraphicsUnit.Pixel);}scaled941[key]=cached;cacheBytes941+=bytes;}g.DrawImageUnscaled(cached,(int)Math.Round(dest.X),(int)Math.Round(dest.Y));
  }
  public void Dispose(){foreach(var frame in scaled941.Values)frame.Dispose();scaled941.Clear();if(video941!=null)video941.Dispose();if(asset!=null)asset.Dispose();}
 }
 public static class FxRender936 {
  static float Limit(double value,double min,double max){return (float)Math.Max(min,Math.Min(max,value));}
  static ColorMatrix Matrix(EffectKind936 kind,double amount){
   var m=new ColorMatrix();float v=(float)amount;
   if(kind==EffectKind936.Luminosité)m.Matrix40=m.Matrix41=m.Matrix42=Limit(v/100,-1,1);
   if(kind==EffectKind936.Contraste){float k=Limit(1+v/100,0,10);m.Matrix00=m.Matrix11=m.Matrix22=k;m.Matrix40=m.Matrix41=m.Matrix42=(1-k)/2;}
   if(kind==EffectKind936.Opacité)m.Matrix33=Limit(1+v/100,0,1);
   if(kind==EffectKind936.Saturation){float s=Limit(1+v/100,0,10),r=.2126f*(1-s),g=.7152f*(1-s),b=.0722f*(1-s);m=new ColorMatrix(new[]{new[]{r+s,r,r,0f,0f},new[]{g,g+s,g,0f,0f},new[]{b,b,b+s,0f,0f},new[]{0f,0f,0f,1f,0f},new[]{0f,0f,0f,0f,1f}});}
   if(kind==EffectKind936.Teinte){float c=(float)Math.Cos(v*Math.PI/180),s=(float)Math.Sin(v*Math.PI/180);m=new ColorMatrix(new[]{
    new[]{.213f+.787f*c-.213f*s,.213f-.213f*c+.143f*s,.213f-.213f*c-.787f*s,0f,0f},
    new[]{.715f-.715f*c-.715f*s,.715f+.285f*c+.140f*s,.715f-.715f*c+.715f*s,0f,0f},
    new[]{.072f-.072f*c+.928f*s,.072f-.072f*c-.283f*s,.072f+.928f*c+.072f*s,0f,0f},
    new[]{0f,0f,0f,1f,0f},new[]{0f,0f,0f,0f,1f}});}
   return m;
  }
  internal static Bitmap Apply(Bitmap source,EffectRun936 run,double time){var fx=run.Fx;int w=source.Width,h=source.Height;if(fx.Kind==EffectKind936.Animation){using(var g=Graphics.FromImage(source)){g.InterpolationMode=InterpolationMode.HighQualityBicubic;run.Overlay(g,w,h,time);}return source;}
   double value=run.Value(time);if(Math.Abs(value)<.000001)return source;if(fx.Kind>=EffectKind936.Colorisation&&fx.Kind<EffectKind936.Mute)return ChromaticR3.Apply(source,run,time,value);if(fx.Kind==EffectKind936.Flou||fx.Kind==EffectKind936.Pixelisation)return PixelsR2.Filter(source,fx.Kind,value);var result=new Bitmap(w,h,PixelFormat.Format32bppArgb);
   using(var g=Graphics.FromImage(result)){g.CompositingMode=CompositingMode.SourceCopy;g.InterpolationMode=InterpolationMode.HighQualityBicubic;
    if(fx.Kind==EffectKind936.Déplacement_X||fx.Kind==EffectKind936.Déplacement_Y){int dx=fx.Kind==EffectKind936.Déplacement_X?(int)Math.Round(w*value/100):0,dy=fx.Kind==EffectKind936.Déplacement_Y?(int)Math.Round(h*value/100):0;g.DrawImageUnscaled(source,dx,dy);}
    else if(fx.Kind==EffectKind936.Zoom){float factor=Limit(1+value/100,.05,10);float rw=w*factor,rh=h*factor;g.DrawImage(source,new RectangleF((float)((w-rw)*fx.ZoomX/100),(float)((h-rh)*fx.ZoomY/100),rw,rh));}
    else using(var attributes=new ImageAttributes()){attributes.SetColorMatrix(Matrix(fx.Kind,value));g.DrawImage(source,new Rectangle(0,0,w,h),0,0,w,h,GraphicsUnit.Pixel,attributes);}
   }source.Dispose();return result;
  }
  static void Finish(Process process,Task<string> error){if(!process.WaitForExit(60000)){process.Kill();throw new Exception("Traitement média sans réponse.");}if(process.ExitCode!=0)throw new Exception(error.GetAwaiter().GetResult());}
  internal static string AudioFilter(TrackFx936 fx,double duration){double shift=fx.AudioShift;long samples=(long)Math.Round(shift*48000);
   return "aresample=48000,"+(shift<0?"atrim=start_sample="+(-samples)+",asetpts=PTS-STARTPTS,":"adelay="+samples+"S:all=1,")+"apad,atrim=duration="+duration.ToString("F9",Engine.Inv)+AudioFxR2.Filter(fx.Audio);}
  public static void Render(Montage m,int track,string input,string output,CancellationToken cancel,Action<int> progress,double origin=0,double limit=-1,int previewHeight=0,bool referenceAudio=false){
   var t=m.Tracks[track];var fx=t.Fx??new TrackFx936();var info=Engine.ProbeMedia(input);double renderDuration=limit<0?m.Duration:limit;string duration=renderDuration.ToString("F9",Engine.Inv);
   if(!info.Video){RenderRangeR3.Run("-hide_banner -y -i "+Engine.Q(input)+" "+(Path.GetExtension(output).Equals(".mp3",StringComparison.OrdinalIgnoreCase)?"-c:a libmp3lame -q:a 2 ":"-c:a pcm_s16le ")+Engine.Q(output),cancel);return;}
   var clock=new Music936(m);var runs=new List<EffectRun936>();var blockRuns=new Dictionary<MediaBlock,List<EffectRun936>>();
   try{foreach(var effect in (m.Blocks.Any(b=>b.Track==track)?new List<Effect936>():fx.Effects).Where(e=>e.Enabled))runs.Add(new EffectRun936(effect,m,clock));
    var relevantBlocks=m.Blocks.Where(b=>b.Track==track&&b.Start<origin+renderDuration&&b.Start+b.Duration>origin).ToArray();
    int rotation=OrientationR2.Degrees(t);int w=rotation%180==0?info.Width:info.Height,h=rotation%180==0?info.Height:info.Width,size=checked(w*h*4);byte[] bytes=new byte[size];double shift=fx.Shift;
    string decodeInput="-i "+Engine.Q(input)+" -an "+(OrientationR2.Filter(t).Length==0?"":"-vf "+Engine.Q(OrientationR2.Filter(t).TrimStart(','))+" ");
    bool alpha=fx.Graphic&&Path.GetExtension(output).Equals(".mov",StringComparison.OrdinalIgnoreCase);
    using(var body=runs.Any(r=>r.Fx.BodyMask&&r.Fx.Kind!=EffectKind936.Mute)||relevantBlocks.Any(b=>(b.Fx939??fx).Effects.Any(e=>e.Enabled&&e.BodyMask&&e.Kind!=EffectKind936.Mute))?new BodyMaskR2(cancel):null)
    using(var decode=Engine.Create(Engine.FF,"-hide_banner -loglevel error "+decodeInput+"-f rawvideo -pix_fmt bgra pipe:1"))
    using(var encode=Engine.Create(Engine.EncodingFF(t.Encoder),"-hide_banner -loglevel error -y -f rawvideo -pix_fmt bgra -s "+w+"x"+h+" -r 30 -i pipe:0 "+(info.Audio?"-i "+Engine.Q(input)+" -map 0:v -map 1:a -c:a aac -b:a 192k ":"-map 0:v ")+(alpha?"-c:v prores_ks -profile:v 4 -pix_fmt yuva444p10le -alpha_bits 16 ":Engine.VideoEncoder(t.Encoder,false))+"-shortest -t "+duration+" -movflags +faststart "+Engine.Q(output))){
     decode.Start();var de=decode.StandardError.ReadToEndAsync();try{encode.Start();}catch(Exception ex){try{decode.Kill();decode.WaitForExit();}catch{}cancel.ThrowIfCancellationRequested();if(t.Encoder=="nvenc"&&!alpha)throw new NvidiaExportExceptionR1(ex.Message,ex);throw;}var ee=encode.StandardError.ReadToEndAsync();var eo=encode.StandardOutput.ReadToEndAsync();
     var partial942=PartialExport942.State;using(cancel.Register(()=>{if(partial942!=null&&partial942.Keep)return;try{if(!decode.HasExited)decode.Kill();}catch{}try{if(!encode.HasExited)encode.Kill();}catch{}}))try{
      int frame=0,last=-1;bool kept942=false;try{while(true){if(cancel.IsCancellationRequested&&PartialExport942.Keeping&&frame>0){kept942=true;break;}cancel.ThrowIfCancellationRequested();int count=0;while(count<size){int read=decode.StandardOutput.BaseStream.Read(bytes,count,size-count);if(read==0)break;count+=read;}if(count==0)break;if(count!=size)throw new Exception("Image décodée incomplète.");
       Bitmap bitmap=new Bitmap(w,h,PixelFormat.Format32bppArgb);try{var data=bitmap.LockBits(new Rectangle(0,0,w,h),ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);try{Marshal.Copy(bytes,0,data.Scan0,size);}finally{bitmap.UnlockBits(data);}double time=origin+frame/30.0;
        var activeBlock=m.Blocks.FirstOrDefault(b=>b.Track==track&&time>=b.Start&&time<b.Start+b.Duration);if(activeBlock!=null&&!blockRuns.ContainsKey(activeBlock)){foreach(var old in blockRuns.Values.SelectMany(x=>x))old.Dispose();blockRuns.Clear();var scoped=BlockRules939.Events(m,activeBlock);var localClock=new Music936(scoped);var localRuns=new List<EffectRun936>();blockRuns[activeBlock]=localRuns;foreach(var effect in (activeBlock.Fx939??fx).Effects.Where(e=>e.Enabled))localRuns.Add(new EffectRun936(effect,scoped,localClock));}var activeRuns=activeBlock!=null&&blockRuns.ContainsKey(activeBlock)?blockRuns[activeBlock]:runs;bool allowed=!Mute940.At(m,track,time);if(!allowed){using(var g=Graphics.FromImage(bitmap))g.Clear(fx.Graphic?Color.Transparent:Color.Black);}double strength=allowed?1:0;
        byte[] mask=allowed&&body!=null&&activeRuns.Any(r=>r.Fx.BodyMask&&r.Fx.Kind!=EffectKind936.Mute&&r.Active(time))?body.Mask(bitmap):null;
        foreach(var run in activeRuns){if(run.Fx.Kind==EffectKind936.Mute){if(allowed)strength*=1-Math.Max(0,Math.Min(100,run.Value(time)))/100;continue;}
         if(strength<=0||!run.Active(time))continue;
         if(!run.Fx.BodyMask&&strength>=1)bitmap=Apply(bitmap,run,time);
         else using(var before=(Bitmap)bitmap.Clone()){bitmap=Apply(bitmap,run,time);if(run.Fx.BodyMask){var shaped=MaskR3.Shape(mask,run.Fx);PixelsR2.Blend(before,bitmap,shaped);}if(strength<1)ChromaticR3.Mix(before,bitmap,strength);}
        }

        // Composite alpha on black once, after all ordered effects and transparent overlays.
        if(alpha){data=bitmap.LockBits(new Rectangle(0,0,w,h),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);try{Marshal.Copy(data.Scan0,bytes,0,size);}finally{bitmap.UnlockBits(data);}}else using(var opaque=new Bitmap(w,h,PixelFormat.Format32bppArgb)){using(var g=Graphics.FromImage(opaque)){g.Clear(Color.Black);g.DrawImageUnscaled(bitmap,0,0);}data=opaque.LockBits(new Rectangle(0,0,w,h),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);try{Marshal.Copy(data.Scan0,bytes,0,size);}finally{opaque.UnlockBits(data);}}
        try{encode.StandardInput.BaseStream.Write(bytes,0,size);}catch(IOException ex){cancel.ThrowIfCancellationRequested();if(t.Encoder=="nvenc"&&!alpha){string detail=encode.WaitForExit(2000)?ee.GetAwaiter().GetResult():ex.Message;throw new NvidiaExportExceptionR1(detail,ex);}throw;}
       }finally{bitmap.Dispose();}frame++;int percent=(int)Math.Min(99,frame*100/(renderDuration*30));if(percent!=last){last=percent;if(progress!=null)progress(percent);}
      }
      }catch(Exception){if(!cancel.IsCancellationRequested||!PartialExport942.Keeping||frame==0)throw;kept942=true;}try{encode.StandardInput.Close();}catch(IOException ex){cancel.ThrowIfCancellationRequested();if(t.Encoder=="nvenc"&&!alpha)throw new NvidiaExportExceptionR1(ex.Message,ex);throw;}if(kept942){try{decode.Kill();decode.WaitForExit();}catch{}}else Finish(decode,de);try{Finish(encode,ee);}catch(Exception ex){cancel.ThrowIfCancellationRequested();if(t.Encoder=="nvenc"&&!alpha)throw new NvidiaExportExceptionR1(ex.Message,ex);throw;}if(frame==0)throw new Exception("Aucune image rendue.");if(kept942)PartialExport942.State.Seconds=frame/30.0;if(progress!=null)progress(100);
     }finally{try{if(!decode.HasExited)decode.Kill();decode.WaitForExit();de.GetAwaiter().GetResult();}catch{}try{if(!encode.HasExited)encode.Kill();encode.WaitForExit();ee.GetAwaiter().GetResult();eo.GetAwaiter().GetResult();}catch{}}
    }
   }finally{foreach(var run in runs.Concat(blockRuns.Values.SelectMany(x=>x)))run.Dispose();}
  }
 }
 public static partial class Engine {
  public static Montage Snapshot936(Montage m){var s=new JavaScriptSerializer{MaxJsonLength=int.MaxValue};var result=s.Deserialize<Montage>(s.Serialize(m));result.Folder=m.Folder;result.DirectMode939=m.DirectMode939;result.LiveFrame939=m.LiveFrame939;return result;}
  public static void Export(Montage m,int track,string output){Export936(Snapshot936(m),track,output,CancellationToken.None,null);}
  public static void Export936(Montage m,int track,string output,CancellationToken cancel,Action<int> progress,double origin=0,double limit=-1,int previewHeight=0,bool referenceAudio=false){
   string full=Path.GetFullPath(output),root=Path.GetFullPath(m.Folder).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;
   if(full.StartsWith(root,StringComparison.OrdinalIgnoreCase)&&File.Exists(full))throw new Exception("Choisir un nouveau fichier pour conserver les médias et les bruts de cette prise.");
   var t=m.Tracks[track];string work=Path.Combine(Path.GetDirectoryName(full),".Snkoz_FX_"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(work);
   try{double length=limit<0?m.Duration-origin:Math.Min(limit,m.Duration-origin);if(origin<PrerollR4.Minimum(m)-.000001||length<=0)throw new Exception("Zone de rendu invalide.");
    if(progress!=null)progress(1);string baseFile=Path.Combine(work,"base.mkv");cancel.ThrowIfCancellationRequested();RenderRangeR3.Prepare(m,track,baseFile,origin,length,previewHeight,cancel,false,false,p=>{if(progress!=null)progress(Math.Max(1,p/5));});
    string rendered=Path.Combine(work,"rendu"+Path.GetExtension(full));FxRender936.Render(m,track,baseFile,rendered,cancel,p=>{if(progress!=null)progress(20+(int)(p*.74));},origin,length,previewHeight);
    if(PartialExport942.Keeping&&PartialExport942.State.Seconds>0&&!PartialExport942.State.Composite)PartialExport942.Save(rendered,full,PartialExport942.State.Seconds);
    if(referenceAudio)rendered=RenderRangeR3.ReferenceAudio(m,track,rendered,work,origin,length,cancel);
    cancel.ThrowIfCancellationRequested();if(ProbeMedia(rendered).Duration<=0)throw new Exception("Rendu vide.");File.Copy(rendered,full,true);if(progress!=null)progress(100);
   }finally{Directory.Delete(work,true);}
  }
 }
}

