using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
namespace Synkoz {
 public sealed partial class EffectsForm936 {
  Label animationSummary938;bool queued938;
  void QueueRefresh938(int selected){if(QueueTextRefreshR13(selected))return;if(queued938)return;queued938=true;BeginInvoke((Action)(()=>{queued938=false;if(!IsDisposed)RefreshList(selected);}));}
  void UpdateAnimationSummary938(Effect936 e){if(animationSummary938!=null&&!animationSummary938.IsDisposed)animationSummary938.Text=AnimationRules938.Summary(e);}
  void TimestampFields938(Effect936 e){
   var measure=UiR2.Number(e.TimestampMeasure938,1,1000000,0);measure.ValueChanged+=delegate{e.TimestampMeasure938=(int)measure.Value;};Field("Mesure (à partir de 1)",measure);
   var beat=UiR2.Number(e.TimestampTime938,1,4,0);beat.ValueChanged+=delegate{e.TimestampTime938=(int)beat.Value;};Field("Temps (1 à 4)",beat);
   var tick=UiR2.Number(e.TimestampTickR11>=0?e.TimestampTickR11:e.TimestampQuarter938*24,0,95,0);tick.ValueChanged+=delegate{e.TimestampTickR11=(int)tick.Value;};Field("Tick dans la noire (0–95)",tick);
   editor.Controls.Add(new Label{AutoSize=true,Text="Ce moment correspond à l’image repère. Aucun événement MIDI requis.\nLa position utilise les repères musicaux de la prise (4/4)."});
  }
  void AnimationFrames938(Effect936 e){
   animationSummary938=new Label{AutoSize=true,MaximumSize=new Size(Math.Max(420,ClientSize.Width-300),0),Text=AnimationRules938.Summary(e)};
   var choose=new Button{Text="Voir / choisir les images…",AutoSize=true};choose.Click+=delegate{try{using(var picker=new AnimationPicker938(model,e))if(picker.ShowDialog(this)==DialogResult.OK){e.StartImage938=picker.StartImage;e.SyncImage938=picker.SyncImage;BeginInvoke((Action)(()=>{if(!IsDisposed)ShowEffect();}));}}catch(Exception ex){status.Text=ex.Message;Synkoz.ErrorR8.Show(this,ex.Message,"Erreur");}};Field("Départ / image repère",choose);
   try{using(var asset=new AnimationAsset938(Store934.MediaPath(model.Folder,e.Asset))){var thumbs=new FlowLayoutPanel{Width=Math.Max(430,ClientSize.Width-290),Height=120,WrapContents=false};foreach(var item in new[]{Tuple.Create("Départ",e.StartImage938),Tuple.Create("Au déclenchement",e.SyncImage938)}){
      int n=Math.Max(1,Math.Min(asset.Count,item.Item2));var box=new PictureBox{Size=new Size(128,92),SizeMode=PictureBoxSizeMode.Zoom,BackColor=Color.FromArgb(45,45,50)};using(var frame=asset.Frame(n-1)){box.Image=new Bitmap(128,92);using(var g=Graphics.FromImage(box.Image)){g.Clear(Color.FromArgb(45,45,50));float s=Math.Min(128f/frame.Width,92f/frame.Height);g.DrawImage(frame,(128-frame.Width*s)/2,(92-frame.Height*s)/2,frame.Width*s,frame.Height*s);}}box.Disposed+=delegate{if(box.Image!=null)box.Image.Dispose();};var panel=new Panel{Width=150,Height=115};panel.Controls.Add(box);panel.Controls.Add(new Label{Text=item.Item1+" : "+n,Location=new Point(0,94),AutoSize=true});thumbs.Controls.Add(panel);
     }editor.Controls.Add(thumbs);}}catch(Exception ex){editor.Controls.Add(new Label{Text="Images indisponibles : "+ex.Message,AutoSize=true});}
   editor.Controls.Add(animationSummary938);
  }
 }
 public sealed class AnimationPicker938:Form {
  public int StartImage{get;private set;}public int SyncImage{get;private set;}
  readonly AnimationAsset938 asset;readonly FrameGrid938 grid;readonly Label info=new Label{Dock=DockStyle.Fill};readonly bool cc;readonly Effect936 effect;
  public AnimationPicker938(Montage model,Effect936 fx){effect=fx;StartImage=fx.StartImage938;SyncImage=fx.SyncImage938;cc=fx.Trigger==TriggerKind936.CC;asset=new AnimationAsset938(Store934.MediaPath(model.Folder,fx.Asset));
   Text="Animation — image de départ et image au déclenchement";UiR2.Fit(this,900,580);
   var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=4};layout.RowStyles.Add(new RowStyle(SizeType.Absolute,45));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,70));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,38));Controls.Add(layout);
   var top=new FlowLayoutPanel{Dock=DockStyle.Fill};var start=new RadioButton{Text="Choisir l’image de départ",Checked=true,AutoSize=true,Enabled=!cc};var sync=new RadioButton{Text="Choisir l’image au déclenchement",AutoSize=true,Enabled=!cc};top.Controls.Add(start);top.Controls.Add(sync);top.Controls.Add(new Label{Text=cc?"CC : grille consultative, toutes les images restent pilotables.":"Cliquer une image ci-dessous",AutoSize=true});layout.Controls.Add(top,0,0);
   grid=new FrameGrid938(asset);grid.Dock=DockStyle.Fill;grid.StartImage=StartImage;grid.SyncImage=SyncImage;grid.Chosen=i=>{if(cc)return;if(start.Checked)StartImage=i;else SyncImage=i;grid.StartImage=StartImage;grid.SyncImage=SyncImage;grid.Invalidate();Summary();};layout.Controls.Add(grid,0,1);layout.Controls.Add(info,0,2);
   var bottom=new FlowLayoutPanel{Dock=DockStyle.Fill};var previous=new Button{Text="◀ Images",AutoSize=true};var next=new Button{Text="Images ▶",AutoSize=true};var page=new Label{AutoSize=true,Margin=new Padding(8)};Action update=()=>{previous.Enabled=grid.Page>0;next.Enabled=(grid.Page+1)*8<asset.Count;page.Text="Images "+(grid.Page*8+1)+"–"+Math.Min(asset.Count,(grid.Page+1)*8)+" / "+asset.Count;};previous.Click+=delegate{grid.SetPage(grid.Page-1);update();};next.Click+=delegate{grid.SetPage(grid.Page+1);update();};bottom.Controls.Add(previous);bottom.Controls.Add(next);bottom.Controls.Add(page);
   var ok=new Button{Text="Valider",AutoSize=true};ok.Click+=delegate{try{var candidate=UiR2.Copy(fx);candidate.StartImage938=StartImage;candidate.SyncImage938=SyncImage;candidate.Validate();AnimationRules938.ValidateFrames(candidate,asset.Count);DialogResult=DialogResult.OK;}catch(Exception ex){info.Text=ex.Message;Synkoz.ErrorR8.Show(this,ex.Message,"Erreur");}};bottom.Controls.Add(ok);bottom.Controls.Add(new Button{Text="Annuler",DialogResult=DialogResult.Cancel,AutoSize=true});layout.Controls.Add(bottom,0,3);update();Summary();FormClosed+=delegate{grid.Dispose();asset.Dispose();};
  }
  void Summary(){var e=UiR2.Copy(effect);e.StartImage938=StartImage;e.SyncImage938=SyncImage;info.Text=AnimationRules938.Summary(e)+"\nLecture : image de départ → dernière image. Les boucles reviennent au départ choisi.";}
 }
 public sealed class FrameGrid938:Control {
  readonly AnimationAsset938 asset;readonly List<Bitmap> thumbnails=new List<Bitmap>();public int Page{get;private set;}public int StartImage=1,SyncImage=1;public Action<int> Chosen;
  public FrameGrid938(AnimationAsset938 source){asset=source;DoubleBuffered=true;BackColor=Color.FromArgb(25,28,35);SetPage(0);}
  public void SetPage(int page){Page=Math.Max(0,Math.Min((asset.Count-1)/8,page));foreach(var b in thumbnails)b.Dispose();thumbnails.Clear();for(int i=Page*8;i<Math.Min(asset.Count,(Page+1)*8);i++)using(var frame=asset.Frame(i)){var b=new Bitmap(180,140);using(var g=Graphics.FromImage(b)){g.Clear(Color.Transparent);g.InterpolationMode=InterpolationMode.NearestNeighbor;float s=Math.Min(180f/frame.Width,140f/frame.Height);g.DrawImage(frame,(180-frame.Width*s)/2,(140-frame.Height*s)/2,frame.Width*s,frame.Height*s);}thumbnails.Add(b);}Invalidate();}
  protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);float w=Width/4f,h=Height/2f;for(int i=0;i<thumbnails.Count;i++){int number=Page*8+i+1;var r=new RectangleF(i%4*w+5,i/4*h+5,Math.Max(1,w-10),Math.Max(1,h-10));for(float y=r.Y;y<r.Bottom;y+=12)for(float x=r.X;x<r.Right;x+=12)e.Graphics.FillRectangle((((int)((x-r.X)/12)+(int)((y-r.Y)/12))%2==0)?Brushes.DimGray:Brushes.Gray,x,y,Math.Min(12,r.Right-x),Math.Min(12,r.Bottom-y));var inner=new RectangleF(r.X+3,r.Y+3,r.Width-6,Math.Max(1,r.Height-28));float ratio=Math.Min(inner.Width/thumbnails[i].Width,inner.Height/thumbnails[i].Height);e.Graphics.DrawImage(thumbnails[i],inner.X+(inner.Width-thumbnails[i].Width*ratio)/2,inner.Y+(inner.Height-thumbnails[i].Height*ratio)/2,thumbnails[i].Width*ratio,thumbnails[i].Height*ratio);using(var pen=new Pen(number==SyncImage?Color.Gold:number==StartImage?Color.Cyan:Color.Black,number==SyncImage||number==StartImage?3:1))e.Graphics.DrawRectangle(pen,r.X,r.Y,r.Width,r.Height);e.Graphics.FillRectangle(Brushes.Black,r.X,r.Bottom-23,r.Width,23);e.Graphics.DrawString(number+(number==StartImage?" · départ":"")+(number==SyncImage?" · repère":""),Font,Brushes.White,r.X+4,r.Bottom-21);}}
  protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(Width<=0||Height<=0)return;int column=Math.Min(3,e.X*4/Width),row=Math.Min(1,e.Y*2/Height),number=Page*8+row*4+column+1;if(number<=asset.Count&&Chosen!=null)Chosen(number);}
  protected override void Dispose(bool disposing){if(disposing){foreach(var b in thumbnails)b.Dispose();thumbnails.Clear();}base.Dispose(disposing);}
 }
}
