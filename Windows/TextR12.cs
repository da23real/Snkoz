using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
namespace Synkoz {
 public enum TextContentR12 { Texte_fixe,Nom_de_note,Valeur_directe,Correspondances,Lettres,Mots,Phrases }
 public enum TextVariableR12 { Hauteur_de_note,Vélocité,Numéro_CC,Valeur_CC }
 public enum TextTargetR12 { Taille,Opacité,Position_X,Position_Y,Épaisseur_contour,Couleur_texte,Couleur_contour }
 public sealed class TextEntryR12 {public int Value;public string Text="";}
 public sealed class TextBindingR12 {
  public bool CenterR19;public double InputCenterR19=60,OutputCenterR19=50,AmplitudeR19=50;public TextVariableR12 Variable;public int CC=74;public double InputMin=0,InputMax=127,Min=0,Max=100;public TextTargetR12 Target;public int ColorMin=Color.White.ToArgb(),ColorMax=Color.Cyan.ToArgb();
 }
 public sealed class TextSettingsR12 {
  public bool GuidedR13,FreeValuesR13,SetupPendingR13;public int CcDisplayR10=-1; // -1 preserves the legacy content/variable; 0..3 are independent CC flags.
  public string FontFileR2="";public double StretchXR2=1,StretchYR2=1;public bool KeepAspectR2=true;public TextContentR12 Content;public string Text="Texte",Prefix="",Suffix="",Font="Segoe UI";public int FontStyle,Color=System.Drawing.Color.White.ToArgb(),OutlineColor=System.Drawing.Color.Black.ToArgb();
  public double Size=72,Outline=8,Opacity=100,X=50,Y=50,Multiplier=1,Offset;public bool Octave,AllCC;public int FirstValue=60,CC=74;
  public TextVariableR12 Variable;public List<TextEntryR12> Entries=new List<TextEntryR12>();public List<TextBindingR12> Bindings=new List<TextBindingR12>();
  public void Validate(){if(CcDisplayR10< -1||CcDisplayR10>3)throw new Exception("Affichage CC invalide.");if(!Enum.IsDefined(typeof(TextContentR12),Content)||!Enum.IsDefined(typeof(TextVariableR12),Variable)||CC<0||CC>127||FirstValue< -127||FirstValue>127||FontStyle<0||FontStyle>15||string.IsNullOrWhiteSpace(Font)||Font.Length>200||Text==null||Text.Length>10000||Prefix==null||Suffix==null||Prefix.Length>1000||Suffix.Length>1000)throw new Exception("Texte ou variable invalide.");
   if(StretchXR2<.1||StretchXR2>5||StretchYR2<.1||StretchYR2>5||double.IsNaN(StretchXR2)||double.IsNaN(StretchYR2))throw new Exception("Dimensions du texte invalides.");double[] values={Size,Outline,Opacity,X,Y,Multiplier,Offset};if(values.Any(v=>double.IsNaN(v)||double.IsInfinity(v))||Size<1||Size>600||Outline<0||Outline>300||Opacity<0||Opacity>100||Math.Abs(X)>999||Math.Abs(Y)>999||Math.Abs(Multiplier)>1000||Math.Abs(Offset)>1000000)throw new Exception("Apparence du texte hors limites.");
   if(Entries==null||Entries.Count>1000||Entries.Any(e=>e==null||e.Value< -127||e.Value>127||e.Text==null||e.Text.Length>10000)||Entries.Select(e=>e.Value).Distinct().Count()!=Entries.Count)throw new Exception("Correspondances : une seule entrée par valeur, de −127 à 127.");
   if(Bindings==null||Bindings.Count>7||Bindings.Any(b=>b==null)||Bindings.Select(b=>b.Target).Distinct().Count()!=Bindings.Count)throw new Exception("Une affectation par paramètre visuel.");
   foreach(var b in Bindings){if(!Enum.IsDefined(typeof(TextTargetR12),b.Target)||!Enum.IsDefined(typeof(TextVariableR12),b.Variable)||b.CC<0||b.CC>127||new[]{b.InputMin,b.InputMax,b.Min,b.Max,b.InputCenterR19,b.OutputCenterR19,b.AmplitudeR19}.Any(v=>double.IsNaN(v)||double.IsInfinity(v))||b.InputMin<0||b.InputMin>127||b.InputMax<0||b.InputMax>127||b.InputMax==b.InputMin||Math.Abs(b.Min)>10000||Math.Abs(b.Max)>10000)throw new Exception("Affectation MIDI du texte invalide.");}
  }
 }
 public sealed partial class Effect936 {public TextSettingsR12 TextR12=new TextSettingsR12();}
 public static class TextPresetsR12 {
  public static Effect936 Make(int preset){var e=new Effect936{Kind=EffectKind936.Texte,Length=Length936.Noire,Release=0,HoldCcA01=true};e.TextR12.Content=preset==0?TextContentR12.Nom_de_note:TextContentR12.Valeur_directe;e.TextR12.Variable=preset==1?TextVariableR12.Numéro_CC:preset==2?TextVariableR12.Valeur_CC:TextVariableR12.Hauteur_de_note;e.TextR12.Prefix=preset==1?"CC ":"";if(preset>0){e.Trigger=TriggerKind936.CC;e.CC=e.TextR12.CC=74;e.TextR12.AllCC=preset==1;}return e;}
 }
 // Text has its own content source and independent bindings. No change to the recorded events.
 sealed partial class TextRunR12:IDisposable {
  readonly Effect936 fx;readonly TextSettingsR12 text;readonly Music936 clock;readonly MidiSpan[] notes;readonly Trigger936[] triggers;readonly Cc936[] controls,starts;readonly string[] pieces;
  GraphicsPath cached;string cacheKey;FontFamily family;readonly double blockStartR19;
  public TextRunR12(Effect936 e,Montage m,Music936 music){fx=e;text=e.TextR12;clock=music;blockStartR19=m.EditorOriginTbo15;notes=m.Notes.Where(n=>EventsR9.Channel(e,n.Channel,n.Track940)).OrderBy(n=>n.Start).ToArray();triggers=EventsR9.Notes(m,e);controls=m.CC.Where(c=>EventsR9.Channel(e,c.Channel,c.Track940)).OrderBy(c=>c.Time).ToArray();starts=controls.Where(c=>text.AllCC||c.Number==e.CC).ToArray();pieces=Split(text.Text,text.Content);
   try{family=PersonalFontsR2.Resolve(text);}catch(ArgumentException){family=new FontFamily("Segoe UI");}
  }
  public static string[] Split(string value,TextContentR12 mode){if(mode==TextContentR12.Lettres){var list=new List<string>();var it=StringInfo.GetTextElementEnumerator(value);while(it.MoveNext())list.Add(it.GetTextElement());return list.ToArray();}if(mode==TextContentR12.Mots)return Regex.Split(value.Trim(),@"\s+").Where(s=>s.Length>0).ToArray();if(mode==TextContentR12.Phrases)return Regex.Split(value.Trim(),@"(?<=[.!?])\s+|\r?\n+").Where(s=>s.Length>0).ToArray();return new string[0];}
  static int Last<T>(T[] data,double time,Func<T,double> at){int lo=0,hi=data.Length;while(lo<hi){int mid=(lo+hi)/2;if(at(data[mid])<=time)lo=mid+1;else hi=mid;}return lo-1;}
  sealed class Cue {public double Start,End;public int Channel,Track=-1,Note=-1,Velocity=-1;public Cc936 CC;}
  double Beat(double t){return clock.Available?clock.Beat(t):t*2;}
  double Time(double b){return clock.Available?clock.Time(b):b/2;}
  Cue At(double t){if(fx.Trigger==TriggerKind936.CC){t=CcTimeR14(t);int at=Last(starts,t,item=>item.Time);if(at<0||starts[at].Value<0)return null;var c=starts[at];double end=fx.HoldCcA01?double.PositiveInfinity:c.Time+fx.CcTail939;if(t>=end)return null;return new Cue{Start=c.Time,End=end,Channel=c.Channel,Track=c.Track940,CC=c};}
   if(fx.Trigger==TriggerKind936.Début_mesure||fx.Trigger==TriggerKind936.Timestamp){double beat=fx.Trigger==TriggerKind936.Timestamp?Beat(blockStartR19)+AnimationRules938.TimestampBeat(fx):Math.Floor(Beat(t)/4)*4;if(beat<0)return null;double start=Time(beat),end=Time(beat+Music936.Length(fx.Length));return t>=start&&t<end?new Cue{Start=start,End=end,Channel=fx.Channel-1}:null;}
   int index=Last(triggers,t,item=>item.Time);if(index<0)return null;var n=triggers[index];double finish=Time(Beat(n.Time)+Music936.Length(fx.Length));var span=notes.FirstOrDefault(s=>s.Channel==n.Channel&&s.Note==n.Note&&(n.Track940<0||s.Track940==n.Track940)&&Math.Abs(s.Start-n.Time)<.002);
   if(fx.Gate){if(span==null)return null;finish=span.Start+span.Duration;}if(t>=finish)return null;
   if(fx.Gate&&span!=null)finish=span.Start+span.Duration;var cue=new Cue{Start=n.Time,End=finish,Channel=n.Channel,Track=n.Track940,Note=fx.Trigger==TriggerKind936.Note?n.Note:-1,Velocity=span==null?n.VelocityA01:span.Velocity};
   if(fx.Trigger==TriggerKind936.CC_As_note)cue.CC=controls.LastOrDefault(c=>!c.StateOnlyR9&&c.Time==n.Time&&c.Channel==n.Channel&&c.Number==fx.CC&&c.Value==fx.CcValueR9);return cue;
  }
  double? Input(TextVariableR12 source,int number,Cue cue,double time,bool parameter=false){if(parameter&&(source==TextVariableR12.Hauteur_de_note||source==TextVariableR12.Vélocité)){int index=Last(notes,time,n=>n.Start);if(index>=0)return source==TextVariableR12.Hauteur_de_note?notes[index].Note:notes[index].Velocity;}if(source==TextVariableR12.Hauteur_de_note)return cue.Note<0?(double?)null:cue.Note;if(source==TextVariableR12.Vélocité)return cue.Note<0||cue.Velocity<0?(double?)null:cue.Velocity;
   if(source==TextVariableR12.Numéro_CC&&cue.CC!=null)return cue.CC.Number;
   time=CcTimeR14(time);var value=controls.LastOrDefault(c=>c.Time<=time&&c.Channel==cue.Channel&&(cue.Track<0||c.Track940==cue.Track)&&(source==TextVariableR12.Numéro_CC||c.Number==number));return value==null||value.Value<0?(double?)null:source==TextVariableR12.Numéro_CC?value.Number:CcValueR14(value,time);
  }
  string Content(Cue cue,double time){if(text.CcDisplayR10>=0&&text.Content==TextContentR12.Valeur_directe){var cc=cue.CC??controls.LastOrDefault(c=>c.Time<=time&&c.Channel==cue.Channel&&c.Number==text.CC);if(cc==null)return "";string shown=TextCcR10.Format(text.CcDisplayR10,cc.Number,(int)Math.Round(CcValueR14(cc,time),MidpointRounding.AwayFromZero));return shown.Length==0?"":text.Prefix+shown+text.Suffix;}if(text.Content==TextContentR12.Texte_fixe)return text.Text;if(text.Content==TextContentR12.Nom_de_note){if(cue.Note<0)return "";string[] names={"Do","Do♯","Ré","Ré♯","Mi","Fa","Fa♯","Sol","Sol♯","La","La♯","Si"};return text.Prefix+names[cue.Note%12]+(text.Octave?(cue.Note/12-1).ToString():"")+text.Suffix;}
   double? input=Input(text.Variable,text.CC,cue,time);if(!input.HasValue)return "";int value=(int)Math.Round(input.Value*text.Multiplier+text.Offset,MidpointRounding.AwayFromZero);string result;
   if(text.Content==TextContentR12.Valeur_directe)result=value.ToString(CultureInfo.InvariantCulture);else if(text.Content==TextContentR12.Correspondances){var entry=text.Entries.FirstOrDefault(e=>e.Value==value);result=entry==null?"":entry.Text;}else{int at=value-text.FirstValue;result=at>=0&&at<pieces.Length?pieces[at]:"";}return result.Length==0?"":text.Prefix+result+text.Suffix;
  }
  public string ContentAt(double time){double t=time-fx.OffsetMs941/1000;var cue=At(t);return cue==null||t>=cue.End?"":Content(cue,t);}
  public bool Active(double time){return ContentAt(time).Length>0;}
  static double Clamp(double x,double lo,double hi){return Math.Max(lo,Math.Min(hi,x));}
  static Color Blend(int a,int b,double t){var x=Color.FromArgb(a);var y=Color.FromArgb(b);return Color.FromArgb((int)Math.Round(x.A+(y.A-x.A)*t),(int)Math.Round(x.R+(y.R-x.R)*t),(int)Math.Round(x.G+(y.G-x.G)*t),(int)Math.Round(x.B+(y.B-x.B)*t));}
  public void Draw(Graphics g,int width,int height,double time){double t=time-fx.OffsetMs941/1000;var cue=At(t);if(cue==null||t>=cue.End)return;string value=Content(cue,t);if(value.Length==0)return;double size=text.Size,stroke=text.Outline,opacity=text.Opacity,x=text.X,y=text.Y;Color fill=Color.FromArgb(text.Color),outline=Color.FromArgb(text.OutlineColor);
   foreach(var binding in text.Bindings){double? input=Input(binding.Variable,binding.CC,cue,t,true);if(!input.HasValue)continue;double ratio=Clamp((input.Value-binding.InputMin)/(binding.InputMax-binding.InputMin),0,1),v=binding.Min+(binding.Max-binding.Min)*ratio;if(binding.CenterR19)v=MidiR4.Map(new Binding15{MidiR4=new MidiSelectR4{Kind="CC"},InputMinR4=binding.InputMin/127,InputMaxR4=binding.InputMax/127,CenterR19=true,InputCenterR19=binding.InputCenterR19/127,OutputCenterR19=binding.OutputCenterR19,AmplitudeR19=binding.AmplitudeR19},new GraphEvent15{Kind="CC",Value=input.Value,Bits=7},v);switch(binding.Target){case TextTargetR12.Taille:size=v;break;case TextTargetR12.Opacité:opacity=v;break;case TextTargetR12.Position_X:x=v;break;case TextTargetR12.Position_Y:y=v;break;case TextTargetR12.Épaisseur_contour:stroke=v;break;case TextTargetR12.Couleur_texte:fill=Blend(binding.ColorMin,binding.ColorMax,ratio);break;case TextTargetR12.Couleur_contour:outline=Blend(binding.ColorMin,binding.ColorMax,ratio);break;}}
   bool guided=TextTrialR13.Guided(text);if(guided){size=Clamp(size,43.2,432);stroke=Clamp(stroke,0,86.4);opacity=Clamp(opacity,70,100);fill=Color.FromArgb(255,fill);outline=Color.FromArgb(255,outline);}double alpha=Clamp(opacity/100,0,1);if(fx.Trigger!=TriggerKind936.CC&&cue.End>cue.Start){double p=(t-cue.Start)/(cue.End-cue.Start),attack=fx.Attack/100,release=fx.Release/100;alpha*=Clamp(attack>0&&p<attack?p/attack:release>0&&p>1-release?(1-p)/release:1,0,1);}if(alpha<=0)return;
   float em=(float)(Clamp(size,1,600)*height/1080),line=(float)(Clamp(stroke,0,300)*height/1080);string key=value+"\0"+em.ToString("R",CultureInfo.InvariantCulture);if(cached==null||key!=cacheKey){if(cached!=null)cached.Dispose();cached=new GraphicsPath();FontStyle style=(FontStyle)text.FontStyle;if(!family.IsStyleAvailable(style))style=FontStyle.Regular;using(var format=(StringFormat)StringFormat.GenericTypographic.Clone()){format.Alignment=StringAlignment.Center;cached.AddString(value,family,(int)style,em,new PointF(0,0),format);}using(var stretch=new Matrix()){stretch.Scale((float)(guided?Clamp(text.StretchXR2,.8,1.25):text.StretchXR2),(float)(guided?Clamp(text.StretchYR2,.8,1.25):text.StretchYR2));cached.Transform(stretch);}cacheKey=key;}
   if(cached.PointCount==0)return;using(var layout=(GraphicsPath)cached.Clone()){if(guided)TextTrialR13.Fit(layout,ref line,width,height,ref x,ref y);var bounds=layout.GetBounds();if(fx.PlacementAngleR19!=0){using(var rotation=new Matrix()){rotation.RotateAt((float)fx.PlacementAngleR19,new PointF(bounds.X+bounds.Width/2,bounds.Y+bounds.Height/2));layout.Transform(rotation);}bounds=layout.GetBounds();}float dx=(float)(width*Clamp(x,-999,999)/100)-bounds.X-bounds.Width/2,dy=(float)(height*Clamp(y,-999,999)/100)-bounds.Y-bounds.Height/2;
   if(alpha>=.999999){Glyph(g,layout,dx,dy,line,fill,outline);return;}
   var area=Rectangle.Intersect(new Rectangle(0,0,width,height),Rectangle.Ceiling(new RectangleF(bounds.X+dx-line-2,bounds.Y+dy-line-2,bounds.Width+line*2+4,bounds.Height+line*2+4)));if(area.Width<=0||area.Height<=0)return;
   // Fade the composed glyph and its halo once, avoiding doubled alpha where they overlap.
   using(var layer=new Bitmap(area.Width,area.Height,PixelFormat.Format32bppArgb)){using(var local=Graphics.FromImage(layer))Glyph(local,layout,dx-area.X,dy-area.Y,line,fill,outline);using(var attr=new ImageAttributes()){var matrix=new ColorMatrix();matrix.Matrix33=(float)alpha;attr.SetColorMatrix(matrix);g.DrawImage(layer,area,0,0,layer.Width,layer.Height,GraphicsUnit.Pixel,attr);}}
  }
  } void Glyph(Graphics g,GraphicsPath layout,float dx,float dy,float line,Color fill,Color outline){var state=g.Save();try{g.SmoothingMode=SmoothingMode.AntiAlias;g.TranslateTransform(dx,dy);
    // Stroke the whole string first, then fill every glyph: neighbouring halos never cover letters.
    if(line>0)using(var pen=new Pen(outline,line*2)){pen.LineJoin=LineJoin.Round;g.DrawPath(pen,layout);}using(var brush=new SolidBrush(fill))g.FillPath(brush,layout);
   }finally{g.Restore(state);}
  }
  public void Dispose(){if(cached!=null)cached.Dispose();if(string.IsNullOrEmpty(text.FontFileR2))family.Dispose();}
 }
}
