using System;using System.Diagnostics;using System.Windows.Forms;
namespace Synkoz {
 internal static class CaptureConcurrentR19 {internal static volatile bool Active;}
 public static partial class Engine {internal static void StartWorkerR19(Process p){p.Start();if(CaptureConcurrentR19.Active)try{p.PriorityClass=ProcessPriorityClass.BelowNormal;}catch{}}}
 public sealed partial class MontagePanel {internal Func<string> RecordingFolderR19;Button completedTakeR19;
 internal void TakeCompletedR19(string folder){if(completedTakeR19==null){completedTakeR19=new Button{Dock=DockStyle.Top,Height=32};Controls.Add(completedTakeR19);}completedTakeR19.Tag=folder;completedTakeR19.Text="Nouvelle prise terminée — ouvrir dans Montage";completedTakeR19.Visible=true;completedTakeR19.BringToFront();completedTakeR19.Click-=OpenCompletedR19;completedTakeR19.Click+=OpenCompletedR19;status.Text="Nouvelle prise terminée. Ton montage ouvert est conservé.";}
 void OpenCompletedR19(object sender,EventArgs args){try{ShowMontage(Montage.Load((string)completedTakeR19.Tag));completedTakeR19.Visible=false;}catch(Exception ex){ErrorR8.Show(FindForm(),ex.Message);}}
 }
}
