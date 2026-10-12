using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
namespace Synkoz {
 public sealed partial class MontagePanel {
  static string ThumbnailKeyR10(string path,int cap) {
   var f=new FileInfo(path);
   string identity="thumb-r10-96x54|"+Path.GetFullPath(path).ToUpperInvariant()+"|"+f.Length+"|"+f.LastWriteTimeUtc.Ticks+"|"+cap;
   using(var hash=SHA256.Create())return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(identity))).Replace("-","").Substring(0,32);
  }
  static List<ThumbA01> CachedThumbsR10(string path,int cap,CancellationToken token,string cacheRoot=null,double[] sampleTimes=null,bool force=false) {
   token.ThrowIfCancellationRequested();string key=ThumbnailKeyR10(path,cap)+SamplingKeyR11(sampleTimes);
   string dir=cacheRoot??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Snkoz","Cache","Miniatures"),file=Path.Combine(dir,key+".bin");
   var result=new List<ThumbA01>();
   try {
    if(force)throw new FileNotFoundException();using(var r=new BinaryReader(File.OpenRead(file))) {
     if(r.ReadInt32()!=100110)throw new InvalidDataException();int count=r.ReadInt32();if(count<1||count>cap)throw new InvalidDataException();
     for(int i=0;i<count;i++){token.ThrowIfCancellationRequested();double t=r.ReadDouble();int size=r.ReadInt32();if(double.IsNaN(t)||double.IsInfinity(t)||t<0||(i>0&&t<result[i-1].Time)||size<1||size>65536)throw new InvalidDataException();byte[] bytes=r.ReadBytes(size);if(bytes.Length!=size)throw new EndOfStreamException();using(var stream=new MemoryStream(bytes))using(var image=new Bitmap(stream)){if(image.Width!=96||image.Height!=54)throw new InvalidDataException();result.Add(new ThumbA01{Time=t,Image=new Bitmap(image)});}}
     if(r.BaseStream.Position!=r.BaseStream.Length)throw new InvalidDataException();
    }
    return result;
   }catch(OperationCanceledException){foreach(var t in result)t.Image.Dispose();throw;}
   catch{foreach(var t in result)t.Image.Dispose();}
   result=ReadThumbsA01(path,cap,token,sampleTimes,dir);
   string temp=file+"."+Guid.NewGuid().ToString("N")+".tmp";
   try {
    token.ThrowIfCancellationRequested();if(result.Count==0||key!=ThumbnailKeyR10(path,cap)+SamplingKeyR11(sampleTimes))return result;
    Directory.CreateDirectory(dir);
    using(var w=new BinaryWriter(File.Create(temp))){w.Write(100110);w.Write(result.Count);foreach(var t in result){token.ThrowIfCancellationRequested();using(var stream=new MemoryStream()){t.Image.Save(stream,ImageFormat.Png);w.Write(t.Time);w.Write((int)stream.Length);w.Write(stream.ToArray());}}}
    if(File.Exists(file))File.Replace(temp,file,null);else File.Move(temp,file);
   }catch(OperationCanceledException){foreach(var t in result)t.Image.Dispose();throw;}
   catch(IOException){}catch(UnauthorizedAccessException){}
   finally{try{if(File.Exists(temp))File.Delete(temp);}catch{}}
   return result;
  }
 }
}
