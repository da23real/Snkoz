using System;
using System.Drawing;
using System.Windows.Forms;
namespace UrbanKaoz.DisplayRecorder {
 partial class MainForm {
  Control MicroPanel939(){var panel=new FlowLayoutPanel{AutoSize=true,WrapContents=false,BorderStyle=BorderStyle.FixedSingle,Margin=Padding.Empty};var run=new Button{Text="🎤 → ♪",AutoSize=true};var edit=new Button{Text="Edit",AutoSize=true};Style(run);Style(edit);panel.Controls.Add(run);panel.Controls.Add(edit);var status=new Label{AutoSize=true,MaximumSize=new Size(160,32)};panel.Controls.Add(status);run.Click+=delegate{EnsureAudio939();audioForm904.Toggle939();};edit.Click+=delegate{OpenAudio904();};var update=new Timer{Interval=250};update.Tick+=delegate{bool active=audioForm904!=null&&!audioForm904.IsDisposed;run.Text=active&&audioForm904.Running939?"■ 🎤 → ♪":"▶ 🎤 → ♪";status.Text=active?audioForm904.Status939:"";};update.Start();panel.Disposed+=delegate{update.Dispose();};return panel;}
 }
}
