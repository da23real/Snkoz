using System;using System.Drawing;using System.Linq;using System.Collections.Generic;using System.Windows.Forms;
namespace Synkoz {
sealed class LeftMenuR9:ToolStripMenuItem{public LeftMenuR9(string text):base(text){}protected override void OnMouseUp(MouseEventArgs e){if(e.Button==MouseButtons.Left)base.OnMouseUp(e);}}
public sealed partial class EffectsForm936 {
 void PreviewSettingsR9(){if(previewDockR6!=null){previewDockR6.Visible=true;previewDockR6.BringToFront();startR14.Focus();}}
}
public static class RenderStagesR9 {
 [ThreadStatic]public static Action<string,int> Reporter;
 [ThreadStatic]static System.Diagnostics.Stopwatch watch;
 public static Action<int> Progress(string title){var report=Reporter;return p=>{if(report!=null)report(title,p);};}
 public static void Reset(){watch=System.Diagnostics.Stopwatch.StartNew();}
 public static void Report(string stage,int percent){if(watch==null)watch=System.Diagnostics.Stopwatch.StartNew();if(Reporter!=null)Reporter(stage,percent);if(percent==0||percent==100)LogR9.Write("Rendu "+stage+" "+percent+"% elapsed="+watch.ElapsedMilliseconds+"ms");}
}
public sealed partial class Preview936 {
 FlowLayoutPanel stagesR9;readonly Dictionary<string,Tuple<Label,ProgressBar>> stageRowsR9=new Dictionary<string,Tuple<Label,ProgressBar>>();
 void InitStagesR9(Panel host){stagesR9=new FlowLayoutPanel{Dock=DockStyle.Top,AutoSize=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,BackColor=Color.FromArgb(30,30,34),Visible=false};host.Controls.Add(stagesR9);stagesR9.BringToFront();stagesR9.SizeChanged+=delegate{foreach(var row in stageRowsR9.Values)row.Item2.Width=Math.Max(120,host.ClientSize.Width-12);};}
 void StageR9(string title,int percent){if(IsDisposed||!IsHandleCreated)return;BeginInvoke((Action)(()=>{if(!rendering)return;Tuple<Label,ProgressBar> row;if(!stageRowsR9.TryGetValue(title,out row)){var label=new Label{AutoSize=true,ForeColor=Color.White};var bar=new ProgressBar{Width=Math.Max(120,stagesR9.ClientSize.Width-12),Height=7};row=Tuple.Create(label,bar);stageRowsR9[title]=row;stagesR9.Controls.Add(label);stagesR9.Controls.Add(bar);}row.Item1.Text=title+(percent<0?" · en cours":" · "+percent+" %");row.Item2.Style=percent<0?ProgressBarStyle.Marquee:ProgressBarStyle.Continuous;if(percent>=0)row.Item2.Value=Math.Max(0,Math.Min(100,percent));stagesR9.Visible=true;stagesR9.BringToFront();}));}
 public void CommandsR9(FlowLayoutPanel actions){var buttons=play.Parent as FlowLayoutPanel;if(buttons==null)return;foreach(Control c in actions.Controls.Cast<Control>().ToArray()){actions.Controls.Remove(c);c.Tag="commandR9";var b=c as Button;if(b!=null)PreviewIconsR9.Set(b,b.Text=="↻"?5:b.Text=="⚙"?6:7);buttons.Controls.Add(c);}}
 public void CompactR9(bool compact){if(midiLayoutR11==null)return;var buttons=play.Parent as FlowLayoutPanel;if(buttons!=null){buttons.WrapContents=false;buttons.AutoScroll=false;var tips=new ToolTip();foreach(Button b in buttons.Controls.OfType<Button>()){if((b.Tag as string)=="commandR9")continue;if(b.Tag==null)b.Tag=b.Text;tips.SetToolTip(b,(string)b.Tag);PreviewIconsR9.Set(b,b==play?0:b==pause?1:b==saveR9?2:b==cancelButton?3:4);b.AutoSize=false;b.Width=30;b.Height=26;b.Margin=new Padding(2,1,2,1);}buttons.Disposed+=delegate{tips.Dispose();};}midiLayoutR11.RowStyles[4].Height=compact?0:22;midiLayoutR11.RowStyles[5].Height=compact?0:28;}
}
}
