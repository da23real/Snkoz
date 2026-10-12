using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Synkoz {
 public sealed class MediaInfo {public double Duration;public bool Video,Audio;public int Width=640,Height=480;}
 public static partial class Engine {
  public static string EncodingFF(string encoder){return encoder=="nvenc"?Path.Combine(Root,"Dependencies","Nvenc","ffmpeg.exe"):FF;}
  public static string VideoEncoder(string encoder,bool capture){return encoder=="nvenc"?"-c:v h264_nvenc -preset p1 -cq 23 -b:v 0 -pix_fmt yuv420p ":"-c:v libx264 -preset "+(capture?"ultrafast":"fast")+" -crf "+(capture?"23":"18")+" -pix_fmt yuv420p ";}
  public static MediaInfo ProbeMedia(string path){
   string json=Run(Probe,"-v error -show_entries format=duration:stream=codec_type,width,height -of json "+Q(path));
   var data=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(json);var result=new MediaInfo();
   var format=data["format"] as Dictionary<string,object>;result.Duration=format!=null&&format.ContainsKey("duration")?double.Parse(Convert.ToString(format["duration"],Inv),Inv):0;
   foreach(var item in (System.Collections.IEnumerable)data["streams"]){var s=(Dictionary<string,object>)item;string kind=(string)s["codec_type"];if(kind=="video"){result.Video=true;result.Width=Convert.ToInt32(s["width"]);result.Height=Convert.ToInt32(s["height"]);}if(kind=="audio")result.Audio=true;}
   return result;
  }
  public static void FinalizeSegment(Capture c,double music,double end){
   if(!c.Valid)throw new Exception("Média indisponible ; intervalle manquant conservé dans le montage.");
   double duration=end-music;if(duration<=0)throw new Exception("Aucune mesure complète dans ce segment.");
   bool v=c.Track.Video!=null,a=HasAudio(c.Track);var f=new List<string>();
   if((v&&double.IsNaN(c.VideoOrigin))||(a&&double.IsNaN(c.AudioOrigin)))throw new Exception("Timestamp absent ; brut conservé, média non raccordé.");
   if(v){double begin=Math.Max(0,music-c.VideoOrigin),lead=Math.Max(0,c.VideoOrigin-music);
    f.Add("[0:v]trim=start="+begin.ToString("F6",Inv)+",setpts=PTS-STARTPTS,tpad=start_duration="+lead.ToString("F6",Inv)+":stop_duration="+duration.ToString("F6",Inv)+":color=black,trim=duration="+duration.ToString("F6",Inv)+"[v]");
    if(lead>0.04||c.VideoOrigin+c.Duration<end-0.06)c.Failed=true;
   }
   if(a){double begin=Math.Max(0,music-c.AudioOrigin),lead=Math.Max(0,c.AudioOrigin-music);
    f.Add("[0:a]atrim=start="+begin.ToString("F6",Inv)+",asetpts=PTS-STARTPTS,adelay="+Math.Round(lead*1000).ToString(Inv)+":all=1,apad,atrim=duration="+duration.ToString("F6",Inv)+"[a]");
    if(lead>0.002||c.AudioOrigin+c.Duration<end-0.04)c.Failed=true;
   }
   string output=c.File.Replace("_brut.",".");if(c.Track.Video!=null&&!string.IsNullOrEmpty(c.Track.Video.TextSource))output=Path.ChangeExtension(output,"mp4");string args="-hide_banner -y -i "+Q(c.File)+" -filter_complex "+Q(string.Join(";",f))+" ";
   if(v)args+="-map [v] "+VideoEncoder(c.Track.Encoder,false);
   if(a)args+="-map [a] "+(v?"-c:a aac -b:a 192k ":c.Track.Format=="MP3"?"-c:a libmp3lame -q:a 2 ":"-c:a pcm_s16le ");
   if(v)args+="-movflags +faststart ";Run(EncodingFF(c.Track.Encoder),args+"-t "+duration.ToString("F6",Inv)+" "+Q(output),3600000);
   var info=ProbeMedia(output);if(info.Duration<=0)throw new Exception("Média vide après coupe.");c.File=output;c.Duration=duration;
  }
  // Normalize at export, including explicitly visible missing intervals. Never close a gap by shifting later clips.
  public static void ExportBase936(Montage m,int track,string output){
   if(m.Duration<=0)throw new Exception("Aucun segment complet à exporter.");
   foreach(var b in m.Blocks.Where(b=>!string.IsNullOrEmpty(b.File)))if(string.Equals(Path.GetFullPath(output),Store934.MediaPath(m.Folder,b.File),StringComparison.OrdinalIgnoreCase))throw new Exception("Choisir un fichier distinct des médias de la prise.");
   var t=m.Tracks[track];bool video=t.Video!=null,audio=HasAudio(t);int w=640,h=480;
   var blocks=m.Blocks.Where(b=>b.Track==track).OrderBy(b=>b.Start).ToArray();
   if(video)foreach(var b in blocks.Where(b=>!string.IsNullOrEmpty(b.File))){try{var info=ProbeMedia(Store934.MediaPath(m.Folder,b.File));w=info.Width+(info.Width%2);h=info.Height+(info.Height%2);break;}catch{}}
   string work=Path.Combine(Path.GetDirectoryName(Path.GetFullPath(output)),".Snkoz_export_"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(work);
   var parts=new List<string>();var lengths=new List<double>();double cursor=0;
   Action<string,double> part=(input,duration)=>{
    if(duration<0.000001)return;string file=Path.Combine(work,parts.Count.ToString("D5")+(video?".mkv":".wav"));MakePart(input,file,video,audio,w,h,duration,t.Encoder);parts.Add(file);lengths.Add(duration);
   };
   try{
    foreach(var b in blocks){if(b.Start>cursor)part(null,b.Start-cursor);if(b.Start<cursor-0.001)throw new Exception("Blocs superposés dans le montage.");
     string input=string.IsNullOrEmpty(b.File)?null:Store934.MediaPath(m.Folder,b.File);if(input!=null&&!File.Exists(input))throw new Exception("Média manquant : "+b.File);
     part(input,b.Duration);cursor=b.Start+b.Duration;
    }
    if(cursor<m.Duration)part(null,m.Duration-cursor);if(parts.Count==0)throw new Exception("Piste vide.");
    string list=Path.Combine(work,"concat.txt");File.WriteAllLines(list,parts.Select((p,i)=>"file '"+Path.GetFileName(p)+"'\nduration "+lengths[i].ToString("F6",Inv)),new UTF8Encoding(false));
    string final=Path.Combine(work,"export"+Path.GetExtension(output));
    string codec=video?VideoEncoder(t.Encoder,false)+(audio?"-c:a aac -b:a 192k ":"")+"-movflags +faststart ":t.Format=="MP3"?"-c:a libmp3lame -q:a 2 ":"-c:a pcm_s16le ";
    Run(video?EncodingFF(t.Encoder):FF,"-hide_banner -y -f concat -safe 1 -i "+Q(list)+" "+codec+"-t "+m.Duration.ToString("F6",Inv)+" "+Q(final),3600000);
    if(ProbeMedia(final).Duration<=0)throw new Exception("Export vide.");File.Copy(final,output,true);
   }finally{Directory.Delete(work,true);}
  }
  static void MakePart(string input,string output,bool video,bool audio,int w,int h,double duration,string encoder){
   string d=duration.ToString("F6",Inv),args="-hide_banner -y ";bool hasV=false,hasA=false;
   if(input!=null){var p=ProbeMedia(input);hasV=p.Video;hasA=p.Audio;args+="-i "+Q(input)+" ";}
   int next=input==null?0:1,vi=0,ai=0;
   if(video&&!hasV){vi=next++;args+="-f lavfi -i "+Q("color=c=black:s="+w+"x"+h+":r=30")+" ";}
   if(audio&&!hasA){ai=next++;args+="-f lavfi -i anullsrc=r=48000:cl=stereo ";}
   var filters=new List<string>();
   if(video)filters.Add("["+vi+":v]setpts=PTS-STARTPTS,scale="+w+":"+h+":force_original_aspect_ratio=decrease,pad="+w+":"+h+":(ow-iw)/2:(oh-ih)/2,setsar=1,fps=30,tpad=stop_duration="+d+":color=black,trim=duration="+d+"[v]");
   if(audio)filters.Add("["+ai+":a]asetpts=PTS-STARTPTS,aresample=48000,aformat=channel_layouts=stereo,apad,atrim=duration="+d+"[a]");
   args+="-filter_complex "+Q(string.Join(";",filters))+" ";if(video)args+="-map [v] "+VideoEncoder(encoder,false);if(audio)args+="-map [a] "+"-c:a pcm_s16le ";
   Run(video?EncodingFF(encoder):FF,args+"-t "+d+" "+Q(output),3600000);
  }
 }
}
