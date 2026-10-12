using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
namespace VoixMidi {
public class Options {
 public double Gate=-48, Low=-40, High=-6; public bool Auto=true; public int CC=20, Channel=0, VelLow=1, VelHigh=127;
 public bool Intensity=true;public int IntensityCC=-1;public int[] Values={0,16,32,48,64,80,96,112,127};
 public void Validate(){if(!Auto && Low>=High)throw new Exception("Le minimum de volume doit être inférieur au maximum."); if(VelLow>VelHigh)throw new Exception("La vélocité minimale doit être inférieure ou égale à la maximale."); if(CC<0||CC>119)throw new Exception("Choisir un CC entre 0 et 119 (120–127 sont des commandes de canal).");}
}
public class Frame {public double Time,Db,Hz,Confidence;public int Shape,Note=-1,Velocity;}
public class Result {public float[] Audio; public List<Frame> Frames=new List<Frame>(); public double Low,High; public double Duration {get{return Audio.Length/16000.0;}}}
public static class Engine {
 public static readonly string[] Names={"Repos","P / B — attaque","M / N — nasal","S / Z — sifflant","CH / J — chuintant","F / V — souffle","A — ouverte","I / É / È — étirée","O / OU / U — arrondie"};
 public static float[] Decode(string path,string ffmpeg,CancellationToken ct){
  var psi=new ProcessStartInfo(ffmpeg,"-v error -nostdin -i \""+path+"\" -vn -ac 1 -ar 16000 -f s16le pipe:1");psi.UseShellExecute=false;psi.CreateNoWindow=true;psi.RedirectStandardOutput=true;psi.RedirectStandardError=true;
  using(var p=Process.Start(psi))using(var reg=ct.Register(()=>{try{if(!p.HasExited)p.Kill();}catch{}}))using(var mem=new MemoryStream()){
   var err=p.StandardError.ReadToEndAsync(); byte[] buf=new byte[65536];int n;
   while((n=p.StandardOutput.BaseStream.Read(buf,0,buf.Length))>0){ct.ThrowIfCancellationRequested();if(mem.Length+n>38400000){try{p.Kill();}catch{}throw new Exception("Limite du prototype : 20 minutes d’audio par analyse.");}mem.Write(buf,0,n);}
   p.WaitForExit();ct.ThrowIfCancellationRequested();string error=err.Result;if(p.ExitCode!=0)throw new Exception("Décodage audio impossible : "+error);
   var b=mem.ToArray();if(b.Length<640)throw new Exception("Audio vide ou trop court (minimum 20 ms).");var a=new float[b.Length/2];for(int i=0;i<a.Length;i++)a[i]=BitConverter.ToInt16(b,i*2)/32768f;return a;
  }
 }
 static void FFT(double[] re,double[] im){int n=re.Length;for(int i=1,j=0;i<n;i++){int bit=n>>1;for(;(j&bit)!=0;bit>>=1)j^=bit;j^=bit;if(i<j){double a=re[i];re[i]=re[j];re[j]=a;}}
  for(int len=2;len<=n;len<<=1){double ar=Math.Cos(-2*Math.PI/len),ai=Math.Sin(-2*Math.PI/len);for(int i=0;i<n;i+=len){double wr=1,wi=0;for(int j=0;j<len/2;j++){int u=i+j,v=u+len/2;double tr=re[v]*wr-im[v]*wi,ti=re[v]*wi+im[v]*wr;re[v]=re[u]-tr;im[v]=im[u]-ti;re[u]+=tr;im[u]+=ti;double w=wr*ar-wi*ai;wi=wr*ai+wi*ar;wr=w;}}}
 }
 static double Band(double[] p,double lo,double hi){double s=0;for(int i=Math.Max(1,(int)(lo*1024/16000));i<Math.Min(p.Length,(int)(hi*1024/16000));i++)s+=p[i];return s;}
 // YIN normalized difference: first sufficiently periodic trough, otherwise no reliable pitch.
 public static double Pitch(double[] x,out double quality){int min=26,max=320,w=640;var d=new double[max+1];double sum=0;quality=0;
  for(int lag=1;lag<=max;lag++){double v=0;for(int i=0;i<w;i++){double a=x[i]-x[i+lag];v+=a*a;}sum+=v;d[lag]=sum>1e-15?v*lag/sum:1;}
  for(int t=min;t<max-1;t++){if(d[t]<.18){while(t+1<max&&d[t+1]<d[t])t++;quality=1-d[t];double den=d[t-1]-2*d[t]+d[t+1];double sub=Math.Abs(den)>1e-9?.5*(d[t-1]-d[t+1])/den:0;return 16000/(t+Math.Max(-.5,Math.Min(.5,sub)));}}return 0;
 }
 public static Result Analyze(float[] a,Options o,CancellationToken ct,Action<int> progress){o.Validate();var r=new Result{Audio=a};double previous=-100;const int hop=320;
  for(int pos=0;pos<a.Length;pos+=hop){ct.ThrowIfCancellationRequested();var x=new double[1024];double mean=0;int start=pos-352;for(int j=0;j<1024;j++){int k=start+j;x[j]=k>=0&&k<a.Length?a[k]:0;mean+=x[j];}mean/=1024;double energy=0;for(int j=0;j<1024;j++){x[j]-=mean;energy+=x[j]*x[j];}
   double db=10*Math.Log10(energy/1024+1e-12);var f=new Frame{Time=pos/16000.0,Db=db};r.Frames.Add(f);
   if(db>o.Gate){double q;f.Hz=Pitch(x,out q);f.Note=f.Hz>0?Math.Max(0,Math.Min(127,(int)Math.Round(69+12*Math.Log(f.Hz/440,2)))):-1;
    var re=new double[1024];var im=new double[1024];for(int j=0;j<1024;j++)re[j]=x[j]*(.5-.5*Math.Cos(2*Math.PI*j/1023));FFT(re,im);var p=new double[512];for(int j=1;j<512;j++)p[j]=re[j]*re[j]+im[j]*im[j];double total=Band(p,80,7900)+1e-12,low=Band(p,80,450)/total,high=Band(p,4000,7900)/total,mid=Band(p,1800,4000)/total;
    // Deliberately coarse acoustic classes, NOT a trained phoneme recognizer.
    if(db-previous>9 && previous<db-9 && (f.Hz==0||high>.12)){f.Shape=1;f.Confidence=.40;}
    else if(high>.42){f.Shape=3;f.Confidence=.65;}
    else if(mid>.42 && high>.09){f.Shape=4;f.Confidence=.50;}
    else if(f.Hz==0){f.Shape=5;f.Confidence=.30;}
    else if(low>.90 && Band(p,700,1800)/total<.035){f.Shape=2;f.Confidence=.35;}
    else {
     // Pre-emphasis and cepstral smoothing suppress individual harmonics for a rough spectral envelope.
     var lr=new double[1024];var li=new double[1024];for(int j=1;j<512;j++){double pre=1+.95*.95-1.9*Math.Cos(2*Math.PI*j/1024);lr[j]=lr[1024-j]=Math.Log(p[j]*pre+1e-12);}FFT(lr,li);
     for(int j=0;j<1024;j++){if(j>24&&j<1000){lr[j]=0;li[j]=0;}else {lr[j]/=1024;li[j]=-li[j]/1024;}}FFT(lr,li);
     int f1=16;for(int j=13;j<=64;j++)if(lr[j]>lr[f1])f1=j;
     int f2=Math.Max(f1+16,58);for(int j=Math.Max(f1+16,58);j<205;j++)if(lr[j]>lr[f2])f2=j;
     if(f1*15.625>620)f.Shape=6;else if(f2*15.625>1650)f.Shape=7;else f.Shape=8;f.Confidence=.40;
    }
   }
   previous=db;if(pos%(hop*25)==0)progress((int)(100L*pos/a.Length));
  }
  // Remove isolated classification flicker while preserving short attack candidates and silences.
  var shapes=r.Frames.Select(f=>f.Shape).ToArray();for(int i=1;i<shapes.Length-1;i++)if(shapes[i]!=0&&shapes[i]!=1&&shapes[i-1]==shapes[i+1]&&shapes[i-1]!=0)r.Frames[i].Shape=shapes[i-1];
  var active=r.Frames.Where(f=>f.Shape!=0).Select(f=>f.Db).ToArray();r.Low=o.Auto?(active.Length>0?active.Min():o.Gate):o.Low;r.High=o.Auto?(active.Length>0?active.Max():o.Gate):o.High;
  if(o.Auto&&r.High-r.Low<18){double center=(r.Low+r.High)/2;r.Low=center-9;r.High=center+9;}foreach(var f in r.Frames)f.Velocity=f.Shape==0?0:Velocity(f.Db,r.Low,r.High,o.VelLow,o.VelHigh);progress(100);return r;
 }
 public static int Velocity(double db,double lo,double hi,int vmin,int vmax){double t=hi-lo<.01?.5:Math.Max(0,Math.Min(1,(db-lo)/(hi-lo)));return Math.Max(1,Math.Min(127,(int)Math.Round(vmin+t*(vmax-vmin))));}
 public static byte[] Wave(float[] a){using(var m=new MemoryStream())using(var w=new BinaryWriter(m)){w.Write(Encoding.ASCII.GetBytes("RIFF"));w.Write(36+a.Length*2);w.Write(Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(16000);w.Write(32000);w.Write((short)2);w.Write((short)16);w.Write(Encoding.ASCII.GetBytes("data"));w.Write(a.Length*2);foreach(float f in a)w.Write((short)Math.Max(-32768,Math.Min(32767,(int)(f*32767))));return m.ToArray();}}
 class Event {public int Tick,Order;public byte[] Bytes;}
 static void Var(Stream s,int n){uint v=(uint)n,b=v&127;while((v>>=7)>0)b=(b<<8)|((v&127)|128);for(;;){s.WriteByte((byte)b);if((b&128)==0)break;b>>=8;}}
 static void BE(Stream s,int v,int bytes){for(int i=bytes-1;i>=0;i--)s.WriteByte((byte)(v>>(8*i)));}
 public static byte[] Midi(Result r,Options o){o.Validate();var ev=new List<Event>();int lastShape=-1,lastNote=-1,lastVel=0,lastOn=-1000,lastIntensity=-1;int intensity=o.IntensityCC<0?(o.CC+1)%120:o.IntensityCC;if(o.Intensity&&(intensity==o.CC||intensity<0||intensity>119))throw new Exception("CC de velocite et CC de sélection animation doivent etre distincts.");Action<int,int,byte[]> add=(t,ord,b)=>ev.Add(new Event{Tick=t,Order=ord,Bytes=b});
  add(0,0,new byte[]{255,81,3,7,161,32}); // 500000us / quarter, 500 ticks/quarter = 1 ms/tick
  foreach(var f in r.Frames){int t=(int)Math.Round(f.Time*1000);if(o.Intensity&&f.Velocity!=lastIntensity){add(t,2,new byte[]{(byte)(176+o.Channel),(byte)intensity,(byte)f.Velocity});lastIntensity=f.Velocity;}if(f.Shape!=lastShape){add(t,2,new byte[]{(byte)(176+o.Channel),(byte)o.CC,(byte)o.Values[f.Shape]});lastShape=f.Shape;}
   int note=f.Shape==0?-1:f.Note;bool change=note!=lastNote||(note>=0&&t-lastOn>=100&&Math.Abs(f.Velocity-lastVel)>=8);
   if(change){if(lastNote>=0)add(t,1,new byte[]{(byte)(128+o.Channel),(byte)lastNote,0});if(note>=0){add(t,3,new byte[]{(byte)(144+o.Channel),(byte)note,(byte)f.Velocity});lastOn=t;lastVel=f.Velocity;}lastNote=note;}
  }
  int end=(int)Math.Ceiling(r.Duration*1000);if(lastNote>=0)add(end,1,new byte[]{(byte)(128+o.Channel),(byte)lastNote,0});add(end,2,new byte[]{(byte)(176+o.Channel),(byte)o.CC,(byte)o.Values[0]});if(o.Intensity)add(end,3,new byte[]{(byte)(176+o.Channel),(byte)intensity,0});add(end,4,new byte[]{255,47,0});
  using(var tr=new MemoryStream()){int prev=0;foreach(var e in ev.OrderBy(e=>e.Tick).ThenBy(e=>e.Order)){Var(tr,e.Tick-prev);tr.Write(e.Bytes,0,e.Bytes.Length);prev=e.Tick;}using(var m=new MemoryStream()){var h=Encoding.ASCII.GetBytes("MThd");m.Write(h,0,4);BE(m,6,4);BE(m,0,2);BE(m,1,2);BE(m,500,2);h=Encoding.ASCII.GetBytes("MTrk");m.Write(h,0,4);BE(m,(int)tr.Length,4);tr.Position=0;tr.CopyTo(m);return m.ToArray();}}
 }
}
}

