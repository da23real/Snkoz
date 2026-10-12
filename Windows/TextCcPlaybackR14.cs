using System;using System.Linq;using System.Collections.Generic;
namespace Synkoz {sealed partial class TextRunR12 {
 readonly Dictionary<string,CcPlaybackR14> readersR14=new Dictionary<string,CcPlaybackR14>();
 double CcTimeR14(double t){return fx.CcUpdateFpsR14>0?Math.Floor(t*fx.CcUpdateFpsR14+1e-8)/fx.CcUpdateFpsR14:t;}
 double CcValueR14(Cc936 cc,double t){string key=cc.Channel+"/"+cc.Track940+"/"+cc.Number;CcPlaybackR14 reader;if(!readersR14.TryGetValue(key,out reader))readersR14[key]=reader=new CcPlaybackR14(controls.Where(c=>c.Channel==cc.Channel&&c.Track940==cc.Track940&&c.Number==cc.Number).Select(c=>new GraphEvent15{Kind="CC",Time=c.Time,Value=c.Value,CC=c.Number,Channel=c.Channel,Track=c.Track940,Bits=c.Bits15}),fx);var e=reader.At(t);return e==null?cc.Value:e.Value;}
}}
