using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
namespace Synkoz {
 public sealed partial class CapturePanel {
  public Func<string> ReadMicroA01;public Action<string> WriteMicroA01;public Action StartMicroA01;
  readonly CheckBox microOnTakeA01=new CheckBox{Text="Micro → MIDI à la prise",Checked=true,AutoSize=true};
  readonly Dictionary<Track,MeterR8> metersA01=new Dictionary<Track,MeterR8>();
  readonly Dictionary<Capture,double> meterReadR8=new Dictionary<Capture,double>();
  void UpdateMetersA01(){foreach(var gone in meterReadR8.Keys.Where(c=>!active.Contains(c)&&!previews.Contains(c)).ToArray())meterReadR8.Remove(gone);foreach(var item in metersA01){var c=(recording?active:previews).FirstOrDefault(x=>x.Track==item.Key);if(c==null){item.Value.Text="Audio : arrêté";item.Value.ForeColor=Color.Gray;continue;}if(c.Failed){item.Value.Text="Audio : défaut";item.Value.ForeColor=Color.OrangeRed;continue;}double peak,age;lock(c.Gate){age=Engine.Epoch()-c.PeakAtA01;double last;if(age<1&&meterReadR8.TryGetValue(c,out last)&&last==c.PeakAtA01)continue;meterReadR8[c]=c.PeakAtA01;peak=c.PeakDbA01;c.PeakDbA01=double.NegativeInfinity;}if(age>1){item.Value.Text=c.Phone!=null?"Niveau Android indisponible":"Audio : attente";continue;}item.Value.Text="";item.Value.ForeColor=Color.LimeGreen;item.Value.Level=peak;item.Value.Invalidate();}}
 }
 sealed class MeterR8:Label {
  public double Level=double.NegativeInfinity;
  public MeterR8(){DoubleBuffered=true;}
  protected override void OnPaint(PaintEventArgs e){e.Graphics.Clear(Color.FromArgb(30,35,40));if(Text.Length>0){TextRenderer.DrawText(e.Graphics,Text,Font,ClientRectangle,ForeColor,TextFormatFlags.WordBreak|TextFormatFlags.HorizontalCenter);return;}TextRenderer.DrawText(e.Graphics,"Audio OK",Font,new Rectangle(0,0,Width,14),Color.LimeGreen,TextFormatFlags.HorizontalCenter);int barHeight=Math.Max(3,Height-30);double ratio=Math.Max(0,Math.Min(1,(Level+60)/60));using(var brush=new SolidBrush(Level>=-1?Color.OrangeRed:Level>=-12?Color.Gold:Color.LimeGreen))e.Graphics.FillRectangle(brush,2,15,(int)((Width-4)*ratio),barHeight);TextRenderer.DrawText(e.Graphics,(double.IsNegativeInfinity(Level)?"−∞":Level.ToString("0"))+" dBFS",Font,new Rectangle(0,Height-13,Width,13),Color.White,TextFormatFlags.HorizontalCenter|TextFormatFlags.NoPadding);}
 }
}
