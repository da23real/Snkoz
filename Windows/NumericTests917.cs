using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
namespace UrbanKaoz.DisplayRecorder {
 internal static class NumericTests917 {
  static int count;
  static void Check(bool ok,string name){++count;if(!ok)throw new Exception(name);}
  static void Call(object target,string method,params object[] args){target.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,args);}
  static void Mouse(Surface s,string method,int clicks,int x,int y){Call(s,method,new MouseEventArgs(MouseButtons.Left,clicks,x,y,0));}
  internal static void Run(string directory){
   Directory.CreateDirectory(directory);count=0;
   Check(NumericEntry917.Compatible("V9.18")&&!NumericEntry917.Compatible("V9.16")&&NumericEntry917.Compatible("V10.00"),"firmware capability");
   Check(NumericEntry917.Compatible("VA01.08")&&NumericEntry917.Compatible("VA01.09")&&!NumericEntry917.Compatible("VAgarbage"),"full revision labels");
   int committed=0,value=0;
   using(var f=new NumericEntry917("K5 — Gate",96,1,65535,delegate(int x){committed++;value=x;})){
    f.Show();Application.DoEvents();f.Input.Text="192";Application.DoEvents();Check(committed==0,"typing is local");
    f.Input.Text="bad";Check(!f.Submit()&&committed==0,"invalid text never commits");
    f.Input.Text="65536";Check(!f.Submit()&&committed==0,"range checked locally");
    f.Input.Text="192";Check(f.Submit()&&committed==1&&value==192,"Enter commits exact value once");Check(!f.Submit()&&committed==1,"no duplicate commit");
   }
   using(var f=new NumericEntry917("K6 — Timing",0,-96,96,delegate(int x){committed++;})){
    f.Show();f.Input.Text="-40";f.Close();Check(committed==1,"close abandons draft");
   }
   var packets=new List<string>();
   using(var host=new Form())using(var s=new Surface{Size=new Size(1240,312),Connected=true,NumericAvailable917=true,Send=packets.Add}){
    host.Controls.Add(s);host.Show();Application.DoEvents();
    Mouse(s,"OnMouseDown",1,340,70);Mouse(s,"OnMouseUp",1,340,70);Check(packets.Count==0,"single encoder click deferred for double click");
    Mouse(s,"OnMouseDown",2,340,70);Mouse(s,"OnMouseUp",2,340,70);Check(packets.Count==1&&packets[0]=="VGET,4","double click only reads no KY click or turn");
    Check(s.AcceptNumeric917("PC1,VALUE,4,7,96,1,65535"),"numeric reply accepted");Application.DoEvents();
    var f=(NumericEntry917)typeof(Surface).GetField("numericForm917",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(s);Check(f!=null,"editor opened");f.Input.Text="30";Check(packets.Count==1,"editing sends nothing");
    using(var bitmap=new Bitmap(f.Width,f.Height)){f.DrawToBitmap(bitmap,new Rectangle(Point.Empty,bitmap.Size));bitmap.Save(Path.Combine(directory,"numeric_entry.png"));}
    f.Submit();Check(packets.Count==2&&packets[1]=="VSET,4,30,7","only validation sends exact typed value and token");
    packets.Clear();Mouse(s,"OnMouseDown",1,190,70);Mouse(s,"OnMouseUp",1,190,70);Call(s,"FlushClick917");Check(packets.Count==2&&packets[0]=="KB,2,1"&&packets[1]=="KB,2,0","single click preserved");
    packets.Clear();Mouse(s,"OnMouseDown",2,190,25);Check(packets.Count==1&&packets[0]=="VGET,2","double click LCD value");
    s.AcceptNumeric917("PC1,VALUE,2,8,60,0,127");Application.DoEvents();f=(NumericEntry917)typeof(Surface).GetField("numericForm917",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(s);f.Input.Text="70";s.ResetSession();Check(packets.Count==1&&f.IsDisposed,"disconnect abandons text");
    s.Connected=true;s.NumericAvailable917=true;packets.Clear();Mouse(s,"OnMouseDown",1,190,80);Mouse(s,"OnMouseMove",1,190,68);Mouse(s,"OnMouseUp",1,190,68);Check(packets.Count==1&&packets[0]=="KT,2,2","drag preserved without click");
    packets.Clear();Mouse(s,"OnMouseDown",2,30,120);Check(packets.Count==1&&packets[0]=="VGET,16","Roy numeric entry");
    s.AcceptNumeric917("PC1,VALUE_ERROR,NO_NUMERIC_VALUE");Check(packets.Count==1,"unsupported context has no write");
    host.Close();
   }
   packets.Clear();
   using(var main=new MainForm()){
    main.TestWriter=packets.Add;main.Show();Application.DoEvents();main.TestPacket("PC1,HELLO,VA01.09,1");
    var s=(Surface)typeof(MainForm).GetField("surface",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(main);
    int x=(int)(340f*s.Width/1240),y=(int)(70f*s.Height/312);
    Mouse(s,"OnMouseDown",2,x,y);main.TestPacket("PC1,ACK,1,OK");main.TestPacket("PC1,VALUE,4,9,96,1,65535");Application.DoEvents();
    var f=(NumericEntry917)typeof(Surface).GetField("numericForm917",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(s);
    Check(f!=null&&!f.IsDisposed,"main form deactivation does not cancel own numeric editor");
    f.Input.Text="25";Check(!packets.Exists(p=>p.Contains(",VSET,")),"main form typing remains local");f.Submit();
    for(int round=0;round<20;round++){foreach(string packet in packets.ToArray()){var parts=packet.Split(',');int seq;if(parts.Length>=3&&parts[0]=="PC1"&&int.TryParse(parts[1],out seq))main.TestPacket("PC1,ACK,"+seq+",OK");}}
    File.WriteAllLines(Path.Combine(directory,"numeric_protocol.txt"),packets);
    Check(packets.Exists(p=>p.EndsWith(",VSET,4,25,9")),"main form submits numeric protocol");
   }
   File.WriteAllText(Path.Combine(directory,"numeric_tests.txt"),"PASS="+count+", FAIL=0\r\nOnly numeric entry and direct mouse/protocol dependencies tested.\r\n");
  }
 }
}
