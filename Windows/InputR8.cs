using System;
using System.Drawing;
using System.Windows.Forms;
namespace Synkoz {
 static class WheelR8 {
  public static void Scroll(Control control,MouseEventArgs e){var handled=e as HandledMouseEventArgs;if(handled!=null)handled.Handled=true;for(var p=control.Parent;p!=null;p=p.Parent){var s=p as ScrollableControl;if(s==null||!s.AutoScroll||!s.VerticalScroll.Visible)continue;int step=SystemInformation.MouseWheelScrollLines<0?s.ClientSize.Height:Math.Max(1,SystemInformation.MouseWheelScrollLines)*20;int y=Math.Max(0,-s.AutoScrollPosition.Y-e.Delta*step/120);s.AutoScrollPosition=new Point(0,y);return;}}
 }
 public class ScrollTrackR8:TrackBar {protected override void OnMouseWheel(MouseEventArgs e){WheelR8.Scroll(this,e);}}
 public class ScrollNumberR8:NumericUpDown {protected override void OnMouseWheel(MouseEventArgs e){WheelR8.Scroll(this,e);}}
 public class ScrollComboR8:ComboBox {protected override void OnMouseWheel(MouseEventArgs e){WheelR8.Scroll(this,e);}}
}
