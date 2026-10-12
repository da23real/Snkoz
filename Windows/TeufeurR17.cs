using System;using System.IO;using System.Linq;
namespace Synkoz {
 static class TeufeurR17 {
  internal static string Folder {get{return Path.Combine(AnimationPickerA01.PackFolder,"VoixMidi","Teufeur_3Banques");}}
  internal static OutputPresetR14 Preset(){
   var e=new Effect936{Kind=EffectKind936.Animation,Size=45,X=50,Y=50,Opacity15=100,Release=0};
   e.BanksR16=new AnimationBanksR16{Enabled=true,VoiceMouth=true,Select=BankSelectR16.Vélocité,Hysteresis=0,Gate=false};
   string[] names={"Chuchoté","Normal","Fort"},files={"01_Chuchote.gif","02_Normal.gif","03_Fort.gif"};int[] min={0,50,90},max={49,89,127};
   for(int i=0;i<3;i++)e.BanksR16.Items.Add(new AnimationBankR16{Name=names[i],Asset=Path.Combine(Folder,files[i]),Min=min[i],Max=max[i]});
   e.Asset=e.BanksR16.Items[0].Asset;
   return OutputPresetsR14.Pack(e,"Teufeur — banques par vélocité","Any");
  }
 }
}
