using System.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows.Forms;

[assembly:System.Reflection.AssemblyInformationalVersion("VA01.15 R21 — composition, automations et audio — build 10.1.15.20")]
[assembly:System.Reflection.AssemblyVersion("10.1.15.20")]
[assembly:System.Reflection.AssemblyFileVersion("10.1.15.20")]
namespace UrbanKaoz.DisplayRecorder {
  internal sealed partial class MainForm : Form {
    AudioMidiForm audioForm904;
    Synkoz.CapturePanel capture933;TabControl tabs933;Panel songHost933;bool controls933,closingCapture933;
    internal double AudioTempo904(){return ready&&session.ElapsedMilliseconds-lastRx<5000?state.Song.Bpm:115;}
    internal void SendUsbMessage901(string kind,int channel,int number,int value){
      if(kind!="NOTE"&&kind!="OFF"&&kind!="CC")throw new ArgumentException("Type USB invalide");
      if(channel<1||channel>16||number<0||number>127||value<0||value>127)throw new ArgumentException("Valeur USB invalide");
      if(!ready)throw new IOException("Connecter le Tee dans la fenêtre principale");
      if(commands.Count>=32)throw new IOException("Commandes USB en attente : envoi audio arrêté");
      Enqueue(kind+","+channel+","+number+","+value);
    }
    void EnsureAudio939(){if(audioForm904==null||audioForm904.IsDisposed)audioForm904=new AudioMidiForm(AudioTempo904,SendUsbMessage901,()=>ready){ConnectR5=()=>BeginConnect909()};}
    void OpenAudio904(){EnsureAudio939();if(!audioForm904.Visible)audioForm904.Show(this);if(audioForm904.WindowState==FormWindowState.Minimized)audioForm904.WindowState=FormWindowState.Normal;audioForm904.BringToFront();audioForm904.Activate();}
    internal CaptureState state=new CaptureState();
    internal Surface surface=new Surface();
    internal UrbanKaoz.Timeline timeline;
    readonly PictureBox ep=new PictureBox{Dock=DockStyle.Fill,SizeMode=PictureBoxSizeMode.Zoom};
    readonly ComboBox ports=new Synkoz.ScrollComboR8{FormattingEnabled=true},lcdPorts=new Synkoz.ScrollComboR8{FormattingEnabled=true},skins=new Synkoz.ScrollComboR8{FormattingEnabled=true},fps=new Synkoz.ScrollComboR8{FormattingEnabled=true};
    readonly Label status=new Label(),songStatus=new Label(),recStatus=new Label();
    readonly Button connect=new Button(),lcdConnect=new Button(),record=new Button(),obsButton=new Button();
    readonly CheckBox thin=new CheckBox{Text="Cadre",Checked=true,AutoSize=true},follow=new CheckBox{Text="Suivre",AutoSize=true};
    readonly Button[] screenButtons=new Button[8];
    readonly System.Windows.Forms.Timer timer=new System.Windows.Forms.Timer{Interval=25};
    readonly ToolTip tip=new ToolTip{ShowAlways=true};
    readonly Queue<string> incoming=new Queue<string>(),commands=new Queue<string>();
    readonly object receiveLock=new object();
    readonly StringBuilder fragment=new StringBuilder();
    readonly bool[] rows=new bool[12];
    SerialPort port,lcdPort;
    readonly StringBuilder lcdFragment=new StringBuilder();
    bool lcdDiscard;
    long lastLcdRx;
    int sequence,rowCount;
    string pending,loadedLog,lastView="",lastFolder,portError;
    long sentAt,lastPing,lastRx,openedAt;
    int retries;
    bool ready,demo,discard,dirty,closing,obsOwned,obsRecording,obsTakeActive,capturePaused;
    readonly Stopwatch session=Stopwatch.StartNew();
    internal Action<string> TestWriter;
    internal Action TestMidiStart;
    internal void TestPacket(string packet){if(packet.StartsWith("PC1,HELLO,"))controls933=true;Accept(packet);}
    internal void TestCommand(string command){Enqueue(command);}
    internal void TestTick(){Tick();}
    internal void LiveConnect(string tee,string lcd){controls933=true;ports.Text=tee;lcdPorts.Text=lcd;Connect();}
    internal bool LiveReady {get{return CanRecord();}}
    internal void LiveStart(string path){StartRecordPath(path);}
    internal void LiveStop(){StopRecord();}
    internal void LiveImage(string path){Render();using(var b=new Bitmap(Width,Height)){DrawToBitmap(b,new Rectangle(Point.Empty,Size));b.Save(path,System.Drawing.Imaging.ImageFormat.Png);}}
    StreamWriter log;Stopwatch recordingClock;
    readonly ObsWatcher obs=new ObsWatcher();
    string obsAddress="ws://127.0.0.1:4455",obsPassword="",obsFolder;
    bool obsAuto=true,obsMidi;
    int midiDevice=-1;
    Form songWindow;UrbanKaoz.Timeline bigTimeline;
    static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
    DisplaySkin Skin {get{return (DisplaySkin)Math.Max(0,skins.SelectedIndex);}}

