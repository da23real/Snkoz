using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
namespace Synkoz {
 public sealed class AudioLive941:IDisposable {
  [StructLayout(LayoutKind.Sequential,Pack=2)]struct Format{public ushort tag,channels;public uint rate,bytes;public ushort align,bits,extra;}
  [StructLayout(LayoutKind.Sequential)]struct Header{public IntPtr data;public uint length,recorded;public UIntPtr user;public uint flags,loops;public IntPtr next;public UIntPtr reserved;}
  [DllImport("winmm.dll")]static extern int waveOutOpen(out IntPtr h,int device,ref Format format,IntPtr callback,IntPtr instance,uint flags);
  [DllImport("winmm.dll")]static extern int waveOutPrepareHeader(IntPtr h,IntPtr header,int size);
  [DllImport("winmm.dll")]static extern int waveOutWrite(IntPtr h,IntPtr header,int size);
  [DllImport("winmm.dll")]static extern int waveOutUnprepareHeader(IntPtr h,IntPtr header,int size);
  [DllImport("winmm.dll")]static extern int waveOutReset(IntPtr h);
  [DllImport("winmm.dll")]static extern int waveOutClose(IntPtr h);
  readonly CancellationTokenSource cancel=new CancellationTokenSource();Task task;public volatile string Error;public volatile AudioFxR2 Settings;public bool Running{get{return task!=null&&!task.IsCompleted;}}
  public void Start(byte[] pcm){if(Running)return;task=Task.Run(()=>{try{Play(pcm,cancel.Token);}catch(Exception ex){Error=ex.GetBaseException().Message;}});}
  static void Check(int error){if(error!=0)throw new Exception("Lecture audio Windows : "+error);}
  void Play(byte[] pcm,CancellationToken token){IntPtr h;var format=new Format{tag=1,channels=2,rate=48000,bytes=192000,align=4,bits=16};Check(waveOutOpen(out h,-1,ref format,IntPtr.Zero,IntPtr.Zero,0));int size=Marshal.SizeOf(typeof(Header));var headers=new IntPtr[2];var buffers=new IntPtr[2];var prepared=new bool[2];var bytes=new byte[3840];AudioFxR2 previous=null;Dsp941 dsp=null;int position=0;try{for(int i=0;i<2;i++){headers[i]=Marshal.AllocHGlobal(size);buffers[i]=Marshal.AllocHGlobal(bytes.Length);}int turn=0;while(!token.IsCancellationRequested){int slot=turn++%2;IntPtr header=headers[slot];if(prepared[slot]){while(!token.IsCancellationRequested&&(((Header)Marshal.PtrToStructure(header,typeof(Header))).flags&1)==0)Thread.Sleep(1);if(token.IsCancellationRequested)break;Check(waveOutUnprepareHeader(h,header,size));prepared[slot]=false;}var settings=Settings;if(settings!=previous){dsp=new Dsp941(settings??new AudioFxR2());previous=settings;}for(int i=0;i<bytes.Length;i+=4){if(position+3>=pcm.Length)position=0;double left=BitConverter.ToInt16(pcm,position)/32768.0,right=BitConverter.ToInt16(pcm,position+2)/32768.0;position+=4;dsp.Process(ref left,ref right);short l=(short)Math.Round(Math.Max(-1,Math.Min(.99996,left))*32768),r=(short)Math.Round(Math.Max(-1,Math.Min(.99996,right))*32768);bytes[i]=(byte)l;bytes[i+1]=(byte)(l>>8);bytes[i+2]=(byte)r;bytes[i+3]=(byte)(r>>8);}Marshal.Copy(bytes,0,buffers[slot],bytes.Length);Marshal.StructureToPtr(new Header{data=buffers[slot],length=(uint)bytes.Length},header,false);Check(waveOutPrepareHeader(h,header,size));prepared[slot]=true;Check(waveOutWrite(h,header,size));}}finally{waveOutReset(h);for(int i=0;i<2;i++){if(prepared[i])waveOutUnprepareHeader(h,headers[i],size);if(buffers[i]!=IntPtr.Zero)Marshal.FreeHGlobal(buffers[i]);if(headers[i]!=IntPtr.Zero)Marshal.FreeHGlobal(headers[i]);}waveOutClose(h);}}
  public void Dispose(){cancel.Cancel();try{if(task!=null)task.GetAwaiter().GetResult();}finally{cancel.Dispose();}}
 }
 public sealed class Dsp941 {
  sealed class Band{readonly double b0,b1,b2,a1,a2;readonly double[] z1=new double[2],z2=new double[2];public Band(EqBandR2 b){double a=Math.Pow(10,b.GainDb/40),w=2*Math.PI*b.Frequency/48000,alpha=Math.Sin(w)/(2*b.Q),a0=1+alpha/a;b0=(1+alpha*a)/a0;b1=-2*Math.Cos(w)/a0;b2=(1-alpha*a)/a0;a1=b1;a2=(1-alpha/a)/a0;}public double Sample(double x,int c){double y=b0*x+z1[c];z1[c]=b1*x-a1*y+z2[c];z2[c]=b2*x-a2*y;return y;}}
  readonly AudioFxR2 fx;readonly Band[] bands;double envelope;public Dsp941(AudioFxR2 settings){fx=UiR2.Copy(settings);bands=fx.Bands.Where(b=>b.Enabled).Select(b=>new Band(b)).ToArray();}
  public void Process(ref double left,ref double right){if(!fx.Enabled)return;double gain=Math.Pow(10,fx.GainDb/20);left*=gain;right*=gain;foreach(var b in bands){left=b.Sample(left,0);right=b.Sample(right,1);}if(fx.Compressor){double peak=Math.Max(Math.Abs(left),Math.Abs(right)),c=Math.Exp(-1/(48000*Math.Max(.00001,(peak>envelope?fx.AttackMs:fx.ReleaseMs)/1000)));envelope=c*envelope+(1-c)*peak;double db=20*Math.Log10(Math.Max(1e-12,envelope)),reduction=db>fx.ThresholdDb?(fx.ThresholdDb+(db-fx.ThresholdDb)/fx.Ratio)-db:0;double compress=Math.Pow(10,(reduction+fx.MakeupDb)/20);left*=compress;right*=compress;}if(fx.Limiter){double peak=Math.Max(Math.Abs(left),Math.Abs(right)),limit=Math.Pow(10,fx.LimitDb/20);if(peak>limit){left*=limit/peak;right*=limit/peak;}}}
 }
}
