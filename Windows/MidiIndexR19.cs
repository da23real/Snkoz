using System;using System.Collections.Generic;
namespace Synkoz {internal sealed class MidiIndexR19 {
 readonly Dictionary<string,List<MidiSpan>> groups=new Dictionary<string,List<MidiSpan>>();static string Key(int c,int t,int n,int d,int r,long bin){return c+"/"+t+"/"+n+"/"+d+"/"+r+"/"+bin;}
 internal MidiIndexR19(List<MidiSpan> spans){foreach(var n in spans){string key=Key(n.Channel,n.Track940,n.Note,n.Delay07,n.Repeat07,(long)Math.Floor(n.Start*100000));List<MidiSpan> list;if(!groups.TryGetValue(key,out list))groups[key]=list=new List<MidiSpan>();list.Add(n);}}
 internal MidiSpan Find(Trigger936 n){long at=(long)Math.Floor(n.Time*100000);for(long i=at-1;i<=at+1;i++){List<MidiSpan> list;if(groups.TryGetValue(Key(n.Channel,n.Track940,n.Note,n.Delay07,n.Repeat07,i),out list))foreach(var span in list)if(Math.Abs(span.Start-n.Time)<.00001)return span;}return null;}
}}
