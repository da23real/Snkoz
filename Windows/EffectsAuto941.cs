using System;
using System.Drawing;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows.Forms;
namespace Synkoz {
 public sealed partial class EffectsForm936 {
  void InitAuto941(){var timer=new Timer{Interval=400};var json=new JavaScriptSerializer{MaxJsonLength=int.MaxValue};string saved=json.Serialize(draft);timer.Tick+=delegate{if(refreshing)return;string next=json.Serialize(draft);if(next!=saved&&Apply())saved=next;};timer.Start();FormClosing+=delegate(object s,FormClosingEventArgs e){timer.Stop();if(json.Serialize(draft)!=saved&&!Apply()){e.Cancel=true;timer.Start();}};FormClosed+=delegate{timer.Dispose();};
   editor.ControlAdded+=delegate(object s,ControlEventArgs e){Help941(e.Control);};foreach(Control c in editor.Controls)Help941(c);
  }
  void Help941(Control c){c.MouseUp+=delegate(object s,MouseEventArgs e){if(e.Button!=MouseButtons.Right)return;var fx=list.SelectedItem as Effect936;if(fx==null)return;string text=fx.Kind==EffectKind936.Déplacement_X||fx.Kind==EffectKind936.Déplacement_Y?"Déplacement : commencer vers 3 % ; 15 % déplace fortement l’image.":fx.Kind==EffectKind936.Saturation?"Saturation : −100 = gris, +100 = saturation doublée ; jusqu’à +300 pour un effet marqué.":fx.Kind==EffectKind936.Teinte?"Teinte : rotation en degrés ; essayer ±30°, puis ±100°.":fx.Kind==EffectKind936.Mute?"Mute atténue les effets situés APRÈS lui dans la liste ; 100 % les coupe complètement.":"Commencer avec une amplitude faible. Avance négative / retard positif en millisecondes. Les valeurs valides sont mémorisées automatiquement.";var menu=new ContextMenuStrip();menu.Items.Add(text).Enabled=false;menu.Closed+=delegate{menu.Dispose();};menu.Show(c,e.Location);};foreach(Control child in c.Controls)Help941(child);}
 }
}
