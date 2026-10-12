using System;using System.Collections.Generic;using System.IO;
namespace Synkoz {
 public sealed partial class Preview936 {
  public string CacheKeyR4;
  sealed class LastR4 {public string Path,Folder;public double Origin,Duration;}
  static readonly Dictionary<string,LastR4> lastR4=new Dictionary<string,LastR4>();
  bool CachedFolderR4(){lock(lastR4)return folder!=null&&System.Linq.Enumerable.Any(lastR4.Values,v=>v.Folder==folder);}
  void RememberR4(){if(string.IsNullOrEmpty(CacheKeyR4)||string.IsNullOrEmpty(renderedR9))return;lock(lastR4){LastR4 old;if(lastR4.TryGetValue(CacheKeyR4,out old)&&old.Folder!=folder)try{Directory.Delete(old.Folder,true);}catch{}lastR4[CacheKeyR4]=new LastR4{Path=renderedR9,Folder=folder,Origin=timelineOrigin,Duration=duration};}}
  public bool ReopenR4(Montage model){if(rendering||string.IsNullOrEmpty(CacheKeyR4))return false;LastR4 item;lock(lastR4)if(!lastR4.TryGetValue(CacheKeyR4,out item)||!File.Exists(item.Path))return false;folder=item.Folder;renderedR9=item.Path;timelineOrigin=item.Origin;duration=item.Duration;timingR11=model;midiR9.LoadMidi(model);if(player!=null)player.Open(renderedR9);SetReady(true);clock.Start();status.Text="Dernier preview calculé — Actualiser pour appliquer les modifications.";return true;}
 }
}
