using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
namespace Synkoz {
 public sealed class EffectScroll940:FlowLayoutPanel {
  protected override Point ScrollToControl(Control activeControl){return new Point(0,base.ScrollToControl(activeControl).Y);}
 }
 public sealed partial class EffectsForm936 {
  readonly List<Action<int>> controls940=new List<Action<int>>();readonly Stack<TrackFx936> undoFx940=new Stack<TrackFx936>();bool restoring940;
  void Bind940(string label,Control control){Action<int> turn=null;var slider=control as SliderR3;var number=control as NumericUpDown;var combo=control as ComboBox;if(slider!=null)turn=d=>slider.Turn940(d);else if(number!=null)turn=d=>number.Value=Math.Max(number.Minimum,Math.Min(number.Maximum,number.Value+d*number.Increment));else if(combo!=null)turn=d=>combo.SelectedIndex=Math.Max(0,Math.Min(combo.Items.Count-1,combo.SelectedIndex+d));if(turn!=null&&controls940.Count<16){controls940.Add(turn);var parent=control.Parent;if(parent!=null){var title=parent.Controls.OfType<Label>().FirstOrDefault();if(title!=null)UiPolicyR20.KeyLabel(title,controls940.Count,label);}}}
  public void ControllerTurn940(int key,int delta){if(key<0||key>=controls940.Count||!Enabled)return;if(!restoring940){if(undoFx940.Count>=2){var recent=undoFx940.Peek();undoFx940.Clear();undoFx940.Push(recent);}undoFx940.Push(UiR2.Copy(draft));}try{controls940[key](delta);status.Text="K"+(key+1)+" réglé — mémorisation automatique.";}catch(Exception ex){status.Text=ex.Message;Synkoz.ErrorR8.Show(this,ex.Message,"Erreur");}}
  public void ControllerUndo940(){if(undoFx940.Count==0){status.Text="Aucun réglage KY à annuler.";return;}restoring940=true;try{var prior=undoFx940.Pop();draft.Effects=prior.Effects;RefreshList(Math.Max(0,list.SelectedIndex));status.Text="Dernier réglage KY annulé.";}finally{restoring940=false;}}
  void ResizeFields940(){int width=Math.Max(180,editor.ClientSize.Width-SystemInformation.VerticalScrollBarWidth-22);foreach(Control field in editor.Controls){if((string)field.Tag=="columnsR9"){field.Width=width;continue;}field.MaximumSize=new Size(width,int.MaxValue);if(field is Panel){field.Width=width;field.Height=field.Controls.OfType<SliderR3>().Any()?86:field.Controls.OfType<TextBox>().Any(t=>t.Multiline)?120:59;foreach(Control child in field.Controls){if(child is Label)child.Width=width;else child.Width=Math.Min(width,child is SliderR3?Math.Min(310,width):Math.Max(120,child.Width));}}}editor.AutoScrollMinSize=new Size(0,editor.Padding.Vertical+editor.Controls.Cast<Control>().Sum(c=>c.Height+c.Margin.Vertical));editor.HorizontalScroll.Enabled=false;}
 }
}

