using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.IO.Ports;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace UrbanKaoz.DisplayRecorder {
  internal sealed class ScreenState {
    public readonly string[][] Lines = {
      new string[4], new string[2], new string[2], new string[2], new string[2]
    };
    public ScreenState() { Clear(); }
    public void Clear() {
      for (int s = 0; s < Lines.Length; s++)
        for (int r = 0; r < Lines[s].Length; r++) Lines[s][r] = "";
    }
    public ScreenState CloneState() {
      ScreenState copy = new ScreenState();
      for (int s = 0; s < Lines.Length; s++)
        for (int r = 0; r < Lines[s].Length; r++) copy.Lines[s][r] = Lines[s][r];
      return copy;
    }
  }

  internal static class UkdProtocol {
    public static int ScreenIndex(string id) {
      if (id == "EP") return 0;
      if (id == "A") return 1;
      if (id == "B") return 2;
      if (id == "C") return 3;
      if (id == "D") return 4;
      return -1;
    }
    public static bool Apply(string packet, ScreenState state) {
      if (packet == null) return false;
      string[] p = packet.TrimEnd('\r', '\n').Split(new[]{'|'}, 5);
      if (p.Length < 2 || p[0] != "UKD1") return false;
      if (p[1] == "HELLO") return true;
      if (p.Length != 5 || p[1] != "L") return false;
      int screen = ScreenIndex(p[2]);
      int row;
      if (screen < 0 || !Int32.TryParse(p[3], out row) || row < 0 || row >= state.Lines[screen].Length) return false;
      string text = System.Text.RegularExpressions.Regex.Replace(System.Text.RegularExpressions.Regex.Replace(p[4],"%U([0-9A-F]{2})",m=>((char)(0xE100+Convert.ToInt32(m.Groups[1].Value,16))).ToString()),"%I([0-9A-F]{2})",m=>((char)(0xE000+Convert.ToInt32(m.Groups[1].Value,16))).ToString()).Replace("%7C", "|").Replace("%7c", "|").Replace("%25", "%");
      int width = screen == 0 ? 20 : 16;
      if (text.Length > width) text = text.Substring(0, width);
      state.Lines[screen][row] = text.PadRight(width);
      return true;
    }
  }

  internal enum DisplaySkin { RealLcd, FreeParty, Kawaii, BlingBling, Futuristic, Minimal }

  internal sealed class SkinTheme {
    public Color Background, DotOff, DotOn, Frame, Glow;
    public bool RoundDots, GlowDots;
    public string FontName;
    public FontStyle FontStyle;
    public SkinTheme(Color background,Color off,Color on,Color frame,Color glow,bool round,bool glowDots,string fontName,FontStyle fontStyle){Background=background;DotOff=off;DotOn=on;Frame=frame;Glow=glow;RoundDots=round;GlowDots=glowDots;FontName=fontName;FontStyle=fontStyle;}
  }

  internal static class LcdRenderer {
    private static SkinTheme Theme(DisplaySkin skin) {
      switch(skin) {
        case DisplaySkin.FreeParty:return new SkinTheme(Color.FromArgb(8,7,3),Color.Empty,Color.FromArgb(255,196,28),Color.FromArgb(176,119,0),Color.FromArgb(255,113,0),false,true,"Bahnschrift SemiCondensed",FontStyle.Bold);
        case DisplaySkin.Kawaii:return new SkinTheme(Color.FromArgb(48,34,63),Color.Empty,Color.FromArgb(255,219,240),Color.FromArgb(118,225,239),Color.FromArgb(244,115,196),false,true,"Segoe UI",FontStyle.Bold);
        case DisplaySkin.BlingBling:return new SkinTheme(Color.FromArgb(10,9,7),Color.Empty,Color.FromArgb(255,227,119),Color.FromArgb(255,239,178),Color.FromArgb(184,101,13),false,true,"Trebuchet MS",FontStyle.Bold);
        case DisplaySkin.Futuristic:return new SkinTheme(Color.FromArgb(3,14,23),Color.Empty,Color.FromArgb(102,240,255),Color.FromArgb(28,151,182),Color.FromArgb(0,113,153),false,true,"Bahnschrift Light",FontStyle.Regular);
        case DisplaySkin.Minimal:return new SkinTheme(Color.FromArgb(13,14,16),Color.Empty,Color.White,Color.FromArgb(72,75,82),Color.FromArgb(35,37,42),false,false,"Segoe UI Semibold",FontStyle.Regular);
        default:return new SkinTheme(Color.FromArgb(5,28,25),Color.FromArgb(10,48,43),Color.FromArgb(225,255,226),Color.FromArgb(66,132,117),Color.FromArgb(150,255,188),false,true,"Consolas",FontStyle.Bold);
      }
    }

    public static Bitmap RenderSingle(string[] lines, int columns, int width, int height, DisplaySkin skin, bool frame) {
      Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
      using (Graphics g = Graphics.FromImage(bitmap)) {
        SkinTheme theme=Theme(skin);g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(theme.Background);
        Rectangle display=DrawSkinChrome(g,new Rectangle(0,0,width,height),skin,theme,frame);
        if(skin==DisplaySkin.RealLcd)DrawDotText(g,display,lines,columns,theme);
        else DrawModernText(g,display,lines,columns,theme,skin);
      }
      return bitmap;
    }

    private static Rectangle DrawSkinChrome(Graphics g,Rectangle bounds,DisplaySkin skin,SkinTheme theme,bool frame){
      int unit=Math.Max(2,Math.Min(bounds.Width,bounds.Height)/28);int inset=frame?unit*2:Math.Max(1,unit/2);
      if(skin==DisplaySkin.RealLcd){
        using(LinearGradientBrush bg=new LinearGradientBrush(bounds,Color.FromArgb(7,41,36),theme.Background,90f))g.FillRectangle(bg,bounds);
        if(frame){using(Pen outer=new Pen(Color.FromArgb(78,153,137),unit))using(Pen inner=new Pen(Color.FromArgb(20,73,65),Math.Max(1,unit/2))){g.DrawRectangle(outer,unit/2,unit/2,bounds.Width-unit,bounds.Height-unit);g.DrawRectangle(inner,unit+1,unit+1,bounds.Width-unit*2-2,bounds.Height-unit*2-2);}}
      }else{
        Color top=theme.Background,bottom=Color.FromArgb(Math.Min(255,theme.Background.R+12),Math.Min(255,theme.Background.G+12),Math.Min(255,theme.Background.B+12));
        using(LinearGradientBrush bg=new LinearGradientBrush(bounds,top,bottom,90f))g.FillRectangle(bg,bounds);
        DrawDecoration(g,bounds,skin,theme,unit);
        if(frame)DrawFrame(g,bounds,skin,theme,unit);
      }
      Rectangle content=Rectangle.Inflate(bounds,-inset,-inset);if(content.Width<1||content.Height<1)return bounds;
      using(SolidBrush veil=new SolidBrush(skin==DisplaySkin.Minimal?Color.FromArgb(205,theme.Background):Color.FromArgb(218,theme.Background)))g.FillRectangle(veil,content);
      if(skin!=DisplaySkin.RealLcd&&skin!=DisplaySkin.Minimal)DrawDecoration(g,bounds,skin,theme,unit);
      return Rectangle.Inflate(content,-Math.Max(1,unit/3),-Math.Max(1,unit/3));
    }

    private static void DrawFrame(Graphics g,Rectangle r,DisplaySkin skin,SkinTheme theme,int u){
      if(skin==DisplaySkin.BlingBling){
        using(Pen dark=new Pen(Color.FromArgb(108,61,8),u*2))using(Pen gold=new Pen(theme.Frame,Math.Max(1,u/2))){g.DrawRectangle(dark,u,u,r.Width-u*2-1,r.Height-u*2-1);g.DrawRectangle(gold,u/2,u/2,r.Width-u-1,r.Height-u-1);g.DrawRectangle(gold,u*2,u*2,r.Width-u*4-1,r.Height-u*4-1);}DrawGoldCorner(g,r,u,theme.Frame);
      }else if(skin==DisplaySkin.Kawaii){
        using(SolidBrush rim=new SolidBrush(Color.FromArgb(190,244,145,205)))g.FillRoundedRectangle(rim,Rectangle.Inflate(r,-u/2,-u/2),u*2);
        using(SolidBrush inner=new SolidBrush(theme.Background))g.FillRoundedRectangle(inner,Rectangle.Inflate(r,-u*2,-u*2),Math.Max(2,u));
      }else if(skin==DisplaySkin.FreeParty){
        using(Pen p=new Pen(theme.Frame,u))g.DrawRectangle(p,u/2,u/2,r.Width-u,r.Height-u);using(Pen hazard=new Pen(Color.FromArgb(120,255,196,28),Math.Max(1,u/2))){for(int x=-r.Height;x<r.Width;x+=u*5)g.DrawLine(hazard,x,r.Bottom-1,x+u*2,r.Bottom-u*2);}
      }else if(skin==DisplaySkin.Futuristic){
        using(Pen p=new Pen(theme.Frame,Math.Max(1,u/2))){g.DrawLine(p,0,u*2,u*4,0);g.DrawLine(p,u*4,0,r.Width-u*4,0);g.DrawLine(p,r.Width-u*4,0,r.Width,u*2);g.DrawLine(p,r.Width,r.Height-u*2,r.Width-u*4,r.Height);g.DrawLine(p,r.Width-u*4,r.Height,u*4,r.Height);g.DrawLine(p,u*4,r.Height,0,r.Height-u*2);}
      }else{using(Pen p=new Pen(theme.Frame,Math.Max(1,u/2)))g.DrawRectangle(p,u/2,u/2,r.Width-u,r.Height-u);}
    }

    private static void DrawDecoration(Graphics g,Rectangle r,DisplaySkin skin,SkinTheme theme,int u){
      if(skin==DisplaySkin.FreeParty){
        int w=Math.Max(22,r.Width/9),h=Math.Max(18,r.Height/3);Rectangle truck=new Rectangle(r.Right-w-u,r.Bottom-h-u,w,h);using(SolidBrush b=new SolidBrush(Color.FromArgb(80,255,183,0))){g.FillRectangle(b,truck.X,truck.Y+h/3,w*2/3,h*2/3);g.FillPolygon(b,new[]{new Point(truck.X+w*2/3,truck.Y+h/3),new Point(truck.Right,truck.Y+h/2),new Point(truck.Right,truck.Bottom),new Point(truck.X+w*2/3,truck.Bottom)});g.FillEllipse(b,truck.X+w/6,truck.Bottom-u*2,u*3,u*3);g.FillEllipse(b,truck.Right-u*4,truck.Bottom-u*2,u*3,u*3);}DrawSpeakerWall(g,new Rectangle(r.Left+u,r.Top+u,w,h),Color.FromArgb(75,255,190,20),u);
      }else if(skin==DisplaySkin.Kawaii){
        int s=Math.Max(18,r.Height/3);Rectangle face=new Rectangle(r.Right-s-u,r.Top+u,s,s);using(SolidBrush b=new SolidBrush(Color.FromArgb(95,255,177,220)))g.FillEllipse(b,face);using(Pen p=new Pen(Color.FromArgb(120,120,60,110),Math.Max(1,u/2))){g.DrawArc(p,face.X+s/4,face.Y+s/2,s/5,s/4,0,180);g.DrawArc(p,face.X+s*3/5,face.Y+s/2,s/5,s/4,0,180);g.DrawArc(p,face.X+s/3,face.Y+s*2/3,s/3,s/5,0,180);}using(SolidBrush star=new SolidBrush(Color.FromArgb(100,128,232,244))){g.FillEllipse(star,r.Left+u*2,r.Top+u*2,u*2,u*2);g.FillEllipse(star,r.Right-u*7,r.Bottom-u*4,u*2,u*2);}
      }else if(skin==DisplaySkin.BlingBling){
        using(SolidBrush shine=new SolidBrush(Color.FromArgb(70,255,221,103))){g.FillEllipse(shine,r.Left+u,r.Top+u,u*5,u*5);g.FillEllipse(shine,r.Right-u*6,r.Bottom-u*6,u*5,u*5);}
      }else if(skin==DisplaySkin.Futuristic){
        using(Pen grid=new Pen(Color.FromArgb(30,theme.DotOn),1)){for(int x=r.Left;x<r.Right;x+=Math.Max(8,u*3))g.DrawLine(grid,x,r.Top,x,r.Bottom);for(int y=r.Top;y<r.Bottom;y+=Math.Max(8,u*3))g.DrawLine(grid,r.Left,y,r.Right,y);}using(SolidBrush led=new SolidBrush(Color.FromArgb(100,theme.DotOn))){for(int i=0;i<3;i++)g.FillRectangle(led,r.Right-u*(2+i*2),r.Top+u,u,u);}
      }
    }

    private static void DrawSpeakerWall(Graphics g,Rectangle r,Color color,int u){using(SolidBrush b=new SolidBrush(color))using(Pen p=new Pen(Color.FromArgb(100,color),Math.Max(1,u/3))){g.FillRectangle(b,r);for(int y=0;y<2;y++)for(int x=0;x<2;x++){Rectangle cone=new Rectangle(r.X+x*r.Width/2+u,r.Y+y*r.Height/2+u,Math.Max(2,r.Width/2-u*2),Math.Max(2,r.Height/2-u*2));g.DrawEllipse(p,cone);}}}
    private static void DrawGoldCorner(Graphics g,Rectangle r,int u,Color color){using(Pen p=new Pen(color,Math.Max(1,u/2))){g.DrawArc(p,r.Left+u,r.Top+u,u*5,u*5,180,90);g.DrawArc(p,r.Right-u*6,r.Top+u,u*5,u*5,270,90);g.DrawArc(p,r.Left+u,r.Bottom-u*6,u*5,u*5,90,90);g.DrawArc(p,r.Right-u*6,r.Bottom-u*6,u*5,u*5,0,90);}}

    private static void DrawModernText(Graphics g, Rectangle area, string[] lines, int columns, SkinTheme theme, DisplaySkin skin) {
      g.SmoothingMode=SmoothingMode.AntiAlias;
      g.PixelOffsetMode=PixelOffsetMode.HighQuality;
      g.TextRenderingHint=TextRenderingHint.AntiAliasGridFit;
      int rows=Math.Max(1,lines.Length);float rowH=area.Height/(float)rows;float cellW=area.Width/(float)columns;
      float size=Math.Max(7f,rowH*.64f);
      while(size>7f){using(Font probe=CreateFont(theme.FontName,size,theme.FontStyle)){if(g.MeasureString("W",probe,100,StringFormat.GenericTypographic).Width<=cellW*1.12f)break;}size-=1f;}
      using(Font font=CreateFont(theme.FontName,size,theme.FontStyle))
      using(StringFormat format=new StringFormat(StringFormat.GenericTypographic))
      using(SolidBrush text=new SolidBrush(theme.DotOn))
      using(SolidBrush shadow=new SolidBrush(Color.FromArgb(125,theme.Glow))) {
        format.Alignment=StringAlignment.Center;format.LineAlignment=StringAlignment.Center;format.Trimming=StringTrimming.None;format.FormatFlags|=StringFormatFlags.NoWrap;
        for(int row=0;row<rows;row++){
          string value=(lines[row]??"").PadRight(columns);
          for(int col=0;col<columns;col++){if(value[col]==' ')continue;RectangleF cell=new RectangleF(area.Left+col*cellW,area.Top+row*rowH,cellW,rowH);char c=value[col];bool under=c>=0xE100&&c<0xE180;if(under){c=(char)(c-0xE100);using(var pen=new Pen(text.Color))g.DrawLine(pen,cell.Left,cell.Bottom-2,cell.Right,cell.Bottom-2);}bool inverse=c>=0xE000&&c<0xE080;if(inverse){c=(char)(c-0xE000);g.FillRectangle(text,cell);using(var ink=new SolidBrush(theme.Background))g.DrawString(c.ToString(),font,ink,cell,format);}else{if(theme.GlowDots){RectangleF shifted=cell;shifted.X+=1;shifted.Y+=1;g.DrawString(c.ToString(),font,shadow,shifted,format);}g.DrawString(c.ToString(),font,text,cell,format);}}
          if(skin==DisplaySkin.Futuristic&&row<rows-1)using(Pen divider=new Pen(Color.FromArgb(45,theme.Frame),1))g.DrawLine(divider,area.Left,area.Top+(row+1)*rowH,area.Right,area.Top+(row+1)*rowH);
        }
      }
    }

    private static Font CreateFont(string name,float size,FontStyle style){
      try{return new Font(name,size,style,GraphicsUnit.Pixel);}catch{return new Font(FontFamily.GenericSansSerif,size,style,GraphicsUnit.Pixel);}
    }

    public static Bitmap RenderStrip(ScreenState state, int width, int height, DisplaySkin skin, bool frame) {
      Bitmap result = new Bitmap(width, height, PixelFormat.Format24bppRgb);
      using (Graphics g = Graphics.FromImage(result)) {
        g.Clear(Theme(skin).Background);
        for (int i = 0; i < 4; i++) {
          int x0 = i * width / 4;
          int x1 = (i + 1) * width / 4;
          using (Bitmap part = RenderSingle(state.Lines[i + 1], 16, x1 - x0, height, skin, frame))
            g.DrawImageUnscaled(part, x0, 0);
        }
      }
      return result;
    }

    private static void DrawDotText(Graphics g, Rectangle area, string[] lines, int columns, SkinTheme theme) {
      int rows = lines.Length;
      float cellW = area.Width / (float)columns;
      float cellH = area.Height / (float)rows;
      float dotW = Math.Max(1f, cellW / 5.65f);
      float dotH = Math.Max(1f, cellH / 7.75f);
      using (SolidBrush off = new SolidBrush(theme.DotOff))
      using (SolidBrush on = new SolidBrush(theme.DotOn))
      using (SolidBrush glow = new SolidBrush(Color.FromArgb(95,theme.Glow))) {
        for (int row = 0; row < rows; row++) {
          string line = (lines[row] ?? "").PadRight(columns);
          for (int col = 0; col < columns; col++) {
            bool[,] glyph = RasterGlyph(line[col]);
            float ox = area.Left + col * cellW + (cellW - dotW * 5f) / 2f;
            float oy = area.Top + row * cellH + (cellH - dotH * 7f) / 2f;
            for (int gy = 0; gy < 7; gy++) for (int gx = 0; gx < 5; gx++) {
              float pad = Math.Max(.18f, Math.Min(dotW, dotH) * .10f);
              RectangleF d = new RectangleF(ox + gx * dotW + pad, oy + gy * dotH + pad,
                                             Math.Max(.5f, dotW - pad * 2), Math.Max(.5f, dotH - pad * 2));
              if(glyph[gx,gy]&&theme.GlowDots){RectangleF halo=RectangleF.Inflate(d,Math.Max(.5f,d.Width*.35f),Math.Max(.5f,d.Height*.35f));if(theme.RoundDots)g.FillEllipse(glow,halo);else g.FillRectangle(glow,halo);}
              Brush brush=glyph[gx,gy]?on:off;if(theme.RoundDots)g.FillEllipse(brush,d);else g.FillRectangle(brush,d);
            }
          }
        }
      }
    }

    private static bool[,] RasterGlyph(char ch) {
      if(ch>=0xE100&&ch<0xE180){var marked=RasterGlyph((char)(ch-0xE100));for(int x=0;x<5;x++)marked[x,6]=true;return marked;}
      if(ch>=0xE000&&ch<0xE080){var inverse=RasterGlyph((char)(ch-0xE000));for(int y=0;y<7;y++)for(int x=0;x<5;x++)inverse[x,y]=!inverse[x,y];return inverse;}
      bool[,] dots=new bool[5,7];string[] rows=GlyphRows(Char.ToUpperInvariant(ch));
      for(int y=0;y<7;y++)for(int x=0;x<5;x++)dots[x,y]=rows[y][x]=='1';return dots;
    }
    private static string[] GlyphRows(char c){
      string p;
      switch(c){
        case 'A':p="011101000111111100011000110001";break;case 'B':p="111101000111110100011000111110";break;
        case 'C':p="011111000010000100001000001111";break;case 'D':p="111101000110001100011000111110";break;
        case 'E':p="111111000011110100001000011111";break;case 'F':p="111111000011110100001000010000";break;
        case 'G':p="011111000010000101111000101111";break;case 'H':p="100011000111111100011000110001";break;
        case 'I':p="111110010000100001000010011111";break;case 'J':p="001110001000010000101001001100";break;
        case 'K':p="100011001011100100101000110001";break;case 'L':p="100001000010000100001000011111";break;
        case 'M':p="100011101110101101011000110001";break;case 'N':p="100011100110101100111000110001";break;
        case 'O':p="011101000110001100011000101110";break;case 'P':p="111101000110001111101000010000";break;
        case 'Q':p="011101000110001101011001001101";break;case 'R':p="111101000110001111101001010001";break;
        case 'S':p="011111000001110000011000111110";break;case 'T':p="111110010000100001000010000100";break;
        case 'U':p="100011000110001100011000101110";break;case 'V':p="100011000110001100010101000100";break;
        case 'W':p="100011000110101101011010101010";break;case 'X':p="100011000101010001001010110001";break;
        case 'Y':p="100011000101010001000010000100";break;case 'Z':p="111110000100010001000100011111";break;
        case '0':p="011101000110011101011100101110";break;case '1':p="001000110000100001000010001110";break;
        case '2':p="011101000100001001100100011111";break;case '3':p="111100000100110000011000101110";break;
        case '4':p="000100011001010100101111100010";break;case '5':p="111111000011110000011000111110";break;
        case '6':p="011101000011110100011000101110";break;case '7':p="111110000100010001000100001000";break;
        case '8':p="011101000101110100011000101110";break;case '9':p="011101000110001011110000101110";break;
        case '-':p="000000000000000111110000000000";break;case '+':p="000000010000100111110010000100";break;
        case '/':p="000010001000100010001000000000";break;case '.':p="000000000000000000000110001100";break;
        case ':':p="000000110001100000000110001100";break;case '_':p="000000000000000000000000011111";break;
        case '%':p="110011101000100010001011100110";break;case '|':p="001000010000100001000010000100";break;
        case '<':p="000100010001000010000010000010";break;case '>':p="010000010000010000100010001000";break;
        case ' ':p="000000000000000000000000000000";break;default:p="111110000100010001000000000100";break;
      }
      p=p.PadRight(35,'0');string[] rows=new string[7];for(int i=0;i<7;i++)rows[i]=p.Substring(i*5,5);return rows;
    }
  }

  internal static class GraphicsExtensions {
    public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle r, int radius) {
      using (GraphicsPath p = new GraphicsPath()) {
        int d = radius * 2;
        p.AddArc(r.Left, r.Top, d, d, 180, 90); p.AddArc(r.Right-d, r.Top, d, d, 270, 90);
        p.AddArc(r.Right-d, r.Bottom-d, d, d, 0, 90); p.AddArc(r.Left, r.Bottom-d, d, d, 90, 90);
        p.CloseFigure(); g.FillPath(brush, p);
      }
    }
  }

  internal sealed class AviWriter : IDisposable {
    private readonly BinaryWriter w;
    private readonly int width, height, fps, maximumFrameBytes;
    private readonly long riffSizeAt, moviSizeAt, avihFramesAt, strhFramesAt, moviDataStart;
    private readonly List<int> offsets = new List<int>();
    private readonly List<int> sizes = new List<int>();
    private int frames;

    public AviWriter(string path, int width, int height, int fps) {
      this.width=width; this.height=height; this.fps=fps;
      maximumFrameBytes=width*height*3;
      w=new BinaryWriter(File.Create(path), Encoding.ASCII);
      Four("RIFF"); riffSizeAt=w.BaseStream.Position; w.Write(0); Four("AVI ");
      Four("LIST"); long hdrlAt=w.BaseStream.Position; w.Write(0); Four("hdrl");
      Four("avih"); w.Write(56); w.Write(1000000/fps); w.Write(maximumFrameBytes*fps); w.Write(0); w.Write(0x10);
      avihFramesAt=w.BaseStream.Position; w.Write(0); w.Write(0); w.Write(1); w.Write(maximumFrameBytes); w.Write(width); w.Write(height);
      for(int i=0;i<4;i++) w.Write(0);
      Four("LIST"); long strlAt=w.BaseStream.Position; w.Write(0); Four("strl");
      Four("strh"); w.Write(56); Four("vids"); Four("MJPG"); w.Write(0); w.Write((short)0); w.Write((short)0);
      w.Write(0); w.Write(1); w.Write(fps); w.Write(0); strhFramesAt=w.BaseStream.Position; w.Write(0);
      w.Write(maximumFrameBytes); w.Write(-1); w.Write(0); w.Write((short)0); w.Write((short)0); w.Write((short)width); w.Write((short)height);
      Four("strf"); w.Write(40); w.Write(40); w.Write(width); w.Write(height); w.Write((short)1); w.Write((short)24);
      Four("MJPG"); w.Write(maximumFrameBytes); w.Write(0); w.Write(0); w.Write(0); w.Write(0);
      PatchSize(strlAt); PatchSize(hdrlAt);
      Four("LIST"); moviSizeAt=w.BaseStream.Position; w.Write(0); Four("movi"); moviDataStart=w.BaseStream.Position;
    }
    public void AddFrame(Bitmap source) {
      if(w.BaseStream.Position>2000000000L-maximumFrameBytes-frames*16L)throw new IOException("Limite AVI classique de 2 Go atteinte. Exportez à une cadence plus basse ou enregistrez une prise plus courte.");
      using(Bitmap b = source.PixelFormat==PixelFormat.Format24bppRgb ? (Bitmap)source.Clone() : source.Clone(new Rectangle(0,0,width,height),PixelFormat.Format24bppRgb)) {
        if(b.Width!=width || b.Height!=height) throw new ArgumentException("Dimensions AVI incorrectes");
        byte[] jpeg;
        using(MemoryStream ms=new MemoryStream()) {
          ImageCodecInfo codec=null; foreach(ImageCodecInfo c in ImageCodecInfo.GetImageEncoders())if(c.FormatID==ImageFormat.Jpeg.Guid){codec=c;break;}
          using(EncoderParameters ep=new EncoderParameters(1)){ep.Param[0]=new EncoderParameter(System.Drawing.Imaging.Encoder.Quality,92L);b.Save(ms,codec,ep);}
          jpeg=ms.ToArray();
        }
        offsets.Add((int)(w.BaseStream.Position-moviDataStart)); sizes.Add(jpeg.Length); Four("00dc"); w.Write(jpeg.Length);w.Write(jpeg);
        if((jpeg.Length&1)!=0) w.Write((byte)0); frames++;
      }
    }
    public void Dispose() {
      if(w==null) return;
      long endMovi=w.BaseStream.Position; long here=w.BaseStream.Position;
      w.BaseStream.Position=moviSizeAt; w.Write((int)(endMovi-moviSizeAt-4)); w.BaseStream.Position=here;
      Four("idx1"); w.Write(frames*16);
      for(int i=0;i<frames;i++){ Four("00dc"); w.Write(0x10); w.Write(offsets[i]); w.Write(sizes[i]); }
      long end=w.BaseStream.Position;
      w.BaseStream.Position=avihFramesAt; w.Write(frames); w.BaseStream.Position=strhFramesAt; w.Write(frames);
      w.BaseStream.Position=riffSizeAt; w.Write((int)(end-8)); w.BaseStream.Position=end; w.Close();
    }
    private void Four(string s){ w.Write(Encoding.ASCII.GetBytes(s)); }
    private void PatchSize(long at){ long end=w.BaseStream.Position; w.BaseStream.Position=at; w.Write((int)(end-at-4)); w.BaseStream.Position=end; }
  }

  internal static class MidiOutput {
    internal sealed class Session : IDisposable {
      IntPtr handle;
      public Session(int device){int error=midiOutOpen(out handle,device,IntPtr.Zero,IntPtr.Zero,0);if(error!=0)throw new InvalidOperationException("Ouverture MIDI : "+error);}
      public void Note(int channel,int note,int velocity,bool on){
        if(handle==IntPtr.Zero)throw new ObjectDisposedException("MIDI");
        int error=midiOutShortMsg(handle,(on?0x90:0x80)|(channel-1)|(note<<8)|(velocity<<16));
        if(error!=0)throw new InvalidOperationException("Envoi MIDI : "+error);
      }
      public void Dispose(){if(handle!=IntPtr.Zero){midiOutReset(handle);midiOutClose(handle);handle=IntPtr.Zero;}}
    }
    [DllImport("winmm.dll")] private static extern int midiOutReset(IntPtr handle);
    [DllImport("winmm.dll")] private static extern int midiOutGetNumDevs();
    [DllImport("winmm.dll", CharSet=CharSet.Auto)] private static extern int midiOutGetDevCaps(IntPtr device, out MidiOutCaps caps, int size);
    [DllImport("winmm.dll")] private static extern int midiOutOpen(out IntPtr handle, int device, IntPtr callback, IntPtr instance, int flags);
    [DllImport("winmm.dll")] private static extern int midiOutShortMsg(IntPtr handle, int message);
    [DllImport("winmm.dll")] private static extern int midiOutClose(IntPtr handle);
    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)] private struct MidiOutCaps {
      public short manufacturer, product; public int driverVersion;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)] public string name;
      public short technology, voices, notes, channelMask; public int support;
    }
    public static string[] Devices() {
      List<string> names=new List<string>(); int count=midiOutGetNumDevs();
      for(int i=0;i<count;i++){MidiOutCaps c;if(midiOutGetDevCaps((IntPtr)i,out c,Marshal.SizeOf(typeof(MidiOutCaps)))==0)names.Add(c.name);}
      return names.ToArray();
    }
    public static void SendStart(int device) {
      IntPtr handle; int error=midiOutOpen(out handle,device,IntPtr.Zero,IntPtr.Zero,0);
      if(error!=0)throw new InvalidOperationException("Ouverture MIDI impossible, code "+error);
      try{error=midiOutShortMsg(handle,0xFA);if(error!=0)throw new InvalidOperationException("MIDI Start impossible, code "+error);}
      finally{midiOutClose(handle);}
    }
  }

  internal sealed class ObsWatcher : IDisposable {
    private ClientWebSocket socket; private CancellationTokenSource cancel;
    private readonly JavaScriptSerializer json=new JavaScriptSerializer();
    public event Action Connected; public event Action<string> Error; public event Action<string> RecordingChanged;
    public bool IsConnected { get { return socket!=null&&socket.State==WebSocketState.Open; } }
    public void Connect(string address,string password) {
      Dispose(); cancel=new CancellationTokenSource(); socket=new ClientWebSocket();socket.Options.AddSubProtocol("obswebsocket.json");
      Task.Run(async delegate {
        try{await socket.ConnectAsync(new Uri(address),cancel.Token);await Listen(password,cancel.Token);}
        catch(OperationCanceledException){}
        catch(Exception ex){if(Error!=null)Error(ex.Message);}
      });
    }
    private async Task Listen(string password,CancellationToken token) {
      bool identified=false;
      while(socket.State==WebSocketState.Open&&!token.IsCancellationRequested) {
        string message=await Receive(token); if(message==null)break;
        Dictionary<string,object> root=json.DeserializeObject(message) as Dictionary<string,object>;if(root==null||!root.ContainsKey("op"))continue;
        int op=Convert.ToInt32(root["op"]);Dictionary<string,object>d=root.ContainsKey("d")?root["d"] as Dictionary<string,object>:null;
        if(op==0&&d!=null){
          Dictionary<string,object> identifyData=new Dictionary<string,object>();identifyData["rpcVersion"]=1;identifyData["eventSubscriptions"]=64;
          if(d.ContainsKey("authentication")){
            Dictionary<string,object>a=d["authentication"] as Dictionary<string,object>;if(a==null)throw new InvalidDataException("Défi OBS invalide");
            identifyData["authentication"]=Authentication(password,Convert.ToString(a["salt"]),Convert.ToString(a["challenge"]));
          }
          await Send(json.Serialize(new Dictionary<string,object>{{"op",1},{"d",identifyData}}),token);
        } else if(op==2){identified=true;if(Connected!=null)Connected();}
        else if(op==5&&identified&&d!=null&&Convert.ToString(d["eventType"])=="RecordStateChanged"){
          Dictionary<string,object> data=d.ContainsKey("eventData")?d["eventData"] as Dictionary<string,object>:null;
          if(data!=null&&data.ContainsKey("outputState")&&RecordingChanged!=null)RecordingChanged(Convert.ToString(data["outputState"]));
        }
      }
      if(!token.IsCancellationRequested&&Error!=null)Error("Connexion OBS fermée");
    }
    private async Task<string> Receive(CancellationToken token){byte[] buffer=new byte[8192];using(MemoryStream ms=new MemoryStream()){WebSocketReceiveResult r;do{r=await socket.ReceiveAsync(new ArraySegment<byte>(buffer),token);if(r.MessageType==WebSocketMessageType.Close)return null;ms.Write(buffer,0,r.Count);}while(!r.EndOfMessage);return Encoding.UTF8.GetString(ms.ToArray());}}
    private async Task Send(string text,CancellationToken token){byte[] b=Encoding.UTF8.GetBytes(text);await socket.SendAsync(new ArraySegment<byte>(b),WebSocketMessageType.Text,true,token);}
    internal static string Authentication(string password,string salt,string challenge){using(SHA256 sha=SHA256.Create()){string secret=Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password+salt)));return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(secret+challenge)));}}
    public void Dispose(){if(cancel!=null){cancel.Cancel();cancel.Dispose();cancel=null;}if(socket!=null){socket.Dispose();socket=null;}}
  }


}
