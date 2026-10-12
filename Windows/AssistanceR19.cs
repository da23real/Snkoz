using System;using System.Linq;using System.Drawing;using System.Windows.Forms;using System.Collections.Generic;
namespace Synkoz {
internal static class AssistanceR19 {
 internal static CheckBox Toggle(string title,bool value,Action<bool> set,string tip){var b=new CheckBox{Text=title,Checked=value,AutoSize=true};var t=new ToolTip{AutoPopDelay=20000};t.SetToolTip(b,tip);b.Disposed+=delegate{t.Dispose();};b.CheckedChanged+=delegate{set(b.Checked);};return b;}
}
public sealed partial class EffectsForm936 {
 Dictionary<string,List<GraphEvent15>> incomingCacheR19;string incomingKeyR19;
 internal int IncomingBuildsR19;
 string IncomingKeyR19(){var s=new System.Text.StringBuilder();foreach(var n in draft.Graph15.Nodes)s.Append(n.Id).Append('/').Append(n.Kind).Append('/').Append(n.ListenVirtual).Append('/').Append(n.VirtualTrack).Append('/').Append(n.Finished).Append('/').Append(new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(n.ActivityR20)).Append(n.ControlCC).Append('/').Append(n.NoteR4).Append('/').Append(n.ChannelR4).Append('/').Append(n.MutedR4).Append('/').Append(new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(n.MidiR4));foreach(var w in draft.Graph15.Wires.Where(w=>w.Port==GraphPort15.Trigger))s.Append(w.From).Append(w.To).Append(w.Order);s.Append(new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(draft.Sources15));s.Append(model.Notes.Count).Append('/').Append(model.CC.Count).Append('/').Append(model.Triggers.Count).Append('/').Append(model.EditorOriginTbo15).Append('/').Append(model.EditorEndR9);return s.ToString();}

}
}
