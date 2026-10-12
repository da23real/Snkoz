using System.Linq;
namespace Synkoz {
 public static class TextFormatR12 {
  public static bool HasText(Montage m){return m.Tracks.Any(t=>t.Fx!=null&&t.Fx.Effects.Any(e=>e.Kind==EffectKind936.Texte))||m.Blocks.Any(b=>b.Fx939!=null&&b.Fx939.Effects.Any(e=>e.Kind==EffectKind936.Texte));}
 }
}
