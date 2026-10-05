using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UrbanKaoz.DisplayRecorder {
  internal sealed partial class MainForm {
    SdFilesForm sdForm;
    TaskCompletionSource<string[]> sdResponse;
    long sdSequence=DateTime.UtcNow.Ticks%2000000000;
    string sdExpected;
    void OpenSd905(){
      if(sdForm==null||sdForm.IsDisposed){sdForm=new SdFilesForm(SdRequest905,()=>ready);sdForm.Show(this);}else sdForm.Activate();
    }
    bool AcceptSd905(string packet){
      if(!packet.StartsWith("SD1,"))return false;
      string[] fields=packet.Split(',');
      if(fields.Length>=4&&fields[1]==sdExpected&&sdResponse!=null)sdResponse.TrySetResult(fields);
      return true;
    }
    async Task<string[]> SdRequest905(string command){
      if(!ready)throw new IOException("Connecter le Tee V9.05 dans la fenêtre principale.");
      if(sdResponse!=null)throw new IOException("Une opération SD est déjà en cours.");
      sdExpected=(++sdSequence).ToString(System.Globalization.CultureInfo.InvariantCulture);
      var response=new TaskCompletionSource<string[]>();sdResponse=response;
      string packet="SD1,"+sdExpected+","+command;
      try {
        for(int attempt=0;attempt<3;attempt++){
          Write(packet);
          if(await Task.WhenAny(response.Task,Task.Delay(4000))==response.Task){
            var fields=await response.Task;
            if(fields[2]=="ERR")throw new IOException(SdFilesForm.Error(fields[3]));
            return fields;
          }
          if(!ready)throw new IOException("Tee déconnecté.");
        }
        throw new IOException("La carte SD ne répond pas. L’opération n’est pas confirmée.");
      } finally {sdResponse=null;sdExpected=null;}
    }
  }

  internal sealed class SdFilesForm : Form {
    readonly Func<string,Task<string[]>> request;
    readonly Func<bool> connected;
    readonly ListView files=new ListView{Dock=DockStyle.Fill,View=View.Details,FullRowSelect=true,MultiSelect=false,HideSelection=false};
    readonly TextBox location=new TextBox{Dock=DockStyle.Top,ReadOnly=true};
    readonly Label status=new Label{Dock=DockStyle.Bottom,Height=32,Text="Lecture arrêtée requise pour accéder à la SD."};
    readonly FlowLayoutPanel actions=new FlowLayoutPanel{Dock=DockStyle.Top,Height=68,WrapContents=true};
    readonly Button cancel=new Button{Text="Annuler",Enabled=false,AutoSize=true};
    bool busy,cancelled;string folder="/";
    internal SdFilesForm(Func<string,Task<string[]>> request,Func<bool> connected){
      this.request=request;this.connected=connected;Text="Snkoz V9.05 — Carte SD Tee";Size=new Size(820,500);MinimumSize=new Size(720,360);StartPosition=FormStartPosition.CenterParent;
      files.Columns.Add("Nom",460);files.Columns.Add("Type",80);files.Columns.Add("Taille (octets)",140);
      Button("Actualiser",()=>RefreshFiles());Button("Dossier parent",async()=>{if(folder!="/"){int n=folder.LastIndexOf('/');folder=n<=0?"/":folder.Substring(0,n);}await RefreshFiles();});
      Button("Télécharger vers Windows",Download);Button("Envoyer vers Tee",Upload);Button("Renommer",Rename);Button("Supprimer",Delete);
      cancel.Click+=delegate{cancelled=true;status.Text="Annulation demandée…";};actions.Controls.Add(cancel);
      Controls.Add(files);Controls.Add(location);Controls.Add(actions);Controls.Add(status);location.Text=folder;
      files.DoubleClick+=async delegate{if(busy||files.SelectedItems.Count==0||files.SelectedItems[0].SubItems[1].Text!="Dossier")return;folder=Full(files.SelectedItems[0].Text);await Run(RefreshFiles);};
      Shown+=async delegate{await Run(RefreshFiles);};
      FormClosing+=(o,e)=>{if(busy){cancelled=true;e.Cancel=true;status.Text="Annulation en cours ; fermer après sa confirmation.";}};
    }
    void Button(string label,Func<Task> action){var b=new Button{Text=label,AutoSize=true};b.Click+=async delegate{await Run(action);};actions.Controls.Add(b);}
    async Task Run(Func<Task> action){
      if(busy)return;if(!connected()){status.Text="Connecter le Tee V9.05 dans la fenêtre principale.";return;}
      busy=true;cancelled=false;files.Enabled=false;foreach(Control c in actions.Controls)c.Enabled=false;cancel.Enabled=true;
      bool opened=false;
      try{await request("OPEN");opened=true;await action();if(!cancelled)status.Text="Opération terminée.";}
      catch(OperationCanceledException){status.Text="Opération annulée.";}
      catch(Exception ex){status.Text=ex.Message;}
      if(opened)try{await request("CLOSE");}catch{status.Text+=" Fermeture non confirmée ; expiration automatique sous 15 s.";}
      busy=false;foreach(Control c in actions.Controls)c.Enabled=true;cancel.Enabled=false;files.Enabled=true;
    }
    void CheckCancel(){if(cancelled)throw new OperationCanceledException();}
    string Full(string name){return folder=="/"?"/"+name:folder+"/"+name;}
    string Selected(){if(files.SelectedItems.Count==0||files.SelectedItems[0].SubItems[1].Text!="Fichier")throw new IOException("Sélectionner un fichier.");return files.SelectedItems[0].Text;}
    internal static string Hex(byte[] data,int count){var b=new StringBuilder(count*2);for(int i=0;i<count;i++)b.Append(data[i].ToString("X2"));return b.ToString();}
    internal static string PathHex(string path){var b=Encoding.UTF8.GetBytes(path);if(b.Length>180)throw new IOException("Chemin trop long (180 octets maximum).");return Hex(b,b.Length);}
    internal static byte[] Decode(string text){if(text.Length%2!=0)throw new IOException("Données SD tronquées.");var b=new byte[text.Length/2];for(int i=0;i<b.Length;i++)b[i]=Convert.ToByte(text.Substring(i*2,2),16);return b;}
    internal static uint Crc(uint crc,byte[] data,int count){for(int n=0;n<count;n++){crc^=data[n];for(int i=0;i<8;i++)crc=(crc>>1)^((crc&1)!=0?0xedb88320u:0);}return crc;}
    internal static string Error(string code){switch(code){case "STOP_PLAYBACK":return "Arrêter la lecture sur Tee avant l’accès SD.";case "BUSY":return "Fermer Save/Load sur Tee ou attendre la fin du transfert.";case "NO_SD":return "Carte SD non détectée sur Tee.";case "EXISTS":return "Un fichier porte déjà ce nom.";case "NO_SESSION":return "Session SD expirée ; relancer l’opération.";case "READ_ONLY":return "La carte SD n’est pas accessible en écriture.";default:return "Carte SD : "+code;}}
    async Task RefreshFiles(){
      location.Text=folder;files.Items.Clear();await request("LIST,"+PathHex(folder));
      for(;;){CheckCancel();var r=await request("NEXT");if(r[2]=="END")break;if(r[2]!="ENTRY"||r.Length!=6)throw new IOException("Liste SD invalide.");
        string name=Encoding.UTF8.GetString(Decode(r[5]));files.Items.Add(new ListViewItem(new[]{name,r[3]=="D"?"Dossier":"Fichier",r[4]}));}
    }
    async Task Download(){
      string name=Selected();using(var d=new SaveFileDialog{FileName=name,OverwritePrompt=true}){
        if(d.ShowDialog(this)!=DialogResult.OK)return;
        // A dialog can exceed the transfer lease; renew before any I/O.
        await Renew();var r=await request("GET,"+PathHex(Full(name)));long size;
        if(r[2]!="SIZE"||!long.TryParse(r[3],out size)||size<0||size>uint.MaxValue)throw new IOException("Taille SD non prise en charge.");
        string temp=d.FileName+".ukdownload-"+Guid.NewGuid().ToString("N");
        try{using(var output=new FileStream(temp,FileMode.CreateNew,FileAccess.Write)){
          long at=0;while(at<size){CheckCancel();r=await request("READ,"+at);if(r[2]!="DATA"||r.Length!=6||r[3]!=at.ToString())throw new IOException("Réponse SD invalide.");var bytes=Decode(r[5]);
            if(bytes.Length==0||at+bytes.Length>size||(~Crc(0xffffffffu,bytes,bytes.Length)).ToString("X8")!=r[4])throw new IOException("Vérification du transfert échouée.");
            await output.WriteAsync(bytes,0,bytes.Length);at+=bytes.Length;status.Text="Téléchargement : "+at+" / "+size+" octets";
          }
        }
        CheckCancel();if(File.Exists(d.FileName))File.Replace(temp,d.FileName,null);else File.Move(temp,d.FileName);
        }finally{if(File.Exists(temp))File.Delete(temp);}
      }
    }
    async Task Renew(){await request("CLOSE");await request("OPEN");CheckCancel();}
    async Task Upload(){
      using(var d=new OpenFileDialog{Multiselect=false}){
        if(d.ShowDialog(this)!=DialogResult.OK)return;string name=Path.GetFileName(d.FileName);bool replace=false;
        foreach(ListViewItem item in files.Items)if(string.Equals(item.Text,name,StringComparison.OrdinalIgnoreCase)){
          if(item.SubItems[1].Text=="Dossier")throw new IOException("Un dossier porte déjà ce nom.");
          if(MessageBox.Show(this,"Remplacer « "+name+" » sur la carte SD ?","Envoi vers Tee",MessageBoxButtons.OKCancel)!=DialogResult.OK)return;replace=true;break;
        }
        await Renew();using(var input=new FileStream(d.FileName,FileMode.Open,FileAccess.Read,FileShare.Read)){
          if(input.Length>uint.MaxValue)throw new IOException("Fichier trop volumineux.");
          await request("PUT,"+PathHex(Full(name))+","+(replace?"1":"0"));byte[] data=new byte[256];uint crc=0xffffffffu;long at=0;int count;
          while((count=await input.ReadAsync(data,0,data.Length))>0){CheckCancel();await request("WRITE,"+at+","+Hex(data,count));crc=Crc(crc,data,count);at+=count;status.Text="Envoi : "+at+" / "+input.Length+" octets";}
          CheckCancel();await request("COMMIT,"+at+","+(~crc).ToString("X8"));
        }
        await RefreshFiles();
      }
    }
    async Task Delete(){string name=Selected();if(MessageBox.Show(this,"Supprimer « "+name+" » de la carte SD ?","Suppression",MessageBoxButtons.OKCancel)!=DialogResult.OK)return;await Renew();await request("DELETE,"+PathHex(Full(name)));await RefreshFiles();}
    async Task Rename(){string name=Selected();using(var dialog=new Form{Text="Renommer",Size=new Size(440,150),StartPosition=FormStartPosition.CenterParent,MinimizeBox=false,MaximizeBox=false}){
      var text=new TextBox{Text=name,Dock=DockStyle.Top};var ok=new Button{Text="Renommer",Dock=DockStyle.Bottom,DialogResult=DialogResult.OK};dialog.Controls.Add(text);dialog.Controls.Add(ok);dialog.AcceptButton=ok;
      if(dialog.ShowDialog(this)!=DialogResult.OK)return;string next=text.Text.Trim();if(next.Length==0||next=="."||next==".."||next.IndexOfAny(Path.GetInvalidFileNameChars())>=0)throw new IOException("Nom invalide.");
      await Renew();await request("RENAME,"+PathHex(Full(name))+","+PathHex(Full(next)));await RefreshFiles();
    }}
  }
}
