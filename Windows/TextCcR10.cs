using System;using System.Linq;using System.Drawing;using System.Windows.Forms;
namespace Synkoz {
 public static class TextCcR10 {
  public static int Flags(TextSettingsR12 t){return t.CcDisplayR10>=0?t.CcDisplayR10:t.Variable==TextVariableR12.Numéro_CC?1:2;}
  public static string Format(int flags,int number,int value){return flags==3?"CC "+number+" : "+value:flags==1?number.ToString():flags==2?value.ToString():"";}
  public static bool IsCc(TextSettingsR12 t){return t.Content==TextContentR12.Valeur_directe&&(t.CcDisplayR10>=0||t.Variable==TextVariableR12.Numéro_CC||t.Variable==TextVariableR12.Valeur_CC);}
 }
 public sealed partial class EffectsForm936 {
  void TextCcFieldsR10(Effect936 e){var t=e.TextR12;int flags=TextCcR10.Flags(t);Action<int,bool> set=(bit,on)=>{int current=TextCcR10.Flags(t);int next=on?current|bit:current&~bit;if(next==0&&TextTrialR13.Guided(t))return;t.CcDisplayR10=next;t.Variable=(t.CcDisplayR10&2)!=0?TextVariableR12.Valeur_CC:TextVariableR12.Numéro_CC;};var boxes=new FlowLayoutPanel{Height=28,WrapContents=false};var number=new CheckBox{Text="Numéro CC",Checked=(flags&1)!=0,AutoSize=true};var value=new CheckBox{Text="Valeur",Checked=(flags&2)!=0,AutoSize=true};number.CheckedChanged+=delegate{set(1,number.Checked);number.Checked=(TextCcR10.Flags(t)&1)!=0;};value.CheckedChanged+=delegate{set(2,value.Checked);value.Checked=(TextCcR10.Flags(t)&2)!=0;};boxes.Controls.AddRange(new Control[]{number,value});Field("Afficher dans le texte",boxes);}
  void SelectTextCcR10(Effect936 e){if(editingGraph15==null)return;var selected=MidiFixR10.Resolve(editingGraph15.ActivationR4,draft.Graph15);if(selected.Kind!="CC")selected=MidiR4.Choices(IncomingR4(editingGraph15)).Where(v=>v.Kind=="CC").Select(v=>MidiFixR10.Resolve(v,draft.Graph15)).FirstOrDefault();if(selected!=null){editingGraph15.ActivationR4=selected;e.CC=e.TextR12.CC=selected.CC>=0?selected.CC:e.CC;}editingGraph15.ThresholdR4=false;}
  void AttachTextPreviewR10(bool show){if(textPreviewR14==null)return;var panel=textPreviewR14.Parent;panel.Parent.Controls.Remove(panel);if(show){panel.Dock=DockStyle.Bottom;panel.Height=185;parameterHost15.Controls.Add(panel);panel.SendToBack();editor.BringToFront();}else{panel.Dock=DockStyle.Fill;previewShellR14.Controls.Add(panel,0,2);}textKeyR14=null;}
  static Bitmap CheckerR10(){var b=new Bitmap(24,24);using(var g=Graphics.FromImage(b)){g.Clear(Color.FromArgb(65,65,65));g.FillRectangle(Brushes.DimGray,0,0,12,12);g.FillRectangle(Brushes.DimGray,12,12,12,12);}return b;}
 }
}
