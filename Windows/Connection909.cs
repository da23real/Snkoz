using System;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace UrbanKaoz.DisplayRecorder {
 internal sealed partial class MainForm {
  readonly CheckBox autoConnect909=new CheckBox{Text="Auto connect",Appearance=Appearance.Button,AutoSize=true,Margin=new Padding(2)};
  readonly Label audioLamp909=new Label{Text="●",AutoSize=true,ForeColor=Color.DimGray,Margin=new Padding(1,5,3,0)};
  bool connecting909; int connectGeneration909; long nextConnect909,audioUntil909;
  internal static bool Compatible909(string packet,out string version){
   version=null;var p=packet.Split(',');if(p.Length!=4||p[0]!="PC1"||p[1]!="HELLO"||p[2].Length==0||p[3]!="1")return false;
   version=p[2];return true;
  }
  void InitConnection909(Control bar){
   bar.Controls.Add(autoConnect909);tip.SetToolTip(autoConnect909,"Retenter le port Tee choisi chaque seconde après une perte de liaison");
   autoConnect909.CheckedChanged+=delegate{if(autoConnect909.Checked){nextConnect909=0;demo=false;}else if(connecting909)Disconnect();};
  }
  void NoteSent909(string packet){
   var p=packet.Split(',');int velocity;
   if(p.Length==6&&p[0]=="PC1"&&p[2]=="NOTE"&&int.TryParse(p[5],out velocity)&&velocity>0){audioUntil909=session.ElapsedMilliseconds+120;audioLamp909.ForeColor=Color.LimeGreen;}
  }
  void ConnectionTick909(long now){
   if(audioUntil909!=0&&now>=audioUntil909){audioUntil909=0;audioLamp909.ForeColor=Color.DimGray;}
   if(!closing&&autoConnect909.Checked&&port==null&&!connecting909&&now>=nextConnect909){nextConnect909=now+1000;BeginConnect909();}
  }
  void BeginConnect909(){
   if(closing||port!=null||connecting909)return;
   string name=ports.Text;if(string.IsNullOrEmpty(name)){Notice("Choisir le port Tee pour Auto connect.");return;}
   connecting909=true;int generation=++connectGeneration909;connect.Text="Annuler connexion";Notice("Connexion au Tee…");
   // Opening a missing/slow USB device must not block painting or the audio UI.
   Task.Run(delegate {
    SerialPort candidate=null;Exception failure=null;
    try{candidate=new SerialPort(name,115200){NewLine="\n",DtrEnable=true,RtsEnable=false,WriteTimeout=150,ReadTimeout=100,Encoding=Encoding.ASCII};candidate.Open();}
    catch(Exception ex){failure=ex;}
    Action finish=delegate {
     if(closing||generation!=connectGeneration909){if(candidate!=null)candidate.Dispose();return;}
     connecting909=false;
     if(failure!=null){if(candidate!=null)candidate.Dispose();nextConnect909=session.ElapsedMilliseconds+1000;connect.Text="Connecter Tee";Notice("Connexion impossible : "+failure.Message);return;}
     try{
      NewState();demo=false;rowCount=0;Array.Clear(rows,0,rows.Length);
      lock(receiveLock){incoming.Clear();fragment.Clear();discard=false;portError=null;}
      port=candidate;port.DataReceived+=Receive;openedAt=lastRx=session.ElapsedMilliseconds;
      Write("PC1,HELLO");connect.Text="Déconnecter";if(lcdPort==null&&!string.IsNullOrEmpty(lcdPorts.Text))ConnectLcd();
     }catch(Exception ex){Disconnect();Notice("Connexion impossible : "+ex.Message);}
    };
    try{if(closing||IsDisposed){if(candidate!=null)candidate.Dispose();}else BeginInvoke(finish);}
    catch(InvalidOperationException){if(candidate!=null)candidate.Dispose();}
   });
  }
 }
}
