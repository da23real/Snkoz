using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
namespace Synkoz {
 public static class PreviewSaveR9 {
  public static void Copy(string source,string destination){if(string.IsNullOrEmpty(source)||!File.Exists(source))throw new IOException("Aucun preview terminé à sauvegarder.");string root=Path.GetFullPath(Path.GetDirectoryName(source))+Path.DirectorySeparatorChar;destination=Path.GetFullPath(destination);if(destination.StartsWith(root,StringComparison.OrdinalIgnoreCase))throw new IOException("Choisir un dossier hors du cache temporaire du preview.");if(!string.Equals(Path.GetExtension(source),Path.GetExtension(destination),StringComparison.OrdinalIgnoreCase))throw new IOException("Conserver l’extension du preview : "+Path.GetExtension(source));File.Copy(source,destination,true);}
 }
 public sealed partial class Preview936 {
  readonly Button saveR9=new Button{Text="Save…",AutoSize=true};readonly MidiStripR9 midiR9=new MidiStripR9();string renderedR9;bool savingR9;
  TableLayoutPanel midiLayoutR11;
  public void CompactMidiR11(bool compact){seek.Visible=true;seek.MinimumSize=new Size(0,25);if(midiLayoutR11!=null){midiLayoutR11.RowStyles[2].SizeType=SizeType.Absolute;midiLayoutR11.RowStyles[2].Height=35;}midiR9.Compact=compact;if(midiLayoutR11!=null)midiLayoutR11.RowStyles[1].Height=compact?60:170;midiR9.Invalidate();}
  void InitR9(TableLayoutPanel layout,FlowLayoutPanel buttons){midiLayoutR11=layout;layout.RowCount=6;layout.RowStyles.Insert(1,new RowStyle(SizeType.Absolute,170));layout.SetRow(seek,2);layout.SetRow(buttons,3);layout.SetRow(status,4);var midiHost=new Panel{Dock=DockStyle.Fill};var filters=new Button{Text=Language945.T("Filtres MIDI…"),Dock=DockStyle.Top,Height=24};filters.Click+=delegate{midiR9.ShowFilters(filters);};midiHost.Controls.Add(midiR9);midiHost.Controls.Add(filters);layout.Controls.Add(midiHost,0,1);layout.RowStyles.Insert(4,new RowStyle(SizeType.Absolute,22));layout.SetRow(status,5);buttons.Controls.Remove(time);time.Margin=Padding.Empty;time.Dock=DockStyle.Fill;layout.Controls.Add(time,0,4);buttons.Controls.Add(saveR9);buttons.Controls.SetChildIndex(saveR9,2);foreach(var b in new[]{play,pause,cancelButton,saveR9}){b.AutoSizeMode=AutoSizeMode.GrowAndShrink;b.MinimumSize=Size.Empty;}saveR9.Enabled=false;saveR9.Click+=async delegate{using(var dialog=new SaveFileDialog{AutoUpgradeEnabled=true,Title="Sauvegarder le preview terminé",FileName="Extrait"+Path.GetExtension(renderedR9),Filter="Preview (*"+Path.GetExtension(renderedR9)+")|*"+Path.GetExtension(renderedR9),DefaultExt=Path.GetExtension(renderedR9).TrimStart('.'),AddExtension=true,OverwritePrompt=true}){if(dialog.ShowDialog(this)!=DialogResult.OK)return;savingR9=true;saveR9.Enabled=false;try{string source=renderedR9;await Task.Run(()=>PreviewSaveR9.Copy(source,dialog.FileName));if(!IsDisposed)status.Text="Preview sauvegardé : "+dialog.FileName;}catch(Exception ex){if(!IsDisposed)status.Text="Sauvegarde : "+ex.GetBaseException().Message;Synkoz.ErrorR8.Show(this,ex.Message,"Erreur");}finally{savingR9=false;if(IsDisposed)Cleanup();else{saveR9.Enabled=File.Exists(renderedR9);if(closeAfter)Close();}}}};}
 }
}