    public MainForm(){
      Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath);Text="Snkoz — VA01.15 R21 — composition, automations et audio — build 10.1.15.20";var errorStatusR8=new Label{Dock=DockStyle.Bottom,Height=24,AutoEllipsis=true};Controls.Add(errorStatusR8);Synkoz.ErrorR8.Reported+=message=>{if(!IsDisposed){if(InvokeRequired)BeginInvoke(new Action(()=>errorStatusR8.Text=message));else errorStatusR8.Text=message;}};AutoScaleMode=AutoScaleMode.None;
      Font=new Font("Segoe UI",8.5f);BackColor=Color.FromArgb(19,22,29);ForeColor=Color.White;
      ClientSize=new Size(1248,632);MinimumSize=new Size(1000,600);StartPosition=FormStartPosition.CenterScreen;
      var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=4,Padding=new Padding(4),Margin=Padding.Empty};
      layout.RowStyles.Add(new RowStyle(SizeType.Absolute,64));layout.RowStyles.Add(new RowStyle(SizeType.Percent,40));layout.RowStyles.Add(new RowStyle(SizeType.Percent,60));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,24));
      var bar=new FlowLayoutPanel{Dock=DockStyle.Fill,Margin=Padding.Empty,WrapContents=true};
      ports.Width=73;ports.DropDownStyle=ComboBoxStyle.DropDownList;bar.Controls.Add(ports);Add(bar,"Ports",Scan);connect.Text="Connecter Tee";connect.AutoSize=true;connect.Click+=delegate{if(ready||connecting909){autoConnect909.Checked=false;controls933=false;StopRecord();Disconnect();}else{controls933=true;BeginConnect909();}};Style(connect);var connectGroup939=new FlowLayoutPanel{AutoSize=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Margin=Padding.Empty};connectGroup939.Controls.Add(connect);bar.Controls.Add(connectGroup939);InitConnection909(bar);
      Add(bar,"Démo",Demo);record.Text="● Enregistrer";record.AutoSize=true;record.Click+=delegate{StartRecord();};Style(record);
      
      fps.Width=48;fps.DropDownStyle=ComboBoxStyle.DropDownList;fps.Items.AddRange(new object[]{"5","10","25","30","50","60"});fps.SelectedIndex=3;
      skins.Width=104;skins.DropDownStyle=ComboBoxStyle.DropDownList;skins.Items.AddRange(new object[]{"Réel LCD","Free Party","Kawaï","Bling Bling","Futuriste","Minimal"});skins.SelectedIndex=0;skins.SelectedIndexChanged+=delegate{dirty=true;};bar.Controls.Add(skins);bar.Controls.Add(thin);thin.CheckedChanged+=delegate{dirty=true;};
      obsButton.Text="OBS : hors ligne";obsButton.AutoSize=true;Style(obsButton);obsButton.Click+=delegate{ObsSettings();};// OBS controls removed; capture owns its independent tab.
      Add(bar,"Carte SD Tee",OpenSd905);Add(bar,"Récupérer projet vers PC",()=>RecoverProject08(false));Add(bar,"Réinjecter projet",()=>RecoverProject08(true));bar.Controls.Add(audioLamp909);tip.SetToolTip(audioLamp909,"Note envoyée par USB (sans attendre de confirmation)");Add(bar,"Relâcher tout (Échap)",Release);Add(bar,"Resynchroniser",()=>{if(ready){SendSong("SYNC");if(lcdPort!=null)lcdPort.WriteLine("SNAPSHOT");rowCount=0;Array.Clear(rows,0,rows.Length);Notice("Resynchronisation Song et écrans…");}});
      bar.Controls.Add(new Label{Text="Picran LCD",AutoSize=true,Margin=new Padding(3,6,0,0)});lcdPorts.Width=70;lcdPorts.DropDownStyle=ComboBoxStyle.DropDownList;bar.Controls.Add(lcdPorts);lcdConnect.Text="Relier LCD";lcdConnect.AutoSize=true;Style(lcdConnect);lcdConnect.Click+=delegate{if(lcdPort!=null){StopRecord();CloseLcd();}else ConnectLcd();};bar.Controls.Add(lcdConnect);
      recStatus.AutoSize=true;recStatus.Margin=new Padding(8,6,0,0);recStatus.Text="Ctrl+clic : maintien · molette : rotation · clic droit : câblage / neutralisation";bar.Controls.Add(recStatus);
      layout.Controls.Add(bar,0,0);
      var views=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Margin=Padding.Empty};views.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,35));views.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,65));
      var lcd=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=4,RowCount=3,Margin=Padding.Empty};for(int i=0;i<4;i++)lcd.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));lcd.RowStyles.Add(new RowStyle(SizeType.Absolute,25));lcd.RowStyles.Add(new RowStyle(SizeType.Percent,100));lcd.RowStyles.Add(new RowStyle(SizeType.Absolute,25));
      int[] be={23,20,22,21,16,19,17,18};
      for(int i=0;i<8;i++){int index=i,bit=be[i];var b=new Button{Text="BE"+(i+1),Dock=DockStyle.Fill,Margin=new Padding(1)};Style(b);screenButtons[i]=b;
        b.MouseDown+=(o,e)=>{if(e.Button==MouseButtons.Left)Enqueue("HC,"+bit+",1");else if(e.Button==MouseButtons.Right)ShowBeInfo(b,index,bit,e.Location);};b.MouseUp+=(o,e)=>{if(e.Button==MouseButtons.Left)Enqueue("HC,"+bit+",0");};b.MouseCaptureChanged+=delegate{if(!b.Capture&&ready)Enqueue("HC,"+bit+",0");};lcd.Controls.Add(b,i%4,i/4==0?0:2);}
      lcd.Controls.Add(ep,0,1);lcd.SetColumnSpan(ep,4);views.Controls.Add(lcd,0,0);
      var songPanel=new Panel{Dock=DockStyle.Fill,Margin=new Padding(4,0,0,0)};songHost933=songPanel;
      var songBar=new FlowLayoutPanel{Dock=DockStyle.Top,Height=29,WrapContents=false,Margin=Padding.Empty};
      Add(songBar,"Song ON/OFF",()=>SendSong("MODE,"+(state.Song.Mode?0:1)));
      Add(songBar,"Blocs / ZL / Mixte",()=>SendSong("VIEW,"+((state.Song.View+1)%3)));
      Add(songBar,"Agrandir",ExpandSong);Add(songBar,"Nom…",()=>SendSong("NAMEEDIT?"));songBar.Controls.Add(follow);
      songStatus.AutoSize=true;songStatus.Margin=new Padding(3,6,0,0);songBar.Controls.Add(songStatus);
      timeline=new UrbanKaoz.Timeline(state.Song,SendSong){Dock=DockStyle.Fill,Row=24,ReadOnly=true};tip.SetToolTip(timeline,"Molette : pistes · Ctrl+molette : zoom temps · Maj+molette : défilement temps\nCliquer/glisser les blocs : édition Song · ZL : clic pour éditer");
      songPanel.Controls.Add(timeline);songPanel.Controls.Add(songBar);views.Controls.Add(songPanel,1,0);layout.Controls.Add(views,0,1);
      surface.Dock=DockStyle.Fill;surface.Margin=Padding.Empty;surface.Screens=state.Screens;surface.Send=Enqueue;surface.Notice=Notice;layout.Controls.Add(surface,0,2);
      status.Dock=DockStyle.Fill;status.AutoEllipsis=true;status.Text="Choisir le port USB du Teensy (COM6 à la dernière installation).";layout.Controls.Add(status,0,3);tabs933=new TabControl{Dock=DockStyle.Fill};var urban933=new TabPage("Urban Kaoz"){BackColor=BackColor};urban933.Controls.Add(layout);var video933=new TabPage("Prise");capture933=new Synkoz.CapturePanel(()=>ready,()=>BeginConnect909(),Enqueue);Synkoz.Engine.ScreenSnapshot935=()=>state.Snapshot().Where(p=>p.StartsWith("UKD1|"));capture933.ReadMicroA01=()=>{EnsureAudio939();return audioForm904.ExportCaptureA01();};capture933.WriteMicroA01=json=>{EnsureAudio939();audioForm904.ImportCaptureA01(json);};capture933.StartMicroA01=()=>{EnsureAudio939();audioForm904.StartCaptureA01();};video933.Controls.Add(capture933);tabs933.TabPages.Add(urban933);tabs933.TabPages.Add(video933);var montage934=new TabPage("Montage");montage934.Controls.Add(capture933.MontageView);tabs933.TabPages.Add(montage934);Controls.Add(tabs933);var common941=new FlowLayoutPanel{Dock=DockStyle.Top,Height=40,FlowDirection=FlowDirection.RightToLeft,WrapContents=true,AutoScroll=true,Padding=new Padding(3)};bar.Controls.Remove(ports);connectGroup939.Controls.Remove(connect);common941.Controls.Add(ports);common941.Controls.Add(connect);bar.Controls.Remove(autoConnect909);common941.Controls.Add(autoConnect909);var scan941=new Button{Text="Ports",AutoSize=true};scan941.Click+=delegate{Scan();};common941.Controls.Add(scan941);common941.Controls.Add(MicroPanel939());tabs933.Appearance=TabAppearance.FlatButtons;tabs933.ItemSize=new Size(0,1);tabs933.SizeMode=TabSizeMode.Fixed;foreach(TabPage page942 in tabs933.TabPages.Cast<TabPage>().Reverse()){var tab942=new Button{Text=page942.Text,Width=page942.Text=="Urban Kaoz"?110:90,Height=28,Image=Synkoz.SectionIconsR13.Create(page942.Text),ImageAlign=ContentAlignment.MiddleLeft,TextImageRelation=TextImageRelation.ImageBeforeText};tab942.Disposed+=delegate{if(tab942.Image!=null)tab942.Image.Dispose();};tab942.Click+=delegate{tabs933.SelectedTab=page942;};common941.Controls.Add(tab942);}Controls.Add(common941);common941.SendToBack();Synkoz.HeaderR14.Install(this,common941,capture933.MontageView);InstallMenus15();var language945=new Synkoz.ScrollComboR8{FormattingEnabled=true,Width=75,DropDownStyle=ComboBoxStyle.DropDownList};language945.Items.AddRange(new object[]{"Français","English"});language945.SelectedIndex=Synkoz.Language945.English?1:0;language945.SelectedIndexChanged+=delegate{Synkoz.Language945.Set(language945.SelectedIndex==1);};language945.Dispose();tabs933.SelectedIndexChanged+=delegate{dirty=true;};tabs933.SelectedTab=video933;
      lastFolder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),"Snkoz");obsFolder=lastFolder;LoadSettings();Scan();
      obs.Connected+=()=>SafeUi(()=>{obsButton.Text="OBS : prêt";Notice("OBS connecté");});
      obs.Error+=message=>SafeUi(()=>{obsButton.Text="OBS : erreur";if(obsOwned){StopRecord();obsOwned=false;}obsTakeActive=false;obsRecording=false;Notice("OBS : "+message);});
      obs.RecordingChanged+=value=>SafeUi(()=>ObsRecording(value));
      timer.Tick+=delegate{Tick();};timer.Start();KeyPreview=true;KeyDown+=(o,e)=>{if(e.KeyCode==Keys.Escape){Release();e.Handled=true;}};
      Deactivate+=delegate{BeginInvoke((Action)delegate{if(!IsDisposed&&!surface.NumericHasFocus917)Release();});};FormClosing+=async (sender,e)=>{if(!closingCapture933){e.Cancel=true;closingCapture933=true;await capture933.Shutdown();Close();return;}closing=true;timer.Stop();StopRecord();Disconnect();obs.Dispose();if(audioForm904!=null)audioForm904.Close();SaveSettings();if(songWindow!=null)songWindow.Close();};
    }
    static void Style(Button b){b.BackColor=Color.FromArgb(43,50,62);b.ForeColor=Color.White;b.FlatStyle=FlatStyle.Flat;b.FlatAppearance.BorderColor=Color.FromArgb(75,85,100);b.Height=25;b.Margin=new Padding(2);b.Padding=Padding.Empty;}
    static Button Add(Control parent,string label,Action action){var b=new Button{Text=label,AutoSize=true};Style(b);b.Click+=delegate{action();};parent.Controls.Add(b);return b;}
    void Notice(string text){status.Text=text;}
    void SafeUi(Action a){if(closing||IsDisposed||!IsHandleCreated)return;try{BeginInvoke(a);}catch(InvalidOperationException){}}
    void Scan(){string old=ports.Text,lcd=lcdPorts.Text;var names=SerialPort.GetPortNames();ports.Items.Clear();ports.Items.AddRange(names);lcdPorts.Items.Clear();lcdPorts.Items.AddRange(names);if(ports.Items.Contains(old))ports.Text=old;else if(ports.Items.Contains("COM6"))ports.Text="COM6";else if(ports.Items.Count>0)ports.SelectedIndex=0;if(lcdPorts.Items.Contains(lcd))lcdPorts.Text=lcd;else if(lcdPorts.Items.Contains("COM7"))lcdPorts.Text="COM7";}
    void NewState(){if(lcdPort!=null)lcdPort.WriteLine("SNAPSHOT");state=new CaptureState();surface.Screens=state.Screens;timeline.Parent.Controls.Remove(timeline);timeline.Dispose();
      var parent=songHost933;
      timeline=new UrbanKaoz.Timeline(state.Song,SendSong){Dock=DockStyle.Fill,Row=24,ReadOnly=!ready||!controls933};parent.Controls.Add(timeline);
      Control top=null;foreach(Control c in parent.Controls)if(c is FlowLayoutPanel)top=c;if(top!=null){parent.Controls.SetChildIndex(timeline,0);parent.Controls.SetChildIndex(top,1);}
      tip.SetToolTip(timeline,"Molette : pistes · Ctrl+molette : zoom · Maj+molette : temps\nGlisser un bloc pour le déplacer ou raccourcir ses bords.");
      if(songWindow!=null)songWindow.Close();surface.Colors.Initialize();dirty=true;
    }
    void Connect(){BeginConnect909();}
    void Disconnect(){Synkoz.UiPolicyR20.Teensy=false;
      ++connectGeneration909;connecting909=false;nextConnect909=session.ElapsedMilliseconds+1000;
      if(sdResponse!=null)sdResponse.TrySetException(new IOException("Tee déconnecté."));
      CloseLcd();
      if(port!=null){try{if(port.IsOpen&&ready){Write("PC1,"+(sequence+1)+",BYE");}}catch{}var old=port;port=null;try{old.DataReceived-=Receive;old.Close();old.Dispose();}catch{}}
      lock(receiveLock){incoming.Clear();fragment.Clear();lcdFragment.Clear();}
      if(capture933!=null)capture933.Disconnected();
      ready=false;commands.Clear();pending=null;sequence=0;surface.ResetSession();timeline.ReadOnly=true;if(bigTimeline!=null)bigTimeline.ReadOnly=true;state.Song.Synced=false;connect.Text="Connecter Tee";dirty=true;
      if(nameForm!=null){var old=nameForm;nameForm=null;old.Close();}
    }
    void ConnectLcd(){try{
      if(lcdPort!=null)CloseLcd();
      if(lcdPorts.Text==ports.Text&&port!=null)throw new Exception("Choisir le port du Picran, distinct du Teensy.");
      lock(receiveLock){lcdFragment.Clear();lcdDiscard=false;}rowCount=0;Array.Clear(rows,0,rows.Length);
      lcdPort=new SerialPort(lcdPorts.Text,115200){DtrEnable=true,RtsEnable=false,NewLine="\n",WriteTimeout=200,ReadTimeout=100,Encoding=Encoding.ASCII};lcdPort.DataReceived+=ReceiveLcd;lcdPort.Open();lcdPort.WriteLine("SNAPSHOT");lastLcdRx=session.ElapsedMilliseconds;lcdConnect.Text="LCD connecté";
    }catch(Exception ex){CloseLcd();Notice("Écrans : "+ex.Message);}}
    void CloseLcd(){if(lcdPort!=null){var old=lcdPort;lcdPort=null;try{old.DataReceived-=ReceiveLcd;old.Close();old.Dispose();}catch{}}lcdConnect.Text="Relier LCD";rowCount=0;Array.Clear(rows,0,rows.Length);}
    void ReceiveLcd(object sender,SerialDataReceivedEventArgs args){try{string data=((SerialPort)sender).ReadExisting();lock(receiveLock){foreach(char c in data){if(c=='\r')continue;if(c=='\n'){if(!lcdDiscard&&lcdFragment.Length>0&&incoming.Count<12000)incoming.Enqueue("LCD831|"+lcdFragment);lcdFragment.Clear();lcdDiscard=false;}else if(!lcdDiscard){if(lcdFragment.Length<512)lcdFragment.Append(c);else{lcdDiscard=true;lcdFragment.Clear();}}}}}catch{lock(receiveLock)incoming.Enqueue("LCD831|LOST");}}
    void Write(string packet){if(TestWriter!=null){TestWriter(packet);return;}if(port==null||!port.IsOpen)throw new IOException("Port USB fermé");port.WriteLine(packet);}
    void Receive(object sender,SerialDataReceivedEventArgs args){
      try{var link=(SerialPort)sender;string text=link.ReadExisting();lock(receiveLock){foreach(char c in text){if(c=='\r')continue;if(c=='\n'){if(!discard&&fragment.Length>0){if(incoming.Count<12000)incoming.Enqueue(fragment.ToString());else portError="Réception saturée ; reconnecter pour resynchroniser.";}fragment.Clear();discard=false;}else if(!discard){if(fragment.Length<2048)fragment.Append(c);else{discard=true;fragment.Clear();}}}}}
      catch(Exception ex){lock(receiveLock)portError=ex.Message;}
      SafeUi(Tick);
    }
    void Enqueue(string command){if(!controls933&&!command.StartsWith("SK")&&command!="PING"&&command!="RELEASE"){Notice("Activer Connecter Tee pour les commandes Snkoz.");return;}if(!ready){Notice("Commandes indisponibles : connecter le Teensy V9.31.");return;}if(commands.Count>=256){Fault("Trop de commandes en attente");return;}commands.Enqueue(command);Pump();}
    void Pump(){if(!ready||pending!=null||commands.Count==0)return;pending="PC1,"+(sequence+1)+","+commands.Dequeue();sentAt=session.ElapsedMilliseconds;retries=0;try{Write(pending);NoteSent909(pending);if(capture933!=null)capture933.Sent(pending);}catch(Exception ex){Fault(ex.Message);}}
    void SendSong(string command){if(!controls933&&command!="SYNC"){Notice("Activer Connecter Tee pour les commandes Snkoz.");return;}if(!ready||!state.Song.Synced&&command!="SYNC"&&command!="NAMEEDIT?"){Notice("Attendre la synchronisation Song.");return;}
      try{Write("SONG,"+command);}catch(Exception ex){Fault(ex.Message);}
    }
    void Release(){if(ready){commands.Clear();Enqueue("RELEASE");}surface.ClearHeld();}
    void Fault(string message){if(log!=null)RecordPacket("SOURCE831,LOST");StopRecord();Disconnect();Notice("Liaison interrompue : "+message+" — commandes relâchées par le Teensy après 2,5 s.");}
    void Tick(){
      List<string> lines=new List<string>();string error=null;lock(receiveLock){for(int i=0;i<600&&incoming.Count>0;i++)lines.Add(incoming.Dequeue());error=portError;portError=null;}
      if(error!=null&&port!=null){Fault(error);return;}foreach(string line in lines)Accept(line);
      long now=session.ElapsedMilliseconds;
      ConnectionTick909(now);
      // An unchanged LCD is legitimately silent; receive errors still report LOST.
      if(port!=null&&!ready&&now-openedAt>4000){Fault("le port ne répond pas au protocole V9.31 ; vérifier la carte et son firmware");return;}
      if(port!=null&&now-lastRx>5000){Fault("aucune donnée depuis 5 secondes");return;}
      if(ready){
        if(pending!=null&&now-sentAt>400){if(retries++>=7){Fault("commande sans accusé de réception");return;}sentAt=now;try{Write(pending);}catch(Exception ex){Fault(ex.Message);return;}}
        if(now-lastPing>500&&commands.Count==0){lastPing=now;Enqueue("PING");}Pump();
      }
      if(follow.Checked&&state.Song.Playing){var view=bigTimeline??timeline;double pos=state.Song.Tick/96.0;double span=Math.Max(16,(view.Width-85)/view.Step);if(pos<view.Start||pos>view.Start+span-8)view.Start=Math.Max(0,pos-8);dirty=true;}
      CaptureView();if(log!=null){try{log.Flush();}catch(Exception ex){StopRecord();Notice("Capture arrêtée : "+ex.Message);}}
      if(dirty&&tabs933.SelectedIndex==0){dirty=false;Render();}if(log!=null)recStatus.Text=(capturePaused?"Ⅱ PAUSE ":"● CAPTURE ")+recordingClock.Elapsed.ToString(@"hh\:mm\:ss")+" · 3 flux";
      if(obsRecording&&!capturePaused)obsButton.BackColor=(now/450)%2==0?Color.Red:Color.DarkRed;
    }
    void Accept(string packet){
      if(capture933!=null&&capture933.Packet(packet)){lastRx=session.ElapsedMilliseconds;return;}
      if(surface.AcceptNumeric917(packet)){lastRx=session.ElapsedMilliseconds;return;}
      if(AcceptSd905(packet)){lastRx=session.ElapsedMilliseconds;return;}
      if(packet.StartsWith("LCD831|")){
        packet=packet.Substring(7);if(packet=="LOST"){RecordPacket("SOURCE831,LCD_LOST");StopRecord();CloseLcd();Notice("Picran déconnecté — commandes Teensy toujours disponibles.");return;}
        lastLcdRx=session.ElapsedMilliseconds;if(!packet.StartsWith("UKD1|"))return;
      }else lastRx=session.ElapsedMilliseconds;
      if(packet.StartsWith("PC1,HELLO,")){
        string peer; if(!Compatible909(packet,out peer)){autoConnect909.Checked=false;Fault("protocole incompatible");return;}ready=true;Synkoz.UiPolicyR20.Teensy=true;surface.NumericAvailable917=NumericEntry917.Compatible(peer);sequence=0;pending=null;commands.Clear();surface.Connected=controls933;timeline.ReadOnly=!controls933;if(capture933!=null){Version fw934;string featureVersionA01=(peer=="V1"||peer=="A01"||(peer.StartsWith("VA",StringComparison.OrdinalIgnoreCase)&&NumericEntry917.Compatible(peer)))?"V9.42":peer;capture933.Firmware934=Version.TryParse(featureVersionA01.TrimStart('V','v'),out fw934)&&fw934>=new Version(9,34);capture933.Firmware935=fw934!=null&&fw934>=new Version(9,35);capture933.Firmware940=fw934!=null&&fw934>=new Version(9,40);capture933.Connected();}connect.Text=controls933?"Déconnecter Tee":"Connecter Tee";Notice("Teensy "+peer+" connecté — réception Song et écrans…");return;
      }
      if(packet.StartsWith("PC1,ACK,")){
        string[] a=packet.Split(',');int id;if(a.Length!=4||!int.TryParse(a[2],out id))return;
        if(pending==null||id!=sequence+1)return;if(a[3]!="OK"){Fault("commande refusée : "+a[3]);return;}sequence=id;pending=null;Pump();return;
      }
      if(packet=="SONG,ERROR,RESYNC_REQUIRED"){state.Song.Synced=false;SendSong("SYNC");Notice("Reprise de la synchronisation Song…");}
      else if(packet.StartsWith("SONG,ERROR,")){Notice("Song : "+packet.Substring(11));timeline.Cancel();}
      if(packet.StartsWith("SONG,NAMEEDIT,"))NameEdit(packet.Substring(14));
      if(packet.StartsWith("SONG,SELALL,")){int scope;if(int.TryParse(packet.Substring(12),out scope))timeline.SelectAll(scope);}
      if(!state.Apply(packet))return;
      if(packet.StartsWith("UKD1|L|")){string[] a=packet.Split('|');int s=UkdProtocol.ScreenIndex(a[2]),r;if(s>=0&&int.TryParse(a[3],out r)){int i=s==0?r:4+(s-1)*2+r;if(i>=0&&i<12&&!rows[i]){rows[i]=true;rowCount++;}}}
      if(packet.StartsWith("SONG,CLIP,")||packet.StartsWith("SONG,ZL,")||packet.StartsWith("SONG,BEGIN")){timeline.Cancel();if(bigTimeline!=null)bigTimeline.Cancel();}
      if(packet.StartsWith("UKD1|L|"))lcdConnect.Text="LCD "+rowCount+"/12";
      if(packet=="SONG,END")Notice("Song synchronisée · écrans "+rowCount+"/12 · commandes actives");
      RecordPacket(packet);dirty=true;
    }
    void CaptureView(){var v=bigTimeline??timeline;state.Start=v.Start;state.Step=v.Step;state.Row=v.Row;state.ScrollY=v.ScrollY;state.ViewWidth=Math.Max(100,v.Width);state.ViewHeight=Math.Max(50,v.Height);string p=state.ViewPacket();if(p!=lastView){lastView=p;RecordPacket(p);}}
    void Render(){surface.Skin=Skin;Array.Copy(state.Leds,surface.Colors,64);surface.Invalidate();timeline.Accent=Surface.Accent(Skin);timeline.Invalidate();if(bigTimeline!=null){bigTimeline.Accent=Surface.Accent(Skin);bigTimeline.Invalidate();}
      var old=ep.Image;ep.Image=LcdRenderer.RenderSingle(state.Screens.Lines[0],20,640,240,Skin,thin.Checked);if(old!=null)old.Dispose();
      for(int i=0;i<8;i++){string line=state.Screens.Lines[0][i<4?2:3].PadRight(20);string label=line.Substring(i%4*5,5).Trim();string joined=line.Replace(" ","");if(joined=="TRACKPATCH--LFO")label=new[]{"TRACK","PATCH","--","LFO"}[i%4];else if(joined=="PARAWRITEFXENV")label=new[]{"PARA","WRITE","FX","ENV"}[i%4];screenButtons[i].Text=label.Length>0?label:"BE"+(i+1);}
      songStatus.Text=(state.Song.Mode?"SONG":"SEQ")+" · "+state.Song.Bpm.ToString("0.#")+" BPM";record.BackColor=log==null?Color.FromArgb(43,50,62):Color.DarkRed;
    }
    void ShowBeInfo(Button control,int index,int bit,Point at){tip.Show("BE"+(index+1)+" — "+control.Text+"\n74HC165 : bit B"+bit+". Teensy 26 SH/LD, 27 CLK, 28 DATA (sérigraphie).\nFonction définie par le menu affiché.",control,at.X,at.Y+20,20000);
      var m=new ContextMenuStrip();m.Items.Add(new ToolStripLabel("BE"+(index+1)+" — "+control.Text+"\n74HC165 : bit B"+bit+"\nTeensy : 26 SH/LD, 27 CLK, 28 DATA (sérigraphie)."));m.Items.Add(new ToolStripSeparator());m.Items.Add("Neutraliser BE"+(index+1)+" physique",null,delegate{Enqueue("MASK,HC,"+bit+",1");});m.Items.Add("Réactiver BE"+(index+1)+" physique",null,delegate{Enqueue("MASK,HC,"+bit+",0");});Synkoz.MenuLifetimeR13.Attach(m,control);m.Show(control,at);
    }
    internal void Demo(){autoConnect909.Checked=false;StopRecord();Disconnect();NewState();demo=true;
      string[] text={"120. 1/4  START END","X01  1    0001 0016","PARA WRITEFX   ENV  ","TRACKPATCH--   LFO  "};for(int i=0;i<4;i++)UkdProtocol.Apply("UKD1|L|EP|"+i+"|"+text[i],state.Screens);
      string[] labels={"VOL CHA NOT VEL","GAT TIM PBN BPM","RLV MOD RST RSP","RAC REX RCV MUTE"},values={"090 01 C 2  100","048 000 000 120","100 OFF 000 024","000 000 000 OFF"};for(int i=0;i<4;i++){UkdProtocol.Apply("UKD1|L|"+"ABCD"[i]+"|0|"+labels[i],state.Screens);UkdProtocol.Apply("UKD1|L|"+"ABCD"[i]+"|1|"+values[i],state.Screens);}
      state.Apply("SONG,BEGIN,2");for(int i=0;i<8;i++)state.Apply("SONG,CLIP,"+i+","+i+","+i%3+","+i*8+",0,32");state.Apply("SONG,STATE,1,0,1536,1,1,8,12000,0,0,0");state.Apply("SONG,END");for(int i=0;i<64;i++)state.Leds[i]=i%5==0?0x168fa2:i%7==0?0xbca141:0;
      timeline.Start=0;timeline.Step=8;timeline.Row=24;CaptureView();Render();Notice("DÉMONSTRATION FICTIVE — aucune commande envoyée, enregistrement bloqué.");
    }
    bool CanRecord(){return ready&&!demo&&rowCount==12&&state.Song.Synced;}
    void StartRecord(){if(log!=null)return;if(!CanRecord()){Notice("Capture : attendre les 12 lignes réelles et la synchronisation Song.");return;}
      using(var d=new SaveFileDialog{AutoUpgradeEnabled=true,Filter="Journal Snkoz|*.ukd",FileName="Snkoz_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".ukd",InitialDirectory=Folder()})if(d.ShowDialog(this)==DialogResult.OK){lastFolder=Path.GetDirectoryName(d.FileName);try{StartRecordPath(d.FileName);}catch(Exception ex){Notice(ex.Message);}}
    }
    void StartRecordPath(string path){if(!CanRecord())throw new InvalidOperationException("Capture réelle incomplète : Song ou écrans non synchronisés.");CaptureView();log=new StreamWriter(path,false,new UTF8Encoding(false));log.WriteLine("# UKDLOG2 V9.31 EP ABCD SONG");capturePaused=false;recordingClock=Stopwatch.StartNew();loadedLog=path;foreach(string p in state.Snapshot())log.WriteLine("0000000000|"+p);log.Flush();dirty=true;Notice("Enregistrement des trois flux dans "+path);}
    void RecordPacket(string packet){if(capture933!=null)capture933.LcdPacket935(packet);if(log==null||capturePaused)return;try{log.WriteLine(recordingClock.ElapsedMilliseconds.ToString("D10")+"|"+packet);}catch(Exception ex){StopRecord();Notice("Erreur capture : "+ex.Message);}}
    void StopRecord(){if(log==null)return;var writer=log;log=null;try{writer.WriteLine(recordingClock.ElapsedMilliseconds.ToString("D10")+"|END831");writer.Dispose();}catch(Exception ex){Notice("Erreur de fermeture capture : "+ex.Message);}recordingClock.Stop();capturePaused=false;recStatus.Text="Capture arrêtée · trois flux dans le .ukd";dirty=true;obsOwned=false;}
    void OpenLog(){using(var d=new OpenFileDialog{AutoUpgradeEnabled=true,Filter="Journal Snkoz|*.ukd"})if(d.ShowDialog(this)==DialogResult.OK){try{autoConnect909.Checked=false;StopRecord();Disconnect();NewState();demo=false;loadedLog=d.FileName;foreach(var e in Recording.Events(loadedLog))state.Apply(e.Packet);timeline.Start=state.Start;timeline.Step=state.Step;timeline.Row=state.Row;timeline.ScrollY=state.ScrollY;timeline.ReadOnly=true;Render();Notice("Journal chargé : "+Path.GetFileName(loadedLog)+(state.HasSong?"":" — ancien journal sans Song"));}catch(Exception ex){Notice("Journal illisible : "+ex.Message);}}}
    void Export(){StopRecord();if(string.IsNullOrEmpty(loadedLog)||!File.Exists(loadedLog)){Notice("Enregistrer ou ouvrir un .ukd avant l'export.");return;}
      using(var d=new SaveFileDialog{AutoUpgradeEnabled=true,Filter="Vidéo AVI|*.avi",FileName=Path.GetFileNameWithoutExtension(loadedLog)+".avi",InitialDirectory=Folder(),Title="Nom commun des trois vidéos",OverwritePrompt=false}){
        if(d.ShowDialog(this)!=DialogResult.OK)return;lastFolder=Path.GetDirectoryName(d.FileName);string stem=Path.Combine(lastFolder,Path.GetFileNameWithoutExtension(d.FileName));var paths=Recording.Paths(stem);bool exists=false;foreach(string p in paths)exists|=File.Exists(p);
        if(exists&&Synkoz.ErrorR8.Show(this,"Remplacer les trois vidéos de ce nom ?","Export",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
        string input=loadedLog;int rate=int.Parse(fps.Text);DisplaySkin skin=Skin;bool frame=thin.Checked;
        using(var progress=new Form{Text="Export des trois vidéos",Size=new Size(420,140),StartPosition=FormStartPosition.CenterParent,FormBorderStyle=FormBorderStyle.FixedDialog,ControlBox=false}){
          var label=new Label{Dock=DockStyle.Top,Height=30,Text="Préparation…"};var bar=new ProgressBar{Dock=DockStyle.Top};progress.Controls.Add(bar);progress.Controls.Add(label);bool cancel=false,done=false;Exception error=null;var stop=new Button{Text="Annuler",Dock=DockStyle.Bottom};stop.Click+=delegate{cancel=true;};progress.Controls.Add(stop);
          var thread=new Thread(()=>{try{Recording.Export(input,stem,rate,skin,frame,p=>{try{progress.BeginInvoke((Action)(()=>{bar.Value=p;label.Text="Export : "+p+" %";}));}catch{}},()=>Volatile.Read(ref cancel));}catch(Exception ex){error=ex;}finally{done=true;try{progress.BeginInvoke((Action)(()=>progress.Close()));}catch{}}});thread.SetApartmentState(ApartmentState.STA);thread.IsBackground=true;progress.Shown+=delegate{thread.Start();};progress.FormClosing+=(o,e)=>{if(!done){cancel=true;e.Cancel=true;}};progress.ShowDialog(this);thread.Join();
          Notice(error==null?"Trois vidéos exportées dans "+lastFolder:error is OperationCanceledException?"Export annulé ; fichiers partiels supprimés.":"Export impossible : "+error.Message);
        }
      }
    }
    void ExpandSong(){if(songWindow!=null){songWindow.Activate();return;}songWindow=new Form{Text="Snkoz — Song",Size=new Size(1240,650),StartPosition=FormStartPosition.CenterParent};bigTimeline=new UrbanKaoz.Timeline(state.Song,SendSong){Dock=DockStyle.Fill,ReadOnly=!ready||!controls933,Row=30,Step=timeline.Step,Start=timeline.Start,Accent=Surface.Accent(Skin)};songWindow.Controls.Add(bigTimeline);songWindow.FormClosed+=delegate{timeline.Start=bigTimeline.Start;timeline.Step=bigTimeline.Step;bigTimeline=null;songWindow=null;};songWindow.Show(this);}
    string nameSession;Form nameForm;TextBox nameText;
    void NameEdit(string message){string[] a=message.Split(new[]{','},7);if(a.Length<7)return;if(a[1]!="1"){if(nameForm!=null){var old=nameForm;nameForm=null;old.Close();}return;}
      if(nameForm!=null&&nameSession==a[0]){if(!nameText.Focused)nameText.Text=a[6];return;}
      if(nameForm!=null){var old=nameForm;nameForm=null;old.Close();}nameSession=a[0];int limit;if(!int.TryParse(a[3],out limit)||limit<1||limit>48)return;
      nameForm=new Form{Text="Nom "+a[2]+" — Teensy",Size=new Size(470,155),StartPosition=FormStartPosition.CenterParent,Font=Font};var form=nameForm;string sid=nameSession;
      nameText=new TextBox{Text=a[6],MaxLength=limit,Dock=DockStyle.Top};form.Controls.Add(nameText);
      var buttons=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=40};form.Controls.Add(buttons);bool sent=false;
      Add(buttons,"Valider",()=>{if(!ValidName(nameText.Text)){Synkoz.ErrorR8.Show(form,"Utiliser des caractères ASCII sans virgule ni barre verticale.");return;}SendSong("NAMEEDIT,SET,"+sid+","+nameText.SelectionStart+","+nameText.Text);SendSong("NAMEEDIT,COMMIT,"+sid);sent=true;form.Close();});
      Add(buttons,"Annuler",()=>{SendSong("NAMEEDIT,CANCEL,"+sid);sent=true;form.Close();});
      form.FormClosed+=delegate{if(nameForm==form){nameForm=null;if(!sent&&ready)SendSong("NAMEEDIT,CANCEL,"+sid);}};form.Show(this);
    }
    static bool ValidName(string text){foreach(char c in text)if(c<32||c>126||c==','||c=='|')return false;return true;}
    void ObsSettings(){Release();using(var f=new Form{Text="OBS et MIDI — Snkoz",Size=new Size(610,340),StartPosition=FormStartPosition.CenterParent,Font=Font}){
      var panel=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(10)};f.Controls.Add(panel);
      panel.Controls.Add(new Label{Text="Serveur OBS WebSocket",AutoSize=true});var addr=new TextBox{Text=obsAddress,Width=555};panel.Controls.Add(addr);
      panel.Controls.Add(new Label{Text="Mot de passe (non enregistré)",AutoSize=true});var password=new TextBox{Text=obsPassword,UseSystemPasswordChar=true,Width=555};panel.Controls.Add(password);
      var auto=new CheckBox{Text="Suivre OBS : démarrage, pause, reprise et arrêt",Checked=obsAuto,AutoSize=true};panel.Controls.Add(auto);
      var folder=new TextBox{Text=obsFolder,Width=555};panel.Controls.Add(folder);var midi=new CheckBox{Text="Envoyer MIDI Start au démarrage OBS",Checked=obsMidi,AutoSize=true};panel.Controls.Add(midi);
      var devices=new Synkoz.ScrollComboR8{FormattingEnabled=true,Width=555,DropDownStyle=ComboBoxStyle.DropDownList};devices.Items.AddRange(MidiOutput.Devices());if(midiDevice>=0&&midiDevice<devices.Items.Count)devices.SelectedIndex=midiDevice;else if(devices.Items.Count>0)devices.SelectedIndex=0;panel.Controls.Add(devices);
      var actions=new FlowLayoutPanel{Width=560,Height=40};panel.Controls.Add(actions);Add(actions,"Choisir dossier",()=>{using(var d=new Synkoz.FolderDialog08{SelectedPath=folder.Text})if(d.ShowDialog(f)==DialogResult.OK)folder.Text=d.SelectedPath;});
      Add(actions,"Test MIDI Start",()=>{try{if(devices.SelectedIndex<0)throw new Exception("Choisir une sortie MIDI");MidiOutput.SendStart(devices.SelectedIndex);}catch(Exception ex){Synkoz.ErrorR8.Show(f,ex.Message);}});
      Add(actions,obs.IsConnected?"Déconnecter OBS":"Connecter OBS",()=>{obsAddress=addr.Text;obsPassword=password.Text;obsFolder=folder.Text;obsAuto=auto.Checked;obsMidi=midi.Checked;midiDevice=devices.SelectedIndex;
        if(obs.IsConnected){obs.Dispose();if(obsOwned)StopRecord();obsTakeActive=false;obsRecording=false;obsButton.Text="OBS : hors ligne";}else obs.Connect(obsAddress,obsPassword);f.Close();});
      f.FormClosing+=delegate{obsAddress=addr.Text;obsPassword=password.Text;obsFolder=folder.Text;obsAuto=auto.Checked;obsMidi=midi.Checked;midiDevice=devices.SelectedIndex;};f.ShowDialog(this);
    }}
    void PauseCapture(){if(log==null||capturePaused)return;recordingClock.Stop();capturePaused=true;log.Flush();recStatus.Text="Ⅱ PAUSE · capture suspendue, commandes disponibles";}
    void ResumeCapture(){if(log==null||!capturePaused)return;
      // Changes during the cut must appear on the first resumed frame, including deletions.
      CaptureView();long time=recordingClock.ElapsedMilliseconds;
      foreach(string packet in state.Snapshot())log.WriteLine(time.ToString("D10")+"|"+packet);
      log.Flush();capturePaused=false;recordingClock.Start();
    }
    internal void ObsRecording(string value){
      if(value=="OBS_WEBSOCKET_OUTPUT_STARTING"||value=="OBS_WEBSOCKET_OUTPUT_STOPPING")return;
      if(value=="OBS_WEBSOCKET_OUTPUT_STOPPED"){
        obsTakeActive=false;obsRecording=false;obsButton.Text="OBS : prêt";obsButton.BackColor=Color.DarkGreen;if(obsOwned)StopRecord();return;
      }
      if(value=="OBS_WEBSOCKET_OUTPUT_PAUSED"||value=="OBS_WEBSOCKET_OUTPUT_RESUMED"){
        if(!obsTakeActive)return;bool pause=value=="OBS_WEBSOCKET_OUTPUT_PAUSED";
        obsRecording=!pause;obsButton.Text=pause?"Ⅱ OBS PAUSE":"● OBS REC";obsButton.BackColor=pause?Color.DarkOrange:Color.DarkRed;
        if(obsOwned)try{if(pause)PauseCapture();else ResumeCapture();}catch(Exception ex){StopRecord();Notice("Capture arrêtée : "+ex.Message);}return;
      }
      if(value!="OBS_WEBSOCKET_OUTPUT_STARTED"||obsTakeActive)return;
      obsTakeActive=true;obsRecording=true;obsButton.Text="● OBS REC";
      if(obsAuto&&log==null){try{if(!CanRecord())throw new Exception("Connecter le Teensy et attendre Song + 12 lignes avant OBS.");Directory.CreateDirectory(obsFolder);StartRecordPath(Path.Combine(obsFolder,"Snkoz_OBS_"+DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")+".ukd"));obsOwned=true;}catch(Exception ex){Notice("OBS enregistre, capture Snkoz non démarrée : "+ex.Message);return;}}
      if(obsMidi)try{if(TestMidiStart!=null){TestMidiStart();return;}if(midiDevice<0)throw new Exception("Sortie MIDI non sélectionnée");MidiOutput.SendStart(midiDevice);}catch(Exception ex){Notice("MIDI Start : "+ex.Message);}
    }
    string Folder(){return Directory.Exists(lastFolder)?lastFolder:Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);}
    string SettingsPath(){return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"UrbanKaoz","Surface831.settings");}
    void LoadSettings(){try{var p=SettingsPath();if(File.Exists(p)){var v=File.ReadAllLines(p);if(v.Length>0)lastFolder=v[0];if(v.Length>1)obsFolder=v[1];}}catch{}}
    void SaveSettings(){try{string p=SettingsPath();Directory.CreateDirectory(Path.GetDirectoryName(p));File.WriteAllLines(p,new[]{lastFolder,obsFolder});}catch{}}
    internal void RenderPreview(string path,int skin){skins.SelectedIndex=skin;Demo();PerformLayout();Render();using(var b=new Bitmap(Width,Height)){DrawToBitmap(b,new Rectangle(Point.Empty,Size));b.Save(path,System.Drawing.Imaging.ImageFormat.Png);}}
    bool disposed831;
    protected override void Dispose(bool disposing){if(disposing&&!disposed831){disposed831=true;if(!closing){closing=true;StopRecord();Disconnect();obs.Dispose();}timer.Dispose();tip.Dispose();if(ep.Image!=null)ep.Image.Dispose();}base.Dispose(disposing);}
  }
}




