using System;using System.IO;using System.Collections.Generic;
namespace Synkoz {public static class DemoPresetsR9 {
 public static GraphPresetR6 Make(EffectKind936 kind,bool strong){var e=new Effect936{Kind=kind,Amount=kind==EffectKind936.Zoom?(strong?18:6):(strong?24:8),Attack=strong?0:15,Release=strong?65:85,Length=strong?Length936.Croche:Length936.Noire};var n=new GraphNode15{Kind=GraphKind15.Effect,EffectId=e.IdR8,Name=kind+ (strong?" · accent":" · doux"),X=300,Y=160,ActivationR4=new MidiSelectR4{Kind="Note"}};return new GraphPresetR6{Nodes=new List<GraphNode15>{n},Effects=new List<Effect936>{e}};}
 public static void Write(string folder){Directory.CreateDirectory(folder);foreach(var k in new[]{EffectKind936.Zoom,EffectKind936.Luminosité})foreach(bool strong in new[]{false,true})Store934.Save(Path.Combine(folder,k+(strong?"-Accent":"-Doux")+".uktbo"),Make(k,strong));}
}}
