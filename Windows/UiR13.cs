using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
namespace Synkoz {
 static class MenuLifetimeR13 {
  // Closed runs inside ToolStrip's mouse dispatch. Dispose only after it unwinds.
  public static void Attach(ContextMenuStrip menu,Control owner){
   EventHandler cleanup=null;
   cleanup=delegate{owner.Disposed-=cleanup;if(!menu.IsDisposed)menu.Dispose();};
   owner.Disposed+=cleanup;
   menu.Closed+=delegate{if(!owner.IsDisposed&&owner.IsHandleCreated)owner.BeginInvoke(new Action(delegate{cleanup(null,EventArgs.Empty);}));};
   menu.Disposed+=delegate{owner.Disposed-=cleanup;};
  }
 }
 static class SectionIconsR13 {
  public static Bitmap Create(string section){var b=new Bitmap(24,24);using(var g=Graphics.FromImage(b))using(var ink=new Pen(Color.FromArgb(35,65,76),1.6f))using(var teal=new SolidBrush(Color.FromArgb(0,166,181)))using(var gold=new Pen(Color.FromArgb(221,153,31),2)){
   g.SmoothingMode=SmoothingMode.AntiAlias;
   if(section=="Prise") {g.FillRectangle(teal,3,7,13,12);g.DrawRectangle(ink,3,7,13,12);g.DrawPolygon(ink,new[]{new Point(17,10),new Point(22,7),new Point(22,19),new Point(17,16)});g.DrawEllipse(ink,6,10,6,6);g.DrawLine(ink,6,4,12,4);}
   else if(section=="Montage") {g.DrawLine(ink,2,4,22,4);g.FillRectangle(teal,3,7,11,4);g.FillRectangle(teal,9,13,12,4);g.FillRectangle(teal,3,19,14,3);g.DrawLine(gold,16,2,16,23);}
   else {g.FillRectangle(teal,5,3,14,18);g.DrawRectangle(ink,5,3,14,18);for(int y=5;y<=19;y+=4){g.DrawLine(ink,2,y,5,y);g.DrawLine(ink,19,y,22,y);}g.FillRectangle(Brushes.Silver,9,1,6,5);g.DrawRectangle(ink,9,1,6,5);g.FillRectangle(Brushes.DarkSlateGray,9,10,6,7);g.DrawLine(gold,7,7,7,9);}
  }return b;}
 }
 public sealed partial class EffectsForm936 {
  void TextModeR13(Effect936 e){var t=e.TextR12;string[] modes={"Texte personnalisé","Nom de note","CC","Valeur directe","Correspondances","Lettres","Mots","Phrases"};int selected=t.Content==TextContentR12.Texte_fixe?0:t.Content==TextContentR12.Nom_de_note?1:TextCcR10.IsCc(t)?2:t.Content==TextContentR12.Valeur_directe?3:t.Content==TextContentR12.Correspondances?4:t.Content==TextContentR12.Lettres?5:t.Content==TextContentR12.Mots?6:7;var combo=TextComboR12(modes,modes[selected]);combo.SelectedIndexChanged+=delegate{int mode=combo.SelectedIndex;if(mode<0)return;t.CcDisplayR10=-1;t.Content=mode==0?TextContentR12.Texte_fixe:mode==1?TextContentR12.Nom_de_note:mode<=3?TextContentR12.Valeur_directe:mode==4?TextContentR12.Correspondances:mode==5?TextContentR12.Lettres:mode==6?TextContentR12.Mots:TextContentR12.Phrases;if(mode==2){t.CcDisplayR10=2;t.Variable=TextVariableR12.Valeur_CC;t.AllCC=true;e.Trigger=TriggerKind936.CC;e.Gate=false;e.HoldCcA01=true;SelectTextCcR10(e);}else if(mode<=1){e.Trigger=TriggerKind936.Note;t.Variable=TextVariableR12.Hauteur_de_note;}else if(mode==3)t.Variable=TextVariableR12.Hauteur_de_note;QueueRefresh938(list.SelectedIndex);};Field("Type de texte",combo);if(TextCcR10.IsCc(t))TextCcFieldsR10(e);}
 }
}
