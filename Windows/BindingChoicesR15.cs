using System;using System.Linq;using System.Drawing;using System.Windows.Forms;
namespace Synkoz {
 static class BindingChoicesR15 {
  public static void Attach(DataGridView grid,TextVariableR12[] allowed){
   grid.CellFormatting+=delegate(object sender,DataGridViewCellFormattingEventArgs e){if(e.ColumnIndex==1&&e.Value is TextVariableR12&&!allowed.Contains((TextVariableR12)e.Value))e.CellStyle.ForeColor=SystemColors.GrayText;};
   grid.EditingControlShowing+=delegate(object sender,DataGridViewEditingControlShowingEventArgs e){var combo=e.Control as ComboBox;if(combo==null)return;combo.DrawItem-=Draw;combo.SelectedIndexChanged-=Reject;if(grid.CurrentCell.ColumnIndex!=1){combo.DrawMode=DrawMode.Normal;combo.Tag=null;return;}combo.Tag=new State{Allowed=allowed,Previous=combo.SelectedIndex};combo.DrawMode=DrawMode.OwnerDrawFixed;combo.DrawItem+=Draw;combo.SelectedIndexChanged+=Reject;};
  }
  sealed class State{public TextVariableR12[] Allowed;public int Previous;public bool Busy;}
  static void Reject(object sender,EventArgs e){var combo=(ComboBox)sender;var state=combo.Tag as State;if(state==null||state.Busy)return;if(combo.SelectedItem is TextVariableR12&&!state.Allowed.Contains((TextVariableR12)combo.SelectedItem)){state.Busy=true;combo.SelectedIndex=state.Previous;state.Busy=false;}else state.Previous=combo.SelectedIndex;}
  static void Draw(object sender,DrawItemEventArgs e){var combo=(ComboBox)sender;var state=combo.Tag as State;e.DrawBackground();if(e.Index>=0){bool ok=state==null||state.Allowed.Contains((TextVariableR12)combo.Items[e.Index]);TextRenderer.DrawText(e.Graphics,combo.GetItemText(combo.Items[e.Index]),combo.Font,e.Bounds,ok?e.ForeColor:SystemColors.GrayText);}e.DrawFocusRectangle();}
 }
}
