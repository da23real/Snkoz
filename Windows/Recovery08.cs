using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace UrbanKaoz.DisplayRecorder {
 internal sealed partial class MainForm {
  bool recoveryBusy08;
  async void RecoverProject08(bool restore,string converted15=null){
   if(recoveryBusy08)return;
   if(!ready){Notice("Connecter le Teensy VA01.08 ou supérieur.");return;}
   string path=converted15;
   if(restore&&converted15==null){using(var d=new OpenFileDialog{AutoUpgradeEnabled=true,Title="Réinjecter un projet dans le Teensy",Filter="Projet Urban Kaoz|*.UKP"}){if(d.ShowDialog(this)!=DialogResult.OK)return;path=d.FileName;}
    if(Synkoz.ErrorR8.Show(this,"Remplacer le projet en mémoire par ce fichier ?", "Récupération Urban Kaoz",MessageBoxButtons.OKCancel)!=DialogResult.OK)return;
   }else if(!restore)using(var d=new SaveFileDialog{AutoUpgradeEnabled=true,Title="Récupérer le projet courant vers le PC",Filter="Projet Urban Kaoz|*.UKP",FileName="Recuperation_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".UKP"}){if(d.ShowDialog(this)!=DialogResult.OK)return;path=d.FileName;}
   recoveryBusy08=true;string temp=path+".transfer-"+Guid.NewGuid().ToString("N");
   try{
    if(restore){
     using(var input=File.OpenRead(path)){
      if(input.Length==0||input.Length>8388608)throw new IOException("Taille de projet invalide.");
      await SdRequest905("RAM08:UPLOAD");byte[] buffer=new byte[256];long at=0;int n;
      while((n=await input.ReadAsync(buffer,0,buffer.Length))>0){await SdRequest905("RAM08:WRITE,"+at+","+SdFilesForm.Hex(buffer,n));at+=n;Notice("Réinjection : "+at+" / "+input.Length);}
      var result=await SdRequest905((converted15!=null&&!Path.GetExtension(path).Equals(".ukp",StringComparison.OrdinalIgnoreCase)?"RAM08:APPLYTEXT15,":"RAM08:APPLY,")+at);if(result[2]!="OK"||result[3]!="RESTORED")throw new IOException("Réinjection non confirmée.");
     }
     Notice("Projet réinjecté en mémoire. Aucune écriture SD.");
    }else{
     var result=await SdRequest905("RAM08:CAPTURE");long size;
     if(result[2]!="SIZE"||!long.TryParse(result[3],out size)||size<=0||size>8388608)throw new IOException("Capture du projet non confirmée.");
     using(var output=new FileStream(temp,FileMode.CreateNew,FileAccess.Write)){
      long at=0;while(at<size){var part=await SdRequest905("RAM08:READ,"+at);if(part.Length!=6||part[2]!="DATA"||part[3]!=at.ToString())throw new IOException("Réponse de récupération invalide.");var data=SdFilesForm.Decode(part[5]);if(data.Length==0||at+data.Length>size||(~SdFilesForm.Crc(0xffffffffu,data,data.Length)).ToString("X8")!=part[4])throw new IOException("Contrôle du transfert échoué.");await output.WriteAsync(data,0,data.Length);at+=data.Length;Notice("Récupération : "+at+" / "+size);}
     }
     if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);Notice("Projet récupéré sur le PC : "+path);
    }
   }catch(Exception ex){Notice("Récupération : "+ex.Message);}
   finally{if(File.Exists(temp))File.Delete(temp);}try{await SdRequest905("RAM08:CLOSE");}catch{}recoveryBusy08=false;
  }
 }
}

