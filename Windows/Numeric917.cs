using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
namespace UrbanKaoz.DisplayRecorder {
 internal sealed class NumericEntry917 : Form {
  internal readonly TextBox Input=new TextBox{Dock=DockStyle.Top};
  readonly Label help=new Label{Dock=DockStyle.Fill,AutoSize=false};
  readonly int minimum,maximum;
  readonly Action<int> commit;
  bool submitted;
  internal static bool Compatible(string firmware){Version v;if(firmware=="V1"||firmware=="A01")return true;if(firmware.StartsWith("VA",StringComparison.OrdinalIgnoreCase)&&Version.TryParse(firmware.Substring(2),out v))return v.Major>=1;return Version.TryParse(firmware.TrimStart('V','v'),out v)&&v>=new Version(9,17);}
  internal NumericEntry917(string title,int value,int lo,int hi,Action<int> apply){
   minimum=lo;maximum=hi;commit=apply;Text=title;Size=new Size(360,150);FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;MinimizeBox=false;ShowInTaskbar=false;StartPosition=FormStartPosition.CenterParent;Padding=new Padding(12);KeyPreview=true;
   Input.Text=value.ToString(CultureInfo.InvariantCulture);help.Text=lo+" à "+hi+"\r\nEntrée : valider · Échap : annuler";Controls.Add(help);Controls.Add(Input);
   Shown+=delegate{Input.Focus();Input.SelectAll();};
  }
  internal bool Submit(){
   int value;if(submitted)return false;
   if(!int.TryParse(Input.Text,NumberStyles.Integer,CultureInfo.InvariantCulture,out value)||value<minimum||value>maximum){help.Text="Entrer un entier de "+minimum+" à "+maximum+".\r\nEntrée : valider · Échap : annuler";return false;}
   submitted=true;commit(value);Close();return true;
  }
  protected override bool ProcessCmdKey(ref Message msg,Keys key){if(key==Keys.Enter){Submit();return true;}if(key==Keys.Escape){Close();return true;}return base.ProcessCmdKey(ref msg,key);}
 }
 internal sealed partial class Surface {
  public bool NumericAvailable917;
  readonly Timer clickTimer917=new Timer{Interval=SystemInformation.DoubleClickTime};
  HardwareControl pendingClick917;
  NumericEntry917 numericForm917;
  int requestedNumeric917=-1;
  string numericLabel917;
  internal bool NumericHasFocus917 {get{return numericForm917!=null&&!numericForm917.IsDisposed;}}
  void InitNumeric917(){clickTimer917.Tick+=delegate{FlushClick917();};}
  void QueueClick917(HardwareControl control){FlushClick917();pendingClick917=control;clickTimer917.Start();}
  void FlushClick917(){clickTimer917.Stop();var c=pendingClick917;pendingClick917=null;if(c!=null&&Connected){Edge(c,true);Edge(c,false);}}
  void CancelNumeric917(){clickTimer917.Stop();pendingClick917=null;requestedNumeric917=-1;if(numericForm917!=null){var f=numericForm917;numericForm917=null;f.Close();}}
  bool NumericMouseDown917(MouseEventArgs e,HardwareControl c){
   if(e.Button!=MouseButtons.Left||!Connected)return false;
   if(e.Clicks<2){if(pendingClick917!=null&&pendingClick917!=c)FlushClick917();return false;}
   if((ModifierKeys&Keys.Control)!=0)return false;
   int id=-1;string label=null;
   if(c!=null&&c.Encoder){id=c.Id+(c.Kind=="RB"?16:0);label=c.Kind=="RB"?"O"+(char)('A'+c.Id):"K"+(c.Id+1);}
   else {var at=Logical(e.Location);if(at.Y>=0&&at.Y<47&&at.X>=2&&at.X<1234){id=Math.Min(15,(int)((at.X-2)/77));label="K"+(id+1);}}
   if(id<0)return false;
   clickTimer917.Stop();pendingClick917=null;active=null;Capture=false;
   if(!NumericAvailable917){Notice("La saisie directe nécessite le Teensy V9.17 ou ultérieur.");return true;}
   if(numericForm917!=null){numericForm917.Activate();return true;}
   requestedNumeric917=id;numericLabel917=label;Send("VGET,"+id);return true;
  }
  internal bool AcceptNumeric917(string packet){
   if(packet.StartsWith("PC1,VALUE_ERROR,")){requestedNumeric917=-1;Notice(packet.EndsWith("CONTEXT_CHANGED")?"Saisie abandonnée : le contexte a changé. Refaire un double clic.":packet.EndsWith("NO_NUMERIC_VALUE")?"Ce contrôle n'a pas de valeur numérique éditable dans cette vue.":"Valeur refusée par le séquenceur.");return true;}
   if(packet=="PC1,VALUE_OK"){Notice("Valeur validée.");return true;}
   if(!packet.StartsWith("PC1,VALUE,"))return false;
   var p=packet.Split(',');int id,token,value,lo,hi;
   if(p.Length!=7||!int.TryParse(p[2],out id)||!int.TryParse(p[3],out token)||!int.TryParse(p[4],out value)||!int.TryParse(p[5],out lo)||!int.TryParse(p[6],out hi)||id!=requestedNumeric917||!Connected)return true;
   requestedNumeric917=-1;if(numericForm917!=null)return true;
   var form=new NumericEntry917(numericLabel917+" — valeur",value,lo,hi,delegate(int entered){if(Connected)Send("VSET,"+id+","+entered.ToString(CultureInfo.InvariantCulture)+","+token);});numericForm917=form;
   form.FormClosed+=delegate{if(numericForm917==form)numericForm917=null;form.Dispose();};form.Show(FindForm());return true;
  }
 }
}

