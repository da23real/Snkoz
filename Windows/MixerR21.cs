using System;using System.Linq;using System.Collections.Generic;
namespace Synkoz {
internal static class MixerHoldR21 {
 internal static List<GraphEvent15> Filter(MixerSettingsR20 settings,List<GraphEvent15> events,double initial){if(settings.MinimumSignalR21<=0)return events;var result=new List<GraphEvent15>();int active=Index(settings,initial),pending=-1;double since=0;GraphEvent15 candidate=null;Action commit=()=>{if(candidate==null)return;var e=candidate.Copy();e.Time=since+settings.MinimumSignalR21;result.Add(e);active=pending;candidate=null;};foreach(var e in events.OrderBy(x=>x.Time)){if(candidate!=null&&e.Time>=since+settings.MinimumSignalR21)commit();int next=Index(settings,TboSignal15.Normalize(e)*127);if(next==active){candidate=null;result.Add(e);continue;}if(candidate==null||next!=pending){since=e.Time;pending=next;}candidate=e;}if(candidate!=null)commit();return result;}
 static int Index(MixerSettingsR20 settings,double value){var weights=MixerR20.Weights(settings,value);return weights.Length==0?-1:Array.IndexOf(weights,weights.Max());}
}
internal sealed partial class GraphRun15 {readonly Dictionary<string,List<GraphEvent15>> mixerHeldR21=new Dictionary<string,List<GraphEvent15>>();List<GraphEvent15> MixerEventsR21(GraphNode15 node){List<GraphEvent15> list;if(!mixerHeldR21.TryGetValue(node.Id,out list)){list=MixerHoldR21.Filter(node.MixerR20,activations[node.Id],node.Mix*127);mixerHeldR21[node.Id]=list;}return list;}}
}
