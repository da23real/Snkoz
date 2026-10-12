using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
namespace Synkoz {
 public static class RenderRangeR3 {
  static string F(double n){return n.ToString("F9",Engine.Inv);}
  public static void Run(string args,CancellationToken token,Action<double> seconds=null){token.ThrowIfCancellationRequested();using(var p=Engine.Create(Engine.FF,(seconds==null?"":"-progress pipe:1 -nostats ")+args)){p.Start();var err=p.StandardError.ReadToEndAsync();var stdout=System.Threading.Tasks.Task.Run(()=>{string line;while((line=p.StandardOutput.ReadLine())!=null){double us;if(seconds!=null&&line.StartsWith("out_time_us=")&&double.TryParse(line.Substring(12),System.Globalization.NumberStyles.Float,Engine.Inv,out us))seconds(us/1000000);}});using(token.Register(()=>{try{p.Kill();}catch{}})){p.WaitForExit();stdout.GetAwaiter().GetResult();token.ThrowIfCancellationRequested();if(p.ExitCode!=0)throw new Exception(err.GetAwaiter().GetResult());}}}
  public static void Prepare(Montage m,int index,string output,double origin,double length,int height,CancellationToken token,bool audioOnly,bool videoOnly=false,Action<int> progress=null){
   var t=m.Tracks[index];var videoProgress=RenderStagesR9.Progress("Vidéo · "+Sources936.Label(t));var audioProgress=RenderStagesR9.Progress("Audio · "+Sources936.Label(t));var muxProgress=RenderStagesR9.Progress("Assemblage · "+Sources936.Label(t));bool video=!audioOnly&&t.Video!=null,audio=!videoOnly&&Engine.HasAudio(t);if(!video&&!audio)throw new Exception("Cette piste ne contient pas de son.");
   var blocks=PrerollR4.Blocks(m,index,!audioOnly&&t.Video!=null).ToArray();int w=t.Fx.Graphic?t.Fx.CanvasWidth:640,h=t.Fx.Graphic?t.Fx.CanvasHeight:480;
   if(video&&!t.Fx.Graphic)foreach(var b in blocks.Where(b=>!b.TransparentR7&&!string.IsNullOrEmpty(b.File))){var info=Engine.ProbeMedia(Store934.MediaPath(m.Folder,b.File));if(info.Video){w=info.Width;h=info.Height;break;}}
   if(height>0){int target=OrientationR2.Degrees(t)%180==0?h:w;if(target>height){double ratio=height/(double)target;w=(int)Math.Round(w*ratio);h=(int)Math.Round(h*ratio);}}
   w=Math.Max(2,w+w%2);h=Math.Max(2,h+h%2);
   string work=Path.Combine(ProjectStorageR11.Cache(m,"Export"),"Range_"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(work);
   try{string vf=null,af=null;if(video)vf=Stream(m,t,blocks,work,true,w,h,origin,length,t.Fx.Shift,token,p=>{videoProgress(p);if(progress!=null)progress((int)(p*(audio?.55:.9)));});if(audio)af=Stream(m,t,PrerollR4.Blocks(m,index,false).ToArray(),work,false,w,h,origin,length,t.Fx.AudioShift,token,p=>{audioProgress(p);if(progress!=null)progress((video?55:0)+(int)(p*(video?.35:.9)));});
    string args="-hide_banner -loglevel error -y "+(video?"-i "+Engine.Q(vf)+" ":"")+(audio?"-i "+Engine.Q(af)+" ":"");
    if(video)args+="-map 0:v -c:v copy ";if(audio)args+="-map "+(video?1:0)+":a -af "+Engine.Q("anull"+AudioFxR2.Filter(t.Fx.Audio))+" -c:a pcm_s16le ";
    Run(args+"-t "+F(length)+" "+Engine.Q(output),token,seconds=>muxProgress((int)Math.Max(0,Math.Min(99,100*seconds/Math.Max(.001,length)))));muxProgress(100);if(video)videoProgress(100);if(audio)audioProgress(100);
    Mute940.Audio(m,index,output,origin,length,token);if(progress!=null)progress(100);
   }finally{Directory.Delete(work,true);}
  }
  static string Stream(Montage m,Track track,MediaBlock[] blocks,string work,bool video,int w,int h,double origin,double length,double shift,CancellationToken token,Action<int> progress){
   string prefix=video?"v":"a";var files=new List<string>();double cursor=origin,end=origin+length;
   Action<string,double,double,MediaBlock> part=(input,offset,duration,block)=>{if(duration<.0000001)return;token.ThrowIfCancellationRequested();string path=Path.Combine(work,prefix+files.Count.ToString("D5")+(video?".mkv":".wav"));
    string args="-hide_banner -loglevel error -y ";bool blank=input==null;
    if(blank)args+="-f lavfi -i "+Engine.Q(video?"color=c=black"+(track.Fx.Graphic||(block!=null&&block.TransparentR7)?"@0":"")+":s="+w+"x"+h+":r=30,format=bgra":"anullsrc=r=48000:cl=stereo")+" ";
    else args+=(video&&Path.GetExtension(input).Equals(".png",StringComparison.OrdinalIgnoreCase)?"-loop 1 -framerate 30 ":video&&Path.GetExtension(input).Equals(".gif",StringComparison.OrdinalIgnoreCase)?"-stream_loop -1 ":"")+"-ss "+F(Math.Max(0,offset))+" -i "+Engine.Q(input)+" ";
    if(video){string filter="setpts=PTS-STARTPTS,scale="+w+":"+h+":force_original_aspect_ratio=decrease,pad="+w+":"+h+":(ow-iw)/2:(oh-ih)/2,setsar=1,fps=30,format=bgra,tpad=stop_duration="+F(duration)+":color=black"+(track.Fx.Graphic||(block!=null&&block.TransparentR7)?"@0":"")+",trim=duration="+F(duration);if(block!=null&&block.Imported942&&!blank){int sw=Math.Max(2,(int)Math.Round(w*block.Size942/100));string bg=track.Fx.Graphic?"black@0":"black";string graph="color=c="+bg+":s="+w+"x"+h+":r=30,format=bgra[bg];[0:v]setpts=PTS-STARTPTS,"+(block.Sprite942?"crop=iw/4:ih/2:x=mod(floor((t+"+F(offset)+")*"+F(block.Fps942)+")\\,4)*out_w:y=floor(mod(floor((t+"+F(offset)+")*"+F(block.Fps942)+")\\,8)/4)*out_h,":"")+"scale="+sw+":"+(block.KeepProportionsR4?"-1":Math.Max(2,(int)(h*block.HeightR4/100)).ToString())+",format=bgra[fg];[bg][fg]overlay=x="+F(w*block.X942/100)+"-overlay_w/2:y="+F(h*block.Y942/100)+"-overlay_h/2:shortest=1:format=auto,format=bgra[out]";args+="-filter_complex "+Engine.Q(graph)+" -map [out] -an -c:v ffv1 -level 3 -pix_fmt bgra ";}else args+="-an -vf "+Engine.Q(filter)+" -c:v ffv1 -level 3 -pix_fmt bgra ";}
    else args+="-vn -af "+Engine.Q("aresample=48000:first_pts=0,asetpts=PTS-STARTPTS,aformat=channel_layouts=stereo,apad,atrim=duration="+F(duration))+" -c:a pcm_s16le ";
    Run(args+"-t "+F(duration)+" "+Engine.Q(path),token,seconds=>{if(progress!=null)progress((int)Math.Max(0,Math.Min(99,100*(cursor-origin+Math.Min(duration,seconds))/length)));});files.Add(path);
   };
   foreach(var b in blocks){double a=Math.Max(origin,b.Start+shift),z=Math.Min(end,b.Start+b.Duration+shift);if(video){a=origin+Math.Ceiling((a-origin)*30-1e-8)/30;z=Math.Min(end,origin+Math.Ceiling((z-origin)*30-1e-8)/30);}if(z<=a)continue;if(a<cursor-.001)throw new Exception("Blocs superposés dans la piste.");if(a>cursor)part(null,0,a-cursor,null);
    string input=video&&b.TransparentR7?null:string.IsNullOrEmpty(b.File)?null:Store934.MediaPath(m.Folder,b.File);if(input!=null){if(!File.Exists(input))throw new Exception("Média manquant : "+input);var info=Engine.ProbeMedia(input);if(video?!info.Video:!info.Audio)input=null;}
    part(input,a-b.Start-shift+(video?b.RawVideoZero??0:b.RawAudioZero??0),z-a,b);cursor=z;
   }
   if(cursor<end)part(null,0,end-cursor,null);if(files.Count==0)throw new Exception("Zone vide.");
   string list=Path.Combine(work,prefix+".txt"),result=Path.Combine(work,prefix+"_joined"+(video?".mkv":".wav"));File.WriteAllLines(list,files.Select(f=>"file '"+Path.GetFileName(f)+"'"),new UTF8Encoding(false));Run("-hide_banner -loglevel error -y -f concat -safe 1 -i "+Engine.Q(list)+" -c copy -t "+F(length)+" "+Engine.Q(result),token);return result;
  }
  public static string ReferenceAudio(Montage m,int track,string video,string work,double origin,double length,CancellationToken token){
   var mixProgress=RenderStagesR9.Progress("Mixage audio");mixProgress(0);var fx=m.PreviewAudio939??new TrackFx936();var choices=fx.PreviewAudioMode==2?fx.PreviewAudio.Where(c=>c.Track>=0&&c.Track<m.Tracks.Count&&Engine.HasAudio(m.Tracks[c.Track])).ToList():new List<AudioChoiceR3>();
   if(fx.PreviewAudioMode==0){int selected=Engine.HasAudio(m.Tracks[track])?track:m.ReferenceAudio;if(selected>=0&&selected<m.Tracks.Count&&Engine.HasAudio(m.Tracks[selected]))choices.Add(new AudioChoiceR3{Track=selected});}
   bool hasVideo=m.Tracks[track].Video!=null;string output=Path.Combine(work,"preview_audio"+(hasVideo?".mp4":".wav"));
   if(choices.Count==0){if(!hasVideo){Run("-hide_banner -loglevel error -y -f lavfi -i anullsrc=r=48000:cl=stereo -t "+F(length)+" "+Engine.Q(output),token);}else Run("-hide_banner -loglevel error -y -i "+Engine.Q(video)+" -an -c:v copy "+Engine.Q(output),token,seconds=>mixProgress((int)Math.Max(0,Math.Min(99,100*seconds/Math.Max(.001,length)))));mixProgress(100);return output;}
   string args="-hide_banner -loglevel error -y -i "+Engine.Q(video)+" ";var filters=new List<string>();var names=new List<string>();
   for(int i=0;i<choices.Count;i++){var c=choices[i];string path=Path.Combine(work,"ref"+i+".mkv");Prepare(m,c.Track,path,origin,length,0,token,true);args+="-i "+Engine.Q(path)+" ";filters.Add("["+(i+1)+":a]volume="+F(Math.Pow(10,c.GainDb/20))+"[a"+i+"]");names.Add("[a"+i+"]");}
   filters.Add(string.Join("",names)+"amix=inputs="+choices.Count+":normalize=0:duration=longest,alimiter=limit=0.98:level=false:latency=true[out]");
   Run(args+"-filter_complex "+Engine.Q(string.Join(";",filters))+" "+(hasVideo?"-map 0:v -c:v copy ":"")+"-map [out] "+(hasVideo?"-c:a aac -b:a 192k ":"-c:a pcm_s16le ")+"-t "+F(length)+" "+Engine.Q(output),token,seconds=>mixProgress((int)Math.Max(0,Math.Min(99,100*seconds/Math.Max(.001,length)))));mixProgress(100);return output;
  }
 }
}
