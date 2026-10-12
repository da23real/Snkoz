using System;using System.Linq;using System.Collections.Generic;
namespace Synkoz {
public sealed partial class OutputPresetR14 {public WaitingSettingsR20 WaitingR20=new WaitingSettingsR20();public bool ZoneFreeR20;public List<ZonePointR20> ZonePointsR20=new List<ZonePointR20>();}
public sealed partial class Effect936 {internal void ValidateR20(){if(WaitingR20==null||ZonePointsR20==null||ZonePointsR20.Count>512||ZonePointsR20.Any(p=>p==null||double.IsNaN(p.X)||double.IsNaN(p.Y)||p.X<0||p.X>100||p.Y<0||p.Y>100)||ZoneFreeR20&&ZonePointsR20.Count<3)throw new Exception("Forme libre invalide (3 à 512 points).");var s=WaitingR20;if(new[]{s.Delay,s.Interval,s.Precision}.Any(v=>double.IsNaN(v)||double.IsInfinity(v))||s.Delay<0||s.Delay>86400||s.Interval<.01||s.Interval>86400||s.Precision<0||s.Precision>100)throw new Exception("Animation d’attente : délai, intervalle ou précision invalide.");}}
}
