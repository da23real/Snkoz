using System;
using System.Collections.Generic;
using System.Globalization;
namespace UrbanKaoz {
 public class Clip { public int Id,Track,Pattern,Offset,Length;public long Position;public Clip Copy(){return (Clip)MemberwiseClone();} }
 public class Zone { public uint Id;public int Track,Kind,A,B;public long Tick,Gate;public Zone Copy(){return (Zone)MemberwiseClone();} }
 public class SongModel {
  public const long MaxStep=1000000000;
  public Dictionary<int,Clip> Clips=new Dictionary<int,Clip>();public Dictionary<uint,Zone> Zones=new Dictionary<uint,Zone>();
  public int[,] Lengths=new int[16,16];public bool[,] Present=new bool[16,16];
  public Selection Selection=new Selection();public bool Shift,Column;
  public bool Mode,Engine,Playing,Loop,Synced,Delete,Record;public int Track,Pattern,View,Ram;public long Tick,Start=1,End=4;public double Bpm=115;
  static int N(string s){return int.Parse(s,CultureInfo.InvariantCulture);} static long L(string s){return long.Parse(s,CultureInfo.InvariantCulture);}
  public void Accept(string line){var s=line.Split(',');switch(s[0]){
   case "SELMODE":Shift=s[1]=="1";Column=s[2]=="1";break;
   case "SELCLEAR":Selection.Clear();break;
   case "BEGIN":Selection.Clear();Synced=false;Clips.Clear();Zones.Clear();Array.Clear(Present,0,Present.Length);Array.Clear(Lengths,0,Lengths.Length);break;
   case "END":Synced=true;break;
   case "PAT":int t=N(s[1]),p=N(s[2]);if(t>=0&&t<16&&p>=0&&p<16){Present[t,p]=s[3]=="1";Lengths[t,p]=N(s[4]);}break;
   case "CLIP":var c=new Clip{Id=N(s[1]),Track=N(s[2]),Pattern=N(s[3]),Position=L(s[4]),Offset=N(s[5]),Length=N(s[6])};if(c.Id>=0&&c.Id<128&&Valid(c))Clips[c.Id]=c;break;
   case "REMOVE":Selection.RemoveClip(N(s[1]));Clips.Remove(N(s[1]));break;
   case "ZL":var z=new Zone{Id=uint.Parse(s[1]),Track=N(s[2]),Kind=N(s[3]),Tick=L(s[4]),Gate=L(s[5]),A=N(s[6]),B=N(s[7])};if(Valid(z))Zones[z.Id]=z;break;
   case "ZREMOVE":Selection.RemoveZone(uint.Parse(s[1]));Zones.Remove(uint.Parse(s[1]));break;
   case "STATE":Mode=s[1]=="1";Playing=s[2]=="1";Tick=L(s[3]);Loop=s[4]=="1";Start=L(s[5]);End=L(s[6]);Bpm=N(s[7])/100.0;Delete=s.Length>10&&s[10]=="1";break;
   case "EXTRA":Engine=s[1]=="1";Record=s[2]=="1";View=N(s[3]);Ram=N(s[5]);break;
   case "SELECT":Track=N(s[1]);Pattern=N(s[2]);break;
  }}
  public static bool Valid(Zone z){return z.Id>0&&z.Track>=0&&z.Track<16&&z.Kind>=0&&z.Kind<=1&&z.Tick>=0&&z.Tick<=MaxStep*96&&z.Gate>0&&z.Gate<=MaxStep*96-z.Tick&&z.A>=0&&z.A<128&&z.B>=0&&z.B<128&&(z.Kind!=0||z.B>0);}
  public static bool Valid(Clip c){return c.Track>=0&&c.Track<16&&c.Pattern>=0&&c.Pattern<16&&c.Position>=0&&c.Position<=MaxStep&&c.Length>0&&c.Offset>=0&&c.Offset<=1024&&c.Length<=1024-c.Offset&&c.Length<=MaxStep-c.Position;}
  public bool Overlaps(Clip c){foreach(var x in Clips.Values)if(x.Id!=c.Id&&x.Track==c.Track&&c.Position<x.Position+x.Length&&x.Position<c.Position+c.Length)return true;return false;}
  public List<int> Rows(){var rows=new List<int>();for(int t=0;t<16;t++){bool blocks=false,free=false;foreach(var c in Clips.Values)if(c.Track==t)blocks=true;foreach(var z in Zones.Values)if(z.Track==t)free=true;if(View==0||(View==2&&blocks))rows.Add(t*2);if(View==1||(View==2&&free))rows.Add(t*2+1);}return rows;}
  public static float Grip(float width,float step){return Math.Min(width/3,Math.Max(16,3*step));}
  public static void TrimLeft(Clip c,Clip original,int delta){c.Offset=Math.Max(original.Offset,Math.Min(original.Offset+original.Length-1,original.Offset+delta));c.Length=original.Offset+original.Length-c.Offset;}
  public static double Hue(int t,int p){return (t*17+p*137.50776)%360;}
 }
}
