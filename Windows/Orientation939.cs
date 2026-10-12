using System;
using System.Drawing;
using System.Windows.Forms;
namespace Synkoz {
 public sealed class Orientation939:Form {
  readonly Bitmap original;readonly PictureBox image=new PictureBox{Dock=DockStyle.Fill,SizeMode=PictureBoxSizeMode.Zoom,BackColor=Color.DimGray};
  public int Rotation;public bool MirrorX,MirrorY;
  public Orientation939(Bitmap source,TrackFx936 fx){original=new Bitmap(source);Rotation=fx.Rotation;MirrorX=fx.MirrorX939;MirrorY=fx.MirrorY939;Text="Orientation — aperçu";ClientSize=new Size(600,420);StartPosition=FormStartPosition.CenterParent;Controls.Add(image);var bar=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=75};Controls.Add(bar);Action<string,Action> add=(label,action)=>{var b=new Button{Text=label,AutoSize=true};b.Click+=delegate{action();RefreshImage();};bar.Controls.Add(b);};add("↶ 90°",()=>Rotation=(Rotation+270)%360);add("↷ 90°",()=>Rotation=(Rotation+90)%360);add("Miroir horizontal",()=>MirrorX=!MirrorX);add("Miroir vertical",()=>MirrorY=!MirrorY);add("Réinitialiser",()=>{Rotation=0;MirrorX=MirrorY=false;});bar.Controls.Add(new Button{Text="Valider",DialogResult=DialogResult.OK});bar.Controls.Add(new Button{Text="Annuler",DialogResult=DialogResult.Cancel});RefreshImage();FormClosed+=delegate{original.Dispose();if(image.Image!=null)image.Image.Dispose();};}
  void RefreshImage(){var b=new Bitmap(original);b.RotateFlip(Rotation==90?RotateFlipType.Rotate90FlipNone:Rotation==180?RotateFlipType.Rotate180FlipNone:Rotation==270?RotateFlipType.Rotate270FlipNone:RotateFlipType.RotateNoneFlipNone);if(MirrorX)b.RotateFlip(RotateFlipType.RotateNoneFlipX);if(MirrorY)b.RotateFlip(RotateFlipType.RotateNoneFlipY);var old=image.Image;image.Image=b;if(old!=null)old.Dispose();}
 }
}
