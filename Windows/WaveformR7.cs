using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace Synkoz {
 public sealed partial class MontagePanel {
  sealed class WaveR7 {public float[] Peaks;public double Step;public List<float[]> Levels=new List<float[]>();}
  readonly Dictionary<string,WaveR7> wavesR7=new Dictionary<string,WaveR7>(StringComparer.OrdinalIgnoreCase);
  readonly HashSet<string> wavesPendingR7=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  async void LoadWavesR7(){if(Current==null||IsDisposed)return;var model=Current;int generation=thumbGenerationA01;var token=thumbsCancelA01.Token;
   var files=model.Blocks.Where(b=>b.Audio&&!string.IsNullOrEmpty(ThumbFileA01(b))).Select(ThumbFileA01).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();int cap=Math.Max(2048,1000000/Math.Max(1,files.Length));foreach(string file in files){
    if(wavesR7.ContainsKey(file)||!wavesPendingR7.Add(file))continue;try{string path=Store934.MediaPath(model.Folder,file);var wave=await Task.Run(()=>ReadWaveR7(path,cap,token),token);if(IsDisposed||generation!=thumbGenerationA01)return;wavesR7[file]=wave;RefreshNavigation940();}catch(OperationCanceledException){return;}catch(Exception ex){if(!IsDisposed&&generation==thumbGenerationA01){wavesR7[file]=new WaveR7{Peaks=new float[0],Step=.01};status.Text="Waveform indisponible : "+Path.GetFileName(file)+" · "+ex.Message;}Synkoz.ErrorR8.Show(this,ex.Message,"Erreur");}finally{if(generation==thumbGenerationA01)wavesPendingR7.Remove(file);}}
  }
  static WaveR7 ReadWaveR7(string path,int cap,CancellationToken token){var info=Engine.ProbeMedia(path);int frames=Math.Max(480,(int)Math.Ceiling(info.Duration*48000/cap));var peaks=new List<float>();
   using(var p=Engine.Create(Engine.FF,"-hide_banner -loglevel error -i "+Engine.Q(path)+" -vn -ac 2 -ar 48000 -c:a pcm_s16le -f s16le pipe:1")){p.Start();var errors=p.StandardError.ReadToEndAsync();using(token.Register(()=>{try{p.Kill();}catch{}})){byte[] buffer=new byte[16384];int count=0,peak=0,carry=-1,samples=0,n;while((n=p.StandardOutput.BaseStream.Read(buffer,0,buffer.Length))>0){token.ThrowIfCancellationRequested();for(int i=0;i<n;i++){if(carry<0){carry=buffer[i];continue;}short sample=(short)(carry|(buffer[i]<<8));carry=-1;peak=Math.Max(peak,Math.Abs((int)sample));if(++samples>=frames*2){peaks.Add(peak/32768f);peak=samples=0;if(peaks.Count>=1000001){try{p.Kill();}catch{}throw new Exception("Média trop long pour le cache waveform.");}}}count+=n;}if(samples>0)peaks.Add(peak/32768f);p.WaitForExit();token.ThrowIfCancellationRequested();if(p.ExitCode!=0)throw new Exception(errors.GetAwaiter().GetResult());}}
   var result=new WaveR7{Peaks=peaks.ToArray(),Step=frames/48000.0};result.Levels.Add(result.Peaks);while(result.Levels.Last().Length>1){var previous=result.Levels.Last();var level=new float[(previous.Length+1)/2];for(int i=0;i<level.Length;i++)level[i]=Math.Max(previous[i*2],previous[Math.Min(previous.Length-1,i*2+1)]);result.Levels.Add(level);}return result;
  }
  void DrawWaveR7(Graphics g,MediaBlock b,Rectangle rect,double scale){if(!b.Audio||b.Muted940)return;WaveR7 wave;if(!wavesR7.TryGetValue(ThumbFileA01(b)??"",out wave)||wave.Peaks.Length==0)return;int level=0;double step=wave.Step;while(level+1<wave.Levels.Count&&step*2<=1/scale){level++;step*=2;}var peaks=wave.Levels[level];double origin=b.RawAudioZero??b.MediaOffset939,delay=Current.Tracks[b.Track].Fx.AudioShift;int center=rect.Top+(b.Video?rect.Height*3/4:rect.Height/2+8),height=Math.Max(2,b.Video?rect.Height/5:rect.Height/2-12);int left=Math.Max(rect.Left,0),right=Math.Min(rect.Right,(int)g.VisibleClipBounds.Right);using(var pen=new Pen(Color.MediumAquamarine)){for(int x=left;x<right;x++){double a=origin+(x-rect.Left)/scale-delay,z=a+1/scale;int lo=Math.Max(0,(int)Math.Floor(a/step)),hi=Math.Min(peaks.Length-1,(int)Math.Ceiling(z/step));float peak=0;for(int i=lo;i<=hi;i++)peak=Math.Max(peak,peaks[i]);g.DrawLine(pen,x,center-peak*height,x,center+peak*height);}}DrawDbR21(g,rect,center,height,Current.Tracks[b.Track].Fx.Audio);
  }
 }
}
