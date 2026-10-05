using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Synkoz {
 sealed class Player936:AxHost {
  public Player936():base("6BF52A52-394A-11D3-B153-00C04F79FAA6"){}
  public void Open(string path){dynamic p=GetOcx();p.settings.autoStart=true;p.uiMode="none";p.stretchToFit=true;p.enableContextMenu=false;p.URL=path;}
  public void Play(){dynamic p=GetOcx();p.controls.play();}
  public void Pause(){dynamic p=GetOcx();p.controls.pause();}
  public double Position{get{try{dynamic p=GetOcx();return (double)p.controls.currentPosition;}catch{return 0;}}set{dynamic p=GetOcx();p.controls.currentPosition=value;}}
  public void Stop(){try{dynamic p=GetOcx();p.controls.stop();p.close();}catch{}}
 }
 public sealed partial class Preview936:Form {
  Player936 player;readonly Label status=new Label(),time=new Label();readonly SeekBar942 seek=new SeekBar942();readonly Button play=new Button{Text="▶ Lecture",AutoSize=true},pause=new Button{Text="Ⅱ Pause",AutoSize=true},cancelButton=new Button{Text="Annuler rendu",AutoSize=true};readonly System.Windows.Forms.Timer clock=new System.Windows.Forms.Timer{Interval=200};CancellationTokenSource cancellation;bool rendering,dragging,closeAfter;double duration,timelineOrigin;string folder;
  public Preview936(){Text="Preview — Snkoz";UiR2.Fit(this,960,565);var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=4};layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,35));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,40));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,38));Controls.Add(layout);
   var host=new Panel{Dock=DockStyle.Fill,BackColor=Color.Black};layout.Controls.Add(host,0,0);try{player=new Player936{Dock=DockStyle.Fill};((ISupportInitialize)player).BeginInit();host.Controls.Add(player);((ISupportInitialize)player).EndInit();}catch(Exception ex){status.Text="Lecteur indisponible : "+ex.Message;}
   seek.Dock=DockStyle.Fill;seek.Minimum=0;seek.Maximum=10000;seek.TickStyle=TickStyle.None;seek.MouseDown+=delegate{dragging=true;Seek();};seek.MouseMove+=delegate(object s,MouseEventArgs e){if(dragging)Seek();};seek.MouseUp+=delegate{dragging=false;Seek();};seek.KeyUp+=delegate{Seek();};layout.Controls.Add(seek,0,1);
   var buttons=new FlowLayoutPanel{Dock=DockStyle.Fill};play.Click+=delegate{ControlPlayer(()=>player.Play());};pause.Click+=delegate{ControlPlayer(()=>player.Pause());};cancelButton.Click+=delegate{if(cancellation!=null)cancellation.Cancel();};buttons.Controls.AddRange(new Control[]{play,pause,cancelButton,time});time.AutoSize=true;time.Margin=new Padding(12,8,0,0);layout.Controls.Add(buttons,0,2);status.Dock=DockStyle.Fill;layout.Controls.Add(status,0,3);SetReady(false);
   clock.Tick+=delegate{if(player==null||dragging)return;double position=player.Position;seek.Value=(int)Math.Max(0,Math.Min(10000,position/Math.Max(.001,duration)*10000));time.Text="Montage "+(position+timelineOrigin).ToString("+0.000;-0.000;0.000")+" s · "+position.ToString("F1")+" / "+duration.ToString("F1")+" s";};
   FormClosing+=delegate(object sender,FormClosingEventArgs e){if(rendering){closeAfter=true;cancellation.Cancel();status.Text="Annulation en cours…";e.Cancel=true;}else{clock.Stop();if(player!=null)player.Stop();}};FormClosed+=delegate{clock.Dispose();if(cancellation!=null)cancellation.Dispose();Cleanup();};
  }
  void ControlPlayer(Action action){try{if(player!=null)action();}catch(Exception ex){status.Text=ex.Message;}}
  void Seek(){ControlPlayer(()=>player.Position=duration*seek.Value/10000);}
  void SetReady(bool ready){play.Enabled=pause.Enabled=seek.Enabled=ready;cancelButton.Enabled=rendering;}
  void Cleanup(){if(folder!=null)try{Directory.Delete(folder,true);}catch{}folder=null;}
  public async void Render(Montage model,int track,double origin=0,double limit=-1,int height=540,bool referenceAudio=false){if(rendering){status.Text="Un rendu est en cours. Annuler avant de relancer.";return;}rendering=true;closeAfter=false;clock.Stop();if(player!=null)player.Stop();SetReady(false);Cleanup();if(cancellation!=null)cancellation.Dispose();cancellation=new CancellationTokenSource();var token=cancellation.Token;var snapshot=Engine.Snapshot936(model);timelineOrigin=origin;duration=limit<0?snapshot.Duration-origin:limit;
   folder=Path.Combine(Engine.Root,"Preview",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);var t=snapshot.Tracks[track];string path=Path.Combine(folder,"preview"+(t.Video!=null?".mp4":t.Format=="MP3"?".mp3":".wav"));Text="Preview — "+Sources936.Label(t);status.Text=limit<0?"Préparation de la piste complète…":"Préparation de l’extrait FX…";
   try{await Task.Run(()=>Engine.Export936(snapshot,track,path,token,percent=>{if(!IsDisposed&&IsHandleCreated)BeginInvoke((Action)(()=>status.Text="Rendu : "+percent+" %"));},origin,limit,height,referenceAudio));token.ThrowIfCancellationRequested();if(IsDisposed)return;if(player==null)throw new Exception("Lecteur Windows Media absent.");player.Open(path);SetReady(true);clock.Start();status.Text=origin<0?"Zéro musical à "+(-origin).ToString("F3")+" s dans cet extrait · temps négatifs = avant le départ.":"Lecture automatique · temps de prise référencé au zéro musical.";}
   catch(Exception ex){status.Text=token.IsCancellationRequested?"Rendu annulé.":"Preview : "+ex.GetBaseException().Message;}finally{rendering=false;if(IsDisposed)Cleanup();else{cancelButton.Enabled=false;if(closeAfter)Close();}}
  }
 }
}
