using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Standalone shared-corpse target paints from $A9:F8A6-F8C4.
/// The six categorical inks, shade ratios and rounding specify this chosen corpse painting; ink numbers are not sampled physical quantities. Its shifted fifteen-color auxiliary image has an independent disposition.</summary>
internal static class SidehopperCorpsePaintDefinitions
{
    /// <summary>$A9:F8A8, ink1: pale yellow limb/ligament highlight.</summary>
    private static readonly Bgr555 LimbLight = new(31, 31, 21);
    /// <summary>$A9:F8AC, ink3: medium olive head/limb details, equal red/green.</summary>
    private static readonly Bgr555 OliveMedium = new(9, 9, 2);
    /// <summary>$A9:F8AE, ink4: dark olive head/limb contours.</summary>
    private static readonly Bgr555 OliveDark = new(5, 5, 0);
    /// <summary>$A9:F8B2, ink6: warm gray eyes visible on the Zoomer corpse.</summary>
    private static readonly Bgr555 EyeGray = new(17, 17, 16);
    /// <summary>$A9:F8B6, ink8: ochre shell/body light, visible on the Ripper corpse.</summary>
    private static readonly Bgr555 BodyLight = new(31, 25, 18);
    /// <summary>$A9:F8C4, ink15: ochre shell/body darkest shade, visible on the Skree corpse.</summary>
    private static readonly Bgr555 BodyDark = new(5, 3, 1);
    /// <summary>$A9:F8A6 copied by EFAC-EFB5: standalone transparent-slot compatibility payload.
    /// The shifted auxiliary copy uses its first word as visible ink1 and is excluded.</summary>
    private static readonly Bgr555 TransparentSlot = new(0, 0, 14);
    /// <summary>$A9:F8AA, ink2 is the selected nearest three-quarter pale-limb shade.</summary>
    private const int LimbShadeNumerator = 3, LimbShadeDenominator = 4;
    /// <summary>$A9:F8B4, ink7 is the selected floor two-thirds dark-body outline/mouth shade.</summary>
    private const int OutlineNumerator = 2, OutlineDenominator = 3;

    internal static Bgr555 Color(int color) => color switch
    {
        0 => TransparentSlot,
        1 => LimbLight,
        2 => Scale(LimbLight, LimbShadeNumerator, LimbShadeDenominator, LimbShadeDenominator / 2),
        3 => OliveMedium,
        4 => OliveDark,
        5 => Bgr555.White,
        6 => EyeGray,
        7 => Scale(BodyDark, OutlineNumerator, OutlineDenominator, 0),
        >= 8 and <= 15 => SidehopperCorpsePalette.Interpolate(BodyLight, BodyDark, color - 8),
        _ => throw new ArgumentOutOfRangeException(nameof(color)),
    };

    private static Bgr555 Scale(Bgr555 color, int numerator, int denominator, int rounding)
    {
        return color.Map((_, a) => ((a * numerator + rounding) / denominator));
    }
}
