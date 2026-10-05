using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Synkoz {
 public sealed partial class CalibrationR5 {
  readonly System.Windows.Forms.Timer playTimer=new System.Windows.Forms.Timer{Interval=15};
  readonly Stopwatch playWatch=new Stopwatch();CancellationTokenSource playCancel;SoundPlayer sound;MemoryStream soundData;long playFirst;FrameCacheR5 playFrames;
  void InitPlayback(){
   playTimer.Tick+=delegate{long n=(long)Math.Floor(playWatch.Elapsed.TotalSeconds*30);if(n>=60){StopPlayback();return;}if(frame==playFirst+n)return;frame=playFirst+n;double t=frame/30.0;canvas.CursorTime=t;canvas.Invalidate();filling=true;seek.Value=(decimal)t;filling=false;position.Text="Image "+frame+" · "+t.ToString("F6")+" s";if(playFrames!=null)SetImage(playFrames.Get(frame));};
   FormClosed+=delegate{StopPlayback();playTimer.Dispose();};
  }
  void StopPlayback(){playTimer.Stop();playWatch.Stop();if(playCancel!=null){playCancel.Cancel();playCancel=null;}if(sound!=null){sound.Stop();sound.Dispose();sound=null;}if(soundData!=null){soundData.Dispose();soundData=null;}playFrames=null;}
  async void PlayExcerpt(){
   if(playTimer.Enabled||playCancel!=null){StopPlayback();return;}Cancel(ref videoCancel);videoRevision++;int v=Selected(videos),a=Selected(audios);if(v<0&&a<0)return;long first=frame;double origin=first/30.0;
   var cancel=new CancellationTokenSource();playCancel=cancel;var token=cancel.Token;var vm=Engine.Snapshot936(draft);var am=Engine.Snapshot936(draft);status.Text="Préparation de 2 secondes à partir du curseur…";
   try{
    var video=v<0?Task.FromResult<FrameCacheR5>(null):Task.Run(()=>CalibrationMediaR5.Video(vm,v,first,60,token));
    var audio=a<0?Task.FromResult<WaveR5>(null):Task.Run(()=>CalibrationMediaR5.Audio(am,a,origin,2,token));
    await Task.WhenAll(video,audio);token.ThrowIfCancellationRequested();if(closed)return;
    playFrames=video.Result;cache=playFrames;playFirst=first;
    if(audio.Result!=null){soundData=CalibrationMediaR5.Sound(audio.Result,origin,2);sound=new SoundPlayer(soundData);sound.Load();sound.Play();}
    if(playFrames!=null)SetImage(playFrames.Get(first));playWatch.Restart();playTimer.Start();status.Text="Lecture de 2 s · la waveform et les repères restent la référence du calage.";
   }catch(OperationCanceledException){}catch(Exception ex){if(!closed)status.Text="Lecture : "+ex.Message;StopPlayback();}finally{if(playCancel==cancel)playCancel=null;cancel.Dispose();}
  }
 }
}
