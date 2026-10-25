using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Original $8D:DF94 logo glare colors: seven equal RGB5 intervals to
/// white followed by the mirrored return. For frame0..13, t is frame+1 up to6
/// and13-frame thereafter. Each channel is floor((base*(7-t)+31*t)/7).
/// All224 original words independently confirm this interpolation. The final
/// row remains supplied base artwork. Its fifteen opaque words equal the original
/// logo palette $8C:EFEB..F008: selected yellow, blue and orange drawing inks, whose
/// numerical recitation would only disguise the artwork. Slot0 instead holds$21A8;
/// OBJ index0 is transparent before CGRAM lookup, so this source payload has no
/// visible color-generation rule. Both are retained narrowly under #1165's nonsense
/// exception; no intermediate animation color is included in that disposition.</summary>
internal static class LogoGlarePaletteColorDefinitions
{
    /// <summary>Calculate only intermediate colors from the final supplied row.
    /// The caller resolves explicit per-frame edits first. No generated row is cached.</summary>
    internal static bool TryCalculatedColor(PaletteFxColorKey key, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors, out RgbColor value)
    {
        value = RgbColor.Black;
        if (!PostCreditsIconGlarePaletteFxColorDefinitions.Rows.TryLocate(key, out int frame, out int color) || frame == 13 ||
            !colors.TryGetValue(PostCreditsIconGlarePaletteFxColorDefinitions.Color(13, color), out RgbColor basis))
            return false;
        int t = frame < 7 ? frame + 1 : 13 - frame;
        int red = ((basis.Red) * (7 - t) + 31 * t) / 7;
        int green = ((basis.Green) * (7 - t) + 31 * t) / 7;
        int blue = ((basis.Blue) * (7 - t) + 31 * t) / 7;
        value = new RgbColor(red, green, blue);
        return true;
    }
}
