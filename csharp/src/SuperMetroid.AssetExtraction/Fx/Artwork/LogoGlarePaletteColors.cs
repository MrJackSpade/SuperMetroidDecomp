using SuperMetroid.Core.Fx.PaletteFx;

using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.Artwork;

/// <summary>The logo glare colors: seven equal level intervals to
/// white followed by the mirrored return. For frame0..13, t is frame+1 up to6
/// and13-frame thereafter. Each channel is floor((base*(7-t)+31*t)/7).
/// All224 original words independently confirm this interpolation. The final
/// frame remains supplied base artwork. Its fifteen opaque words equal the original
/// logo palette: selected yellow, blue and orange drawing inks, whose
/// numerical recitation would only disguise the artwork. Slot0 instead holds an unused word;
/// sprite colour 0 is transparent before lookup, so this source payload has no
/// visible color-generation rule. Both are retained narrowly under #1165's nonsense
/// exception; no intermediate animation color is included in that disposition.</summary>
internal static class LogoGlarePaletteColors
{
    /// <summary>Calculate only intermediate colors from the final supplied frame.
    /// The caller resolves explicit per-frame edits first. No generated colour set is cached.</summary>
    internal static bool TryCalculatedColor(PaletteFxColorKey key, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors, out RgbColor value)
    {
        value = RgbColor.Black;
        if (!PostCreditsIconGlarePaletteFxColorDefinitions.ColorFrames.TryLocate(key, out int frame, out int color) || frame == 13 ||
            !colors.TryGetValue(PostCreditsIconGlarePaletteFxColorDefinitions.Color(13, color), out RgbColor basis))
            return false;
        int t = frame < 7 ? frame + 1 : 13 - frame;
        int red = ((basis.FadeLevels.Red) * (7 - t) + 31 * t) / 7;
        int green = ((basis.FadeLevels.Green) * (7 - t) + 31 * t) / 7;
        int blue = ((basis.FadeLevels.Blue) * (7 - t) + 31 * t) / 7;
        value = new FadeLevelColour(red, green, blue).ToColour();
        return true;
    }
}
