using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
namespace UrbanKaoz {
 public sealed class Timeline:Control {
  public bool ReadOnly;
  public float Step {get{return step;}set{step=Math.Max(1,Math.Min(60,value));}}
  public double Start {get{return start;}set{start=Math.Max(0,value);}}
  public float Row {get{return row;}set{row=Math.Max(16,Math.Min(96,value));}}
  public float ScrollY {get{return sy;}set{sy=Math.Max(0,value);}}
  public Color Surface=Color.FromArgb(17,20,29), Accent=Color.Turquoise;
  SongModel m;Action<string> send;float step=9,row=38,left=85,header=30,sy;double start;Point down,last;bool moved;Clip original,draft;Zone zone;int gesture;
  bool rectangle;int anchorKey,allCycle;
  int VisibleTracks(){int mask=0;foreach(int key in rows)if(key%2==(m.Selection.Kind==0?0:1)&&Y(key)<Height&&Y(key)+row>header)mask|=1<<(key/2);return mask;}
  public void SelectAll(int scope){if(m.View==0)m.Selection.Context(0);else if(m.View==1&&m.Selection.Kind==0)m.Selection.Context(1);m.Selection.All(m,scope,(long)(start*96),(long)((start+(Width-left)/step)*96),VisibleTracks());Invalidate();}
  protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.Control&&e.KeyCode==Keys.A){SelectAll(allCycle++%4);e.Handled=true;}if(e.KeyCode==Keys.Escape){m.Selection.Clear();Invalidate();}}
  List<int> rows {get{return m.Rows();}}
  public Timeline(SongModel model,Action<string> sender){m=model;send=sender;DoubleBuffered=true;SetStyle(ControlStyles.ResizeRedraw,true);MouseWheel+=Wheel;TabStop=true;}
  float X(double s){return left+(float)((s-start)*step);}float Y(int key){int i=rows.IndexOf(key);return i<0?-100:header+i*row-sy;}
  void Clamp(){start=Math.Max(0,Math.Min(SongModel.MaxStep,start));sy=Math.Max(0,Math.Min(Math.Max(0,rows.Count*row-(Height-header)),sy));}
  public void Reveal(){int key=m.Track*2+(m.View==1?1:0);int i=rows.IndexOf(key);if(i>=0){if(i*row<sy)sy=i*row;else if((i+1)*row>sy+Height-header)sy=(i+1)*row-Height+header;}Clamp();Invalidate();}
  public void Cancel(){original=draft=null;zone=null;Invalidate();}
  static Color Shade(int t,int p){double h=SongModel.Hue(t,p)/60;double x=1-Math.Abs(h%2-1);double r=0,g=0,b=0;if(h<1){r=1;g=x;}else if(h<2){r=x;g=1;}else if(h<3){g=1;b=x;}else if(h<4){g=x;b=1;}else if(h<5){r=x;b=1;}else{r=1;b=x;}return Color.FromArgb((int)(70+r*110),(int)(70+g*110),(int)(70+b*110));}
  void Fill(Graphics g,Color c,float x,float y,float w,float h){if(w<=0||h<=0)return;using(var b=new SolidBrush(c))g.FillRectangle(b,x,y,w,h);}
  void TextAt(Graphics g,string s,float x,float y,Color c){using(var b=new SolidBrush(c))g.DrawString(s,Font,b,x,y);}
  protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);Graphics g=e.Graphics;g.Clear(Surface);Clamp();var rs=rows;
   var state=g.Save();g.SetClip(new RectangleF(left,header,Math.Max(0,Width-left),Math.Max(0,Height-header)));
   for(int i=0;i<rs.Count;i++)Fill(g,i%2==0?Color.FromArgb(26,31,43):Color.FromArgb(21,26,37),left,header+i*row-sy,Width-left,row-1);
   long first=(long)start/16,lastBar=(long)(start+(Width-left)/step)/16+1;
   for(long b=first;b<=lastBar;b++)Fill(g,Color.FromArgb(50,58,75),X(b*16),header,1,Height);
   if(m.Loop)Fill(g,Color.FromArgb(22,45,216,173),X((m.Start-1)*16),header,(m.End-m.Start+1)*16*step,Height-header);
   foreach(var c0 in m.Clips.Values){Clip c=draft!=null&&draft.Id==c0.Id?draft:c0;float y=Y(c.Track*2)+4,x=X(c.Position),w=c.Length*step;if(y<header-row||y>Height||x+w<left||x>Width)continue;
    Fill(g,Shade(c.Track,c.Pattern),x,y,w,row-8);if(m.Selection.Clips.Contains(c.Id)){using(var pen=new Pen(Color.HotPink,2))g.DrawRectangle(pen,x+1,y+1,Math.Max(1,w-2),row-10);}float grip=SongModel.Grip(w,step);Fill(g,Color.FromArgb(70,255,255,255),x,y,grip,row-8);Fill(g,Color.FromArgb(70,255,255,255),x+w-grip,y,grip,row-8);
    var clip=g.Save();g.SetClip(new RectangleF(x+grip,y,Math.Max(0,w-2*grip),row-8),CombineMode.Intersect);TextAt(g,"P"+(c.Pattern+1)+" · "+(c.Length/16.0).ToString("0.##")+" m",x+grip+3,y+5,Color.White);g.Restore(clip);
   }
   foreach(var z in m.Zones.Values){float y=Y(z.Track*2+1)+5,x=X(z.Tick/96.0),w=Math.Max(7,(float)(z.Gate/96.0)*step);if(y<header-row||y>Height||x+w<left||x>Width)continue;
    Fill(g,z.Kind==0?Color.FromArgb(36,153,137):Color.FromArgb(207,144,56),x,y,w,row-10);if(m.Selection.Zones.Contains(z.Id)){using(var pen=new Pen(Color.HotPink,2))g.DrawRectangle(pen,x+1,y+1,Math.Max(1,w-2),row-12);}var labelClip=g.Save();g.SetClip(new RectangleF(x,y,w,row-10),CombineMode.Intersect);TextAt(g,z.Kind==0?"N"+z.A:"CC"+z.A+"="+z.B,x+3,y+5,Color.White);g.Restore(labelClip);
   }
   if(rectangle)Fill(g,Color.FromArgb(70,138,218,255),Math.Min(down.X,last.X),Math.Min(down.Y,last.Y),Math.Abs(down.X-last.X),Math.Abs(down.Y-last.Y));
   Fill(g,Color.Gold,X(m.Tick/96.0),header,2,Height);g.Restore(state);
   Fill(g,Color.FromArgb(37,43,59),0,0,Width,header);for(long b=first;b<=lastBar;b++)if(X(b*16)>=left)TextAt(g,(b+1).ToString(),X(b*16)+3,7,Color.Silver);
   if(X((m.Start-1)*16)>=left&&X((m.Start-1)*16)<Width)TextAt(g,"S",X((m.Start-1)*16)+20,7,Accent);
   if(X(m.End*16)>=left&&X(m.End*16)<Width)TextAt(g,"E",X(m.End*16)-14,7,Accent);
   TextAt(g,m.Loop?"↻ ON":"↻ OFF",5,7,Accent);
   for(int i=0;i<rs.Count;i++){float y=header+i*row-sy;if(y<header||y>Height)continue;Fill(g,Color.FromArgb(32,38,53),0,y,left,row-1);TextAt(g,"T"+(rs[i]/2+1)+(rs[i]%2==1?" ZL":" P"),6,y+9,Color.White);}
   if(!m.Synced)TextAt(g,"Connecter le Teensy pour synchroniser la timeline",left+12,header+12,Color.Silver);
   if(m.Delete)TextAt(g,"DELETE : cliquer un bloc ou un événement ZL",left+12,Height-25,Color.Orange);
  }
  int Key(float y){int i=(int)((y-header+sy)/row);var r=rows;return i>=0&&i<r.Count?r[i]:-1;}
  protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(ReadOnly)return;Focus();down=last=e.Location;moved=false;Cancel();Capture=true;if(!m.Synced||e.Y<header||e.X<left)return;int key=Key(e.Y);rectangle=false;
   if(key>=0){m.Selection.LastTrack=key/2;if(!m.Delete&&(m.Shift||m.Column||(ModifierKeys&Keys.Shift)!=0)){
     int kind=key%2==0?0:1;foreach(var z in m.Zones.Values)if(z.Track*2+1==key&&e.X>=X(z.Tick/96.0)&&e.X<=X((z.Tick+z.Gate)/96.0))kind=z.Kind+1;m.Selection.Context(kind);anchorKey=key;
     if(m.Column){int grid=step<8?16:1;long begin=Math.Max(0,(long)Math.Floor((start+(e.X-left)/step)/grid)*grid*96);m.Selection.Column(m,begin,begin+grid*96,VisibleTracks());}
     else rectangle=true;Invalidate();return;
   }}
   if(key%2==0)foreach(var c in m.Clips.Values)if(c.Track*2==key&&e.X>=X(c.Position)&&e.X<X(c.Position+c.Length)){original=c.Copy();draft=c.Copy();float grip=SongModel.Grip(c.Length*step,step);gesture=e.X<X(c.Position)+grip?2:e.X>X(c.Position+c.Length)-grip?3:1;break;}
   else{} // selection handled by the free lane below
   if(key%2==1)foreach(var z in m.Zones.Values)if(z.Track*2+1==key&&e.X>=X(z.Tick/96.0)&&e.X<=X(z.Tick/96.0)+Math.Max(8,z.Gate/96f*step)){zone=z.Copy();break;}
  }
  protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(!Capture)return;if(rectangle){last=e.Location;Invalidate();return;}if(m.Column&&!m.Delete)return;if(Math.Abs(e.X-down.X)+Math.Abs(e.Y-down.Y)>6)moved=true;if(!moved)return;
   if(original!=null&&e.Button==MouseButtons.Left&&!m.Delete){int unit=gesture==1&&step<8?16:1;int delta=(int)Math.Round((e.X-down.X)/step/unit)*unit;if(gesture==1)draft.Position=Math.Max(0,Math.Min(SongModel.MaxStep-draft.Length,original.Position+delta));else if(gesture==2)SongModel.TrimLeft(draft,original,delta);else draft.Length=Math.Max(1,Math.Min(original.Length,original.Length+delta));}
   else if(zone==null&&original==null||e.Button==MouseButtons.Middle){start-=(e.X-last.X)/step;sy-=e.Y-last.Y;Clamp();}last=e.Location;Invalidate();}
  protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);if(ReadOnly)return;Capture=false;if(rectangle){rectangle=false;int end=Key(Math.Max((int)header,Math.Min(Height-1,e.Y)));if(end>=0){int mask=0;for(int t=Math.Min(anchorKey/2,end/2);t<=Math.Max(anchorKey/2,end/2);++t)mask|=1<<t;
    long a=Math.Max(0,(long)((start+(Math.Min(down.X,e.X)-left)/step)*96)),b=Math.Max(a+1,(long)((start+(Math.Max(down.X,e.X)-left)/step)*96));m.Selection.Rectangle(m,a,b,mask);}Invalidate();return;}if(m.Column&&!m.Delete&&down.Y>=header&&down.X>=left)return;if(!m.Synced){Cancel();return;}
   if(e.Button==MouseButtons.Right&&!moved){int key=Key(e.Y);if(key>=0&&key%2==1)EditZone(key/2,zone,(long)Math.Max(0,Math.Round((start+(e.X-left)/step))*96));Cancel();return;}
   if(e.Button==MouseButtons.Left){if(original!=null){if(m.Delete&&!moved)send("DELETE,"+original.Id);else if(moved&&SongModel.Valid(draft)&&!m.Overlaps(draft))send("EDIT,"+draft.Id+","+draft.Position+","+draft.Offset+","+draft.Length);}
    else if(zone!=null){if(m.Delete&&!moved)send("ZDELETE,"+zone.Id);else if(moved&&!m.Delete){long tick=Math.Max(0,zone.Tick+(long)Math.Round((e.X-down.X)/step)*96);send("ZEDIT,"+zone.Id+","+tick+","+zone.Gate+","+zone.A+","+zone.B);}else if(!m.Delete)EditZone(zone.Track,zone,zone.Tick);}
    else if(!moved){if(e.Y<header&&e.X<left)send("LOOP,"+(m.Loop?0:1)+","+m.Start+","+m.End);else if(e.X>=left&&e.Y>=header&&!m.Delete){int key=Key(e.Y);long at=Math.Max(0,(long)Math.Round((start+(e.X-left)/step)/(step<8?16:1))*(step<8?16:1));if(key>=0&&key%2==1)EditZone(key/2,null,at*96);else if(key>=0&&key/2==m.Track)send("ADD,"+m.Track+","+m.Pattern+","+at);}}}
   Cancel();}
  void Wheel(object sender,MouseEventArgs e){if((ModifierKeys&(Keys.Control|Keys.Shift))==(Keys.Control|Keys.Shift)){row=Math.Max(24,Math.Min(96,row*(e.Delta>0?1.2f:1/1.2f)));}else if((ModifierKeys&Keys.Control)!=0){double anchor=start+(e.X-left)/step;step=Math.Max(1,Math.Min(60,step*(e.Delta>0?1.2f:1/1.2f)));start=anchor-(e.X-left)/step;}else if((ModifierKeys&Keys.Shift)!=0)start-=e.Delta/120.0*16;else sy-=e.Delta/120f*row*3;Clamp();Invalidate();}
  void EditZone(int track,Zone z,long tick){using(var form=new Form{Text="T"+(track+1)+" · Zone libre",Width=310,Height=255,StartPosition=FormStartPosition.CenterParent,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false}){
    var kind=new ComboBox{FormattingEnabled=true,Left=15,Top=15,Width=265,DropDownStyle=ComboBoxStyle.DropDownList};kind.Items.AddRange(new object[]{"Note MIDI","Automation CC"});kind.SelectedIndex=z==null?0:z.Kind;kind.Enabled=z==null;form.Controls.Add(kind);
    var a=Number(form,"Note / numéro CC",50,0,127,z==null?60:z.A);var b=Number(form,"Vélocité / valeur",85,0,127,z==null?100:z.B);var gate=Number(form,"Durée (ticks, pas=96)",120,1,98304,z==null?96:z.Gate);
    var ok=new Button{Text="Valider",Left=175,Top=163,DialogResult=DialogResult.OK};form.Controls.Add(ok);form.AcceptButton=ok;
    if(form.ShowDialog(FindForm())==DialogResult.OK){if(kind.SelectedIndex==0&&b.Value==0)return;send(z==null?"ZADD,"+kind.SelectedIndex+","+track+","+tick+","+(long)gate.Value+","+(int)a.Value+","+(int)b.Value:"ZEDIT,"+z.Id+","+tick+","+(long)gate.Value+","+(int)a.Value+","+(int)b.Value);}
  }}
  NumericUpDown Number(Form f,string label,int y,long min,long max,long value){f.Controls.Add(new Label{Text=label,Left=15,Top=y+3,Width=175});var n=new NumericUpDown{Left=195,Top=y,Width=80,Minimum=min,Maximum=max,Value=Math.Max(min,Math.Min(max,value))};f.Controls.Add(n);return n;}
 }

}
