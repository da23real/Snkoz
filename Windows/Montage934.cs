using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Synkoz {
 public sealed class ClockMark { public uint Tick; public double Time; }
 public sealed class NoteEvent { public int Track940=-1; public double Time; public int Note,Velocity,Channel; public bool On; }
 public sealed class MidiSpan { public int Track940=-1; public double Start,Duration; public int Note,Velocity,Channel; }
 public sealed class MediaBlock {public bool Imported942,Sprite942;public double Fps942=8;public double X942=50,Y942=50,Size942=100;public string Link941="";public double EventOffset941;public bool Muted940;
  public string RawFile="";public double? RawVideoZero,RawAudioZero;public double RawDuration;
  public TrackFx936 Fx939; public double MediaOffset939; public int Track,Segment; public string File,Issue; public double Start,Duration;
  public bool Video,Audio;
 }
 public sealed class Montage {
  public System.Collections.Generic.List<Program940> Programs940=new System.Collections.Generic.List<Program940>();public TrackFx936 PreviewAudio939=new TrackFx936(); public int Version=9420; public List<Cc936> CC=new List<Cc936>();public List<Beat936> Beats=new List<Beat936>();public List<Trigger936> Triggers=new List<Trigger936>(); public int ReferenceAudio=-1;public double Duration;
  public List<Track> Tracks=new List<Track>();
  public List<MediaBlock> Blocks=new List<MediaBlock>();
  public List<MidiSpan> Notes=new List<MidiSpan>();
  public List<string> Warnings=new List<string>();
  [ScriptIgnore] public long RecoveryRevisionA01;[ScriptIgnore] public bool RecoveryDirtyA01;[ScriptIgnore] public DateTime RecoveryChangedA01;[ScriptIgnore] public string Folder;[ScriptIgnore] public string ProjectFile;
  [ScriptIgnore] public Func<int,Bitmap> LiveFrame939;[ScriptIgnore] public bool DirectMode939;[ScriptIgnore] public Action Save939; public void Save(){if(Save939!=null){Save939();return;}RecoveryDirtyA01=true;RecoveryRevisionA01++;RecoveryChangedA01=DateTime.UtcNow;}
  public static Montage Load(string folder){
   var p=Path.Combine(folder,"Montage.synkoz.json");
   if(File.Exists(p)){var m=Store934.Load<Montage>(p);if(m.Version!=934&&m.Version!=936&&m.Version!=9362&&m.Version!=9363&&m.Version!=9364&&m.Version!=9365&&m.Version!=9380&&m.Version!=9390&&m.Version!=9400&&m.Version!=9410&&m.Version!=9420&&m.Version!=9450)throw new Exception("Version de montage non prise en charge.");m.Folder=folder;foreach(var t in m.Tracks)if(t.Fx==null)t.Fx=new TrackFx936();BlockRules939.Migrate(m);return m;}
   // Import the previous version's folders; prefer trimmed media over the raw copy.
   var result=new Montage{Folder=folder};
   var files=Directory.GetFiles(folder).Where(f=>new[]{".mp4",".wav",".mp3"}.Contains(Path.GetExtension(f).ToLowerInvariant()));
   foreach(var f in files.OrderBy(f=>f)){
    if(f.Contains("_brut.")&&File.Exists(f.Replace("_brut.",".")))continue;
    bool video=Path.GetExtension(f).ToLowerInvariant()==".mp4";var info=Engine.ProbeMedia(f);
    int index=result.Tracks.Count;result.Tracks.Add(new Track{Video=video?new Device{Name="Vidéo importée",Video=true}:null,Audio=info.Audio?new Device{Name="Audio importé"}:null});
    result.Blocks.Add(new MediaBlock{Track=index,File=Path.GetFileName(f),Video=video,Audio=info.Audio,Duration=info.Duration});result.Duration=Math.Max(result.Duration,info.Duration);
   }
   if(result.Tracks.Count==0)throw new Exception("Aucun montage ou média reconnu dans ce dossier.");
   return result;
  }
 }
 public static class Store934 {
  static JavaScriptSerializer Serializer(){return new JavaScriptSerializer{MaxJsonLength=int.MaxValue,RecursionLimit=100};}
  public static T Load<T>(string path){return Serializer().Deserialize<T>(File.ReadAllText(path));}
  public static void Save(string path,object value){
   Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
   string temp=path+".tmp";File.WriteAllText(temp,Serializer().Serialize(value),System.Text.Encoding.UTF8);
   if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);
  }
  public static string MediaPath(string folder,string relative){
   string root=Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;
   string p=Path.GetFullPath(Path.Combine(root,relative));
   if(!p.StartsWith(root,StringComparison.OrdinalIgnoreCase))throw new Exception("Média extérieur au dossier de prise.");return p;
  }
 }
 public static class Cuts934 {
  public static ClockMark LastComplete(IEnumerable<ClockMark> marks,double music,double stop,uint musicTick){
   return marks.Where(m=>m.Time>music&&m.Time<=stop&&m.Tick>musicTick&&(m.Tick-musicTick)%1536==0).OrderBy(m=>m.Time).LastOrDefault();
  }
  public static List<MidiSpan> Notes(IEnumerable<NoteEvent> events,double start,double end,double offset){
   var result=new List<MidiSpan>();var held=new Dictionary<int,Queue<NoteEvent>>();
   Action<NoteEvent,double> close=(on,off)=>{double a=Math.Max(start,on.Time),b=Math.Min(end,off);if(b>a)result.Add(new MidiSpan{Start=offset+a-start,Duration=b-a,Note=on.Note,Velocity=on.Velocity,Channel=on.Channel,Track940=on.Track940});};
   foreach(var e in events.OrderBy(n=>n.Time)){
    if(e.Time>end)break;int key=(e.Track940+1)*2048+e.Channel*128+e.Note;Queue<NoteEvent> q;
    if(!held.TryGetValue(key,out q)){q=new Queue<NoteEvent>();held[key]=q;}
    if(e.On)q.Enqueue(e);else if(q.Count>0)close(q.Dequeue(),e.Time);
   }
   foreach(var q in held.Values)foreach(var e in q)close(e,end);
   return result;
  }
 }
 public sealed partial class MontagePanel:UserControl {
  readonly FlowLayoutPanel rows=new FlowLayoutPanel();readonly Label status=new Label();readonly Func<bool> occupied;
  public Func<string> TakeFolder936;public Montage Current{get;private set;}
  public MontagePanel(Func<bool> busy){occupied=busy;Dock=DockStyle.Fill;BackColor=Color.FromArgb(22,25,33);ForeColor=Color.WhiteSmoke;
   var bar=new FlowLayoutPanel{Dock=DockStyle.Top,Height=42};var open=new Button{Text="Ouvrir un dossier",AutoSize=true};
   open.Click+=delegate{if(occupied()){status.Text="Terminer la prise avant de charger un autre montage.";return;}using(var d=new FolderBrowserDialog{SelectedPath=FoldersR2.Get("montage")})if(d.ShowDialog()==DialogResult.OK)try{ShowMontage(Montage.Load(d.SelectedPath));FoldersR2.Set("montage",d.SelectedPath);}catch(Exception ex){status.Text=ex.Message;}};
   bar.Controls.Add(open);var folder=new Button{Text="Ouvrir les prises",AutoSize=true};folder.Click+=delegate{string p=Current!=null?Current.Folder:TakeFolder936==null?Preferences936.Load().DefaultTakeFolder:TakeFolder936();Directory.CreateDirectory(p);System.Diagnostics.Process.Start("explorer.exe",Engine.Q(p));};bar.Controls.Add(folder);var clear=new Button{Text="Nouveau montage",AutoSize=true};clear.Click+=delegate{if(occupied()){status.Text="Terminer la prise avant de vider le montage.";return;}QueueLeavingA01();Current=null;clipboardA01=null;selection940.Clear();undo940.Clear();cursor940=start940=0;RefreshNavigation940();Render();status.Text="Montage vidé ; fichiers conservés. Enregistrer crée une nouvelle prise.";};bar.Controls.Add(clear);AddToolsR3(bar);rows.Dock=DockStyle.Fill;rows.AutoScroll=true;rows.FlowDirection=FlowDirection.TopDown;rows.WrapContents=false;status.Dock=DockStyle.Bottom;status.Height=32;
   Controls.Add(rows);Controls.Add(bar);Controls.Add(status);InitMontage942(bar);InitA01(bar);Resize+=delegate{Render();};VisibleChanged+=delegate{if(Visible)Render();};
  }
  public void ShowMontage(Montage m){QueueLeavingA01();MidiBlocks945.Ensure(m);selection940.Clear();undo940.Clear();cursor940=start940=0;Current=m;clipboardA01=null;RefreshNavigation940();if(Visible)Render();status.Text=m.Warnings.Count>0?m.Warnings.Last():"Dernière prise — "+Path.GetFileName(m.Folder);}
  void Render(){if(!Visible)return;int savedScroll942=-rows.AutoScrollPosition.Y;while(rows.Controls.Count>0)rows.Controls[0].Dispose();if(Current==null)return;MidiBlocks945.Ensure(Current);
   int width=Math.Max(450,ClientSize.Width-34);for(int i=0;i<Current.Tracks.Count;i++){
    int track=i;if(Current.Tracks[track].Midi945)continue;var row=new Panel{Width=width,Height=102};row.Controls.Add(new Label{Text=(i+1)+" — "+Sources936.Label(Current.Tracks[i]),Location=new Point(0,4),Width=180,Height=32});
    var view=new BlocksView{Owner940=this,Model=Current,Track=i,Location=new Point(185,0),Size=new Size(width-225,70),EditBlock939=b=>EditBlock939(track,b),Changed939=()=>{Current.Save();Render();}};row.Controls.Add(view);BetweenTracks944(row,view.Left,view.Width);
    var fx=new Button{Text="FX du bloc",Location=new Point(0,40),Width=150,BackColor=Current.Blocks.Where(b=>b.Track==track).Any(b=>(b.Fx939??Current.Tracks[track].Fx).Effects.Any(x=>x.Enabled))?Color.Teal:Color.FromArgb(45,51,65),ForeColor=Color.White,Enabled=Current.Tracks[track].Video!=null};fx.Click+=delegate{var blocks=Current.Blocks.Where(b=>b.Track==track).ToArray();var chosen=selection940.Where(b=>b.Track==track).ToArray();if(chosen.Length>1){status.Text="FX : choisir un seul bloc.";return;}var block=chosen.FirstOrDefault()??BlockAt940(track);if(block!=null)EditBlock939(track,block);else status.Text="Placer le curseur dans le bloc à régler.";Render();};row.Controls.Add(fx);
    var rotate=new Button{Text="↻ "+Current.Tracks[track].Fx.Rotation+"°",Location=new Point(0,73),Width=80,Enabled=Current.Tracks[track].Video!=null};rotate.Click+=async delegate{rotate.Enabled=false;try{var snap=Engine.Snapshot936(Current);snap.Tracks[track].Fx.Rotation=0;snap.Tracks[track].Fx.MirrorX939=snap.Tracks[track].Fx.MirrorY939=false;double time=Current.Blocks.Where(b=>b.Track==track).Select(b=>b.Start).DefaultIfEmpty(0).First();using(var frame=await Task.Run(()=>FrameR2.Read(snap,track,time)))using(var f=new Orientation939(frame,Current.Tracks[track].Fx)){if(f.ShowDialog(FindForm())==DialogResult.OK){var fx939=Current.Tracks[track].Fx;fx939.Rotation=f.Rotation;fx939.MirrorX939=f.MirrorX;fx939.MirrorY939=f.MirrorY;Current.Save();}}}catch(Exception ex){status.Text=ex.Message;}finally{Render();}};row.Controls.Add(rotate);
    var audioFx=new Button{Text="Audio FX",Location=new Point(85,73),Width=90,Enabled=true};audioFx.BackColor=Current.Tracks[track].Fx.Audio.Enabled?Color.Teal:Color.FromArgb(45,51,65);audioFx.Click+=delegate{using(var f=new AudioFormR2(Current,track))f.ShowDialog(FindForm());Render();};row.Controls.Add(audioFx);
    var pv=new Button{Text="▶",Location=new Point(width-35,22),Width=30,Enabled=(Current.Tracks[track].Fx.Graphic||Current.Blocks.Any(b=>b.Track==track))};pv.Click+=delegate{Preview936(track);};tipsA01.SetToolTip(pv,Language945.T("Preview"));row.Controls.Add(pv);
    var export=new Button{Enabled=(Current.Tracks[track].Fx.Graphic||Current.Blocks.Any(b=>b.Track==track)),Text="Export",Location=new Point(width-80,22),Width=75};export.Click+=async(s,e)=>await Export(track,export);export.Dispose();rows.Controls.Add(row);
   }
   AddMidiRows940(width);rows.AutoScrollPosition=new Point(0,savedScroll942);
  }
  void PreviewFxR2(int track,Effect936 effect,int measures){if(occupied())return;try{var window=PreviewWindow940.ForTrack(Current,track,measures,effect);if(preview936==null||preview936.IsDisposed)preview936=new Preview936();if(!preview936.Visible)preview936.Show(Application.OpenForms.OfType<EffectsForm936>().FirstOrDefault() as Form??FindForm());preview936.Activate();preview936.Render(Current,track,window.Item1,window.Item2,Current.Tracks[track].Fx.PreviewHeight941,true);}catch(Exception ex){MessageBox.Show(FindForm(),ex.Message,"Preview FX");}}
  Preview936 preview936;void Preview936(int track){if(occupied()){status.Text="Terminer la prise avant un aperçu.";return;}if(preview936==null||preview936.IsDisposed)preview936=new Preview936();if(!preview936.Visible)preview936.Show(Application.OpenForms.OfType<EffectsForm936>().FirstOrDefault() as Form??FindForm());preview936.Activate();using(var range=new RangeFormR3(Current,false)){if(range.ShowDialog(FindForm())==DialogResult.OK)preview936.Render(Current,track,range.Origin,range.Length,range.HeightChoice,true);}}async Task Export(int track,Button button){
   if(occupied()){status.Text="Terminer la prise avant l'export.";return;}var model=Current;var t=model.Tracks[track];bool video=t.Video!=null;
   using(var d=new SaveFileDialog{InitialDirectory=FoldersR2.Get("export"),FileName="Piste_"+(track+1).ToString("00")+"_"+Sources936.FileLabel(t)+(video?(t.Fx.Graphic?".mov":".mp4"):t.Format=="MP3"?".mp3":".wav"),Filter=video?(t.Fx.Graphic?"MOV ProRes 4444 — transparence|*.mov":"Vidéo MP4|*.mp4"):t.Format=="MP3"?"Audio MP3|*.mp3":"Audio WAV|*.wav"}){
    if(d.ShowDialog()!=DialogResult.OK)return;FoldersR2.Set("export",Path.GetDirectoryName(d.FileName));button.Enabled=false;status.Text="Export de la piste "+(track+1)+"…";
    try{using(var options=new ExportOptions941(video&&!t.Fx.Graphic)){if(options.ShowDialog(FindForm())!=DialogResult.OK)return;var snap=Engine.Snapshot936(model);var job=new RenderJob941("Export piste "+(track+1));await job.Run((token,report)=>Task.Run(()=>PartialExport942.Run(job.Partial942,()=>HardwareExportR1.Track(snap,track,d.FileName,options.HeightChoice,options.EncoderChoice,token,report))));}}catch(Exception ex){status.Text="Export : "+ex.Message;}finally{if(!button.IsDisposed)button.Enabled=true;}
   }
  }
  void EditBlock939(int track,MediaBlock block){if(occupied())return;var local=BlockRules939.EditorModel(Current,track,block);using(var f=new EffectsForm936(local,track,(effect,measures)=>{block.Fx939=UiR2.Copy(local.Tracks[track].Fx);Current.Tracks[track].Fx.PreviewMeasures941=local.Tracks[track].Fx.PreviewMeasures941;Current.Tracks[track].Fx.PreviewHeight941=local.Tracks[track].Fx.PreviewHeight941;Current.Save();var window=PreviewWindow940.ForBlock(Current,track,block,measures,effect);var editor942=Application.OpenForms.OfType<EffectsForm936>().FirstOrDefault();if(editor942!=null)editor942.RenderPreview942(Current,track,window.Item1,window.Item2);}))f.ShowDialog(FindForm());block.Fx939=UiR2.Copy(local.Tracks[track].Fx);Current.Tracks[track].Fx.PreviewMeasures941=local.Tracks[track].Fx.PreviewMeasures941;Current.Tracks[track].Fx.PreviewHeight941=local.Tracks[track].Fx.PreviewHeight941;Current.Save();Render();}
  sealed class BlocksView:Control {
   Point dragDown942;MediaBlock[] dragSet942;bool dragging942;public MontagePanel Owner940;public Montage Model;public int Track;public Action<MediaBlock> EditBlock939;public Action Changed939;public BlocksView(){SetStyle(ControlStyles.Selectable,true);TabStop=true;DoubleBuffered=true;AllowDrop=true;DragEnter+=delegate(object s,DragEventArgs e){if(e.Data.GetDataPresent(DataFormats.FileDrop))e.Effect=DragDropEffects.Copy;};DragDrop+=delegate(object s,DragEventArgs e){var p=PointToClient(new Point(e.X,e.Y));Owner940.ImportFiles942((string[])e.Data.GetData(DataFormats.FileDrop),Track,Owner940.start940+p.X*Owner940.Span940/Math.Max(1,Width));};}
   protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);Focus();double time=Owner940.start940+e.X*Owner940.Span940/Math.Max(1,Width);var b=Model.Blocks.FirstOrDefault(x=>x.Track==Track&&time>=x.Start&&time<x.Start+x.Duration);if(e.Button==MouseButtons.Right){Owner940.Context942(this,e.Location,b,Track,time);return;}if(e.Button!=MouseButtons.Left)return;dragDown942=e.Location;dragging942=false;dragSet942=b!=null&&Owner940.selection940.Contains(b)?Owner940.selection940.ToArray():null;if(b==null){Owner940.SetCursor940(time);return;}if((ModifierKeys&Keys.Control)==0)Owner940.selection940.Clear();if(!Owner940.selection940.Add(b))Owner940.selection940.Remove(b);if(dragSet942==null)dragSet942=Owner940.selection940.ToArray();Capture=true;Owner940.RefreshNavigation940();if(e.Clicks==2)Owner940.FxSelection940(Track,b);}
   protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(Capture&&dragSet942!=null&&Math.Abs(e.X-dragDown942.X)>5){dragging942=true;Cursor=Cursors.SizeWE;Owner940.status.Text="Déplacement : "+((e.X-dragDown942.X)*Owner940.Span940/Math.Max(1,Width)).ToString("+0.000;-0.000;0")+" s";}}
   protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);Capture=false;Cursor=Cursors.Default;if(dragging942&&dragSet942!=null)try{Edit941.Move(Model,dragSet942,(e.X-dragDown942.X)*Owner940.Span940/Math.Max(1,Width));Owner940.selection940.Clear();foreach(var b in dragSet942)Owner940.selection940.Add(b);Owner940.Changed941("Blocs déplacés.");}catch(Exception ex){Owner940.status.Text=ex.Message;}dragging942=false;dragSet942=null;}


   protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);double scale=Width/Owner940.Span940;e.Graphics.Clear(Color.FromArgb(35,39,49));
    if(!Model.Tracks[Track].Midi945)TimelineView940.Grid(e.Graphics,Font,Model,ClientRectangle,Owner940.start940,Owner940.Span940,false);foreach(var b in Model.Blocks.Where(b=>b.Track==Track)){
     var r=new Rectangle((int)((b.Start-Owner940.start940)*scale),0,Math.Max(2,(int)(b.Duration*scale)),Height-2);using(var brush=new SolidBrush(b.Muted940?Color.FromArgb(65,65,65):string.IsNullOrEmpty(b.Issue)?Color.SteelBlue:Color.Firebrick))e.Graphics.FillRectangle(brush,r);
     if(Owner940.selection940.Contains(b))e.Graphics.DrawRectangle(Pens.Cyan,r);e.Graphics.DrawString((b.Muted940?"MUTE · ":"")+(Model.Tracks[Track].Midi945?"MIDI ":(Language945.English?"FX · Take ":"FX · Prise "))+b.Segment+" · "+b.Duration.ToString("F2")+" s"+(string.IsNullOrEmpty(b.Issue)?"":" · incomplet"),Font,Brushes.White,r);
    }
    float x=(float)((Owner940.cursor940-Owner940.start940)*scale);e.Graphics.DrawLine(Pens.OrangeRed,x,0,x,Height);
   }
  }
  sealed class NotesView:Control {
   public Montage Model;public NotesView(){DoubleBuffered=true;BackColor=Color.FromArgb(30,33,42);}
   protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);if(Model.Notes.Count==0){e.Graphics.DrawString("Aucune note reçue",Font,Brushes.Gray,10,15);return;}
    int low=Model.Notes.Min(n=>n.Note),high=Model.Notes.Max(n=>n.Note);float h=Math.Min(12,(Height-12)/(float)Math.Max(1,high-low+1));double scale=Width/Math.Max(1,Model.Duration);
    foreach(var n in Model.Notes)e.Graphics.FillRectangle(Brushes.MediumAquamarine,(float)(n.Start*scale),5+(high-n.Note)*h,Math.Max(2,(float)(n.Duration*scale)),Math.Max(2,h-1));
   }
  }
 }
}

