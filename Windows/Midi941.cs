using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Synkoz {
 public sealed class Midi941:IDisposable {
  [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]struct InCaps{public ushort Mid,Pid;public uint Version;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=32)]public string Name;public uint Support;}
  delegate void Callback(IntPtr h,uint message,UIntPtr instance,UIntPtr data,UIntPtr time);
  [DllImport("winmm.dll")]static extern uint midiInGetNumDevs();
  [DllImport("winmm.dll",CharSet=CharSet.Unicode)]static extern uint midiInGetDevCapsW(UIntPtr id,out InCaps caps,uint size);
  [DllImport("winmm.dll")]static extern uint midiInOpen(out IntPtr h,uint id,Callback callback,UIntPtr instance,uint flags);
  [DllImport("winmm.dll")]static extern uint midiInStart(IntPtr h);
  [DllImport("winmm.dll")]static extern uint midiInStop(IntPtr h);
  [DllImport("winmm.dll")]static extern uint midiInReset(IntPtr h);
  [DllImport("winmm.dll")]static extern uint midiInClose(IntPtr h);
  [DllImport("winmm.dll")]static extern uint midiOutOpen(out IntPtr h,uint id,IntPtr callback,IntPtr instance,uint flags);
  [DllImport("winmm.dll")]static extern uint midiOutShortMsg(IntPtr h,uint message);
  [DllImport("winmm.dll")]static extern uint midiOutReset(IntPtr h);
  [DllImport("winmm.dll")]static extern uint midiOutClose(IntPtr h);
  public string Error;volatile bool masterActive;public readonly ConcurrentQueue<Tuple<int,double>> Events=new ConcurrentQueue<Tuple<int,double>>();IntPtr input,output;readonly Callback callback;readonly object gate=new object();CancellationTokenSource clock;Task worker;readonly double epoch=Engine.Epoch(),mono=Stopwatch.GetTimestamp()/(double)Stopwatch.Frequency;
  public Midi941(){callback=(h,msg,instance,data,time)=>{if(msg==0x3C3||msg==0x3CC){int packet=(int)data.ToUInt64();int type=packet&255;if(!masterActive||(type!=0xF8&&type!=0xFA&&type!=0xFB&&type!=0xFC&&type!=0xF2))Queue(packet);}};}
  void Queue(int message){if(Events.Count<32768)Events.Enqueue(Tuple.Create(message,epoch+Stopwatch.GetTimestamp()/(double)Stopwatch.Frequency-mono));}
  public static string[] Inputs(){var result=new List<string>();for(uint i=0;i<midiInGetNumDevs();i++){InCaps caps;result.Add(midiInGetDevCapsW((UIntPtr)i,out caps,(uint)Marshal.SizeOf(typeof(InCaps)))==0?caps.Name:"MIDI "+i);}return result.ToArray();}
  static void Check(uint error){if(error!=0)throw new Exception("Port MIDI : erreur "+error);}
  public void Open(int inId,int outId){ClosePorts();try{if(inId>=0){Check(midiInOpen(out input,(uint)inId,callback,UIntPtr.Zero,0x30000));Check(midiInStart(input));}if(outId>=0)Check(midiOutOpen(out output,(uint)outId,IntPtr.Zero,IntPtr.Zero,0));}catch{ClosePorts();throw;}}
  public void Send(int message){lock(gate){if(output==IntPtr.Zero)throw new Exception("Choisir une sortie MIDI et connecter.");Check(midiOutShortMsg(output,(uint)message));}}
  public async Task StartMaster(double bpm,bool marker){StopClock();masterActive=true;Error=null;if(marker){for(int c=0;c<16;c++){int note=0x90|c|(60<<8)|(100<<16);Send(note);Queue(note);}try{await Task.Delay(100);}finally{for(int c=0;c<16;c++){int off=0x80|c|(60<<8);Send(off);Queue(off);}}await Task.Delay(Math.Max(0,(int)(60000/bpm)-100));}Send(0xFA);Queue(0xFA);clock=new CancellationTokenSource();var token=clock.Token;worker=Task.Run(()=>{var sw=Stopwatch.StartNew();double interval=60/bpm/24,next=0;while(!token.IsCancellationRequested){double now=sw.Elapsed.TotalSeconds;if(now>=next){Send(0xF8);Queue(0xF8);next+=interval;if(now-next>interval*4)next=now+interval;}else Thread.Sleep(1);}},token);}
  public void StopClock(){masterActive=false;if(clock!=null){clock.Cancel();try{if(worker!=null)worker.GetAwaiter().GetResult();}catch(OperationCanceledException){}catch(Exception ex){Error=ex.GetBaseException().Message;}finally{masterActive=false;clock.Dispose();clock=null;worker=null;}lock(gate)if(output!=IntPtr.Zero)midiOutShortMsg(output,0xFC);Queue(0xFC);}}
  void ClosePorts(){StopClock();if(input!=IntPtr.Zero){midiInStop(input);midiInReset(input);midiInClose(input);input=IntPtr.Zero;}lock(gate)if(output!=IntPtr.Zero){midiOutReset(output);midiOutClose(output);output=IntPtr.Zero;}}
  public void Dispose(){ClosePorts();GC.KeepAlive(callback);}
 }
 public sealed partial class CapturePanel {
  readonly System.Windows.Forms.Timer midiDrain941=new System.Windows.Forms.Timer{Interval=10};readonly Midi941 midi941=new Midi941();readonly ComboBox midiMode941=new ComboBox{FormattingEnabled=true,DropDownStyle=ComboBoxStyle.DropDownList,Width=135};readonly CheckBox marker941=new CheckBox{Text="Note de calage 1 temps avant Start",AutoSize=true};readonly NumericUpDown bpm941=new NumericUpDown{Minimum=30,Maximum=300,Value=120,Width=60};bool midiConnected941,midiPlaying941;uint midiTicks941;bool Universal941{get{return midiMode941.SelectedIndex>0;}}
  void InitMidi941(){var inputs=new ComboBox{FormattingEnabled=true,DropDownStyle=ComboBoxStyle.DropDownList,Width=150};var outputs=new ComboBox{FormattingEnabled=true,DropDownStyle=ComboBoxStyle.DropDownList,Width=150};midiMode941.Items.AddRange(new object[]{"Teensy / autonome","MIDI maître PC","MIDI esclave"});midiMode941.SelectedIndex=0;bar.Controls.Add(midiMode941);bar.Controls.Add(inputs);bar.Controls.Add(outputs);bar.Controls.Add(bpm941);bar.Controls.Add(marker941);Action scanMidi=()=>{string previousIn=inputs.SelectedItem as string,previousOut=outputs.SelectedItem as string;inputs.Items.Clear();outputs.Items.Clear();inputs.Items.Add("Entrée : aucune");outputs.Items.Add("Sortie : aucune");inputs.Items.AddRange(Midi941.Inputs());outputs.Items.AddRange(UrbanKaoz.DisplayRecorder.MidiOutput.Devices());inputs.SelectedIndex=previousIn!=null&&inputs.Items.Contains(previousIn)?inputs.Items.IndexOf(previousIn):0;outputs.SelectedIndex=previousOut!=null&&outputs.Items.Contains(previousOut)?outputs.Items.IndexOf(previousOut):0;};Button("Rafraîchir MIDI",()=>{if(!Busy&&!paused)scanMidi();});Button("Connecter MIDI",()=>{if(Busy)return;try{midi941.Open(inputs.SelectedIndex-1,outputs.SelectedIndex-1);midiConnected941=true;status.Text="Ports MIDI ouverts.";}catch(Exception ex){midiConnected941=false;status.Text=ex.Message;}});scanMidi();
   midiMode941.SelectedIndexChanged+=delegate{if(Busy)return;startTee.Checked=false;startTee.Enabled=!Universal941;marker941.Enabled=midiMode941.SelectedIndex==1;if(midiMode941.SelectedIndex==2)SlaveNotice941();Lock();};midiDrain941.Tick+=delegate{DrainMidi941();};midiDrain941.Start();}
  void SlaveNotice941(){string path=System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Snkoz","MidiSlave941.txt");if(System.IO.File.Exists(path))return;using(var f=new Form{Text="Horloge MIDI esclave",ClientSize=new System.Drawing.Size(450,155),StartPosition=FormStartPosition.CenterParent}){f.Controls.Add(new Label{Text="Snkoz est optimisé pour piloter le départ en maître.\nEn esclave, le Start externe ne découpe pas la prise :\nle début enregistré est conservé pour régler les latences.",Left=15,Top=15,Width=420,Height=65});var no=new CheckBox{Text="Ne plus m’alerter",Left=15,Top=85,Width=230};f.Controls.Add(no);var ok=new Button{Text="Compris",Left=320,Top=110,DialogResult=DialogResult.OK};f.Controls.Add(ok);f.ShowDialog(FindForm());if(no.Checked){System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));System.IO.File.WriteAllText(path,"1");}}}
  void DrainMidi941(){if(midi941.Error!=null){status.Text="Horloge MIDI : "+midi941.Error;midi941.Error=null;}Tuple<int,double> item;while(midi941.Events.TryDequeue(out item)){if(!Universal941)continue;AcceptMidi941(item.Item1,item.Item2);}}
  public void AcceptMidi941(int message,double epoch){int s=message&255,kind=s&240,ch=s&15,a=(message>>8)&127,b=(message>>16)&127;if(s==0xFA){midiPlaying941=true;if(!recording)midiTicks941=0;}else if(s==0xFB)midiPlaying941=true;else if(s==0xFC)midiPlaying941=false;else if(s==0xF2&&!recording)midiTicks941=(uint)((a|(b<<7))*96);
   if(s==0xF8&&midiPlaying941){if(recording)marks.Add(new ClockMark{Tick=midiTicks941,Time=epoch});if(direct939!=null)direct939.Packet("SK1,E,0,0,"+midiTicks941+",0,0");midiTicks941+=16;}
   if(direct939!=null){if(s==0xFA||s==0xFB||s==0xFC)direct939.Packet("SK1,E,0,0,"+midiTicks941+","+(s==0xFC?3:2)+",0");else if(kind==0xB0)direct939.Packet("SK1,C,0,0,0,"+a+","+b+","+ch+",0");else if(kind==0x90||kind==0x80)direct939.Packet("SK1,N,0,0,0,"+(kind==0x90&&b>0?1:0)+","+a+","+b+","+ch+",0");else if(kind==0xC0)direct939.Packet("SKPRG,0,0,"+a+","+ch);}
   if(!recording)return;if(kind==0x90||kind==0x80)notes.Add(new NoteEvent{Time=epoch,Channel=ch,Note=a,Velocity=b,On=kind==0x90&&b>0});else if(kind==0xB0)cc936.Add(new Cc936{Time=epoch,Channel=ch,Number=a,Value=b});else if(kind==0xC0)programsTake940.Add(new Program940{Time=epoch,Channel=ch,Value=a});}
 }
}

