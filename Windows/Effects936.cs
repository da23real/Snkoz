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
 public sealed class Cc936 { public int Bits15=7; public bool StateOnlyR9; public int Track940=-1; public double Time; public int Channel,Number,Value; }
 public sealed class Trigger936 { public int Delay07,Repeat07; public int VelocityA01=-1; public int Track940=-1; public double Time; public int Channel,Note; }
 public sealed class Beat936 { public double Time,Beat; }
 public enum EffectKind936 { Luminosité,Contraste,Saturation,Teinte,Opacité,Zoom,Animation,Flou,Pixelisation,Déplacement_X,Déplacement_Y,Colorisation,Dégradé,Cycle_couleurs,Inversion,Postérisation,Mute,Gamma,Température,Seuil,Rotation,Étirement_X,Étirement_Y,Secousse,Netteté,Grain,Séparation_RVB,Onde,Rémanence,Échos,Traînée_lumineuse,Révélation,Texte,Placement_2D,Mouvement }
 public enum TriggerKind936 { Note,CC,Début_mesure,Timestamp,CC_As_note }
 public enum Length936 { Octuple_croche,Quadruple_croche,Double_croche,Croche,Noire,Blanche,Mesure,Tick }
 public sealed partial class TrackFx936 {
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
 public sealed partial class Effect936 {
  public int TimestampTickR11=-1;public double OffsetMs941{get;set;}public bool PixelArt941{get;set;}public int StartImage938{get;set;}public int SyncImage938{get;set;}public int TimestampMeasure938{get;set;}public int TimestampTime938{get;set;}public int TimestampQuarter938{get;set;}
  public int DirectProgram939{get;set;}public int NoteImages939{get;set;}public int CenterNote939{get;set;}public int CenterImage939{get;set;}public int OtherImage939{get;set;}public int[] NoteMap939{get;set;}
  public double CcTail939{get;set;}public bool CenterPosition939{get;set;}
  public bool BodyMask{get;set;}
  internal Effect936 MaskCopyA01(){return (Effect936)MemberwiseClone();}
  public ValueSourceA01 ValueSourceA01{get;set;}public ValueTargetA01 ValueTargetA01{get;set;}public int InputMinA01{get;set;}public int InputMaxA01{get;set;}public bool HoldCcA01{get;set;}public bool SegmentA01{get;set;}public double ContourPositionA01{get;set;}public double ContourLengthA01{get;set;}public int ContourBandsA01{get;set;}
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
  public string IdR8{get;set;}public string ParentR8{get;set;}public bool ZoneR8{get;set;}public bool EllipseR8{get;set;}public bool InvertR8{get;set;}public double ZoneXr8{get;set;}public double ZoneYr8{get;set;}public double ZoneWr8{get;set;}public double ZoneHr8{get;set;}public double FeatherR8{get;set;}
  public bool ChoiceStateR2;public bool TriggerChosenR2,CcChosenR2,CcValueChosenR2;public bool KeepAspectR2=true;public double StretchYR2=1;public int ChannelsR9{get;set;}public int CcValueR9{get;set;}public bool ConditionR9{get;set;}public int ConditionCcR9{get;set;}public int ConditionValueR9{get;set;}public bool FadeR9{get;set;}
  public Effect936(){Release=50;ConditionCcR9=27;IdR8=Guid.NewGuid().ToString("N");ZoneXr8=ZoneYr8=50;ZoneWr8=ZoneHr8=100;FeatherR8=10;InputMaxA01=127;ContourLengthA01=25;ContourBandsA01=1;DirectProgram939=-1;CcTail939=0.5;CenterNote939=60;CenterImage939=1;NoteMap939=new int[12];StartImage938=SyncImage938=TimestampMeasure938=TimestampTime938=1;CycleBeats=4;ZoomX=ZoomY=50;MaskDepth=100;MaskSoftness=10;ColorA=Color.Cyan.ToArgb();ColorB=Color.Magenta.ToArgb();ToX=100;FromY=ToY=50;Enabled=true;CcCenter=64;CcGain=1;Channel=0;Note=-1;Amount=50;At127=50;Length=Length936.Croche;FrameLength=Length936.Double_croche;Size=30;X=Y=50;Asset="";}
  public override string ToString(){return (string.IsNullOrEmpty(ParentR8)?"":"    ↳ ")+(Enabled?"● ":"○ ")+Kind+(Trigger==TriggerKind936.Note||Trigger==TriggerKind936.CC||Trigger==TriggerKind936.CC_As_note?" · Ch "+string.Join(",",Enumerable.Range(0,32).Where(c=>EventsR9.Channel(this,c<16?c:0,c>=16?c+1:-1)).Select(c=>(c+1).ToString())):"")+" · "+(Trigger==TriggerKind936.Timestamp?"Timestamp "+TimestampMeasure938+"·"+TimestampTime938+"·"+TimestampQuarter938:Trigger==TriggerKind936.Début_mesure?"début mesure":Trigger==TriggerKind936.CC_As_note?"CC "+CC+" = "+CcValueR9+" As note":Trigger==TriggerKind936.CC?"CC "+CC:Note<0?"toutes notes":"note "+Note);}
  public void Validate(){ValidateR20();if(new[]{PlacementAngleR19,MoveCenterXR19,MoveCenterYR19,MoveAmplitudeXR19,MoveAmplitudeYR19}.Any(v=>double.IsNaN(v)||double.IsInfinity(v)))throw new Exception("Placement ou mouvement invalide.");if(BanksR16==null)throw new Exception("Banques absentes.");BanksR16.Validate();if(ImagesR7==null)throw new Exception("Réglages images absents.");ImagesR7.Validate();Validate15();if(StretchYR2<.1||StretchYR2>5||double.IsNaN(StretchYR2))throw new Exception("Hauteur animation invalide.");if(Kind==EffectKind936.Texte){if(TextR12==null)throw new Exception("Réglages texte absents.");TextR12.Validate();}if(CcValueR9<0||CcValueR9>127||ConditionCcR9<0||ConditionCcR9>127||ConditionValueR9<0||ConditionValueR9>127)throw new Exception("Canaux ou condition CC invalides.");if(ZoneXr8<0||ZoneXr8>100||ZoneYr8<0||ZoneYr8>100||ZoneWr8<=0||ZoneWr8>200||ZoneHr8<=0||ZoneHr8>200||FeatherR8<0||FeatherR8>100)throw new Exception("Zone hors limites.");
   if(!Enum.IsDefined(typeof(ValueSourceA01),ValueSourceA01)||!Enum.IsDefined(typeof(ValueTargetA01),ValueTargetA01)||InputMinA01<0||InputMaxA01>127||InputMinA01>=InputMaxA01||ContourLengthA01<1||ContourLengthA01>100||ContourBandsA01<1||ContourBandsA01>8)throw new Exception("Paramètres MIDI/contour invalides.");
   if(NoteImages939<0||NoteImages939>3||CenterNote939<0||CenterNote939>127||CenterImage939<1||OtherImage939<0||NoteMap939==null||NoteMap939.Length!=12||NoteMap939.Any(n=>n<0))throw new Exception("Association note/image invalide.");
   if(StartImage938<1||SyncImage938<1||TimestampMeasure938<1||TimestampTime938<1||TimestampTime938>4||TimestampQuarter938<0||TimestampQuarter938>3||TimestampTickR11< -1||TimestampTickR11>95)throw new Exception("Repères d’animation ou position musicale invalides.");
   if(Trigger==TriggerKind936.Timestamp&&Kind!=EffectKind936.Animation&&Kind!=EffectKind936.Texte)throw new Exception("Timestamp est réservé aux animations et aux textes.");
   if(Kind==EffectKind936.Animation&&Trigger!=TriggerKind936.CC&&SyncImage938<StartImage938)throw new Exception("L’image repère doit suivre ou égaler le départ.");
   if(CcTail939<0||CcTail939>3600||double.IsNaN(CcTail939)||double.IsInfinity(CcTail939))throw new Exception("Prolongement CC : 0 à 3600 secondes.");
   if(DelayShape07<0||DelayShape07>3||DelaySlices07<1||DelaySlices07>128||DelayFilter07<0||DelayFilter07>4||DelayDiameter07<0||DelayDiameter07>100||double.IsNaN(DelayDiameter07))throw new Exception("Placement 2D invalide.");
   double[] finite={ZoneXr8,ZoneYr8,ZoneWr8,ZoneHr8,FeatherR8,ContourPositionA01,ContourLengthA01,OffsetMs941,Amount,Attack,Release,At0,At127,Size,X,Y,CcCenter,EffectCenter,CcGain,ZoomX,ZoomY,MaskDepth,MaskSoftness,MaskX,MaskY,CycleBeats,Angle,FromX,FromY,ToX,ToY};if(finite.Any(n=>double.IsNaN(n)||double.IsInfinity(n)))throw new Exception("Valeur numérique invalide.");
   if(Channel<0||Channel>32||Note< -1||Note>127||CC<0||CC>127)throw new Exception("Canal : 0 (aucun) ou 1..32 ; note : -1 ou 0..127 ; CC : 0..127.");
   if(!Enum.IsDefined(typeof(EffectKind936),Kind)||!Enum.IsDefined(typeof(TriggerKind936),Trigger)||!Enum.IsDefined(typeof(Length936),Length)||!Enum.IsDefined(typeof(Length936),FrameLength))throw new Exception("Type d’effet ou durée inconnus.");
   if(Attack<0||Release<0||Attack+Release>100)throw new Exception("Attaque et retour positifs, somme maximale 100 % de la durée.");
   if(Size<=0||Size>400||X< -999||X>999||Y< -999||Y>999)throw new Exception("Animation : largeur 0..400 %, position −999..999 %.");
   if(CycleBeats<.0625||CycleBeats>64)throw new Exception("Cycle : 1/64 à 16 mesures.");
   if(ZoomX<0||ZoomX>100||ZoomY<0||ZoomY>100||MaskDepth<0||MaskDepth>100||MaskSoftness<0||MaskSoftness>100||Math.Abs(MaskX)>100||Math.Abs(MaskY)>100)throw new Exception("Masque/centre hors limites.");
   if(!Enum.IsDefined(typeof(LoopR3),Loop)||!Enum.IsDefined(typeof(MaskSideR3),MaskSide))throw new Exception("Boucle/contour invalide.");
   if(Gate&&Trigger!=TriggerKind936.Note)throw new Exception("Gate nécessite un déclencheur Note.");
   if(CcCenter<0||CcCenter>127)throw new Exception("Centre CC : 0..127.");
   
  }
 }
 public sealed class Music936 {
  readonly Beat936[] marks;
  public Music936(Montage m){marks=m.Beats.OrderBy(x=>x.Time).GroupBy(x=>x.Time).Select(g=>g.Last()).ToArray();}
  public bool Available{get{return marks.Length>=2;}}
  public double Time(double beat){if(!Available)throw new Exception("Repères musicaux absents.");int lo=0,hi=marks.Length-1;while(lo+1<hi){int mid=(lo+hi)/2;if(marks[mid].Beat<=beat)lo=mid;else hi=mid;}var a=marks[lo];var b=marks[hi];if(b.Beat<=a.Beat)throw new Exception("Repères musicaux non croissants.");return a.Time+(beat-a.Beat)*(b.Time-a.Time)/(b.Beat-a.Beat);}
  public double Beat(double time){if(!Available)throw new Exception("Repères musicaux absents : enregistrer une prise synchronisée avec Tee pour les effets par note.");int lo=0,hi=marks.Length-1;while(lo+1<hi){int mid=(lo+hi)/2;if(marks[mid].Time<=time)lo=mid;else hi=mid;}var a=marks[lo];var b=marks[hi];return a.Beat+(time-a.Time)*(b.Beat-a.Beat)/(b.Time-a.Time);}
  public static double Length(Length936 length){return new[]{0.0625,0.125,0.25,0.5,1.0,2.0,4.0,1.0/96}[(int)length];}
 }
 // Event lists only. Rendering walks sparse MIDI events, never constructs state for every musical tick.
 sealed partial class EffectRun936:IDisposable {
  public byte[] ZoneCacheR8;
  public readonly HistoryA01 HistoryA01=new HistoryA01();
  internal readonly TextRunR12 TextRunR12;public readonly Effect936 Fx;readonly Music936 music;readonly Trigger936[] notes;readonly Cc936[] cc;readonly MidiSpan[] spans;int ni=-1,ci=-1;
  readonly Dictionary<string,Bitmap> scaled941=new Dictionary<string,Bitmap>();long cacheBytes941;readonly AnimationTiming938 animation938;VideoAnimation941 video941;Image asset;FrameDimension dimension;public int Frames;readonly bool sprite;readonly int fw,fh;
  public EffectRun936(Effect936 fx,Montage m,Music936 clock){fx=Resolve15(fx,m);Fx=fx;music=clock;fx.Validate();InitSources15(fx,m,clock);spans=m.Notes.Where(n=>EventsR9.Channel(fx,n.Channel,n.Track940)&&(fx.Note<0||n.Note==fx.Note)).OrderBy(n=>n.Start).ToArray();
   notes=EventsR9.Notes(m,fx).Where(n=>fx.DelayFilter07==0||n.Delay07==0||n.Delay07==fx.DelayFilter07).ToArray();
   cc=m.CC.Where(c=>c.Value>=0&&EventsR9.Channel(fx,c.Channel,c.Track940)&&c.Number==fx.CC).OrderBy(c=>c.Time).ToArray();
   if(fx.Kind==EffectKind936.Texte){TextRunR12=new TextRunR12(fx,m,clock);return;}
   if(fx.Kind==EffectKind936.Animation&&!fx.BanksR16.Enabled&&string.IsNullOrWhiteSpace(fx.Asset))return;if(fx.Kind==EffectKind936.Animation&&fx.BanksR16.Enabled){banksRunR16=new BankRunR16(fx.BanksR16,m.Folder);return;}if(!fx.ImagesR7.Enabled&&fx.Trigger!=TriggerKind936.CC&&!(fx.Trigger==TriggerKind936.Note&&fx.ValueSourceA01!=ValueSourceA01.Enveloppe&&fx.Kind!=EffectKind936.Animation)&&!music.Available)throw new Exception("Cette prise ne contient pas les repères musicaux requis pour les effets par note.");
   if(fx.Kind==EffectKind936.Animation&&fx.AnimationsR9.Count>0){animationSetR9=new AnimationSetRunR9(fx,m,clock,notes);return;}if(fx.Kind==EffectKind936.Animation){string path=Store934.MediaPath(m.Folder,fx.Asset);if(Path.GetExtension(path).Equals(".mp4",StringComparison.OrdinalIgnoreCase)){video941=new VideoAnimation941(path);asset=new Bitmap(video941.Frames[0]);}else asset=Image.FromFile(path);sprite=Path.GetExtension(path).Equals(".png",StringComparison.OrdinalIgnoreCase);
    if(sprite){if(asset.Width%4!=0||asset.Height%2!=0){asset.Dispose();throw new Exception("La sprite sheet PNG doit être divisible en 4 colonnes × 2 lignes.");}Frames=8;fw=asset.Width/4;fh=asset.Height/2;}
    else {dimension=FrameDimension.Time;Frames=video941==null?asset.GetFrameCount(dimension):video941.Frames.Count;fw=asset.Width;fh=asset.Height;}
    try{fx.ImagesR7.ValidateFrames(Frames);if(fx.Kind==EffectKind936.Animation&&!fx.BanksR16.Enabled&&string.IsNullOrWhiteSpace(fx.Asset))return;if(fx.Kind==EffectKind936.Animation&&fx.BanksR16.Enabled){banksRunR16=new BankRunR16(fx.BanksR16,m.Folder);return;}if(!fx.ImagesR7.Enabled&&fx.Trigger!=TriggerKind936.CC)animation938=new AnimationTiming938(fx,m,music,Frames);if(animation938!=null&&!video941EnabledTbo15())animation938.SpeedAgeTbo15=new SpeedIntegralTbo15(fx,m).Age;}catch{asset.Dispose();if(video941!=null)video941.Dispose();throw;}
   }
  }
  public double Age(double t){while(ni>=0&&notes[ni].Time>t)ni--;while(ni+1<notes.Length&&notes[ni+1].Time<=t)ni++;if(Fx.Trigger==TriggerKind936.Début_mesure){double beat=music.Beat(t);return beat<0?-1:beat-Math.Floor(beat/4)*4;}if(ni<0)return -1;if(Fx.Gate){var span=spans.FirstOrDefault(n=>n.Channel==notes[ni].Channel&&n.Track940==notes[ni].Track940&&n.Note==notes[ni].Note&&n.Delay07==notes[ni].Delay07&&n.Repeat07==notes[ni].Repeat07&&Math.Abs(n.Start-notes[ni].Time)<.002);if(span==null||t>=span.Start+span.Duration)return -1;}return music.Beat(t)-music.Beat(notes[ni].Time);}
  public double Value(double time){return ValueUnscaledR12(time)*Fx.AmplitudeScaleR12/100;}double ValueUnscaledR12(double time){if(Fx.Amplitude15.MidiR4!=null)return MidiBoundR4(Fx.Amplitude15,time,Fx.Amount)*GraphEnvelope15;if(GraphValue15.HasValue&&Fx.Amplitude15.Source<0)return GraphValue15.Value;if(Fx.Amplitude15.Source>=0)return Bound15(Fx.Amplitude15,time,0)*GraphEnvelope15;return Fx.BodyMask&&Fx.ValueTargetA01!=ValueTargetA01.Amplitude?(HasValueA01(time)?Fx.Amount:0):DriverA01(time);}
  public bool HasValueA01(double time){double t=time-Fx.OffsetMs941/1000;DriverA01(time);if(Fx.Trigger==TriggerKind936.CC)return ci>=0&&(Fx.HoldCcA01||t<=cc[ci].Time+Fx.CcTail939);if(Fx.Trigger==TriggerKind936.Note&&Fx.ValueSourceA01!=ValueSourceA01.Enveloppe)return ni>=0;return Math.Abs(DriverA01(time))>.000001;}
  public double DriverA01(double time){double t=time-Fx.OffsetMs941/1000;while(ni>=0&&notes[ni].Time>t)ni--;while(ci>=0&&cc[ci].Time>t)ci--;if(Fx.Trigger==TriggerKind936.CC){while(ci+1<cc.Length&&cc[ci+1].Time<=t)ci++;return ci<0||(!Fx.HoldCcA01&&t>cc[ci].Time+Fx.CcTail939)?0:Fx.CenterMapping?Fx.EffectCenter+(cc[ci].Value-Fx.CcCenter)*Fx.CcGain:Fx.At0+(Fx.At127-Fx.At0)*cc[ci].Value/127.0;}
   if(Fx.Trigger==TriggerKind936.Note&&Fx.ValueSourceA01!=ValueSourceA01.Enveloppe){while(ni+1<notes.Length&&notes[ni+1].Time<=t)ni++;if(ni<0)return 0;var span=spans.FirstOrDefault(n=>n.Channel==notes[ni].Channel&&n.Track940==notes[ni].Track940&&n.Note==notes[ni].Note&&n.Delay07==notes[ni].Delay07&&n.Repeat07==notes[ni].Repeat07&&Math.Abs(n.Start-notes[ni].Time)<.002);double input=Fx.ValueSourceA01==ValueSourceA01.Hauteur_de_note?notes[ni].Note:span==null?notes[ni].VelocityA01:span.Velocity;if(input<0)throw new Exception("Vélocité absente de cette ancienne prise : choisir Hauteur de note ou Enveloppe.");double ratio=Math.Max(0,Math.Min(1,(input-Fx.InputMinA01)/(Fx.InputMaxA01-Fx.InputMinA01)));return Fx.At0+(Fx.At127-Fx.At0)*ratio;}
   double age=Age(t),length=Music936.Length(Fx.Length);if(Fx.Gate&&ni>=0){var span=spans.FirstOrDefault(n=>n.Channel==notes[ni].Channel&&n.Track940==notes[ni].Track940&&n.Note==notes[ni].Note&&n.Delay07==notes[ni].Delay07&&n.Repeat07==notes[ni].Repeat07&&Math.Abs(n.Start-notes[ni].Time)<.002);if(span!=null)length=music.Beat(span.Start+span.Duration)-music.Beat(span.Start);}if(age<0||age>=length)return 0;double attack=length*Fx.Attack/100,release=length*Fx.Release/100;
   double envelope=attack>0&&age<attack?age/attack:release>0&&age>length-release?(length-age)/release:1;return Fx.Amount*envelope;
  }
  public double MusicalBeat(double t){return music.Available?music.Beat(t):t*2;}
  public double Phase(double t){if(!music.Available)return Math.Max(0,Math.Min(1,DriverA01(t)/100));double a=Age(t-Fx.OffsetMs941/1000);return a<0?0:Math.Max(0,Math.Min(1,a/Music936.Length(Fx.Length)));}
  bool video941EnabledTbo15(){return video941!=null;}
  public int FrameAt(double time){if(Frames<=0)return -1;if(Fx.ImagesR7.Enabled)return ImageFrameR7(time);if(CurrentGraphEvent15!=null&&CurrentGraphEvent15.Kind=="CC")return (int)Math.Round(Math.Max(0,Math.Min(1,TboSignal15.Normalize(CurrentGraphEvent15)))*(Frames-1));double t=time-Fx.OffsetMs941/1000;if(Fx.NoteImages939==3)return CycleFrame15(t);if(Fx.DelayImages07&&Fx.Trigger==TriggerKind936.Note){double age=Age(t);if(age<0||(!Fx.Gate&&age>=Music936.Length(Fx.Length)))return -1;return (Math.Max(0,notes[ni].Repeat07)+Fx.StartImage938-1)%Frames;}if(Fx.Trigger==TriggerKind936.Note&&Fx.NoteImages939!=0){double age=Age(t);if(age<0||(!Fx.Gate&&age>=Music936.Length(Fx.Length)))return -1;int n=notes[ni].Note;int image=Fx.NoteImages939==1?Fx.CenterImage939+n-Fx.CenterNote939:(Fx.NoteMap939[n%12]>0?Fx.NoteMap939[n%12]:Fx.OtherImage939);return image>=1&&image<=Frames?image-1:-1;}if(Fx.Trigger==TriggerKind936.CC){Value(time);return ci<0||(!Fx.HoldCcA01&&t>cc[ci].Time+Fx.CcTail939)?-1:(int)Math.Round(Math.Max(0,Math.Min(100,Value(time)))*(Frames-1)/100);}
   return animation938.FrameAt(t);
  }
  public bool Active(double t){if(waitingR20!=null&&waitingR20.Frame(t)>=0)return true;if(Fx.Kind==EffectKind936.Animation&&banksRunR16==null&&animationSetR9==null&&asset==null)return false;if(banksRunR16!=null)return banksRunR16.Position(t-Fx.OffsetMs941/1000)!=null;if(animationSetR9!=null)return true;if(Fx.Kind==EffectKind936.Placement_2D&&Fx.PathEnabled15)return true;if(TextRunR12!=null)return TextRunR12.Active(t);return Fx.Kind==EffectKind936.Animation?FrameAt(t)>=0:(Fx.Kind==EffectKind936.Révélation||Fx.Kind==EffectKind936.Seuil?HasValueA01(t):Math.Abs(Value(t))>.000001);}
  public void Overlay(Graphics g,int width,int height,double t){if(DrawWaitingR20(g,width,height,t))return;if(Fx.Kind==EffectKind936.Animation&&banksRunR16==null&&animationSetR9==null&&asset==null)return;if(banksRunR16!=null){DrawBanksR16(g,width,height,t);return;}if(animationSetR9!=null){animationSetR9.Draw(g,width,height,t);return;}int frame=FrameAt(t);if(frame<0)return;
   Image currentAsset=video941!=null?video941.Frames[frame]:asset;float w=(float)(width*Fx.Size/100),h=(float)(w*fh/fw*Fx.StretchYR2);var dest=new RectangleF((float)(Fx.CenterPosition939?width*Fx.X/100-w/2:(width-w)*Fx.X/100),(float)(Fx.CenterPosition939?height*Fx.Y/100-h/2:(height-h)*Fx.Y/100),w,h);
   if(Fx.DelayShape07!=0&&Fx.Trigger==TriggerKind936.Note){var point=DelayPosition07(t);dest.X=width*point.X/100-w/2;dest.Y=height*point.Y/100-h/2;}
   var src=sprite?new Rectangle((frame%4)*fw,(frame/4)*fh,fw,fh):new Rectangle(0,0,fw,fh);int cw=Math.Max(1,(int)Math.Round(w)),ch=Math.Max(1,(int)Math.Round(h));string key=frame+":"+cw+":"+ch+":"+Fx.PixelArt941;Bitmap cached;if(!scaled941.TryGetValue(key,out cached)){if(video941==null&&!sprite)asset.SelectActiveFrame(dimension,frame);long bytes=(long)cw*ch*4;if(bytes>32*1024*1024){DrawSpriteR9(g,currentAsset,dest,src,t);return;}if(cacheBytes941+bytes>32*1024*1024){foreach(var entry in scaled941.Values)entry.Dispose();scaled941.Clear();cacheBytes941=0;}cached=new Bitmap(cw,ch,PixelFormat.Format32bppArgb);using(var cg=Graphics.FromImage(cached)){cg.CompositingMode=CompositingMode.SourceCopy;cg.InterpolationMode=Fx.PixelArt941?InterpolationMode.NearestNeighbor:InterpolationMode.HighQualityBicubic;cg.PixelOffsetMode=PixelOffsetMode.Half;cg.DrawImage(currentAsset,new Rectangle(0,0,cw,ch),src,GraphicsUnit.Pixel);}scaled941[key]=cached;cacheBytes941+=bytes;}DrawSpriteR9(g,cached,new RectangleF((float)Math.Round(dest.X),(float)Math.Round(dest.Y),cached.Width,cached.Height),new Rectangle(0,0,cached.Width,cached.Height),t);
  }
  public void Dispose(){if(waitingR20!=null)waitingR20.Dispose();if(banksRunR16!=null)banksRunR16.Dispose();if(animationSetR9!=null)animationSetR9.Dispose();if(TextRunR12!=null)TextRunR12.Dispose();HistoryA01.Dispose();foreach(var frame in scaled941.Values)frame.Dispose();scaled941.Clear();if(video941!=null)video941.Dispose();if(asset!=null)asset.Dispose();}
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
  internal static Bitmap Apply(Bitmap source,EffectRun936 run,double time){if(!run.Fx.ZoneR8)return ApplyCoreR8(source,run,time);using(var before=(Bitmap)source.Clone()){var result=ApplyCoreR8(source,run,time);PixelsR2.Blend(before,result,run.ZoneCacheR8??(run.ZoneCacheR8=ZoneMaskR8.Make(run.Fx)));return result;}}
  internal static Bitmap ApplyCoreR8(Bitmap source,EffectRun936 run,double time){var fx=run.Fx;int w=source.Width,h=source.Height;if(fx.Kind!=EffectKind936.Texte&&fx.Kind!=EffectKind936.Animation&&run.AttenuationR9<=0)return source;if((fx.Kind==EffectKind936.Seuil||fx.Kind==EffectKind936.Révélation)&&run.AttenuationR9<1){double amount=run.AttenuationR9;using(var before=(Bitmap)source.Clone()){run.AttenuationR9=1;try{using(var processed=ApplyCoreR8(source,run,time))return TboSignal15.Mix(before,1,processed,1,amount);}finally{run.AttenuationR9=amount;}}}if(fx.Kind==EffectKind936.Texte){using(var g=Graphics.FromImage(source))run.TextRunR12.Draw(g,w,h,time);return source;}if(fx.Kind==EffectKind936.Animation){using(var g=Graphics.FromImage(source)){g.InterpolationMode=InterpolationMode.HighQualityBicubic;run.Overlay(g,w,h,time);}return source;}
   if(fx.Kind==EffectKind936.Placement_2D){var point=fx.PathEnabled15?run.Path15(time):run.DelayPosition07(time);var moved=new Bitmap(w,h,PixelFormat.Format32bppArgb);using(var g=Graphics.FromImage(moved))g.DrawImageUnscaled(source,(int)Math.Round(w*(point.X-50)*run.AttenuationR9/100),(int)Math.Round(h*(point.Y-50)*run.AttenuationR9/100));source.Dispose();return moved;}
   double value=run.Value(time)*run.AttenuationR9;if(fx.Kind>=EffectKind936.Gamma&&fx.Kind!=EffectKind936.Mouvement)return ExtraRenderA01.Apply(source,run,time,value);if(Math.Abs(value)<.000001&&fx.Kind!=EffectKind936.Mouvement)return source;if(fx.Kind>=EffectKind936.Colorisation&&fx.Kind<EffectKind936.Mute)return ChromaticR3.Apply(source,run,time,value);if(fx.Kind==EffectKind936.Flou||fx.Kind==EffectKind936.Pixelisation)return PixelsR2.Filter(source,fx.Kind,value);var result=new Bitmap(w,h,PixelFormat.Format32bppArgb);
   using(var g=Graphics.FromImage(result)){g.CompositingMode=CompositingMode.SourceCopy;g.InterpolationMode=InterpolationMode.HighQualityBicubic;
    if(fx.Kind==EffectKind936.Mouvement){g.DrawImageUnscaled(source,(int)Math.Round(w*run.MovementR19(fx.MoveXR19,fx.MoveCenterXR19,fx.MoveAmplitudeXR19,time)/100),(int)Math.Round(h*run.MovementR19(fx.MoveYR19,fx.MoveCenterYR19,fx.MoveAmplitudeYR19,time)/100));}
    else if(fx.Kind==EffectKind936.Déplacement_X||fx.Kind==EffectKind936.Déplacement_Y){int dx=fx.Kind==EffectKind936.Déplacement_X?(int)Math.Round(w*value/100):0,dy=fx.Kind==EffectKind936.Déplacement_Y?(int)Math.Round(h*value/100):0;g.DrawImageUnscaled(source,dx,dy);}
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
   var graphRuns15=new Dictionary<TrackFx936,GraphRun15>();var mediaTbo15=new TboMedia15(m,cancel);
   try{
    var relevantBlocks=m.Blocks.Where(b=>b.Track==track&&b.Start<origin+renderDuration&&b.Start+b.Duration>origin).ToArray();
    int rotation=OrientationR2.Degrees(t);int w=rotation%180==0?info.Width:info.Height,h=rotation%180==0?info.Height:info.Width,size=checked(w*h*4);byte[] bytes=new byte[size];double shift=fx.Shift;
    string decodeInput="-i "+Engine.Q(input)+" -an "+(OrientationR2.Filter(t).Length==0?"":"-vf "+Engine.Q(OrientationR2.Filter(t).TrimStart(','))+" ");
    bool alpha=Path.GetExtension(output).Equals(".mov",StringComparison.OrdinalIgnoreCase);
    using(var body=relevantBlocks.Any(b=>(b.Fx939??fx).Effects.Any(e=>e.Enabled&&e.BodyMask&&e.Kind!=EffectKind936.Mute))?new BodyMaskR2(cancel):null)
    using(var decode=Engine.Create(Engine.FF,"-hide_banner -loglevel error "+decodeInput+"-f rawvideo -pix_fmt bgra pipe:1"))
    using(var encode=Engine.Create(Engine.EncodingFF(t.Encoder),"-hide_banner -loglevel error -y -f rawvideo -pix_fmt bgra -s "+w+"x"+h+" -r 30 -i pipe:0 "+(info.Audio?"-i "+Engine.Q(input)+" -map 0:v -map 1:a -c:a aac -b:a 192k ":"-map 0:v ")+(alpha?"-c:v prores_ks -profile:v 4 -pix_fmt yuva444p10le -alpha_bits 16 ":Engine.VideoEncoder(t.Encoder,false))+"-shortest -t "+duration+" -movflags +faststart "+Engine.Q(output))){
     decode.Start();var de=decode.StandardError.ReadToEndAsync();try{encode.Start();}catch(Exception ex){try{decode.Kill();decode.WaitForExit();}catch{}cancel.ThrowIfCancellationRequested();if(t.Encoder=="nvenc"&&!alpha)throw new NvidiaExportExceptionR1(ex.Message,ex);throw;}var ee=encode.StandardError.ReadToEndAsync();var eo=encode.StandardOutput.ReadToEndAsync();
     var partial942=PartialExport942.State;using(cancel.Register(()=>{if(partial942!=null&&partial942.Keep)return;try{if(!decode.HasExited)decode.Kill();}catch{}try{if(!encode.HasExited)encode.Kill();}catch{}}))try{
      int frame=0,last=-1;bool kept942=false;try{while(true){if(cancel.IsCancellationRequested&&PartialExport942.Keeping&&frame>0){kept942=true;break;}cancel.ThrowIfCancellationRequested();int count=0;while(count<size){int read=decode.StandardOutput.BaseStream.Read(bytes,count,size-count);if(read==0)break;count+=read;}if(count==0)break;if(count!=size)throw new Exception("Image décodée incomplète.");
       Bitmap bitmap=new Bitmap(w,h,PixelFormat.Format32bppArgb);try{var data=bitmap.LockBits(new Rectangle(0,0,w,h),ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);try{Marshal.Copy(bytes,0,data.Scan0,size);}finally{bitmap.UnlockBits(data);}double time=origin+frame/30.0;
        var activeBlock=m.Blocks.FirstOrDefault(b=>b.Track==track&&time>=b.Start&&time<b.Start+b.Duration);if(activeBlock==null&&origin<0&&time<0)activeBlock=m.Blocks.FirstOrDefault(b=>b.Track==track&&b.Start==0);bool allowed=!Mute940.VideoR21(m,track,time);if(!allowed){using(var g=Graphics.FromImage(bitmap))g.Clear(fx.Graphic?Color.Transparent:Color.Black);}double strength=allowed?1:0;
        byte[] mask=allowed&&body!=null&&(activeBlock==null?fx:activeBlock.Fx939??fx).Graph15!=null?body.Mask(bitmap):null;
        var graphFx15=activeBlock==null?fx:(activeBlock.Fx939??fx);if(allowed&&graphFx15.Graph15!=null){GraphRun15 graph15;if(!graphRuns15.TryGetValue(graphFx15,out graph15)){foreach(var old in graphRuns15.Values)old.Dispose();graphRuns15.Clear();RenderStagesR9.Report("Analyse du graphe",-1);graph15=new GraphRun15(graphFx15,mediaTbo15.Events,mediaTbo15,activeBlock==null?0:activeBlock.Start);RenderStagesR9.Report("Analyse du graphe",100);graphRuns15[graphFx15]=graph15;}var rendered15=graph15.Render(bitmap,time,mask);bitmap.Dispose();bitmap=rendered15;}
        // Composite alpha on black once, after all ordered effects and transparent overlays.
        if(alpha){data=bitmap.LockBits(new Rectangle(0,0,w,h),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);try{Marshal.Copy(data.Scan0,bytes,0,size);}finally{bitmap.UnlockBits(data);}}else using(var opaque=new Bitmap(w,h,PixelFormat.Format32bppArgb)){using(var g=Graphics.FromImage(opaque)){g.Clear(Color.Black);g.DrawImageUnscaled(bitmap,0,0);}data=opaque.LockBits(new Rectangle(0,0,w,h),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);try{Marshal.Copy(data.Scan0,bytes,0,size);}finally{opaque.UnlockBits(data);}}
        try{encode.StandardInput.BaseStream.Write(bytes,0,size);}catch(IOException ex){cancel.ThrowIfCancellationRequested();if(t.Encoder=="nvenc"&&!alpha){string detail=encode.WaitForExit(2000)?ee.GetAwaiter().GetResult():ex.Message;throw new NvidiaExportExceptionR1(detail,ex);}throw;}
       }finally{bitmap.Dispose();}frame++;int percent=(int)Math.Min(99,frame*100/(renderDuration*30));if(percent!=last){last=percent;if(progress!=null)progress(percent);}
      }
      }catch(Exception){if(!cancel.IsCancellationRequested||!PartialExport942.Keeping||frame==0)throw;kept942=true;}try{encode.StandardInput.Close();}catch(IOException ex){cancel.ThrowIfCancellationRequested();if(t.Encoder=="nvenc"&&!alpha)throw new NvidiaExportExceptionR1(ex.Message,ex);throw;}if(kept942){try{decode.Kill();decode.WaitForExit();}catch{}}else Finish(decode,de);try{Finish(encode,ee);}catch(Exception ex){cancel.ThrowIfCancellationRequested();if(t.Encoder=="nvenc"&&!alpha)throw new NvidiaExportExceptionR1(ex.Message,ex);throw;}if(frame==0)throw new Exception("Aucune image rendue.");if(kept942)PartialExport942.State.Seconds=frame/30.0;if(progress!=null)progress(100);
     }finally{try{if(!decode.HasExited)decode.Kill();decode.WaitForExit();de.GetAwaiter().GetResult();}catch{}try{if(!encode.HasExited)encode.Kill();encode.WaitForExit();ee.GetAwaiter().GetResult();eo.GetAwaiter().GetResult();}catch{}}
    }
   }finally{mediaTbo15.Dispose();foreach(var graph15 in graphRuns15.Values)graph15.Dispose();}
  }
 }
 public static partial class Engine {
  public static Montage Snapshot936(Montage m){var s=new JavaScriptSerializer{MaxJsonLength=int.MaxValue};var result=s.Deserialize<Montage>(s.Serialize(m));result.Folder=m.Folder;result.DirectMode939=m.DirectMode939;result.LiveFrame939=m.LiveFrame939;return result;}
  public static void Export(Montage m,int track,string output){Export936(Snapshot936(m),track,output,CancellationToken.None,null);}
  public static void Export936(Montage m,int track,string output,CancellationToken cancel,Action<int> progress,double origin=0,double limit=-1,int previewHeight=0,bool referenceAudio=false){
   string full=Path.GetFullPath(output),root=Path.GetFullPath(m.Folder).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;
   if(full.StartsWith(root,StringComparison.OrdinalIgnoreCase)&&File.Exists(full))throw new Exception("Choisir un nouveau fichier pour conserver les médias et les bruts de cette prise.");
   var t=m.Tracks[track];string work=Path.Combine(ProjectStorageR11.Cache(m,"Export"),".Snkoz_FX_"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(work);
   try{double length=limit<0?m.Duration-origin:Math.Min(limit,m.Duration-origin);if(origin<PrerollR4.Minimum(m)-.000001||length<=0)throw new Exception("Zone de rendu invalide.");
    if(progress!=null)progress(1);string baseFile=Path.Combine(work,"base.mkv");cancel.ThrowIfCancellationRequested();RenderRangeR3.Prepare(m,track,baseFile,origin,length,previewHeight,cancel,false,false,p=>{if(progress!=null)progress(Math.Max(1,p/5));});
    string rendered=Path.Combine(work,"rendu"+Path.GetExtension(full));FxRender936.Render(m,track,baseFile,rendered,cancel,p=>{RenderStagesR9.Report("Images et encodage",p);if(progress!=null)progress(20+(int)(p*.74));},origin,length,previewHeight);
    if(PartialExport942.Keeping&&PartialExport942.State.Seconds>0&&!PartialExport942.State.Composite)PartialExport942.Save(rendered,full,PartialExport942.State.Seconds);
    if(referenceAudio){rendered=RenderRangeR3.ReferenceAudio(m,track,rendered,work,origin,length,cancel);}
    cancel.ThrowIfCancellationRequested();if(ProbeMedia(rendered).Duration<=0)throw new Exception("Rendu vide.");RenderStagesR9.Report("Finalisation",0);File.Copy(rendered,full,true);RenderStagesR9.Report("Finalisation",100);if(progress!=null)progress(100);
   }finally{Directory.Delete(work,true);}
  }
 }
}










