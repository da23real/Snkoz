using System;
using System.Linq;
using System.Windows.Forms;
namespace Synkoz {
 public sealed partial class EffectsForm936 {
  void NoteImagesFields939(Effect936 e){if(e.Trigger!=TriggerKind936.Note)return;var mode=Combo(new object[]{"Animation temporelle","Note absolue → image","Accord / gamme → image"},new[]{"Animation temporelle","Note absolue → image","Accord / gamme → image"}[e.NoteImages939]);mode.SelectedIndexChanged+=delegate{e.NoteImages939=mode.SelectedIndex;QueueRefresh938(list.SelectedIndex);};Field("Choix des images",mode);if(e.NoteImages939==0)return;int count;using(var asset=new AnimationAsset938(Store934.MediaPath(model.Folder,e.Asset)))count=asset.Count;if(e.NoteImages939==1){Number("Note centrale (MIDI)",e.CenterNote939,0,127,v=>e.CenterNote939=(int)v,0);Number("Image centrale",e.CenterImage939,1,count,v=>e.CenterImage939=(int)v,0);}else{string[] notes={"Do","Do♯","Ré","Ré♯","Mi","Fa","Fa♯","Sol","Sol♯","La","La♯","Si"};var detected=model.Triggers.Where(n=>n.Channel==e.Channel-1).Select(n=>n.Note%12).Concat(model.Notes.Where(n=>n.Channel==e.Channel-1).Select(n=>n.Note%12)).Distinct().ToArray();for(int i=0;i<12;i++){int n=i;if(!model.DirectMode939&&detected.Length>0&&!detected.Contains(i)&&e.NoteMap939[i]==0)continue;Number(notes[i]+" → image (0 = sans)",e.NoteMap939[n],0,count,v=>e.NoteMap939[n]=(int)v,0);}Number("Sans association → image",e.OtherImage939,0,count,v=>e.OtherImage939=(int)v,0);}editor.Controls.Add(new Label{AutoSize=true,Text="Image hors plage ou 0 : invisible. Durée / Gate définit le temps d’affichage."});}
 }
}
