using System;using System.Runtime.InteropServices;using System.Windows.Forms;
namespace Synkoz {static class WindowFocusR15 {
 static readonly System.Collections.Generic.Dictionary<Form,Form> parentsR20=new System.Collections.Generic.Dictionary<Form,Form>();
 internal static Form ParentR20(Form form){Form parent;return parentsR20.TryGetValue(form,out parent)?parent:form.Owner;}
 internal static Form[] ChildrenR20(Form form){return System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(parentsR20,p=>p.Value==form),p=>p.Key));}
 internal static void RegisterR20(Form form,Form parent){if(parentsR20.ContainsKey(form))return;parentsR20[form]=parent;form.Disposed+=delegate{parentsR20.Remove(form);foreach(var child in ChildrenR20(form))if(!child.IsDisposed)child.Dispose();};}
 [DllImport("user32.dll",EntryPoint="SetWindowLongPtrW")]static extern IntPtr Set64(IntPtr window,int index,IntPtr value);
 [DllImport("user32.dll",EntryPoint="SetWindowLongW")]static extern int Set32(IntPtr window,int index,int value);
 internal static void Independent(Form form){if(form.Modal||form.Owner==null||!form.IsHandleCreated)return;RegisterR20(form,form.Owner);var remembered=form as RememberFormR12;if(remembered!=null){remembered.ParentR20=form.Owner;var parent=form.Owner as RememberFormR12;if(parent!=null&&!parent.ChildrenR20.Contains(remembered))parent.ChildrenR20.Add(remembered);}form.Owner=null;if(IntPtr.Size==8)Set64(form.Handle,-8,IntPtr.Zero);else Set32(form.Handle,-8,0);}
 [DllImport("user32.dll")]static extern bool SetForegroundWindow(IntPtr window);
 internal static void Raise(Form form){if(form==null||form.IsDisposed)return;if(!form.Visible)form.Show();if(form.WindowState==FormWindowState.Minimized)form.WindowState=FormWindowState.Normal;form.BringToFront();form.Activate();if(form.IsHandleCreated)SetForegroundWindow(form.Handle);}
}}
