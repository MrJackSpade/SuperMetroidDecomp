using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Enemies.Artwork.SidehopperCorpsePaintDefinitions;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Paints the standalone shared-corpse palette from its authored inks and derived limb and body shades.</summary>
internal static class SidehopperCorpsePaint
{
    /// <summary>Selects the standalone corpse palette ink, including derived limb and body shades.</summary>
    internal static RgbColor Color(int color) => color switch
    {
        0 => TransparentSlot,
        1 => LimbLight,
        2 => Scale(LimbLight, LimbShadeNumerator, LimbShadeDenominator, LimbShadeDenominator / 2),
        3 => OliveMedium,
        4 => OliveDark,
        5 => RgbColor.White,
        6 => EyeGray,
        7 => Scale(BodyDark, OutlineNumerator, OutlineDenominator, 0),
        >= 8 and <= 15 => SidehopperCorpsePalette.Interpolate(BodyLight, BodyDark, color - 8),
        _ => throw new ArgumentOutOfRangeException(nameof(color)),
    };

    /// <summary>Scales each five-bit channel with the authored integer rounding for a corpse shade.</summary>
    private static RgbColor Scale(RgbColor color, int numerator, int denominator, int rounding)
    {
        return color.Map((_, a) => ((a * numerator + rounding) / denominator));
    }
}
