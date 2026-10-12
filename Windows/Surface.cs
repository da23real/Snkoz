using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace UrbanKaoz.DisplayRecorder {
  internal sealed class HardwareControl {
    public string Label, Kind, Wiring, MaskKind;
    public int Id;
    public RectangleF Bounds;
    public bool Encoder, Held, Masked;
  }

  internal sealed partial class Surface : Control {
    public readonly List<HardwareControl> Items=new List<HardwareControl>();
    public readonly int[] Colors=new int[64];
    public DisplaySkin Skin;
    public ScreenState Screens;
    public bool Connected;
    public Action<string> Send=delegate{};
    public Action<string> Notice=delegate{};
    readonly ToolTip tip=new ToolTip{ShowAlways=true,AutoPopDelay=25000,InitialDelay=0};
    HardwareControl active;
    Point last;
    float drag;
    bool turning, latchedClick;
    const float DesignWidth=1240, DesignHeight=312;
    static readonly int[] PicoPins={1,2,4,5,6,7,9,10,11,12,14,15,16,17,19,20,21,22,24,25,26,27,29,-1,-1,-1,31,32,34,-1};
    public static string PicoPin(int gp){return gp>=0&&gp<PicoPins.Length&&PicoPins[gp]>0?"patte "+PicoPins[gp]+" (GP"+gp+")":"GP"+gp+" (non exposé)";}
    public static Color Accent(DisplaySkin skin){switch(skin){case DisplaySkin.FreeParty:return Color.Gold;case DisplaySkin.Kawaii:return Color.HotPink;case DisplaySkin.BlingBling:return Color.Khaki;case DisplaySkin.Futuristic:return Color.Turquoise;case DisplaySkin.Minimal:return Color.White;default:return Color.LightGreen;}}
    public Surface(){DoubleBuffered=true;TabStop=true;SetStyle(ControlStyles.ResizeRedraw|ControlStyles.StandardClick|ControlStyles.StandardDoubleClick,true);Build();InitNumeric917();}
    HardwareControl Add(string label,string kind,int id,int x,int y,int w,int h,string wiring,bool encoder){var c=new HardwareControl{Label=label,Kind=kind,Id=id,Bounds=new RectangleF(x,y,w,h),Wiring=wiring,Encoder=encoder,MaskKind=kind=="KB"?"KY":kind=="RB"?"ROY":kind};Items.Add(c);return c;}
    void Hc(string label,int bit,int x,int y,int w){Add(label,"HC",bit,x,y,w,27,"Chaîne 7 × 74HC165 · bit B"+bit+" (index à partir de 0).\nTeensy : broches sérigraphiées 26 (SH/LD), 27 (CLK), 28 (DATA).\nEntrée active à 0. Position de la borne sur la carte : non documentée.",false);}
    void Build(){
      string[] ky={"Volume / Piste","Canal MIDI","Note","Vélocité","Gate","Timing / Move","Pitch Bend","Libre / Contexte","Roll vélocité","Roll MOD","Roll start","Roll espace","Roll accél.","Roll exposant","Roll courbe","Mute"};
      int[] clk={2,5,8,11,14,17,20,26};
      for(int i=0;i<16;i++){int gp=clk[i%8];Add("K"+(i+1)+" · "+ky[i],"KB",i,i*77+2,51,74,52,
        "KY40 K"+(i+1)+" · Pico KY "+(i/8+1)+" · index UART "+i%8+".\nCLK : "+PicoPin(gp)+" ; DT : "+PicoPin(gp+1)+" ; SW : "+PicoPin(gp+2)+".\nUART Pico : pattes 1/2 (GP0 TX / GP1 RX), vers Teensy "+(i<8?"0 RX1 / 1 TX1":"25 RX6 / 24 TX6")+" (sérigraphie).\nMolette ou glisser verticalement = tourner ; clic = appui.\nCtrl+clic = maintenir pour combiner avec un autre geste.",true);}
      string[] roy={"OA · Zoom / BPM","OB · Position / Longueur","OC · Début boucle","OD · Fin boucle"};int[] sw={9,17,14,12};
      for(int i=0;i<4;i++)Add(roy[i],"RB",i,i*137+2,107,132,43,"Roy Quad Rotary · I²C 0x49 · encodeur interne "+(3-i)+".\nSW seesaw "+sw[i]+" ; connecteur "+"ABCD"[i]+" / index P"+(4-i)+".\nI²C Teensy : 18 SDA / 19 SCL (sérigraphie).\nMolette / glisser = tourner ; clic = appui ; Ctrl+clic = maintien.",true);
      int[] addr={0x2f,0x2e,0x32,0x30};
      for(int n=0;n<4;n++)for(int p=0;p<16;p++)Add(""+(p+1),"NEO",n*16+p,n*137+(p%4)*33+2,173+(p/4)*33,30,30,
        "Néo "+(n+1)+" · I²C 0x"+addr[n].ToString("X2")+" · pad "+(p+1)+" / index brut "+p+".\nLigne "+(p/4+1)+", colonne "+(p%4+1)+" du module.\nTeensy : 18 SDA / 19 SCL (sérigraphie), alimentation et masse communes.\nCtrl+clic maintient le pad pour les combinaisons.\nLe sens musical suit le firmware, pas le numéro brut du pad.",false);
      string[] labels={"Rec","Play / Pause","Stop","Retour début","Mesure −","Mesure +","Track / Pattern","Groove / Remix","Notes CUT","Clavier","Select","Delete","Undo","Redo","Colonne","Banque","Sauver","Charger","Patch −","Patch +","Delay CUT","Shift","Tout","Vue pistes"};
      int[] bits={27,24,29,26,25,28,10,3,30,11,0,5,8,2,7,9,12,13,14,15,31,1,6,52};
      for(int i=0;i<labels.Length;i++)Hc(labels[i],bits[i],558+(i%6)*76,108+(i/6)*30,73);
      string[] clip={"Copier","Choisir","Coller","Écouter"};
      for(int slot=0;slot<3;slot++)for(int act=0;act<4;act++)Hc("C"+(slot+1)+" "+clip[act],44-slot*4+act,558+act*113,231+slot*25,110);
      string keys="123A456B789C*0#D";int[] rows={15,14,13,12},cols={16,17,19,18};
      for(int i=0;i<16;i++)Add(keys[i]=='#'?"# OK":keys[i]=='*'?"* Retour":keys[i].ToString(),"KEY",keys[i],1018+i%4*53,173+i/4*33,50,30,
        "Digipad Picran · touche "+keys[i]+".\nRangée : "+PicoPin(rows[i/4])+" ; colonne : "+PicoPin(cols[i%4])+".\nPico vu côté composants, USB en haut : patte 1 en haut à gauche.\n# valide ; * annule ; A/B/C/D suivent le contexte du firmware.",false).MaskKind="KEY";
      for(int j=0;j<2;j++){
        int x=1018+j*107;string info="Joystick J"+(j+1)+" · Nano P23 vers Teensy RX5, broche sérigraphiée 21.\nAxes et clic : trame DATA,Y1,…,Y2,…\nBroches analogiques Nano : non confirmées dans cette livraison.\nJ1 : navigation / retour ; J2 : texte des menus selon contexte.";
        Add("◀","JOY",j*10,x,132,25,25,info,false);Add("▲","JOY",j*10+1,x+26,108,25,25,info,false);Add("● J"+(j+1),"JOY",j*10+2,x+26,134,47,25,info,false);Add("▼","JOY",j*10+3,x+52,108,25,25,info,false);Add("▶","JOY",j*10+4,x+78,132,25,25,info,false);
      }
    }
    public void ClearHeld(){CancelNumeric917();active=null;foreach(var c in Items)c.Held=false;Capture=false;Invalidate();}
    public void ResetSession(){ClearHeld();NumericAvailable917=false;foreach(var c in Items)c.Masked=false;Connected=false;}
    public void ReleaseAll(){if(Connected)Send("RELEASE");ClearHeld();}
    PointF Logical(Point p){return new PointF(p.X*DesignWidth/Math.Max(1,Width),p.Y*DesignHeight/Math.Max(1,Height));}
    HardwareControl Hit(Point p){PointF at=Logical(p);foreach(var c in Items)if(c.Bounds.Contains(at))return c;return null;}
    void Edge(HardwareControl c,bool down){
      if(!Connected){Notice("Connecter le Teensy V9.01 pour piloter les commandes.");return;}
      c.Held=down;
      if(c.Kind=="KEY"){if(down)Send("KEY,"+(char)c.Id);c.Held=false;}
      else if(c.Kind=="JOY"){int id=c.Id/10,dir=c.Id%10;int axis=dir==2?2:dir==0||dir==4?0:1;int value=down?(dir==2?1:dir==0||dir==1?512:-512):0;Send("JOY,"+id+","+axis+","+value);}
      else Send(c.Kind+","+c.Id+","+(down?1:0));
      Invalidate();
    }
    protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);Focus();var c=Hit(e.Location);if(NumericMouseDown917(e,c))return;if(c==null)return;
      if(e.Button==MouseButtons.Right){var menu=new ContextMenuStrip();menu.Items.Add(new ToolStripLabel(c.Label+"\n\n"+c.Wiring));menu.Items.Add(new ToolStripSeparator());
        var mask=new ToolStripMenuItem(c.Masked?"Réactiver l'entrée physique":"Neutraliser l'entrée physique (session)");mask.Enabled=Connected;
        mask.Click+=delegate{int id=c.Kind=="JOY"?c.Id/10:c.Kind=="KEY"?0:c.Id;bool value=!c.Masked;Send("MASK,"+c.MaskKind+","+id+","+(value?1:0));foreach(var x in Items)if(x.MaskKind==c.MaskKind&&(x.Kind=="JOY"?x.Id/10:x.Kind=="KEY"?0:x.Id)==id)x.Masked=value;Invalidate();};
        menu.Items.Add(mask);Synkoz.MenuLifetimeR13.Attach(menu,this);menu.Show(this,e.X,e.Y);return;
      }
      tip.Hide(this);if(e.Button!=MouseButtons.Left||!Connected)return;
      active=c;last=e.Location;drag=0;turning=false;latchedClick=(ModifierKeys&Keys.Control)!=0;
      if(latchedClick){Edge(c,!c.Held);active=null;return;}
      if(c.Held){Edge(c,false);active=null;return;}
      Capture=true;if(!c.Encoder)Edge(c,true);
    }
    protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(active==null||!active.Encoder||e.Button!=MouseButtons.Left)return;
      drag+=(last.Y-e.Y)/6f;last=e.Location;int delta=(int)drag;if(delta==0)return;drag-=delta;turning=true;Turn(active,delta);
    }
    void Turn(HardwareControl c,int delta){if(!Connected)return;FlushClick917();delta=Math.Max(-16,Math.Min(16,delta));Send((c.Kind=="KB"?"KT":"RT")+","+c.Id+","+delta);}
    protected override void OnMouseWheel(MouseEventArgs e){base.OnMouseWheel(e);var c=Hit(e.Location);if(c!=null&&c.Encoder)Turn(c,e.Delta>0?1:-1);}
    protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);if(e.Button!=MouseButtons.Left||active==null)return;var c=active;active=null;Capture=false;
      if(c.Encoder){if(!turning)QueueClick917(c);}else Edge(c,false);
    }
    protected override void OnMouseCaptureChanged(EventArgs e){base.OnMouseCaptureChanged(e);if(!Capture&&active!=null){var c=active;active=null;if(c.Held)Edge(c,false);}}
    protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Escape){ReleaseAll();e.Handled=true;}}
    protected override void OnPaint(PaintEventArgs e){
      var g=e.Graphics;g.Clear(Color.FromArgb(19,22,29));g.ScaleTransform(Width/DesignWidth,Height/DesignHeight);g.SmoothingMode=SmoothingMode.AntiAlias;
      Color accent=Accent(Skin);
      if(Screens!=null)using(var img=LcdRenderer.RenderStrip(Screens,1232,47,Skin,true))g.DrawImageUnscaled(img,2,0);
      using(var font=new Font("Segoe UI",10,FontStyle.Regular,GraphicsUnit.Pixel))using(var small=new Font("Segoe UI",9,FontStyle.Regular,GraphicsUnit.Pixel))using(var pen=new Pen(accent))using(var white=new SolidBrush(Color.White))using(var text=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center}){
        for(int n=0;n<4;n++)g.DrawString("NÉO "+(n+1),small,white,n*137+45,157);
        foreach(var c in Items){RectangleF r=c.Bounds;Color bg=c.Held?accent:Color.FromArgb(41,47,59);
          if(c.Kind=="NEO"&&!c.Held){int rgb=Colors[c.Id];bg=rgb==0?Color.FromArgb(37,40,48):Color.FromArgb((rgb>>16)&255,(rgb>>8)&255,rgb&255);}
          using(var brush=new SolidBrush(bg))g.FillRectangle(brush,r);
          using(var border=new Pen(c.Masked?Color.OrangeRed:c.Held?Color.White:Color.FromArgb(80,92,108),c.Masked?2:1))g.DrawRectangle(border,r.X,r.Y,r.Width,r.Height);
          if(c.Encoder){float cx=r.X+r.Width/2;using(var brush=new SolidBrush(Color.FromArgb(16,19,25)))g.FillEllipse(brush,cx-9,r.Y+3,18,18);g.DrawArc(pen,cx-9,r.Y+3,18,18,130,280);g.DrawLine(pen,cx,r.Y+5,cx,r.Y+10);r.Y+=22;r.Height-=22;}
          using(var brush=new SolidBrush(c.Held?Color.Black:Color.White))g.DrawString(c.Label,font,brush,r,text);
        }
      }
      if(!Connected)using(var font=new Font("Segoe UI",10,FontStyle.Bold,GraphicsUnit.Pixel))using(var brush=new SolidBrush(Color.Orange))g.DrawString("APERÇU · commandes inactives",font,brush,560,302);
    }
    protected override void Dispose(bool disposing){if(disposing){CancelNumeric917();clickTimer917.Dispose();tip.Dispose();}base.Dispose(disposing);}
  }
}
