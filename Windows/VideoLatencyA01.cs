using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
namespace Synkoz {
 // Experimental local optical detector. Native frame PTS, no AI and no FPS conversion.
 public static class VideoLatencyA01 {
  const int Width=160,Height=96,Tile=8,Columns=Width/Tile,Rows=Height/Tile,Bytes=Width*Height*3;
  static string F(double v){return v.ToString("F9",Engine.Inv);}
  public static Tuple<double,double> Onset(string path,double reference,double start,double length,CancellationToken token){
   // Decode a bounded prefix to keep original file-relative PTS, including stream start offsets.
   // trim does not reset timestamps; showinfo and raw output see precisely the same frames.
   string filter="trim=start="+F(start)+":end="+F(start+length)+",scale=160:96,format=rgb24,showinfo";
   string args="-hide_banner -loglevel info -t "+F(start+length)+" -i "+Engine.Q(path)+" -map 0:v:0 -an -sn -dn -vf "+Engine.Q(filter)+" -fps_mode passthrough -frames:v 512 -f rawvideo -pix_fmt rgb24 pipe:1";
   byte[] raw;string log;
   token.ThrowIfCancellationRequested();using(var p=Engine.Create(Engine.FF,args)){
    p.Start();var errors=p.StandardError.ReadToEndAsync();using(token.Register(()=>{try{p.Kill();}catch{}}))using(var data=new MemoryStream()){
     p.StandardOutput.BaseStream.CopyTo(data);p.WaitForExit();token.ThrowIfCancellationRequested();log=errors.GetAwaiter().GetResult();if(p.ExitCode!=0)throw new Exception("Décodage vidéo impossible : "+log.Substring(Math.Max(0,log.Length-600)));raw=data.ToArray();
    }
   }
   var times=new List<double>();foreach(Match match in Regex.Matches(log,@"\bn:\s*\d+\s+pts:\s*-?\d+\s+pts_time:([-+0-9.eE]+)")){double time;if(double.TryParse(match.Groups[1].Value,System.Globalization.NumberStyles.Float,Engine.Inv,out time))times.Add(time);}
   if(raw.Length%Bytes!=0||times.Count!=raw.Length/Bytes||times.Count>=512)throw new Exception("Images / horodatages incomplets : mesure vidéo refusée.");
   if(times.Count<3)return null;
   var levels=new double[times.Count][];
   for(int f=0;f<times.Count;f++){
    token.ThrowIfCancellationRequested();if(f>0&&times[f]<=times[f-1])throw new Exception("Horodatages vidéo non croissants.");levels[f]=new double[Columns*Rows];
    for(int y=0;y<Height;y++)for(int x=0;x<Width;x++){int p=f*Bytes+(y*Width+x)*3;levels[f][y/Tile*Columns+x/Tile]+=Math.Max(raw[p],Math.Max(raw[p+1],raw[p+2]))/(double)(Tile*Tile);}
   }
   var candidates=new List<Tuple<int,int,double>>();
   for(int f=1;f<times.Count-1;f++){
    if(times[f]<reference-.05||times[f]>=start+length)continue;
    var differences=levels[f].Select((v,i)=>v-levels[f-1][i]).ToArray();
    // The lit device may fill the picture. Accept broad rises too; users must keep
    // framing/lighting fixed because exposure changes cannot be distinguished reliably.
    int best=Array.IndexOf(differences,differences.Max());double rise=differences[best];
    var noise=new List<double>();for(int q=1;q<f;q++)if(times[q]<reference-.05)noise.Add(Math.Abs(levels[q][best]-levels[q-1][best]));
    double threshold=Math.Max(24,noise.DefaultIfEmpty(0).Max()*4);
    if(rise<threshold||levels[f+1][best]-levels[f-1][best]<rise*.6)continue;
    int adjacent=0,bx=best%Columns,by=best/Columns;
    for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)if(bx+dx>=0&&bx+dx<Columns&&by+dy>=0&&by+dy<Rows&&differences[(by+dy)*Columns+bx+dx]>threshold*.5)adjacent++;
    if(adjacent<2)continue;
    // A gradual rise on consecutive frames is one onset; distinct rises are ambiguous.
    if(candidates.Count>0&&f==candidates[candidates.Count-1].Item1+1)continue;
    candidates.Add(Tuple.Create(f,best,rise));
   }
   if(candidates.Count!=1)return null;int frame=candidates[0].Item1;
   return Tuple.Create(times[frame]-reference,times[frame]-times[frame-1]);
  }
 }
}
