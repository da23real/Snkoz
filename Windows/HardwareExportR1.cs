using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
namespace Synkoz {
 public enum ExportEncoderR1 { Automatique, NVIDIA, Logiciel }
 public sealed class NvidiaExportExceptionR1:Exception {public NvidiaExportExceptionR1(string text,Exception inner=null):base(text,inner){}}
 public static class HardwareExportR1 {
  public static void Process(string executable,string args,CancellationToken token,int timeout=0){token.ThrowIfCancellationRequested();using(var p=Engine.Create(executable,args)){p.Start();var error=p.StandardError.ReadToEndAsync();var output=p.StandardOutput.ReadToEndAsync();using(token.Register(()=>{try{p.Kill();}catch{}})){bool done=timeout==0?p.WaitForExit(int.MaxValue):p.WaitForExit(timeout);if(!done){try{p.Kill();}finally{p.WaitForExit();}}token.ThrowIfCancellationRequested();if(!done)throw new TimeoutException("L’encodeur ne répond pas.");if(p.ExitCode!=0)throw new Exception(error.GetAwaiter().GetResult());}}}
  public static void Probe(CancellationToken token){Process(Engine.EncodingFF("nvenc"),"-hide_banner -loglevel error -f lavfi -i color=black:s=320x180:r=30 -frames:v 3 -an "+Engine.VideoEncoder("nvenc",false)+"-f null -",token,15000);}
  public static void Encode(string args,string encoder,CancellationToken token){try{Process(Engine.EncodingFF(encoder),args,token);}catch(OperationCanceledException){throw;}catch(Exception ex){if(encoder=="nvenc")throw new NvidiaExportExceptionR1(ex.Message,ex);throw;}}
  // One decision per job; a composite retry restarts every segment with one codec configuration.
  public static string Execute(ExportEncoderR1 mode,CancellationToken token,Action<string,int> report,Action<CancellationToken> probe,Action<string> render){string encoder="x264",fallback=null;Action<string> log=message=>{try{string folder=Path.Combine(Engine.Root,"Logs","Exports");Directory.CreateDirectory(folder);File.AppendAllText(Path.Combine(folder,"encoders.log"),DateTime.Now.ToString("o")+" "+message+Environment.NewLine);}catch{}};
   token.ThrowIfCancellationRequested();if(mode!=ExportEncoderR1.Logiciel){report("Vérification de NVIDIA…",0);try{probe(token);encoder="nvenc";}catch(OperationCanceledException){throw;}catch(Exception ex){if(mode==ExportEncoderR1.NVIDIA)throw new NvidiaExportExceptionR1("NVIDIA indisponible. Choisir Automatique ou Logiciel. "+ex.Message,ex);fallback="NVIDIA indisponible";log(ex.ToString());}}
   report(encoder=="nvenc"?"Encodeur : NVIDIA NVENC":"Encodeur : logiciel"+(fallback==null?"":" — "+fallback),0);
   try{render(encoder);}catch(NvidiaExportExceptionR1 ex){token.ThrowIfCancellationRequested();if(mode!=ExportEncoderR1.Automatique||encoder!="nvenc")throw;fallback="repli après échec NVIDIA";encoder="x264";log(ex.ToString());report("Reprise depuis le début en logiciel — échec NVIDIA",-1);render(encoder);}
   token.ThrowIfCancellationRequested();string result=encoder=="nvenc"?"NVIDIA NVENC":"logiciel"+(fallback==null?"":" · "+fallback);report("Terminé · "+result,100);return result;
  }
  public static void Track(Montage model,int track,string output,int height,ExportEncoderR1 mode,CancellationToken token,Action<string,int> report){var original=model.Tracks[track];if(original.Video==null||original.Fx.Graphic){report(original.Video==null?"Export audio":"ProRes 4444 · transparence conservée",0);var special=Engine.Snapshot936(model);special.Tracks[track].Encoder="x264";Engine.Export936(special,track,output,token,p=>report(original.Video==null?"Audio":"ProRes 4444 · transparence",p),0,-1,height);report(original.Video==null?"Terminé · audio":"Terminé · ProRes 4444 transparent",100);return;}
   Execute(mode,token,report,Probe,encoder=>{var snapshot=Engine.Snapshot936(model);snapshot.Tracks[track].Encoder=encoder;Engine.Export936(snapshot,track,output,token,p=>report((encoder=="nvenc"?"NVIDIA":"Logiciel")+" · "+(p<20?"préparation":p<95?"effets et encodage":"finalisation"),p),0,-1,height);});}
  public static void Montage(Montage model,string output,int height,ExportEncoderR1 mode,CancellationToken token,Action<string,int> report){Execute(mode,token,report,Probe,encoder=>Composite941.Export(model,output,height,token,(phase,p)=>report((encoder=="nvenc"?"NVIDIA":"Logiciel")+" · "+phase,p),encoder));}
 }
}

