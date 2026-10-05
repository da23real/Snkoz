using System;
using System.IO;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace UrbanKaoz.DisplayRecorder {
  // Time is supplied by the caller so the gate can be tested without audio hardware.
  internal sealed class AudioGate904 {
    public bool Active { get; private set; }
    long above=-1,below=-1,next;
    public void Reset(){Active=false;above=below=-1;next=0;}
    public bool Tick(long now,bool high,int attack,int release,double bpm){
      if(!Active){
        if(!high){above=-1;return false;}
        if(above<0)above=now;
        if(now-above<attack)return false;
        Active=true;below=-1;next=now;
      }
      if(high)below=-1;
      else {
        if(below<0)below=now;
        if(now-below>=release){Reset();return false;}
      }
      if(now<next)return false;
      if(double.IsNaN(bpm)||double.IsInfinity(bpm)||bpm<30||bpm>300)bpm=115;
      next=now+(long)Math.Round(60000.0/bpm); // No burst of missed notes after a stall.
      return true;
    }
  }
  internal sealed class AudioMidiForm : Form {
    public bool Running939{get{return running;}}public string Status939{get{return state.Text;}}public void Toggle939(){if(running)Stop();else Start();}
    readonly CheckBox ccMode941=new CheckBox{Text="Niveau → CC",AutoSize=true};readonly NumericUpDown ccNumber941=new NumericUpDown{Minimum=0,Maximum=127,Value=85},interval941=new NumericUpDown{Minimum=10,Maximum=1000,Value=10};long nextCc941;int lastCc941=-1;readonly ComboBox sources=new ComboBox{FormattingEnabled=true},outputs=new ComboBox{FormattingEnabled=true};
    readonly NumericUpDown channel=new NumericUpDown(),note=new NumericUpDown(),threshold=new NumericUpDown(),attack=new NumericUpDown(),release=new NumericUpDown();
    readonly ProgressBar meter=new ProgressBar();readonly Label value=new Label(),state=new Label(),tempoLabel=new Label();
    readonly Button run=new Button(),refresh=new Button();
    readonly Timer timer=new Timer{Interval=10};readonly Stopwatch clock=Stopwatch.StartNew();
    readonly AudioGate904 gate=new AudioGate904();readonly Func<double> tempo;
    List<AudioEndpoint> endpoints=new List<AudioEndpoint>(); readonly Action<string,int,int,int> send; readonly Func<bool> connected;
    bool running,held;int heldChannel,heldNote;long offAt;
    public AudioMidiForm(Func<double> tempoSource,Action<string,int,int,int> sender,Func<bool> connection){
      tempo=tempoSource;send=sender;connected=connection;Text="Audio → Notes / CC — V9.41";ClientSize=new Size(640,525);MinimumSize=Size;
      StartPosition=FormStartPosition.CenterParent;Font=new Font("Segoe UI",9f);
      var grid=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(10),ColumnCount=3,RowCount=11};
      grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,160));grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,110));
      sources.DropDownStyle=outputs.DropDownStyle=ComboBoxStyle.DropDownList;
      Row(grid,0,"Entrée audio",sources,refresh);refresh.Text="Scanner";refresh.Click+=delegate{Reload();};
      Row(grid,1,"Destination",outputs,null);
      Setup(channel,1,16,1);Row(grid,2,"Canal MIDI",channel,null);
      Setup(note,0,127,60);Row(grid,3,"Note (C4 = 60)",note,null);
      Setup(threshold,1,100,10);Row(grid,4,"Seuil (% du maximum)",threshold,null);
      Setup(attack,0,5000,100);attack.Increment=10;Row(grid,5,"Attaque (ms)",attack,null);
      Setup(release,0,10000,800);release.Increment=50;Row(grid,6,"Release (ms)",release,null);
      meter.Maximum=100;Row(grid,7,"Niveau d'entrée",meter,value);
      Row(grid,8,"Cadence : chaque temps",tempoLabel,null);
      grid.Controls.Add(new Label{Text="Valeur 100 · impulsion 100 ms · C4 = 60 · repli 115 BPM",AutoSize=true},0,9);grid.SetColumnSpan(grid.GetControlFromPosition(0,9),3);
      run.Text="Démarrer";run.Click+=delegate{if(running)Stop();else Start();};Row(grid,10,"Envoi",run,null);
      grid.RowCount=12;grid.RowStyles.Add(new RowStyle(SizeType.Absolute,40));state.Dock=DockStyle.Fill;grid.Controls.Add(state,0,11);grid.SetColumnSpan(state,3);grid.RowCount=15;Row(grid,12,"Mode",ccMode941,null);Row(grid,13,"Numéro CC",ccNumber941,null);Row(grid,14,"Intervalle minimal (ms)",interval941,null);Controls.Add(grid);ccMode941.CheckedChanged+=delegate{Stop();SaveAudio918();};ccNumber941.ValueChanged+=delegate{lastCc941=-1;SaveAudio918();};interval941.ValueChanged+=delegate{SaveAudio918();};
      sources.SelectedIndexChanged+=delegate{Stop();foreach(var endpoint in endpoints)endpoint.Dispose();};
      outputs.SelectedIndexChanged+=delegate{Stop();};channel.ValueChanged+=delegate{Stop();};note.ValueChanged+=delegate{Stop();};
      timer.Tick+=delegate{TickMeter();};FormClosing+=delegate{Stop();timer.Stop();DisposeEndpoints();};
      Reload();LoadAudio918();timer.Start();
      foreach(var control in new[]{channel,note,threshold,attack,release})control.ValueChanged+=delegate{SaveAudio918();};
      sources.SelectedIndexChanged+=delegate{SaveAudio918();};outputs.SelectedIndexChanged+=delegate{SaveAudio918();};
    }
    bool loading918;
    internal sealed class AudioSettings918 {public bool CcMode941;public int CcNumber941=85,Interval941=10;public int Channel=1,Note=60,Threshold=10,Attack=100,Release=800;public string Source="",Output="";}
    internal static string TestSettingsPath918; static string AudioPath918(){if(TestSettingsPath918!=null)return TestSettingsPath918;return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Snkoz","AudioMidi.settings.json");}
    void LoadAudio918(){loading918=true;try{string p=AudioPath918();if(!File.Exists(p))return;var v=new JavaScriptSerializer().Deserialize<AudioSettings918>(File.ReadAllText(p));if(v==null)return;ccMode941.Checked=v.CcMode941;ccNumber941.Value=Math.Max(0,Math.Min(127,v.CcNumber941));interval941.Value=Math.Max(10,Math.Min(1000,v.Interval941));channel.Value=Math.Max(1,Math.Min(16,v.Channel));note.Value=Math.Max(0,Math.Min(127,v.Note));threshold.Value=Math.Max(1,Math.Min(100,v.Threshold));attack.Value=Math.Max(0,Math.Min(5000,v.Attack));release.Value=Math.Max(0,Math.Min(10000,v.Release));int i=sources.Items.IndexOf(v.Source??"");if(i>=0)sources.SelectedIndex=i;i=outputs.Items.IndexOf(v.Output??"");if(i>=0)outputs.SelectedIndex=i;}catch(Exception ex){state.Text="Réglages audio : "+ex.Message;}finally{loading918=false;}}
    void SaveAudio918(){if(loading918)return;try{var v=new AudioSettings918{CcMode941=ccMode941.Checked,CcNumber941=(int)ccNumber941.Value,Interval941=(int)interval941.Value,Channel=(int)channel.Value,Note=(int)note.Value,Threshold=(int)threshold.Value,Attack=(int)attack.Value,Release=(int)release.Value,Source=sources.SelectedItem as string??"",Output=outputs.SelectedItem as string??""};string p=AudioPath918();Directory.CreateDirectory(Path.GetDirectoryName(p));string temp=p+".tmp";File.WriteAllText(temp,new JavaScriptSerializer().Serialize(v));if(File.Exists(p))File.Replace(temp,p,null);else File.Move(temp,p);}catch(Exception ex){state.Text="Réglages audio non enregistrés : "+ex.Message;}}
    static void Setup(NumericUpDown n,int min,int max,int initial){n.Minimum=min;n.Maximum=max;n.Value=initial;}
    static void Row(TableLayoutPanel grid,int row,string label,Control c,Control extra){
      grid.RowStyles.Add(new RowStyle(SizeType.Absolute,33));grid.Controls.Add(new Label{Text=label,AutoSize=true,Margin=new Padding(0,6,0,0)},0,row);
      c.Dock=DockStyle.Fill;grid.Controls.Add(c,1,row);if(extra!=null){extra.Dock=DockStyle.Fill;grid.Controls.Add(extra,2,row);}
    }
    void Reload(){
      loading918=true;Stop();string selected=sources.SelectedItem as string,output=outputs.SelectedItem as string;
      DisposeEndpoints();sources.Items.Clear();outputs.Items.Clear();
      try{endpoints=AudioEndpoint.Enumerate();foreach(var e in endpoints)sources.Items.Add(e.Name);outputs.Items.Add("Tee connecté — USB série");
        if(sources.Items.Count>0)sources.SelectedIndex=Math.Max(0,sources.Items.IndexOf(selected??""));
        int index=outputs.Items.IndexOf(output??"");if(index<0)for(int i=0;i<outputs.Items.Count;i++)if(outputs.Items[i].ToString().IndexOf("Teensy",StringComparison.OrdinalIgnoreCase)>=0){index=i;break;}
        if(outputs.Items.Count>0)outputs.SelectedIndex=Math.Max(0,index);
      }catch(Exception ex){state.Text=ex.Message;}finally{loading918=false;}
    }
    void Start(){
      if(sources.SelectedIndex<0||outputs.SelectedIndex<0){state.Text="Choisir les ports";return;}
      try{if(!connected())throw new InvalidOperationException("Connecter le Tee dans la fenêtre principale");gate.Reset();lastCc941=-1;nextCc941=0;running=true;run.Text="Arrêter";state.Text="En attente";}
      catch(Exception ex){Stop();state.Text=ex.Message;}
    }
    void NoteOff(){if(!held)return;send("OFF",heldChannel,heldNote,0);held=false;}
    void Stop(){
      try{NoteOff();}catch{}finally{held=false;running=false;gate.Reset();}
      run.Text="Démarrer";state.Text="Arrêté";
    }
    void TickMeter(){
      try{
        long now=clock.ElapsedMilliseconds;if(held&&now>=offAt)NoteOff();
        double bpm=tempo();if(double.IsNaN(bpm)||double.IsInfinity(bpm)||bpm<30||bpm>300)bpm=115;
        tempoLabel.Text=bpm.ToString("0.#")+" BPM";
        if(sources.SelectedIndex<0||sources.SelectedIndex>=endpoints.Count)return;
        float peak=endpoints[sources.SelectedIndex].Peak;meter.Value=Math.Max(0,Math.Min(100,(int)Math.Round(peak*100)));value.Text=meter.Value+" %";
        if(!running)return;if(!connected())throw new InvalidOperationException("Liaison Tee perdue");
        if(ccMode941.Checked){int cc=Math.Max(0,Math.Min(127,(int)Math.Round(peak*127)));if(now>=nextCc941){nextCc941=now+(int)interval941.Value;if(cc!=lastCc941){send("CC",(int)channel.Value,(int)ccNumber941.Value,cc);lastCc941=cc;}}state.Text="CC "+ccNumber941.Value+" = "+cc;return;}if(gate.Tick(now,peak*100>(float)threshold.Value,(int)attack.Value,(int)release.Value,bpm)){
          NoteOff();heldChannel=(int)channel.Value;heldNote=(int)note.Value;
          send("NOTE",heldChannel,heldNote,100);held=true;offAt=now+100;
        }
        state.Text=gate.Active?"Actif":"En attente";
      }catch(Exception ex){Stop();state.Text="Erreur : "+ex.Message;}
    }
    void DisposeEndpoints(){foreach(var e in endpoints)e.Dispose();endpoints.Clear();}
  }
}

