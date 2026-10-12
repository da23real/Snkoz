using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
namespace UrbanKaoz.DisplayRecorder {
  internal sealed class AudioEndpoint : IDisposable {
    public readonly string Name;readonly int device;WaveInMeter capture;
    AudioEndpoint(int device,string name){this.device=device;Name=name;}
    public float Peak {get{if(capture==null)capture=new WaveInMeter(device);return capture.Peak;}}
    public void Dispose(){if(capture!=null){capture.Dispose();capture=null;}}
    public static List<AudioEndpoint> Enumerate(){var result=new List<AudioEndpoint>();int count=waveInGetNumDevs();for(int i=0;i<count;i++){WaveInCaps caps;if(waveInGetDevCaps((IntPtr)i,out caps,Marshal.SizeOf(typeof(WaveInCaps)))==0)result.Add(new AudioEndpoint(i,caps.name));}return result;}
    [DllImport("winmm.dll")]static extern int waveInGetNumDevs();
    [DllImport("winmm.dll",CharSet=CharSet.Auto)]static extern int waveInGetDevCaps(IntPtr device,out WaveInCaps caps,int size);
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto)]struct WaveInCaps{public short manufacturer,product;public int driverVersion;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=32)]public string name;public int formats;public short channels,reserved;}
  }

  internal sealed class WaveInMeter : IDisposable {
    delegate void WaveCallback(IntPtr input,int message,IntPtr instance,IntPtr header,IntPtr reserved);
    readonly WaveCallback callback;readonly byte[] data=new byte[4096];GCHandle pinned;IntPtr handle,header;volatile int peak,lastSample;volatile bool active;
    public WaveInMeter(int device){callback=OnWave;WaveFormat format=new WaveFormat{formatTag=1,channels=1,samplesPerSecond=44100,bitsPerSample=16,blockAlign=2,averageBytesPerSecond=88200};int error=waveInOpen(out handle,device,ref format,callback,IntPtr.Zero,0x30000);if(error!=0)throw new InvalidOperationException("Ouverture audio impossible, code "+error);try{pinned=GCHandle.Alloc(data,GCHandleType.Pinned);WaveHeader h=new WaveHeader{data=pinned.AddrOfPinnedObject(),bufferLength=data.Length};header=Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WaveHeader)));Marshal.StructureToPtr(h,header,false);Check(waveInPrepareHeader(handle,header,Marshal.SizeOf(typeof(WaveHeader))),"Préparation audio");active=true;Check(waveInAddBuffer(handle,header,Marshal.SizeOf(typeof(WaveHeader))),"Tampon audio");Check(waveInStart(handle),"Démarrage audio");}catch{Dispose();throw;}}
    public float Peak{get{return unchecked((uint)(Environment.TickCount-lastSample))>250?0:peak/32768f;}}
    void OnWave(IntPtr input,int message,IntPtr instance,IntPtr waveHeader,IntPtr reserved){if(message!=0x3C0||!active)return;WaveHeader h=(WaveHeader)Marshal.PtrToStructure(waveHeader,typeof(WaveHeader));int maximum=0;for(int i=0;i+1<(int)h.bytesRecorded;i+=2){int sample=(short)(data[i]|data[i+1]<<8);int absolute=sample==short.MinValue?32768:Math.Abs(sample);if(absolute>maximum)maximum=absolute;}peak=maximum;lastSample=Environment.TickCount;if(active)waveInAddBuffer(handle,header,Marshal.SizeOf(typeof(WaveHeader)));}
    static void Check(int error,string action){if(error!=0)throw new InvalidOperationException(action+" impossible, code "+error);}
    public void Dispose(){active=false;if(handle!=IntPtr.Zero){waveInStop(handle);waveInReset(handle);if(header!=IntPtr.Zero)waveInUnprepareHeader(handle,header,Marshal.SizeOf(typeof(WaveHeader)));waveInClose(handle);handle=IntPtr.Zero;}if(header!=IntPtr.Zero){Marshal.FreeHGlobal(header);header=IntPtr.Zero;}if(pinned.IsAllocated)pinned.Free();GC.KeepAlive(callback);}
    [DllImport("winmm.dll")]static extern int waveInOpen(out IntPtr input,int device,ref WaveFormat format,WaveCallback callback,IntPtr instance,int flags);
    [DllImport("winmm.dll")]static extern int waveInPrepareHeader(IntPtr input,IntPtr header,int size);
    [DllImport("winmm.dll")]static extern int waveInUnprepareHeader(IntPtr input,IntPtr header,int size);
    [DllImport("winmm.dll")]static extern int waveInAddBuffer(IntPtr input,IntPtr header,int size);
    [DllImport("winmm.dll")]static extern int waveInStart(IntPtr input);
    [DllImport("winmm.dll")]static extern int waveInStop(IntPtr input);
    [DllImport("winmm.dll")]static extern int waveInReset(IntPtr input);
    [DllImport("winmm.dll")]static extern int waveInClose(IntPtr input);
    [StructLayout(LayoutKind.Sequential)]struct WaveFormat{public short formatTag,channels;public int samplesPerSecond,averageBytesPerSecond;public short blockAlign,bitsPerSample,size;}
    [StructLayout(LayoutKind.Sequential)]struct WaveHeader{public IntPtr data;public int bufferLength,bytesRecorded;public IntPtr user;public int flags,loops;public IntPtr next,reserved;}
  }
}
