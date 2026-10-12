using System;using System.Windows.Forms;
namespace Synkoz {static class MenuLifetimeR15 {
 // Click dispatch continues after Closed. Dispose only on the next message-loop turn.
 internal static void Own(ContextMenuStrip menu,Control owner){EventHandler disposeWithOwner=delegate{if(!menu.IsDisposed)menu.Dispose();};owner.Disposed+=disposeWithOwner;menu.Disposed+=delegate{owner.Disposed-=disposeWithOwner;};menu.Closed+=delegate{if(owner.IsDisposed||!owner.IsHandleCreated)return;owner.BeginInvoke((Action)(()=>{if(!menu.IsDisposed)menu.Dispose();}));};}
}}
