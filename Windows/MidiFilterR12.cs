using System;using System.Linq;using System.Collections.Generic;
namespace Synkoz {
public sealed partial class MidiFilterR4 {public List<string> KindsR12=new List<string>();public List<int> CcsR12=new List<int>(),NotesR12=new List<int>();public bool ConditionR12;public double MinR12=0,MaxR12=127;}
public static class MidiFilterLogicR12 {
 public static void Validate(MidiFilterR4 f){if(f==null||f.ChannelsR9==null||f.KindsR12==null||f.CcsR12==null||f.NotesR12==null||f.ChannelsR9.Any(x=>x<1||x>32)||f.CcsR12.Concat(f.NotesR12).Any(x=>x<0||x>127)||f.KindsR12.Any(x=>!new[]{"Note","CC","Horloge","Start","Stop"}.Contains(x))||double.IsNaN(f.MinR12)||double.IsNaN(f.MaxR12)||f.MinR12<0||f.MaxR12>127||f.MinR12>f.MaxR12)throw new Exception("Filtre MIDI invalide : valeurs CC et notes de 0 à 127, minimum inférieur ou égal au maximum.");}
 public static string Kind(GraphEvent15 e){return e.Kind=="NoteOn"||e.Kind=="NoteOff"?"Note":e.Kind=="Clock"||e.Kind=="Measure"?"Horloge":e.Kind;}
 public static bool Match(MidiFilterR4 f,GraphEvent15 e,bool channels=true,bool kinds=true){int channel=MidiChannelR9.Number(e);return (!channels||(f.ByTrack?(f.Mask==0||(e.Track>=0&&e.Track<16&&(f.Mask&(1<<e.Track))!=0)):f.ChannelsR9.Count==0||f.ChannelsR9.Contains(channel)))&&(!kinds||f.KindsR12.Count==0||f.KindsR12.Contains(Kind(e)))&&(e.Kind!="CC"||(f.CcsR12.Count==0||f.CcsR12.Contains(e.CC))&&(!f.ConditionR12||TboSignal15.Normalize(e)*127>=f.MinR12&&TboSignal15.Normalize(e)*127<=f.MaxR12))&&(Kind(e)!="Note"||f.NotesR12.Count==0||f.NotesR12.Contains(e.Note));}
 public static List<GraphEvent15> Apply(MidiFilterR4 f,List<GraphEvent15> events){return events.Where(e=>Match(f,e)).ToList();}
}}
