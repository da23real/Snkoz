using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace UrbanKaoz.DisplayRecorder {
  internal sealed class CaptureState {
    public ScreenState Screens=new ScreenState();
    public UrbanKaoz.SongModel Song=new UrbanKaoz.SongModel();
    public int[] Leds=new int[64];
    public double Start;
    public float Step=9,Row=28,ScrollY;
    public int ViewWidth=780,ViewHeight=180;
    public bool HasSong;
    static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
    public bool Apply(string packet){
      if(packet.StartsWith("UKD1|"))return UkdProtocol.Apply(packet,Screens);
      if(packet.StartsWith("SONG,")){
        string p=packet.Substring(5);try{Song.Accept(p);if(p=="END")HasSong=true;return true;}catch(FormatException){return false;}catch(OverflowException){return false;}catch(IndexOutOfRangeException){return false;}
      }
      string[] v=packet.Split(',');
      if(v.Length==19&&v[0]=="PC1"&&v[1]=="LED"){
        int n;if(!int.TryParse(v[2],out n)||n<0||n>3)return false;int[] values=new int[16];
        for(int i=0;i<16;i++)if(!int.TryParse(v[i+3],NumberStyles.HexNumber,Inv,out values[i])||values[i]<0||values[i]>0xffffff)return false;
        Array.Copy(values,0,Leds,n*16,16);return true;
      }
      if(v.Length==7&&v[0]=="VIEW831"){
        double start;float step,row,scroll;int w,h;
        if(!double.TryParse(v[1],NumberStyles.Float,Inv,out start)||!float.TryParse(v[2],NumberStyles.Float,Inv,out step)||!float.TryParse(v[3],NumberStyles.Float,Inv,out row)||!float.TryParse(v[4],NumberStyles.Float,Inv,out scroll)||!int.TryParse(v[5],out w)||!int.TryParse(v[6],out h))return false;
        if(double.IsNaN(start)||double.IsInfinity(start)||start<0||start>UrbanKaoz.SongModel.MaxStep||!(step>=1&&step<=60)||!(row>=16&&row<=96)||!(scroll>=0&&scroll<=3072)||w<100||w>4096||h<50||h>2160)return false;
        Start=start;Step=step;Row=row;ScrollY=scroll;ViewWidth=w;ViewHeight=h;return true;
      }
      if(v.Length==2&&v[0]=="SOURCE831"){Song.Synced=false;return true;}
      return packet=="END831";
    }
    public string ViewPacket(){return string.Format(Inv,"VIEW831,{0},{1},{2},{3},{4},{5}",Start,Step,Row,ScrollY,ViewWidth,ViewHeight);}
    public IEnumerable<string> Snapshot(){
      string[] ids={"EP","A","B","C","D"};
      for(int n=0;n<5;n++)for(int row=0;row<Screens.Lines[n].Length;row++)yield return "UKD1|L|"+ids[n]+"|"+row+"|"+Screens.Lines[n][row].Replace("%","%25").Replace("|","%7C");
      yield return "SONG,BEGIN,2";
      yield return string.Format(Inv,"SONG,STATE,{0},{1},{2},{3},{4},{5},{6},0,0,{7}",Song.Mode?1:0,Song.Playing?1:0,Song.Tick,Song.Loop?1:0,Song.Start,Song.End,(int)(Song.Bpm*100),Song.Delete?1:0);
      yield return string.Format(Inv,"SONG,EXTRA,{0},{1},{2},0,{3},0,0,0",Song.Engine?1:0,Song.Record?1:0,Song.View,Song.Ram);
      yield return "SONG,SELECT,"+Song.Track+","+Song.Pattern;
      for(int t=0;t<16;t++)for(int p=0;p<16;p++)yield return "SONG,PAT,"+t+","+p+","+(Song.Present[t,p]?1:0)+","+Song.Lengths[t,p];
      foreach(var c in Song.Clips.Values)yield return "SONG,CLIP,"+c.Id+","+c.Track+","+c.Pattern+","+c.Position+","+c.Offset+","+c.Length;
      foreach(var z in Song.Zones.Values)yield return "SONG,ZL,"+z.Id+","+z.Track+","+z.Kind+","+z.Tick+","+z.Gate+","+z.A+","+z.B;
      if(Song.Synced)yield return "SONG,END";
      for(int n=0;n<4;n++){var line=new StringBuilder("PC1,LED,"+n);for(int p=0;p<16;p++)line.Append(","+Leds[n*16+p].ToString("X6"));yield return line.ToString();}
      yield return ViewPacket();
    }
  }

  internal sealed class LogEvent {public long Time;public string Packet;}
  internal static class Recording {
    public static IEnumerable<LogEvent> Events(string path){
      long previous=0;
      foreach(string raw in File.ReadLines(path)){
        if(raw.Length==0||raw[0]=='#')continue;int cut=raw.IndexOf('|');long time;
        if(cut<1||!long.TryParse(raw.Substring(0,cut),out time)||time<previous)throw new InvalidDataException("Chronologie .ukd invalide.");
        previous=time;yield return new LogEvent{Time=time,Packet=raw.Substring(cut+1)};
      }
    }
    public static long Duration(string path){long last=0;bool explicitEnd=false;foreach(var e in Events(path)){last=e.Time;if(e.Packet=="END831")explicitEnd=true;}return Math.Max(1,last+(explicitEnd?0:1000));}
    public static string[] Paths(string stem){return new[]{stem+"_EP_640x240.avi",stem+"_ABCD_1280x120.avi",stem+"_SONG_1280x720.avi"};}
    public static Bitmap SongBitmap(CaptureState state,DisplaySkin skin){
      using(var view=new UrbanKaoz.Timeline(state.Song,delegate{}){Size=new Size(1280,720),Step=Math.Min(60,state.Step*1195/Math.Max(15,state.ViewWidth-85)),Row=40,Start=state.Start,ScrollY=state.ScrollY*40/state.Row,ReadOnly=true,Accent=Surface.Accent(skin)}){
        view.CreateControl();var result=new Bitmap(1280,720,PixelFormat.Format24bppRgb);view.DrawToBitmap(result,new Rectangle(Point.Empty,view.Size));
        if(!state.HasSong)using(var g=Graphics.FromImage(result))using(var font=new Font("Segoe UI",18))g.DrawString("Ce journal ne contient pas de vue Song synchronisée",font,Brushes.Orange,30,30);
        return result;
      }
    }
    // Streaming replay: one model, no copy of the whole Song on each LCD row.
    public static void Export(string log,string stem,int fps,DisplaySkin skin,bool frame,Action<int> progress,Func<bool> cancelled){
      if(fps<1||fps>60)throw new ArgumentOutOfRangeException("fps");
      long duration=Duration(log);long total=(long)Math.Ceiling(duration*fps/1000.0);
      if(total>int.MaxValue)throw new InvalidDataException("Enregistrement trop long.");
      var paths=Paths(stem);string token=".partial-"+Guid.NewGuid().ToString("N");string[] temp={paths[0]+token,paths[1]+token,paths[2]+token};
      try{
        var state=new CaptureState();
        using(var events=Events(log).GetEnumerator())using(var ep=new AviWriter(temp[0],640,240,fps))using(var abcd=new AviWriter(temp[1],1280,120,fps))using(var song=new AviWriter(temp[2],1280,720,fps)){
          bool next=events.MoveNext();
          for(int i=0;i<total;i++){
            if(cancelled())throw new OperationCanceledException();long time=(long)i*1000/fps;
            while(next&&events.Current.Time<=time){state.Apply(events.Current.Packet);next=events.MoveNext();}
            using(var a=LcdRenderer.RenderSingle(state.Screens.Lines[0],20,640,240,skin,frame))using(var b=LcdRenderer.RenderStrip(state.Screens,1280,120,skin,frame))using(var c=SongBitmap(state,skin)){ep.AddFrame(a);abcd.AddFrame(b);song.AddFrame(c);}
            if(i%fps==0)progress((int)(i*100/total));
          }
        }
        for(int i=0;i<3;i++){if(File.Exists(paths[i]))File.Delete(paths[i]);File.Move(temp[i],paths[i]);}progress(100);
      }finally{foreach(var p in temp)if(File.Exists(p))File.Delete(p);}
    }
  }
}
