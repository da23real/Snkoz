using System;using System.Drawing;using System.Windows.Forms;
namespace Synkoz {
 static class PreviewFramesR10 {public static double Step(double position,int direction,double duration){double frame=Math.Round(position*30);return Math.Max(0,Math.Min(Math.Max(0,Math.Ceiling(duration*30)-1),frame+direction))/30;}}
 public sealed partial class Preview936 {
  public Action ExitLargeR10;bool playingR10;Image playImageR10,pauseImageR10;PreviewKeysR10 keysR10;
  sealed class PreviewKeysR10:IMessageFilter {readonly Preview936 owner;public PreviewKeysR10(Preview936 form){owner=form;}public bool PreFilterMessage(ref Message m){if(owner.IsDisposed||!owner.Visible||!owner.ContainsFocus)return false;if(m.Msg!=0x100&&m.Msg!=0x104)return false;Keys key=(Keys)m.WParam.ToInt32()|Control.ModifierKeys;return owner.HandleKeyR10(key);}}
  void InitKeysR10(FlowLayoutPanel buttons){buttons.Controls.Remove(pause);pause.Visible=false;play.AccessibleName="Lecture / Pause";var a=new Button();var b=new Button();PreviewIconsR9.Set(a,0);PreviewIconsR9.Set(b,1);playImageR10=(Image)a.Image.Clone();pauseImageR10=(Image)b.Image.Clone();a.Dispose();b.Dispose();play.Image=playImageR10;play.Text="";keysR10=new PreviewKeysR10(this);Application.AddMessageFilter(keysR10);Disposed+=delegate{Application.RemoveMessageFilter(keysR10);playImageR10.Dispose();pauseImageR10.Dispose();pause.Dispose();};var tips=new ToolTip();tips.SetToolTip(play,"Lecture / Pause (Espace) · ← / → : une image · Échap : quitter le plein écran");Disposed+=delegate{tips.Dispose();};}
  internal bool HandleKeyR10(Keys key){if(key==Keys.Escape&&ExitLargeR10!=null){ExitLargeR10();return true;}if(key==Keys.Escape&&WindowState==FormWindowState.Maximized){WindowState=FormWindowState.Normal;return true;}if(!play.Enabled||player==null)return false;if(key==Keys.Space){ToggleR10();return true;}if(key==Keys.Left||key==Keys.Right){StepR10(key==Keys.Left?-1:1);return true;}return false;}
  void ToggleR10(){ControlPlayer(()=>{if(player.PlayingR10)player.Pause();else player.Play();UpdatePlayR10();});}
  void UpdatePlayR10(){playingR10=player!=null&&player.PlayingR10;play.Image=playingR10?pauseImageR10:playImageR10;play.Text="";play.AccessibleDescription=playingR10?"Pause":"Lecture";}
  void StepR10(int direction){ControlPlayer(()=>{player.Pause();double target=PreviewFramesR10.Step(player.Position,direction,duration);player.Position=target;player.Pause();seek.Value=(int)Math.Round(target/Math.Max(.001,duration)*10000);midiR9.Position(timelineOrigin+target);UpdatePlayR10();});}
 }
}
