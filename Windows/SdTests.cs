using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace UrbanKaoz.DisplayRecorder {
  internal static class SdTests905 {
    internal static void Run(string folder){
      Directory.CreateDirectory(folder);int checks=0;Action<bool,string> check=(ok,name)=>{if(!ok)throw new Exception(name);checks++;};
      var bytes=Encoding.ASCII.GetBytes("123456789");check(~SdFilesForm.Crc(0xffffffffu,bytes,bytes.Length)==0xcbf43926u,"CRC reference");
      bytes=new byte[]{0,127,128,255};check(SdFilesForm.Hex(SdFilesForm.Decode(SdFilesForm.Hex(bytes,4)),4)=="007F80FF","binary roundtrip");
      check(SdFilesForm.PathHex("/Tée")=="2F54C3A965","UTF8 names");
      bool refused=false;try{SdFilesForm.PathHex("/"+new string('x',200));}catch(IOException){refused=true;}check(refused,"path bound");
      using(var f=new SdFilesForm(command=>{
        if(command=="NEXT")return Task.FromResult(new[]{"SD1","1","END","LIST"});
        return Task.FromResult(new[]{"SD1","1","OK","READY"});
      },()=>true)){
        f.Show();Application.DoEvents();using(var bitmap=new Bitmap(f.Width,f.Height)){f.DrawToBitmap(bitmap,new Rectangle(Point.Empty,f.Size));bitmap.Save(Path.Combine(folder,"sd_window.png"));}
        check(f.Controls.Count==4,"SD layout");f.Close();
      }
      using(var f=new MainForm()){
        f.TestWriter=p=>{};f.TestPacket("PC1,HELLO,V9.05,1");
        check(f.surface.Connected,"V9.05 handshake");
        var method=typeof(MainForm).GetMethod("SdRequest905",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
        f.TestWriter=p=>{string[] a=p.Split(',');f.TestPacket("SD1,"+a[1]+",OK,READY");};
        var task=(Task<string[]>)method.Invoke(f,new object[]{"OPEN"});check(task.Result[3]=="READY","SD response routing");
        f.TestWriter=p=>{string[] a=p.Split(',');f.TestPacket("SD1,"+a[1]+",ERR,STOP_PLAYBACK");};
        task=(Task<string[]>)method.Invoke(f,new object[]{"OPEN"});check(task.IsFaulted,"playing rejection shown");f.TestWriter=p=>{};
      }
      File.WriteAllText(Path.Combine(folder,"sd_windows_tests.txt"),checks+" PASS\n");
    }
  }
}
