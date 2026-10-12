using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Drawing;using System.Windows.Forms;using System.Security.Cryptography;using System.Text;
namespace Synkoz {
public class ErrorChoicesR8 {public List<string> Silent=new List<string>(),Hidden=new List<string>();}
public static class ErrorR8 {
 public static string PreferencesPath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Snkoz","error-choices.json");
 static ErrorChoicesR8 choices;public static string LastMessage;public static event Action<string> Reported;
 static ErrorChoicesR8 Choices {get{if(choices==null){try{choices=Store934.Load<ErrorChoicesR8>(PreferencesPath);}catch{choices=new ErrorChoicesR8();}}return choices;}}
 public static string Key(string operation,string message){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(operation+"\n"+message))).Replace("-","");}
 public static DialogResult Show(string message){return Show(null,message,"Erreur");}public static DialogResult Show(string message,string title){return Show(null,message,title);}public static DialogResult Show(IWin32Window owner,string message){return Show(owner,message,"Erreur");}
 public static DialogResult Show(IWin32Window owner,string message,string title){LastMessage=title+" : "+message;LogR9.Write(LastMessage);if(Reported!=null)Reported(LastMessage);return DialogResult.OK;}

 static void Set(List<string> list,string key,bool value){list.Remove(key);if(value)list.Add(key);}
 // Confirmation dialogs preserve their original choices and semantics.
 public static DialogResult Show(string text,string caption,MessageBoxButtons buttons){return MessageBox.Show(text,caption,buttons);}
 public static DialogResult Show(IWin32Window owner,string text,string caption,MessageBoxButtons buttons){return MessageBox.Show(owner,text,caption,buttons);}
 public static DialogResult Show(IWin32Window owner,string text,string caption,MessageBoxButtons buttons,MessageBoxIcon icon){return buttons==MessageBoxButtons.OK?Show(owner,text,caption):MessageBox.Show(owner,text,caption,buttons,icon);}
 public static DialogResult Show(IWin32Window owner,string text,string caption,MessageBoxButtons buttons,MessageBoxIcon icon,MessageBoxDefaultButton def){return buttons==MessageBoxButtons.OK?Show(owner,text,caption):MessageBox.Show(owner,text,caption,buttons,icon,def);}
 public static DialogResult Show(string text,string caption,MessageBoxButtons buttons,MessageBoxIcon icon){return buttons==MessageBoxButtons.OK?Show(text,caption):MessageBox.Show(text,caption,buttons,icon);}
}
}
