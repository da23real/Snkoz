using System;using System.Globalization;using System.Linq;using System.Windows.Forms;
namespace Synkoz {
 public static class MicroGain15 {public static int Apply(double input,double gain){double v=Math.Max(0,Math.Min(1,input*Math.Abs(gain)));return (int)Math.Round(127*(gain<0?1-v:v));}}
 public static class Arithmetic15 {
  public static decimal Parse(string text){if(text==null||text.Length>128)throw new FormatException("Calcul trop long.");int p=0,depth=0;Func<decimal> sum=null,product=null,atom=null;Action skip=()=>{while(p<text.Length&&char.IsWhiteSpace(text[p]))p++;};
   atom=()=>{skip();if(++depth>16)throw new FormatException("Trop de parenthèses.");try{if(p<text.Length&&(text[p]=='+'||text[p]=='-')){char sign=text[p++];decimal v=atom();return sign=='-'?-v:v;}if(p<text.Length&&text[p]=='('){p++;decimal v=sum();skip();if(p>=text.Length||text[p++]!=')')throw new FormatException("Parenthèse manquante.");return v;}int start=p;while(p<text.Length&&(char.IsDigit(text[p])||text[p]=='.'||text[p]==','))p++;decimal n;if(start==p||!decimal.TryParse(text.Substring(start,p-start).Replace(',','.'),NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out n))throw new FormatException("Nombre invalide.");return n;}finally{depth--;}};
   product=()=>{decimal v=atom();while(true){skip();if(p>=text.Length||(text[p]!='*'&&text[p]!='/'))return v;char op=text[p++];decimal n=atom();v=op=='*'?v*n:v/n;}};
   sum=()=>{decimal v=product();while(true){skip();if(p>=text.Length||(text[p]!='+'&&text[p]!='-'))return v;char op=text[p++];decimal n=product();v=op=='+'?v+n:v-n;}};decimal result=sum();skip();if(p!=text.Length)throw new FormatException("Calcul invalide.");return result;
  }
 }
 public sealed class ExpressionNumber15:NumericUpDown {
  public ExpressionNumber15(){foreach(var text in Controls.OfType<TextBox>())text.KeyPress+=delegate(object sender,KeyPressEventArgs e){if("+-*/(),.".IndexOf(e.KeyChar)>=0)e.Handled=false;};}
  protected override void ValidateEditText(){try{decimal n=Arithmetic15.Parse(Text);if(n<Minimum||n>Maximum)throw new FormatException("Position hors limites.");Value=n;ForeColor=System.Drawing.SystemColors.WindowText;UpdateEditText();}catch{ForeColor=System.Drawing.Color.Firebrick;}}
 }
}
