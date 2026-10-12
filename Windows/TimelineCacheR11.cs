using System;using System.Linq;using System.Windows.Forms;
namespace Synkoz {public sealed partial class MontagePanel {DateTime viewDueR11=DateTime.MinValue;internal bool FramesOnlyR11{get{return viewDueR11!=DateTime.MinValue;}}void DeferViewR11(){viewDueR11=DateTime.UtcNow.AddSeconds(5);}void SelectionPaintR11(int[] previous){var changed=previous.Concat(selection940.Select(b=>b.Track)).Distinct().ToArray();foreach(var v in Descendants945(rows).OfType<BlocksView>())if(changed.Contains(v.Track))v.Invalidate();SendLeds940();}
void ResizeRowsR11(){int width=Math.Max(500,rows.ClientSize.Width-25);foreach(Control row in rows.Controls){row.Width=width;foreach(var v in row.Controls.OfType<BlocksView>())v.Width=Math.Max(1,width-225);}DeferViewR11();InvalidateTimelines940();}
}}
