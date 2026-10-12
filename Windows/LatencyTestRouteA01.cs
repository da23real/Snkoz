using System;
using System.Threading;
using System.Threading.Tasks;
namespace Synkoz {
 public sealed partial class CapturePanel {
  TaskCompletionSource<int[]> testRouteA01;int testRouteRequestA01;
  bool TestRoutePacketA01(string packet){
   if(!packet.StartsWith("SK1,SERIESCFG2,"))return false;
   var p=packet.Split(',');int request,track,channel,note,mask;
   if(p.Length==7&&int.TryParse(p[2],out request)&&request==testRouteRequestA01&&int.TryParse(p[3],out track)&&track>=1&&track<=16&&int.TryParse(p[4],out channel)&&channel>=1&&channel<=16&&int.TryParse(p[5],out note)&&note==request%128&&int.TryParse(p[6],out mask)&&mask>=0&&mask<=255&&testRouteA01!=null)testRouteA01.TrySetResult(new[]{track,channel,note,mask});return true;
  }
  async Task ConfigureTestRouteA01(CancellationToken token){
   if(!ready()||!enabled)throw new Exception("Activer Snkoz sur le Teensy avant le test.");
   if(MontageView.ControllerWanted940)throw new Exception("Désactiver Contrôleur Montage avant le test.");
   var reply=new TaskCompletionSource<int[]>();testRouteA01=reply;testRouteRequestA01=latencyA01.Channel*17*128+latencyA01.UkTrack*128+latencyA01.Note;
   try{send("SKSERIESCFG2,"+testRouteRequestA01);var timeout=Task.Delay(4000,token);if(await Task.WhenAny(reply.Task,timeout)!=reply.Task){token.ThrowIfCancellationRequested();throw new Exception("Le Teensy n’a pas confirmé le routage du test. Installer le correctif A01 du Teensy avant cette série.");}token.ThrowIfCancellationRequested();var route=await reply.Task;if(route[3]==0)throw new Exception("La sortie MIDI de la piste UK "+route[0]+" est désactivée.");latencyA01.Channel=route[1];status.Text="Test : piste UK "+route[0]+", canal "+route[1]+", note "+route[2]+" — sortie directe sans modulations ni delays.";}
   finally{testRouteA01=null;}
  }
 }
}
