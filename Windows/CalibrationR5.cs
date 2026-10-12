using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Synkoz {
 public sealed class DelayEditorR5:UserControl {
  readonly ComboBox unit=new Synkoz.ScrollComboR8{FormattingEnabled=true,DropDownStyle=ComboBoxStyle.DropDownList,Width=115};
  readonly ComboBox step=new Synkoz.ScrollComboR8{FormattingEnabled=true,DropDownStyle=ComboBoxStyle.DropDownList,Width=130};
  readonly NumericUpDown value=new Synkoz.ScrollNumberR8{Minimum=-3600000,Maximum=3600000,Width=120};
  readonly Label result=new Label{AutoSize=true};readonly Music936 music;readonly Func<double> cursor;readonly Action<double> change;
  double seconds,anchor;bool filling;readonly double[] steps={1.0/96,.0625,.125,.25,.5,1,2,4};
  public DelayEditorR5(string title,Music936 clock,Func<double> time,Action<double> changed){music=clock.Available?clock:new Music936(new Montage{Beats=new System.Collections.Generic.List<Beat936>{new Beat936{Time=0,Beat=0},new Beat936{Time=2,Beat=4}}});cursor=time;change=changed;Dock=DockStyle.Fill;Margin=Padding.Empty;
   var row=new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=false};Controls.Add(row);
   row.Controls.Add(new Label{Text=title,Width=125,Margin=new Padding(3,6,3,0)});unit.Items.AddRange(new object[]{"Millisecondes","Images (30 fps)","Musical (noires)"});unit.SelectedIndex=2;unit.Visible=false;
   step.Items.AddRange(new object[]{"Tick (96/noire)","Quadruple croche","Triple croche","Double croche","Croche","Noire","Blanche","Mesure (4/4)"});step.SelectedIndex=2;
   row.Controls.Add(unit);row.Controls.Add(value);row.Controls.Add(step);row.Controls.Add(result);
   unit.SelectedIndexChanged+=delegate{if(unit.SelectedIndex==2&&!music.Available){unit.SelectedIndex=0;return;}anchor=cursor();RefreshValue();};
   step.SelectedIndexChanged+=delegate{RefreshValue();};value.ValueChanged+=delegate{if(filling)return;try{double n=(double)value.Value;double s=unit.SelectedIndex==1?n/30:unit.SelectedIndex==2?CalibrationTimeR5.Musical(music,anchor,n):n/1000;if(Math.Abs(s)>3600){RefreshValue();return;}seconds=s;change(s);ShowResult();}catch(Exception ex){result.Text=ex.Message;}};
   RefreshValue();
  }
  void ShowResult(){result.Text=CalibrationTimeR5.Beats(music,anchor,seconds).ToString("+0.######;-0.######;0")+" noires";}
  void RefreshValue(){filling=true;step.Visible=unit.SelectedIndex==2;value.DecimalPlaces=unit.SelectedIndex==1?0:unit.SelectedIndex==2?6:3;value.Increment=unit.SelectedIndex==2?(decimal)steps[step.SelectedIndex]:1;
   double n=unit.SelectedIndex==1?seconds*30:unit.SelectedIndex==2?CalibrationTimeR5.Beats(music,anchor,seconds):seconds*1000;value.Value=Math.Max(value.Minimum,Math.Min(value.Maximum,(decimal)n));filling=false;ShowResult();}
  public void Set(double s){seconds=s;anchor=cursor();RefreshValue();}
 }
 public sealed class CalibrationCanvasR5:Control {
  public Montage Model;public WaveR5 Wave;public double Start=-1,Span=8,CursorTime,AudioShift,Gain=1;public int Channel=-1;public bool Starts=true,Notes=true,CC;
  public string WaveMessage="Waveform : sélectionner une piste audio";public Action<double> Seek;readonly ToolTip tip=new ToolTip();
  WaveR5 peakSource;double peakStart,peakSpan,peakShift;int peakWidth;float[,] peakLow,peakHigh;
  void PreparePeaks(){if(Wave==null)return;if(peakSource==Wave&&peakStart==Start&&peakSpan==Span&&peakShift==AudioShift&&peakWidth==Width)return;
   peakSource=Wave;peakStart=Start;peakSpan=Span;peakShift=AudioShift;peakWidth=Width;peakLow=new float[2,Math.Max(1,Width)];peakHigh=new float[2,Math.Max(1,Width)];double moved=AudioShift-Wave.Shift;
   for(int c=0;c<2;c++)for(int x=74;x<Width-6;x++){float lo,hi;CalibrationMediaR5.Peaks(Wave,Time(x)-moved,Time(x+1)-moved,c,out lo,out hi);peakLow[c,x]=lo;peakHigh[c,x]=hi;}
  }
  public CalibrationCanvasR5(){DoubleBuffered=true;BackColor=Color.FromArgb(27,31,40);ForeColor=Color.White;}
  public float X(double t){return (float)(74+(t-Start)/Span*Math.Max(1,Width-80));}
  double Time(int x){return Start+(x-74)*Span/Math.Max(1,Width-80);}
  protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.X>=74&&Seek!=null)Seek(Time(e.X));}
  protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(Model==null)return;double time=Time(e.X);var n=Model.Notes.FirstOrDefault(a=>(Channel<0||a.Channel==Channel)&&time>=a.Start&&time<=a.Start+Math.Max(a.Duration,Span*5/Math.Max(1,Width-80)));tip.SetToolTip(this,time.ToString("F6")+" s"+(n==null?"":" · Ch "+(n.Channel+1)+" · note "+n.Note+" · vélocité "+n.Velocity+" · "+n.Start.ToString("F6")+" → "+(n.Start+n.Duration).ToString("F6")+" s"));}
  protected override void Dispose(bool disposing){if(disposing)tip.Dispose();base.Dispose(disposing);}
  protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;int ruler=24,waveBottom=Math.Max(90,Height/2),midiTop=waveBottom+24;
   g.DrawString("Audio G/D",Font,Brushes.LightCyan,2,30);g.DrawString("MIDI fixe",Font,Brushes.LightGreen,2,midiTop);
   double tick=Math.Pow(10,Math.Floor(Math.Log10(Span/8)));if(Span/tick>16)tick*=5;else if(Span/tick>10)tick*=2;
   var clip=g.Save();g.SetClip(new Rectangle(74,0,Math.Max(1,Width-80),Height));
   for(double t=Math.Ceiling(Start/tick)*tick;t<=Start+Span;t+=tick){float x=X(t);g.DrawLine(Pens.DimGray,x,ruler,x,Height);g.DrawString(t.ToString("0.###")+" s",Font,Brushes.Silver,x+2,1);}
   PreparePeaks();for(int channel=0;channel<2;channel++){float mid=ruler+(waveBottom-ruler)*(channel+.5f)/2,scale=(waveBottom-ruler)/4f-3;g.DrawLine(Pens.Gray,74,mid,Width,mid);
    if(Wave!=null)for(int x=74;x<Width-6;x++)g.DrawLine(Pens.Cyan,x,mid-(float)Math.Min(1,peakHigh[channel,x]*Gain)*scale,x,mid-(float)Math.Max(-1,peakLow[channel,x]*Gain)*scale);}
   if(Wave==null)g.DrawString(WaveMessage,Font,Brushes.Gray,90,45);
   g.DrawLine(Pens.Gray,74,waveBottom+6,Width,waveBottom+6);
   if(Model!=null){
    if(Starts){float x=X(0);g.DrawLine(Pens.Orange,x,18,x,Height);g.DrawString(Model.Beats.Count>=2?"Start / zéro musical":"Zéro montage (Start absent)",Font,Brushes.Orange,x+3,waveBottom+8);}
    var visible=Model.Notes.Where(n=>(Channel<0||n.Channel==Channel)&&n.Start<Start+Span&&n.Start+n.Duration>=Start).ToArray();
    if(Notes){int low=visible.Length==0?0:visible.Min(n=>n.Note),high=visible.Length==0?127:visible.Max(n=>n.Note);float lane=Math.Min(20,(Height-midiTop-5)/(float)Math.Max(1,high-low+1));
     foreach(var n in visible){float x=X(n.Start),end=X(n.Start+n.Duration),y=midiTop+(high-n.Note)*lane;g.FillRectangle(Brushes.MediumSeaGreen,x,y,Math.Max(2,end-x),Math.Max(2,lane-2));g.DrawLine(Pens.LightGreen,x,y,x,y+Math.Max(2,lane-2));if(lane>=12&&end-x>=35)g.DrawString(n.Note+" / Ch"+(n.Channel+1),Font,Brushes.Black,x+2,y);}
     foreach(var n in Model.Triggers.Where(n=>(Channel<0||n.Channel==Channel)&&n.Time>=Start&&n.Time<=Start+Span)){float x=X(n.Time);g.DrawLine(Pens.Lime,x,waveBottom+7,x,waveBottom+13);}
    }
    if(CC)foreach(var c in Model.CC.Where(c=>(Channel<0||c.Channel==Channel)&&c.Time>=Start&&c.Time<Start+Span)){float x=X(c.Time);g.DrawLine(Pens.Violet,x,midiTop,x,Height);g.DrawString("CC"+c.Number+"="+c.Value,Font,Brushes.Violet,x+2,Height-17);}
    if(visible.Length==0&&Notes)g.DrawString("Aucune note sur cette zone / ce canal",Font,Brushes.Gray,90,midiTop+10);
   }
   float cx=X(CursorTime);g.DrawLine(Pens.Yellow,cx,0,cx,Height);g.Restore(clip);
  }
 }
 public sealed partial class CalibrationR5:Form {
  readonly Montage original,draft;readonly Music936 music;readonly ComboBox videos=Combo(),audios=Combo(),channels=Combo();
  readonly PictureBox picture=new PictureBox{Dock=DockStyle.Fill,SizeMode=PictureBoxSizeMode.Zoom,BackColor=Color.Black};
  readonly CalibrationCanvasR5 canvas=new CalibrationCanvasR5{Dock=DockStyle.Fill};
  readonly Label status=new Label{Dock=DockStyle.Fill},position=new Label{AutoSize=true};readonly NumericUpDown seek,span;
  readonly DelayEditorR5 videoDelay,audioDelay;CancellationTokenSource videoCancel,waveCancel;int videoRevision,waveRevision;FrameCacheR5 cache;bool filling,dirty,closed;long frame;double minimum;
  static ComboBox Combo(){return new Synkoz.ScrollComboR8{FormattingEnabled=true,DropDownStyle=ComboBoxStyle.DropDownList,Width=215};}
  sealed class Choice {public int Index;public string Name;public override string ToString(){return Name;}}
  int Selected(ComboBox c){return c.SelectedItem==null?-1:((Choice)c.SelectedItem).Index;}
  static Button Button(string text,Action action){var b=new Button{Text=text,AutoSize=true};b.Click+=delegate{action();};return b;}
  static Label Label(string s){return new Label{Text=s,AutoSize=true,Margin=new Padding(5,7,3,0)};}
  public CalibrationR5(Montage model,double initialPosition15=0){original=model;draft=Engine.Snapshot936(model);music=new Music936(draft);minimum=Math.Min(-2,PrerollR4.Minimum(draft));Text="Latence — vidéo / waveform / MIDI fixe";UiR2.Fit(this,1220,620);KeyPreview=true;
   var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=7};layout.RowStyles.Add(new RowStyle(SizeType.Absolute,68));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,62));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,36));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,190));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,32));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,36));Controls.Add(layout);
   var selects=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true};selects.Controls.Add(Label("Vidéo"));selects.Controls.Add(videos);selects.Controls.Add(Label("Audio"));selects.Controls.Add(audios);selects.Controls.Add(Label("MIDI de la prise"));selects.Controls.Add(channels);
   var starts=new CheckBox{Text="Start / zéro",Checked=true,AutoSize=true};var notes=new CheckBox{Text="Notes",Checked=true,AutoSize=true};var cc=new CheckBox{Text="CC",AutoSize=true};selects.SetFlowBreak(channels,true);selects.Controls.Add(starts);selects.Controls.Add(notes);selects.Controls.Add(cc);selects.Controls.Add(Label("Horloge MIDI masquée · + = retarder, − = avancer"));layout.Controls.Add(selects,0,0);
   var delays=new TableLayoutPanel{Dock=DockStyle.Fill,RowCount=2,ColumnCount=1};delays.RowStyles.Add(new RowStyle(SizeType.Percent,50));delays.RowStyles.Add(new RowStyle(SizeType.Percent,50));
   videoDelay=new DelayEditorR5("Décalage vidéo",music,()=>frame/30.0,s=>Offset(true,s));audioDelay=new DelayEditorR5("Décalage audio",music,()=>frame/30.0,s=>Offset(false,s));delays.Controls.Add(videoDelay,0,0);delays.Controls.Add(audioDelay,0,1);layout.Controls.Add(delays,0,1);layout.Controls.Add(picture,0,2);
   var nav=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true};nav.Controls.Add(Button("◀ Image",()=>Move(frame-1)));nav.Controls.Add(Button("Image ▶",()=>Move(frame+1)));nav.Controls.Add(Button("Start",()=>Move(0)));nav.Controls.Add(Button("Note ◀",()=>NextNote(-1)));nav.Controls.Add(Button("Note ▶",()=>NextNote(1)));
   seek=new Synkoz.ScrollNumberR8{Minimum=(decimal)(minimum-3600),Maximum=(decimal)Math.Max(1,draft.Duration+3600),DecimalPlaces=3,Increment=1,Width=100};seek.DecimalPlaces=6;seek.Increment=1m/96;nav.Controls.Add(Button("Lire 1 mesure / Stop",PlayExcerpt));nav.Controls.Add(seek);nav.Controls.Add(Label("noires"));nav.Controls.Add(position);layout.Controls.Add(nav,0,3);
   canvas.Model=draft;canvas.Seek=t=>Move(CalibrationTimeR5.Frame(t));canvas.Start=Math.Max(minimum,-1);layout.Controls.Add(canvas,0,4);
   var zoom=new FlowLayoutPanel{Dock=DockStyle.Fill};zoom.Controls.Add(Button("◀ Zone",()=>Pan(-.75)));zoom.Controls.Add(Button("Zone ▶",()=>Pan(.75)));zoom.Controls.Add(Button("Zoom +",()=>Zoom(.5)));zoom.Controls.Add(Button("Zoom −",()=>Zoom(2)));zoom.Controls.Add(Label("Largeur (noires)"));span=new Synkoz.ScrollNumberR8{Minimum=.01m,Maximum=10000,DecimalPlaces=6,Increment=1m/96,Value=(decimal)MusicalTimeR11.Beats(draft,8),Width=70};zoom.Controls.Add(span);zoom.Controls.Add(Label("Amplitude visuelle"));var gain=new Synkoz.ScrollComboR8{FormattingEnabled=true,DropDownStyle=ComboBoxStyle.DropDownList,Width=65};gain.Items.AddRange(new object[]{"×1","×2","×4","×8"});gain.SelectedIndex=0;zoom.Controls.Add(gain);layout.Controls.Add(zoom,0,5);
   var footer=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=3};footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,110));footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,100));footer.Controls.Add(status,0,0);footer.Controls.Add(Button("Valider",Save),1,0);footer.Controls.Add(Button("Fermer",Close),2,0);layout.Controls.Add(footer,0,6);
   videos.Items.Add(new Choice{Index=-1,Name="Aucune vidéo"});audios.Items.Add(new Choice{Index=-1,Name="Aucun audio"});for(int i=0;i<draft.Tracks.Count;i++){string name=(i+1)+" — "+Sources936.Label(draft.Tracks[i]);if(draft.Tracks[i].Video!=null&&!draft.Tracks[i].Fx.Graphic)videos.Items.Add(new Choice{Index=i,Name=name});if(Engine.HasAudio(draft.Tracks[i]))audios.Items.Add(new Choice{Index=i,Name=name});}
   channels.Items.Add(new Choice{Index=-1,Name="Tous les canaux"});for(int c=0;c<16;c++)channels.Items.Add(new Choice{Index=c,Name="Canal "+(c+1)});
   videos.SelectedIndex=Math.Min(1,videos.Items.Count-1);audios.SelectedIndex=Math.Min(1,audios.Items.Count-1);channels.SelectedIndex=0;
   videos.SelectedIndexChanged+=delegate{Selection(true);};audios.SelectedIndexChanged+=delegate{Selection(false);};channels.SelectedIndexChanged+=delegate{canvas.Channel=Selected(channels);canvas.Invalidate();};
   starts.CheckedChanged+=delegate{canvas.Starts=starts.Checked;canvas.Invalidate();};notes.CheckedChanged+=delegate{canvas.Notes=notes.Checked;canvas.Invalidate();};cc.CheckedChanged+=delegate{canvas.CC=cc.Checked;canvas.Invalidate();};
   seek.ValueChanged+=delegate{if(!filling)Move(CalibrationTimeR5.Frame(MusicalTimeR11.Time(draft,(double)seek.Value)));};span.ValueChanged+=delegate{canvas.Span=MusicalTimeR11.Seconds(draft,(double)span.Value,canvas.Start);RefreshWave();canvas.Invalidate();};gain.SelectedIndexChanged+=delegate{canvas.Gain=Math.Pow(2,gain.SelectedIndex);canvas.Invalidate();};
   Shown+=delegate{Selection(true);Selection(false);Move(CalibrationTimeR5.Frame(initialPosition15));};InitPlayback();FormClosing+=OnClosing;FormClosed+=delegate{closed=true;Cancel(ref videoCancel);Cancel(ref waveCancel);if(picture.Image!=null)picture.Image.Dispose();};
  }
  static void Cancel(ref CancellationTokenSource c){if(c!=null){c.Cancel();c=null;}}
  void Selection(bool video){StopPlayback();int index=Selected(video?videos:audios);var editor=video?videoDelay:audioDelay;editor.Enabled=index>=0;if(index>=0)editor.Set(video?draft.Tracks[index].Fx.Shift:draft.Tracks[index].Fx.AudioShift);if(video){cache=null;RefreshVideo();}else {canvas.Wave=null;canvas.Invalidate();RefreshWave();}}
  void Offset(bool video,double seconds){StopPlayback();int index=Selected(video?videos:audios);if(index<0)return;CalibrationTimeR5.Set(draft.Tracks[index].Fx,video,seconds);dirty=true;status.Text="Décalage modifié — Valider pour mémoriser.";if(video){cache=null;RefreshVideo();}else{canvas.AudioShift=seconds;canvas.Invalidate();RefreshWave();}}
  public new void Move(long requested){StopPlayback();frame=Math.Max(CalibrationTimeR5.Frame(minimum-3600),Math.Min(CalibrationTimeR5.Frame(draft.Duration+3600),requested));double time=frame/30.0;filling=true;seek.Value=Math.Max(seek.Minimum,Math.Min(seek.Maximum,(decimal)MusicalTimeR11.Beat(draft,time)));filling=false;canvas.CursorTime=time;int vi=Selected(videos),ai=Selected(audios);if(vi>=0)videoDelay.Set(draft.Tracks[vi].Fx.Shift);if(ai>=0)audioDelay.Set(draft.Tracks[ai].Fx.AudioShift);position.Text="Image "+frame+" · "+MusicalTimeR11.Position(draft,time);
   if(time<canvas.Start||time>=canvas.Start+canvas.Span){canvas.Start=time-canvas.Span/4;RefreshWave();}canvas.Invalidate();RefreshVideo();}
  void NextNote(int direction){double t=frame/30.0;var events=draft.Triggers.Count>0?draft.Triggers.Where(n=>canvas.Channel<0||n.Channel==canvas.Channel).Select(n=>n.Time):draft.Notes.Where(n=>canvas.Channel<0||n.Channel==canvas.Channel).Select(n=>n.Start);var choices=events.Where(n=>direction>0?CalibrationTimeR5.Frame(n)>frame:CalibrationTimeR5.Frame(n)<frame).OrderBy(n=>direction*n).ToArray();if(choices.Length>0)Move(CalibrationTimeR5.Frame(choices[0]));}
  void Pan(double fraction){canvas.Start+=canvas.Span*fraction;canvas.Start=Math.Max(minimum-3600,Math.Min(draft.Duration+3600-canvas.Span,canvas.Start));canvas.Invalidate();RefreshWave();}
  void Zoom(double factor){double next=Math.Max(.1,Math.Min(120,canvas.Span*factor));canvas.Start=frame/30.0-next/2;span.Value=Math.Max(span.Minimum,Math.Min(span.Maximum,(decimal)MusicalTimeR11.Beats(draft,next,canvas.Start)));}
  protected override bool ProcessCmdKey(ref Message msg,Keys key){return KeysR5(ref msg,key);}
  bool KeysR5(ref Message msg,Keys key){if(key==(Keys.Control|Keys.Left)){Move(frame-1);return true;}if(key==(Keys.Control|Keys.Right)){Move(frame+1);return true;}return base.ProcessCmdKey(ref msg,key);}
  void SetImage(Bitmap b){var old=picture.Image;picture.Image=b;if(old!=null)old.Dispose();}
  async void RefreshVideo(){int index=Selected(videos);if(index<0){Cancel(ref videoCancel);videoRevision++;SetImage(null);return;}if(cache!=null){var hit=cache.Get(frame);if(hit!=null){SetImage(hit);return;}}
   Cancel(ref videoCancel);var cancellation=new CancellationTokenSource();videoCancel=cancellation;int revision=++videoRevision;var token=cancellation.Token;long first=(long)Math.Floor(frame/60.0)*60;var snap=Engine.Snapshot936(draft);SetImage(null);status.Text="Décodage des images — Ctrl+← / Ctrl+→ pour avancer image par image…";
   try{var result=await Task.Run(()=>CalibrationMediaR5.Video(snap,index,first,60,token));if(closed||revision!=videoRevision)return;cache=result;SetImage(cache.Get(frame));status.Text="Images prêtes · MIDI fixe · média sans FX. Ctrl+← / Ctrl+→ : une image.";}catch(OperationCanceledException){}catch(Exception ex){if(!closed&&revision==videoRevision)status.Text="Vidéo : "+ex.Message;Synkoz.ErrorR8.Show(this,ex.Message,"Erreur");}finally{if(videoCancel==cancellation)videoCancel=null;cancellation.Dispose();}}
  async void RefreshWave(){int index=Selected(audios);Cancel(ref waveCancel);int revision=++waveRevision;if(index<0){canvas.Wave=null;canvas.WaveMessage="Waveform : sélectionner une piste audio";canvas.Invalidate();return;}canvas.WaveMessage="Chargement de la waveform…";canvas.Invalidate();canvas.AudioShift=draft.Tracks[index].Fx.AudioShift;var cancellation=new CancellationTokenSource();waveCancel=cancellation;var token=cancellation.Token;double start=canvas.Start,length=canvas.Span;var snap=Engine.Snapshot936(draft);
   try{var result=await Task.Run(()=>CalibrationMediaR5.Audio(snap,index,start,length,token));if(closed||revision!=waveRevision)return;canvas.Wave=result;canvas.Invalidate();}catch(OperationCanceledException){}catch(Exception ex){if(!closed&&revision==waveRevision){canvas.Wave=null;canvas.WaveMessage="Waveform indisponible : "+ex.Message;canvas.Invalidate();status.Text="Audio : "+ex.Message;}Synkoz.ErrorR8.Show(this,ex.Message,"Erreur");}finally{if(waveCancel==cancellation)waveCancel=null;cancellation.Dispose();}}
  void Save(){var reviewed=original.Tracks.Select(t=>t.LatencyA01!=null&&t.LatencyA01.Validated).ToArray();var old=original.Tracks.Select(t=>new[]{t.Fx.VideoDelayR5,t.Fx.AudioDelayR5}).ToArray();try{for(int i=0;i<draft.Tracks.Count;i++){original.Tracks[i].Fx.VideoDelayR5=draft.Tracks[i].Fx.VideoDelayR5;original.Tracks[i].Fx.AudioDelayR5=draft.Tracks[i].Fx.AudioDelayR5;}foreach(int index in new[]{Selected(videos),Selected(audios)}.Where(i=>i>=0).Distinct())if(original.Tracks[index].LatencyA01!=null)original.Tracks[index].LatencyA01.Validated=true;original.Save();dirty=false;status.Text="Décalages mémorisés ; pistes affichées validées, même sans changement de valeur.";}catch(Exception ex){for(int i=0;i<old.Length;i++){original.Tracks[i].Fx.VideoDelayR5=old[i][0];original.Tracks[i].Fx.AudioDelayR5=old[i][1];if(original.Tracks[i].LatencyA01!=null)original.Tracks[i].LatencyA01.Validated=reviewed[i];}Synkoz.ErrorR8.Show(this,ex.Message,"Enregistrement impossible");}}
  void OnClosing(object sender,FormClosingEventArgs e){if(dirty){var answer=Synkoz.ErrorR8.Show(this,"Mémoriser les décalages modifiés ?","Latence",MessageBoxButtons.YesNoCancel);if(answer==DialogResult.Cancel){e.Cancel=true;return;}if(answer==DialogResult.Yes){Save();if(dirty)e.Cancel=true;}}}
 }
}
