using System;
using System.Drawing;
using System.Windows.Forms;
namespace Synkoz {
 public sealed class SeekBar942:Control {
  int value;public int Minimum=0,Maximum=10000;public TickStyle TickStyle;public int Value{get{return value;}set{this.value=Math.Max(Minimum,Math.Min(Maximum,value));Invalidate();}}
  public SeekBar942(){DoubleBuffered=true;TabStop=true;Height=35;}
  void At(int x){Value=(int)Math.Round(Minimum+(Maximum-Minimum)*Math.Max(0,Math.Min(1,(x-8.0)/Math.Max(1,Width-16))));}
  protected override void OnMouseDown(MouseEventArgs e){if(e.Button==MouseButtons.Left){At(e.X);Capture=true;Focus();}base.OnMouseDown(e);}
  protected override void OnMouseMove(MouseEventArgs e){if(Capture)At(e.X);base.OnMouseMove(e);}
  protected override void OnMouseUp(MouseEventArgs e){if(Capture)At(e.X);Capture=false;base.OnMouseUp(e);}
  protected override bool IsInputKey(Keys key){return key==Keys.Left||key==Keys.Right||base.IsInputKey(key);}
  protected override void OnKeyDown(KeyEventArgs e){if(e.KeyCode==Keys.Left)Value-=25;if(e.KeyCode==Keys.Right)Value+=25;base.OnKeyDown(e);}
  protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);int x=8+(int)((Width-16)*(Value-Minimum)/(double)Math.Max(1,Maximum-Minimum));e.Graphics.FillRectangle(Brushes.DimGray,8,Height/2-5,Math.Max(1,Width-16),10);e.Graphics.FillRectangle(Enabled?Brushes.DeepSkyBlue:Brushes.Gray,x-6,2,12,Height-4);}
 }
 public sealed partial class EffectsForm936 {
  Preview936 embedded942;Panel previewHost942;Form large942;
  void InitPreview942(TableLayoutPanel layout){var shell=new TableLayoutPanel{Dock=DockStyle.Fill,RowCount=2,ColumnCount=1};shell.RowStyles.Add(new RowStyle(SizeType.Absolute,32));shell.RowStyles.Add(new RowStyle(SizeType.Percent,100));var enlarge=new Button{Text="Agrandir le preview",Dock=DockStyle.Fill};shell.Controls.Add(enlarge,0,0);previewHost942=new Panel{Dock=DockStyle.Fill,BackColor=Color.Black};shell.Controls.Add(previewHost942,0,1);layout.Controls.Add(shell,2,1);layout.SetRowSpan(shell,3);InitTextPreviewR14(shell);embedded942=new Preview936{TopLevel=false,FormBorderStyle=FormBorderStyle.None,Dock=DockStyle.Fill,MinimumSize=Size.Empty};previewHost942.Controls.Add(embedded942);embedded942.CompactMidiR11(true);embedded942.Show();enlarge.Click+=delegate{if(large942!=null){WindowFocusR15.Raise(large942);return;}large942=new Form{Icon=embedded942.Icon,Text="Preview — agrandi",Size=new Size(1100,700),StartPosition=FormStartPosition.CenterParent};previewHost942.Controls.Remove(embedded942);large942.Controls.Add(embedded942);embedded942.CompactMidiR11(false);large942.FormClosing+=delegate{large942.Controls.Remove(embedded942);if(!IsDisposed){previewHost942.Controls.Add(embedded942);embedded942.CompactMidiR11(true);embedded942.Show();}};large942.FormClosed+=delegate{large942=null;};embedded942.ExitLargeR10=()=>{if(large942!=null)large942.Close();};large942.FormClosed+=delegate{embedded942.ExitLargeR10=null;};large942.Show(this);};FormClosed+=delegate{if(large942!=null)large942.Close();embedded942.Dispose();};}
  public void RenderPreview942(Montage m,int track,double start,double length){embedded942.AutomationCommitR21=AutomationCommitR21;embedded942.Render(m,track,start,length,m.Tracks[track].Fx.PreviewHeight941,true);}
 }
 public sealed partial class Preview936 {
  protected override void Dispose(bool disposing){if(disposing){clock.Stop();clock.Dispose();if(cancellation!=null)try{cancellation.Cancel();}catch(ObjectDisposedException){}if(player!=null)player.Stop();if(!rendering&&!savingR9)Cleanup();}base.Dispose(disposing);}
 }
}
