using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Collections.Generic;using System.Windows.Forms;
namespace Synkoz {
 public sealed partial class CapturePanel {
  readonly Dictionary<Track,Control> namesR10=new Dictionary<Track,Control>();readonly HashSet<Track> healthyR10=new HashSet<Track>();
  internal static Color HealthColorR10(Capture c,bool known,bool previewOff){if(c!=null&&c.Failed)return Color.OrangeRed;if(c!=null&&c.DisconnectedR8)return Color.Orange;if(c!=null&&!c.Closed)return Color.LimeGreen;return previewOff&&known?Color.LimeGreen:Color.Gray;}
  void NameHealthR10(Track t,Capture c){if(c!=null&&!c.Failed&&!c.DisconnectedR8&&!c.Closed)healthyR10.Add(t);if(c!=null&&c.Failed)healthyR10.Remove(t);Control name;if(namesR10.TryGetValue(t,out name)&&!name.IsDisposed)name.ForeColor=HealthColorR10(c,healthyR10.Contains(t),t.PreviewOffR9||t.CollapsedR9);}
 }
 public static class MicroIconR10 {public static Bitmap Create(bool active){var image=new Bitmap(58,28);using(var g=Graphics.FromImage(image))using(var pen=new Pen(active?Color.LimeGreen:Color.SteelBlue,1.7f)){g.SmoothingMode=SmoothingMode.AntiAlias;for(int x=2;x<57;x+=3){if(x>21&&x<37)continue;float amp=2+9*Math.Abs(x-29)/27f;g.DrawLine(pen,x,14-amp,x,14+amp);}g.DrawArc(pen,24,2,10,10,180,180);g.DrawArc(pen,24,9,10,10,0,180);g.DrawLine(pen,24,7,24,14);g.DrawLine(pen,34,7,34,14);g.DrawArc(pen,21,8,16,15,0,180);g.DrawLine(pen,29,23,29,26);g.DrawLine(pen,24,26,34,26);}return image;}}
}
