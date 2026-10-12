using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
namespace Synkoz {
 public static class TextTrialR13 {
  public static bool Guided(TextSettingsR12 t){return t.GuidedR13&&!t.FreeValuesR13;}
  public static double Limit(double v,double lo,double hi){return Math.Max(lo,Math.Min(hi,v));}
  public static void NewText(Effect936 e){e.TextR12=new TextSettingsR12{Content=TextContentR12.Valeur_directe,Variable=TextVariableR12.Valeur_CC,CcDisplayR10=3,AllCC=true,GuidedR13=true,SetupPendingR13=true,Size=108,Outline=3};e.Trigger=TriggerKind936.CC;e.ChannelsR9=-1;e.HoldCcA01=true;e.Gate=false;e.Attack=e.Release=0;}
  public static bool Connect(Effect936 e,GraphNode15 node,List<GraphEvent15> data){if(e.Kind!=EffectKind936.Texte||!e.TextR12.SetupPendingR13)return false;if(!data.Any(v=>v.Kind=="CC")){if(!data.Any(v=>v.Kind=="NoteOn"))return false;e.Trigger=TriggerKind936.Note;OutputPresetsR14.Apply(e,OutputPresetsR14.Builtins().First(p=>p.Name=="Gate lisible"));e.TextR12.SetupPendingR13=false;e.Trigger=TriggerKind936.Note;node.ActivationR4=new MidiSelectR4{Kind="Note"};node.ThresholdR4=false;return true;}node.ActivationR4=new MidiSelectR4{Kind="CC",FilterR12=new MidiFilterR4{KindsR12=new List<string>{"CC"}}};node.ThresholdR4=false;e.TextR12.SetupPendingR13=false;e.Trigger=TriggerKind936.CC;return true;}
  // Values remain stored unchanged. The guided renderer bounds the actual glyph and halo together.
  public static void Fit(GraphicsPath path,ref float line,int width,int height,ref double x,ref double y){var b=path.GetBounds();float margin=Math.Max(2,Math.Min(width,height)*.03f);float scale=Math.Min(1,Math.Min((width-2*margin)/(b.Width+2*line+2),(height-2*margin)/(b.Height+2*line+2)));if(scale<1){using(var transform=new Matrix()){transform.Scale(scale,scale);path.Transform(transform);}line*=scale;b=path.GetBounds();}double hx=b.Width/2+line+1,hy=b.Height/2+line+1;x=Limit(x,(margin+hx)*100/width,(width-margin-hx)*100/width);y=Limit(y,(margin+hy)*100/height,(height-margin-hy)*100/height);}
 }
 // Explicit output-only schema. No event selector, channel, CC number, MIDI binding or graph identity.
 public sealed class TextOutputPresetR13 {
  public int Version=1,Flags=3,FontStyle,Color=System.Drawing.Color.White.ToArgb(),OutlineColor=System.Drawing.Color.Black.ToArgb();
  public string Name="CC lisible",Prefix="",Suffix="",Font="Segoe UI",FontData="",FontExtension="";
  public double Size=108,Outline=3,Opacity=100,X=50,Y=50,StretchX=1,StretchY=1;
  public bool KeepAspect=true,FreeValues;
 }
 public static class TextOutputsR13 {
  public static string RootOverride;
  public static string Folder {get{return RootOverride??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Snkoz","Presets","Texte");}}
  public static TextOutputPresetR13 Pack(TextSettingsR12 t,string name){var p=new TextOutputPresetR13{Name=name,Flags=TextCcR10.Flags(t),Prefix=t.Prefix,Suffix=t.Suffix,Font=t.Font,FontStyle=t.FontStyle,Color=t.Color,OutlineColor=t.OutlineColor,Size=t.Size,Outline=t.Outline,Opacity=t.Opacity,X=t.X,Y=t.Y,StretchX=t.StretchXR2,StretchY=t.StretchYR2,KeepAspect=t.KeepAspectR2,FreeValues=t.FreeValuesR13};if(!string.IsNullOrEmpty(t.FontFileR2)){p.FontExtension=Path.GetExtension(t.FontFileR2).ToLowerInvariant();p.FontData=Convert.ToBase64String(File.ReadAllBytes(t.FontFileR2));}return p;}
  public static void Apply(TextSettingsR12 t,TextOutputPresetR13 p){if(p==null||p.Version!=1||p.Flags<1||p.Flags>3)throw new InvalidDataException("Preset Texte CC invalide : choisir numéro, valeur ou les deux.");var candidate=UiR2.Copy(t);candidate.Content=TextContentR12.Valeur_directe;candidate.Variable=TextVariableR12.Valeur_CC;candidate.CcDisplayR10=p.Flags;candidate.Prefix=p.Prefix;candidate.Suffix=p.Suffix;candidate.Font=p.Font;candidate.FontStyle=p.FontStyle;candidate.Color=p.Color;candidate.OutlineColor=p.OutlineColor;candidate.Size=p.Size;candidate.Outline=p.Outline;candidate.Opacity=p.Opacity;candidate.X=p.X;candidate.Y=p.Y;candidate.StretchXR2=p.StretchX;candidate.StretchYR2=p.StretchY;candidate.KeepAspectR2=p.KeepAspect;candidate.GuidedR13=true;candidate.FreeValuesR13=p.FreeValues;candidate.FontFileR2="";candidate.Validate();
   if(!string.IsNullOrEmpty(p.FontData)){if(p.FontExtension!=".ttf"&&p.FontExtension!=".otf")throw new InvalidDataException("Police du preset incompatible.");var bytes=Convert.FromBase64String(p.FontData);if(bytes.Length>20*1024*1024)throw new InvalidDataException("Police trop volumineuse.");string hash;using(var sha=System.Security.Cryptography.SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","");string fonts=Path.Combine(Folder,"Polices");Directory.CreateDirectory(fonts);candidate.FontFileR2=Path.Combine(fonts,hash+p.FontExtension);if(!File.Exists(candidate.FontFileR2))File.WriteAllBytes(candidate.FontFileR2,bytes);PersonalFontsR2.Family(candidate.FontFileR2);}
   // Keep the object identity used by live editor callbacks and preserve source-specific bindings.
   foreach(var field in typeof(TextSettingsR12).GetFields())field.SetValue(t,field.GetValue(candidate));
  }
 }
}
