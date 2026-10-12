using System;
using System.Drawing;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows.Forms;
namespace Synkoz {
 public sealed partial class EffectsForm936 {
  void InitAuto941(){RememberCommitR12=()=>{Apply();if(embedded942!=null)embedded942.PauseR12();};FormClosing+=delegate(object s,FormClosingEventArgs e){Apply();};FormClosed+=delegate{helpR8.Dispose();};
   editor.ControlAdded+=delegate(object s,ControlEventArgs e){Help941(e.Control);};foreach(Control c in editor.Controls)Help941(c);
  }
  readonly ContextMenuStrip helpR8=new ContextMenuStrip();
  void Help941(Control c){c.MouseUp+=delegate(object s,MouseEventArgs e){if(e.Button!=MouseButtons.Right)return;var fx=list.SelectedItem as Effect936;if(fx==null)return;string text=fx.Kind==EffectKind936.Déplacement_X||fx.Kind==EffectKind936.Déplacement_Y?"Déplacement : commencer vers 3 % ; 15 % déplace fortement l’image.":fx.Kind==EffectKind936.Saturation?"Saturation : −100 = gris, +100 = saturation doublée ; jusqu’à +300 pour un effet marqué.":fx.Kind==EffectKind936.Teinte?"Teinte : rotation en degrés ; essayer ±30°, puis ±100°.":fx.Kind==EffectKind936.Mute?"Mute atténue les effets situés APRÈS lui dans la liste ; 100 % les coupe complètement.":"Commencer avec une amplitude faible. Avance négative / retard positif en durée musicale. Les valeurs valides sont mémorisées automatiquement.";if(fx.Kind==EffectKind936.Texte&&TextCcR10.IsCc(fx.TextR12))text=string.IsNullOrEmpty(c.AccessibleDescription)?TextHelpR13(c.Text):c.AccessibleDescription;helpR8.Close();helpR8.Items.Clear();helpR8.Items.Add(text).Enabled=false;helpR8.Show(c,e.Location);};foreach(Control child in c.Controls)Help941(child);}
 }
}

