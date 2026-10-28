using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Standalone shared-corpse target paints from $A9:F8A6-F8C4.
/// The six categorical inks, shade ratios and rounding specify this chosen corpse painting; ink numbers are not sampled physical quantities. Its shifted fifteen-color auxiliary image has an independent disposition.</summary>
internal static class SidehopperCorpsePaintDefinitions
{
    /// <summary>$A9:F8A8, ink1: pale yellow limb/ligament highlight.</summary>
    internal static readonly RgbColor LimbLight = new(31, 31, 21);
    /// <summary>$A9:F8AC, ink3: medium olive head/limb details, equal red/green.</summary>
    internal static readonly RgbColor OliveMedium = new(9, 9, 2);
    /// <summary>$A9:F8AE, ink4: dark olive head/limb contours.</summary>
    internal static readonly RgbColor OliveDark = new(5, 5, 0);
    /// <summary>$A9:F8B2, ink6: warm gray eyes visible on the Zoomer corpse.</summary>
    internal static readonly RgbColor EyeGray = new(17, 17, 16);
    /// <summary>$A9:F8B6, ink8: ochre shell/body light, visible on the Ripper corpse.</summary>
    internal static readonly RgbColor BodyLight = new(31, 25, 18);
    /// <summary>$A9:F8C4, ink15: ochre shell/body darkest shade, visible on the Skree corpse.</summary>
    internal static readonly RgbColor BodyDark = new(5, 3, 1);
    /// <summary>$A9:F8A6 copied by EFAC-EFB5: standalone transparent-slot compatibility payload.
    /// The shifted auxiliary copy uses its first word as visible ink1 and is excluded.</summary>
    internal static readonly RgbColor TransparentSlot = new(0, 0, 14);
    /// <summary>$A9:F8AA, ink2 is the selected nearest three-quarter pale-limb shade.</summary>
    internal const int LimbShadeNumerator = 3, LimbShadeDenominator = 4;
    /// <summary>$A9:F8B4, ink7 is the selected floor two-thirds dark-body outline/mouth shade.</summary>
    internal const int OutlineNumerator = 2, OutlineDenominator = 3;
}
