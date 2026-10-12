using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
namespace Synkoz {
 public sealed class VideoAnimation941:IDisposable {
  public readonly List<Bitmap> Frames=new List<Bitmap>();public int Width,Height;
  public VideoAnimation941(string path){var info=Engine.ProbeMedia(path);if(!info.Video||info.Duration<=0||info.Duration>10.05||new FileInfo(path).Length>20*1024*1024)throw new Exception("Mini MP4 : vidéo de 10 secondes et 20 Mo maximum.");double scale=Math.Min(1,320.0/Math.Max(info.Width,info.Height));Width=Math.Max(2,(int)(info.Width*scale)/2*2);Height=Math.Max(2,(int)(info.Height*scale)/2*2);using(var p=Engine.Create(Engine.FF,"-v error -i "+Engine.Q(path)+" -an -vf fps=15,scale="+Width+":"+Height+" -frames:v 150 -pix_fmt bgra -f rawvideo pipe:1")){try{p.Start();var error=p.StandardError.ReadToEndAsync();byte[] bytes=new byte[Width*Height*4];while(Frames.Count<150){int count=0;while(count<bytes.Length){int n=p.StandardOutput.BaseStream.Read(bytes,count,bytes.Length-count);if(n==0)break;count+=n;}if(count==0)break;if(count!=bytes.Length)throw new Exception("Image MP4 incomplète.");var bitmap=new Bitmap(Width,Height,PixelFormat.Format32bppArgb);var data=bitmap.LockBits(new Rectangle(0,0,Width,Height),ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);Marshal.Copy(bytes,0,data.Scan0,bytes.Length);bitmap.UnlockBits(data);Frames.Add(bitmap);}p.WaitForExit();if(p.ExitCode!=0||Frames.Count==0)throw new Exception("Décodage mini MP4 : "+error.Result);}catch{Dispose();try{if(!p.HasExited)p.Kill();}catch{}throw;}}}
  public void Dispose(){foreach(var b in Frames)b.Dispose();Frames.Clear();}
 }
}
