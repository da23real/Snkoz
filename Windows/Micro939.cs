using System;
using System.Drawing;
using System.Windows.Forms;
namespace UrbanKaoz.DisplayRecorder {
 partial class MainForm {
  Control MicroPanel939(){var panel=new FlowLayoutPanel{AutoSize=true,WrapContents=false,BorderStyle=BorderStyle.FixedSingle,Margin=Padding.Empty};var run=new Button{Text="",Width=68,Height=32,AccessibleName="Conversion micro vers MIDI"};var offIcon=Synkoz.MicroIconR10.Create(false);var onIcon=Synkoz.MicroIconR10.Create(true);run.Image=offIcon;var tip=new ToolTip();tip.SetToolTip(run,"Activer / désactiver la conversion micro → MIDI / CC");var edit=new Button{Text="Edit",AutoSize=true};Style(run);Style(edit);panel.Controls.Add(run);panel.Controls.Add(edit);var status=new Label{AutoSize=true,MaximumSize=new Size(160,32)};panel.Controls.Add(status);run.Click+=delegate{EnsureAudio939();audioForm904.Toggle939();};edit.Click+=delegate{OpenAudio904();};var update=new Timer{Interval=250};update.Tick+=delegate{bool active=audioForm904!=null&&!audioForm904.IsDisposed;run.Image=active&&audioForm904.Running939?onIcon:offIcon;status.Text=active?audioForm904.Status939:"";};update.Start();panel.Disposed+=delegate{update.Dispose();offIcon.Dispose();onIcon.Dispose();tip.Dispose();};return panel;}
 }
}
