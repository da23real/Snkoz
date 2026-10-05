using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Synkoz {
 public sealed class Latency936:Form {
  public Latency936(Montage model,int track){Text="Latence — "+Sources936.Label(model.Tracks[track]);ClientSize=new Size(500,290);StartPosition=FormStartPosition.CenterParent;FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;
   var fx=UiR2.Copy(model.Tracks[track].Fx??new TrackFx936());var mode=new ComboBox{FormattingEnabled=true,DropDownStyle=ComboBoxStyle.DropDownList,Location=new Point(20,55),Width=200};mode.Items.AddRange(new object[]{"Fin — millisecondes","Image — 1/30 seconde"});mode.SelectedIndex=fx.ImageMode?1:0;
   var value=new NumericUpDown{Location=new Point(240,55),Width=170,Minimum=-3600000,Maximum=3600000};var display=new Label{Location=new Point(20,95),Width=460,Height=25};
   Action show=()=>{double seconds=mode.SelectedIndex==1?(double)value.Value/30:(double)value.Value/1000;display.Text=seconds.ToString("+0.000000000;-0.000000000;0")+" s"+(mode.SelectedIndex==1?"  ("+value.Value+" / 30 s)":"");};
   decimal fine=fx.Milliseconds,frames=fx.Frames;int previous=mode.SelectedIndex;
   Action setup=()=>{value.DecimalPlaces=mode.SelectedIndex==1?0:3;value.Increment=1;value.Value=Math.Max(value.Minimum,Math.Min(value.Maximum,mode.SelectedIndex==1?frames:fine));show();};setup();
   mode.SelectedIndexChanged+=delegate{if(previous==1)frames=value.Value;else fine=value.Value;previous=mode.SelectedIndex;setup();};value.ValueChanged+=delegate{show();};
   var only=new CheckBox{Text="Décaler seulement la vidéo (audio inchangé)",Location=new Point(20,135),Width=455,Checked=fx.VideoOnly,Enabled=model.Tracks[track].Video!=null&&Engine.HasAudio(model.Tracks[track])};
   var ok=new Button{Text="Appliquer",Location=new Point(290,195),Width=90};ok.Click+=delegate{if(mode.SelectedIndex==1)frames=value.Value;else fine=value.Value;fx.ImageMode=mode.SelectedIndex==1;fx.Frames=(int)frames;fx.Milliseconds=fine;fx.VideoOnly=only.Enabled&&only.Checked;model.Tracks[track].Fx=fx;try{model.Save();DialogResult=DialogResult.OK;}catch(Exception ex){MessageBox.Show(this,ex.Message);}};
   var pv=new Button{Text="Preview latence…",Location=new Point(20,235),Width=180};pv.Click+=delegate{try{if(mode.SelectedIndex==1)frames=value.Value;else fine=value.Value;fx.ImageMode=mode.SelectedIndex==1;fx.Frames=(int)frames;fx.Milliseconds=fine;fx.VideoOnly=only.Enabled&&only.Checked;var snap=Engine.Snapshot936(model);snap.Tracks[track].Fx=UiR2.Copy(fx);using(var range=new RangeFormR3(snap,true)){if(range.ShowDialog(this)==DialogResult.OK){var player=new Preview936();player.Show(this);player.Render(snap,track,range.Origin,range.Length,range.HeightChoice,true);}}}catch(Exception ex){MessageBox.Show(this,ex.Message);}};Controls.Add(pv);
   var cancel=new Button{Text="Annuler",Location=new Point(390,195),DialogResult=DialogResult.Cancel};Controls.AddRange(new Control[]{new Label{Text="Positif : retarder. Négatif : avancer. Noir/silence aux extrémités.",Location=new Point(20,15),Width=465,Height=30},mode,value,display,only,ok,cancel});CancelButton=cancel;
  }
 }

}
