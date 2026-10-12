using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace Synkoz {
 // Native Explorer folder picker, including Quick Access and the usual sidebar.
 public sealed class FolderDialog08:IDisposable {
  public string SelectedPath{get;set;}
  public void Dispose(){}
  public DialogResult ShowDialog(){return ShowDialog(null);}
  public DialogResult ShowDialog(IWin32Window owner){
   var dialog=(IFileDialog08)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")));
   try{
    dialog.SetOkButtonLabel("Sélectionner");uint flags;dialog.GetOptions(out flags);dialog.SetOptions(flags|0x20u|0x40u|0x800u);
    if(!string.IsNullOrEmpty(SelectedPath)&&Directory.Exists(SelectedPath)){
     IShellItem08 folder;Guid iid=typeof(IShellItem08).GUID;
     if(SHCreateItemFromParsingName(SelectedPath,IntPtr.Zero,ref iid,out folder)==0){try{dialog.SetFolder(folder);}finally{Marshal.ReleaseComObject(folder);}}
    }
    int hr=dialog.Show(owner==null?IntPtr.Zero:owner.Handle);
    if(hr==unchecked((int)0x800704C7))return DialogResult.Cancel;
    Marshal.ThrowExceptionForHR(hr);IShellItem08 result;dialog.GetResult(out result);
    try{IntPtr text;result.GetDisplayName(0x80058000u,out text);try{SelectedPath=Marshal.PtrToStringUni(text);}finally{Marshal.FreeCoTaskMem(text);}}finally{Marshal.ReleaseComObject(result);}
    return DialogResult.OK;
   }finally{Marshal.ReleaseComObject(dialog);}
  }
  [DllImport("shell32.dll",CharSet=CharSet.Unicode,PreserveSig=true)]static extern int SHCreateItemFromParsingName(string path,IntPtr context,ref Guid iid,out IShellItem08 item);
 }
 [ComImport,Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]interface IShellItem08 {
  void BindToHandler(IntPtr context,ref Guid handler,ref Guid iid,out IntPtr value);
  void GetParent(out IShellItem08 parent);
  void GetDisplayName(uint kind,out IntPtr name);
  void GetAttributes(uint mask,out uint attributes);
  void Compare(IShellItem08 other,uint hint,out int order);
 }
 [ComImport,Guid("42F85136-DB7E-439C-85F1-E4075D135FC8"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]interface IFileDialog08 {
  [PreserveSig]int Show(IntPtr owner);
  void SetFileTypes(uint count,IntPtr types);void SetFileTypeIndex(uint index);void GetFileTypeIndex(out uint index);
  void Advise(IntPtr events,out uint cookie);void Unadvise(uint cookie);void SetOptions(uint options);void GetOptions(out uint options);
  void SetDefaultFolder(IShellItem08 item);void SetFolder(IShellItem08 item);void GetFolder(out IShellItem08 item);void GetCurrentSelection(out IShellItem08 item);
  void SetFileName([MarshalAs(UnmanagedType.LPWStr)]string name);void GetFileName(out IntPtr name);void SetTitle([MarshalAs(UnmanagedType.LPWStr)]string title);
  void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)]string title);void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)]string title);void GetResult(out IShellItem08 item);
 }
 public sealed partial class MontagePanel {
  void OpenLastTake08(){
   if(occupied()){status.Text="Terminer la prise avant de charger un autre montage.";return;}
   string path=Preferences936.Load().LastTakeR8;
   if(string.IsNullOrEmpty(path)||!Directory.Exists(path)){status.Text="Aucune dernière prise terminée disponible.";return;}
   try{ShowMontage(Montage.Load(path));FoldersR2.Set("montage",path);}catch(Exception ex){status.Text=ex.Message;Synkoz.ErrorR8.Show(this,ex.Message,"Erreur");}
  }
 }
}
