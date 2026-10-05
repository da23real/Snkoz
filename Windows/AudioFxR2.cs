using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Synkoz {
 public sealed class EqBandR2 {
  public bool Enabled{get;set;}public double Frequency{get;set;}public double GainDb{get;set;}public double Q{get;set;}
  public EqBandR2(){Enabled=true;Frequency=1000;Q=1;}
  public override string ToString(){return (Enabled?"● ":"○ ")+Frequency+" Hz / "+GainDb+" dB";}
 }
 public sealed class AudioFxR2 { public bool GainEnabled941;
  [DisplayName("Traitements audio actifs")]public bool Enabled{get;set;}
  [DisplayName("Gain d’entrée (dB)")]public double GainDb{get;set;}
  [DisplayName("Bandes de l’égaliseur")]public List<EqBandR2> Bands{get;set;}
  [DisplayName("Compresseur actif")]public bool Compressor{get;set;}
  [DisplayName("Seuil compresseur (dB)")]public double ThresholdDb{get;set;}
  [DisplayName("Ratio compresseur")]public double Ratio{get;set;}
  [DisplayName("Attaque (ms)")]public double AttackMs{get;set;}
  [DisplayName("Relâchement (ms)")]public double ReleaseMs{get;set;}
  [DisplayName("Gain après compression (dB)")]public double MakeupDb{get;set;}
  [DisplayName("Limiteur actif")]public bool Limiter{get;set;}
  [DisplayName("Plafond limiteur (dB)")]public double LimitDb{get;set;}
  public AudioFxR2(){ThresholdDb=-18;Ratio=3;AttackMs=20;ReleaseMs=250;LimitDb=-1;Bands=new List<EqBandR2>();}
  static string F(double x){return x.ToString("0.########",Engine.Inv);}
  static void Range(double x,double a,double b,string name){if(double.IsNaN(x)||double.IsInfinity(x)||x<a||x>b)throw new Exception(name+" : "+a+".."+b);}
  public void Validate(){Range(GainDb,-60,24,"Gain");Range(ThresholdDb,-60,0,"Seuil");Range(Ratio,1,20,"Ratio");Range(AttackMs,.01,2000,"Attaque");Range(ReleaseMs,.01,9000,"Relâchement");Range(MakeupDb,0,24,"Gain de compensation");Range(LimitDb,-24,0,"Plafond");if(Bands==null)throw new Exception("Liste EQ absente.");foreach(var b in Bands){Range(b.Frequency,20,20000,"Fréquence");Range(b.GainDb,-24,24,"Gain EQ");Range(b.Q,.1,20,"Q");}}
  public static string Filter(AudioFxR2 fx){if(fx==null||!fx.Enabled)return "";fx.Validate();var chain=new List<string>();chain.Add("volume="+F(fx.GainDb)+"dB");foreach(var band in fx.Bands)if(band.Enabled)chain.Add("equalizer=f="+F(band.Frequency)+":t=q:w="+F(band.Q)+":g="+F(band.GainDb));if(fx.Compressor)chain.Add("acompressor=threshold="+F(Math.Pow(10,fx.ThresholdDb/20))+":ratio="+F(fx.Ratio)+":attack="+F(fx.AttackMs)+":release="+F(fx.ReleaseMs)+":makeup="+F(Math.Pow(10,fx.MakeupDb/20)));if(fx.Limiter)chain.Add("alimiter=limit="+F(Math.Pow(10,fx.LimitDb/20))+":level=false:latency=true");return ","+string.Join(",",chain);}
 }
 public sealed class LegacyAudioFormR2:Form {
  AudioFxR2 draft;readonly PropertyGrid grid=new PropertyGrid();readonly Label status=new Label();
  public LegacyAudioFormR2(Montage m,int track){draft=UiR2.Copy(m.Tracks[track].Fx.Audio??new AudioFxR2());Text="Audio FX — "+Sources936.Label(m.Tracks[track]);UiR2.Fit(this,760,565);var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=3};layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,78));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,42));Controls.Add(layout);grid.Dock=DockStyle.Fill;grid.SelectedObject=draft;layout.Controls.Add(grid,0,0);var actions=new FlowLayoutPanel{Dock=DockStyle.Fill};Action<string,Action> button=(name,action)=>{var b=new Button{Text=name,AutoSize=true};b.Click+=delegate{try{action();}catch(Exception ex){status.Text=ex.Message;}};actions.Controls.Add(b);};
   Action apply=()=>{draft.Validate();m.Tracks[track].Fx.Audio=UiR2.Copy(draft);m.Save();status.Text="Audio mémorisé. Ordre : gain → EQ → compresseur → limiteur.";};button("Appliquer",apply);button("Audio preview FX/latence",()=>{using(var f=new PreviewAudioR3(m,track,m.Tracks[track].Fx)){if(f.ShowDialog(this)==DialogResult.OK)m.Save();}});button("Preview piste",()=>{apply();var p=new Preview936();p.Show();p.Render(m,track);});
   button("Sauver preset",()=>{draft.Validate();using(var d=new SaveFileDialog{InitialDirectory=FoldersR2.Get("audio"),Filter="Preset audio Snkoz|*.snkozaudio",FileName="Audio.snkozaudio"})if(d.ShowDialog(this)==DialogResult.OK){Store934.Save(d.FileName,draft);FoldersR2.Set("audio",Path.GetDirectoryName(d.FileName));}});
   button("Charger preset",()=>{using(var d=new OpenFileDialog{InitialDirectory=FoldersR2.Get("audio"),Filter="Preset audio Snkoz|*.snkozaudio"})if(d.ShowDialog(this)==DialogResult.OK){var loaded=Store934.Load<AudioFxR2>(d.FileName);loaded.Validate();draft=loaded;grid.SelectedObject=draft;FoldersR2.Set("audio",Path.GetDirectoryName(d.FileName));}});button("Fermer",()=>Close());layout.Controls.Add(actions,0,1);status.Text="Effets intégrés sans MIDI. Bandes EQ : ouvrir la liste […] pour les éditer. Preset = chaîne complète.";status.Dock=DockStyle.Fill;layout.Controls.Add(status,0,2);
  }
 }
}

