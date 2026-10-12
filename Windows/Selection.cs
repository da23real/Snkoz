using System.Collections.Generic;
namespace UrbanKaoz {
 public sealed class Selection {
  public HashSet<int> Clips=new HashSet<int>();public HashSet<uint> Zones=new HashSet<uint>();public int Kind,LastTrack;
  sealed class Col {public int Stage;public HashSet<int> C=new HashSet<int>();public HashSet<uint>Z=new HashSet<uint>();}
  Dictionary<long,Col> columns=new Dictionary<long,Col>();HashSet<int> baseC=new HashSet<int>();HashSet<uint>baseZ=new HashSet<uint>();
  public void Clear(){Clips.Clear();Zones.Clear();columns.Clear();baseC.Clear();baseZ.Clear();}
  public void RemoveClip(int id){Clips.Remove(id);baseC.Remove(id);foreach(var c in columns.Values)c.C.Remove(id);}
  public void RemoveZone(uint id){Zones.Remove(id);baseZ.Remove(id);foreach(var c in columns.Values)c.Z.Remove(id);}
  public void Context(int kind){if(kind!=Kind){Clear();Kind=kind;}}
  void Collect(SongModel m,long a,long b,int mask,HashSet<int>c,HashSet<uint>z){
   if(Kind==0){foreach(var e in m.Clips.Values)if((mask&(1<<e.Track))!=0&&e.Position*96<b&&(e.Position+e.Length)*96>a)c.Add(e.Id);}
   else foreach(var e in m.Zones.Values)if(e.Kind==Kind-1&&(mask&(1<<e.Track))!=0&&e.Tick<b&&e.Tick+e.Gate>a)z.Add(e.Id);
  }
  public void Rectangle(SongModel m,long a,long b,int mask){var c=new HashSet<int>();var z=new HashSet<uint>();Collect(m,a,b,mask,c,z);
   if(Clips.IsSupersetOf(c)&&Zones.IsSupersetOf(z)){Clips.ExceptWith(c);Zones.ExceptWith(z);}else{Clips.UnionWith(c);Zones.UnionWith(z);}columns.Clear();baseC.Clear();baseZ.Clear();}
  public void All(SongModel m,int scope,long a,long b,int visible){Clear();int mask=scope>=2?1<<LastTrack:65535;if((scope&1)!=0)mask&=visible;else{a=0;b=SongModel.MaxStep*96;}Collect(m,a,b,mask,Clips,Zones);}
  public void Column(SongModel m,long a,long b,int visible){if(columns.Count==0){baseC=new HashSet<int>(Clips);baseZ=new HashSet<uint>(Zones);}Col col;if(!columns.TryGetValue(a,out col)){col=new Col();columns[a]=col;}
   col.Stage++;col.C.Clear();col.Z.Clear();if(col.Stage==3)columns.Remove(a);else Collect(m,a,b,col.Stage==1?visible:65535,col.C,col.Z);
   Clips=new HashSet<int>(baseC);Zones=new HashSet<uint>(baseZ);foreach(var c in columns.Values){Clips.UnionWith(c.C);Zones.UnionWith(c.Z);}}
 }
}
