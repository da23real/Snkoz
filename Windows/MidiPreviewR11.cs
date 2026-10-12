using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
namespace Synkoz {
 sealed class MidiStripR9:Control {
  MidiSpan[] notes=new MidiSpan[0];Cc936[] cc=new Cc936[0];double position,duration;
  readonly ToolTip tip=new ToolTip();readonly HashSet<int> hiddenChannels=new HashSet<int>(),hiddenNotes=new HashSet<int>(),hiddenCC=new HashSet<int>();
  bool showNotes=true,showCC=true;public bool Compact;
  static readonly Color[] colors={Color.Orange,Color.LimeGreen,Color.Orchid,Color.Gold,Color.Coral,Color.Turquoise,Color.HotPink};
  public MidiStripR9(){DoubleBuffered=true;BackColor=Color.FromArgb(25,28,32);ForeColor=Color.White;Dock=DockStyle.Fill;}
  public void LoadMidi(Montage m){duration=m.Duration;notes=m.Notes.OrderBy(n=>n.Start).ToArray();cc=m.CC.OrderBy(c=>c.Time).ToArray();Invalidate();}
  public void Position(double value){position=value;Invalidate();}
  bool NoteVisible(MidiSpan n){return showNotes&&!hiddenChannels.Contains(MidiChannelR9.Number(n.Channel,n.Track940)-1)&&!hiddenNotes.Contains(n.Note);}
  bool CcVisible(Cc936 c){return showCC&&!hiddenChannels.Contains(MidiChannelR9.Number(c.Channel,c.Track940)-1)&&!hiddenCC.Contains(c.Number);}
  public void ShowFilters(Control anchor){var menu=new ContextMenuStrip{AutoClose=false};
   Action<ToolStripItemCollection,string,bool,Action<bool>> check=(items,name,value,set)=>{var item=new ToolStripMenuItem(Language945.T(name)){Checked=value,CheckOnClick=true};item.CheckedChanged+=delegate{set(item.Checked);Invalidate();};items.Add(item);};
   check(menu.Items,"Notes",showNotes,v=>showNotes=v);check(menu.Items,"CC",showCC,v=>showCC=v);
   Action<string,int[],HashSet<int>,string> group=(title,values,hidden,prefix)=>{var parent=new ToolStripMenuItem(Language945.T(title));menu.Items.Add(parent);foreach(int val in values){int key=val;check(parent.DropDownItems,prefix+(title=="Canaux"?key+1:key),!hidden.Contains(key),v=>{if(v)hidden.Remove(key);else hidden.Add(key);});}};
   group("Canaux",notes.Select(n=>MidiChannelR9.Number(n.Channel,n.Track940)-1).Concat(cc.Select(c=>MidiChannelR9.Number(c.Channel,c.Track940)-1)).Distinct().OrderBy(x=>x).ToArray(),hiddenChannels,"Ch ");
   group("Notes affichées",notes.Select(n=>n.Note).Distinct().OrderBy(x=>x).ToArray(),hiddenNotes,"N ");
   group("Contrôleurs",cc.Select(c=>c.Number).Distinct().OrderBy(x=>x).ToArray(),hiddenCC,"CC ");
   menu.Items.Add(Language945.T("Tout afficher"),null,delegate{showNotes=showCC=true;hiddenChannels.Clear();hiddenNotes.Clear();hiddenCC.Clear();Invalidate();menu.Close();});
   menu.Items.Add(Language945.T("Fermer"),null,delegate{menu.Close();});Synkoz.MenuLifetimeR13.Attach(menu,anchor);menu.Show(anchor,0,anchor.Height);
  }
  protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);double t=position-2+e.X*8.0/Math.Max(1,Width),tolerance=32.0/Math.Max(1,Width);
   var labels=notes.Where(n=>NoteVisible(n)&&n.Start<=t+tolerance&&n.Start+n.Duration>=t-tolerance).Take(6).Select(n=>"Note "+n.Note+" · canal "+MidiChannelR9.Number(n.Channel,n.Track940)+" · vélocité "+n.Velocity)
    .Concat(cc.Where(c=>CcVisible(c)&&c.Value>=0&&Math.Abs(c.Time-t)<tolerance).Take(6).Select(c=>"CC "+c.Number+" = "+c.Value+" · canal "+MidiChannelR9.Number(c.Channel,c.Track940)));
   tip.SetToolTip(this,string.Join("\n",labels));
  }
  protected override void Dispose(bool disposing){if(disposing)tip.Dispose();base.Dispose(disposing);}
  protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;double start=position-2,end=start+8,scale=Math.Max(1,Width)/8.0;Func<double,float> x=t=>(float)((t-start)*scale);
   float top=3,bottom=Math.Max(4,Height-4),noteBottom=Compact?bottom:Math.Max(4,Height*.53f-3),ccTop=Compact?top:Height*.53f+3;
   if(!Compact)g.DrawLine(Pens.DimGray,0,Height*.53f,Width,Height*.53f);
   var visible=notes.Where(NoteVisible).ToArray();int low=visible.Select(n=>n.Note).DefaultIfEmpty(0).Min(),high=visible.Select(n=>n.Note).DefaultIfEmpty(127).Max();
   using(var brush=new SolidBrush(Color.FromArgb(180,Color.DeepSkyBlue)))foreach(var n in visible){if(n.Start>end)break;if(n.Start+n.Duration<start)continue;float y=top+(high-n.Note)*Math.Max(1,noteBottom-top-4)/Math.Max(1,high-low);g.FillRectangle(brush,x(Math.Max(start,n.Start)),y,Math.Max(2,x(Math.Min(end,n.Start+n.Duration))-x(Math.Max(start,n.Start))),4);}
   var saved=g.Save();g.SetClip(ClientRectangle);g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
   foreach(var group in cc.Where(CcVisible).GroupBy(c=>new{c.Track940,c.Channel,c.Number})){
    using(var pen=new Pen(colors[(group.Key.Channel+group.Key.Number)%colors.Length],1.5f)){
     Cc936 previous=null;foreach(var c in group){if(c.Value<0){previous=null;continue;}if(c.StateOnlyR9)previous=null;
      if(previous!=null&&c.Time>=start&&previous.Time<=end){double a=Math.Max(start,previous.Time),b=Math.Min(end,c.Time);double dt=c.Time-previous.Time;
       float av=dt>0?(float)(previous.Value+(c.Value-previous.Value)*(a-previous.Time)/dt):c.Value,bv=dt>0?(float)(previous.Value+(c.Value-previous.Value)*(b-previous.Time)/dt):c.Value;
       if(b>=a)g.DrawLine(pen,x(a),bottom-av/127f*(bottom-ccTop),x(b),bottom-bv/127f*(bottom-ccTop));}
      if(c.Time>=start&&c.Time<=end){float y=bottom-c.Value/127f*(bottom-ccTop);g.DrawEllipse(pen,x(c.Time)-1,y-1,2,2);}previous=c;if(c.Time>end)break;
     }
     if(previous!=null&&previous.Time<Math.Min(end,duration)){float y=bottom-previous.Value/127f*(bottom-ccTop);g.DrawLine(pen,x(Math.Max(start,previous.Time)),y,x(Math.Min(end,duration)),y);}
    }
   }
   g.DrawLine(Pens.White,x(position),top,x(position),bottom);g.Restore(saved);
  }
 }
}
