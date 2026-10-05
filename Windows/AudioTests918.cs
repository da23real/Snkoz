using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
namespace UrbanKaoz.DisplayRecorder {
 internal static class AudioTests918 {
  static NumericUpDown Field(AudioMidiForm f,string name){return (NumericUpDown)typeof(AudioMidiForm).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(f);}
  internal static void Run(string folder){Directory.CreateDirectory(folder);AudioMidiForm.TestSettingsPath918=Path.Combine(folder,"audio-test.json");int pass=0;
   string[] names={"channel","note","threshold","attack","release"};int[] values={9,77,24,330,1750};
   using(var f=new AudioMidiForm(()=>120,(a,b,c,d)=>{},()=>false)){for(int i=0;i<names.Length;++i)Field(f,names[i]).Value=values[i];}
   using(var f=new AudioMidiForm(()=>120,(a,b,c,d)=>{},()=>false)){for(int i=0;i<names.Length;++i){if(Field(f,names[i]).Value!=values[i])throw new Exception("restore "+names[i]);++pass;}}
   File.WriteAllText(AudioMidiForm.TestSettingsPath918,"{\"Channel\":99,\"Note\":-1,\"Threshold\":0,\"Attack\":9000,\"Release\":-2}");
   int[] bounded={16,0,1,5000,0};using(var f=new AudioMidiForm(()=>120,(a,b,c,d)=>{},()=>false)){for(int i=0;i<names.Length;++i){if(Field(f,names[i]).Value!=bounded[i])throw new Exception("bounds "+names[i]);++pass;}}
   File.WriteAllText(AudioMidiForm.TestSettingsPath918,"broken");using(var f=new AudioMidiForm(()=>120,(a,b,c,d)=>{},()=>false)){if(Field(f,"note").Value!=60)throw new Exception("invalid file");++pass;}
   AudioMidiForm.TestSettingsPath918=null;File.WriteAllText(Path.Combine(folder,"result.txt"),"PASS="+pass+",FAIL=0");
  }
 }
}
