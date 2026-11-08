using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>Standalone shared-corpse target paints.
/// The six categorical inks, shade ratios and rounding specify this chosen corpse painting; ink numbers are not sampled physical quantities. Its shifted fifteen-color auxiliary image has an independent disposition.</summary>
internal static class SidehopperCorpsePaintDefinitions
{
    /// <summary>Pale yellow limb/ligament highlight.</summary>
    internal static readonly RgbColor LimbLight = new(255, 255, 173);
    /// <summary>Medium olive head/limb details, equal red/green.</summary>
    internal static readonly RgbColor OliveMedium = new(74, 74, 16);
    /// <summary>Dark olive head/limb contours.</summary>
    internal static readonly RgbColor OliveDark = new(41, 41, 0);
    /// <summary>Warm gray eyes visible on the Zoomer corpse.</summary>
    internal static readonly RgbColor EyeGray = new(140, 140, 132);
    /// <summary>Ochre shell/body light, visible on the Ripper corpse.</summary>
    internal static readonly RgbColor BodyLight = new(255, 206, 148);
    /// <summary>Ochre shell/body darkest shade, visible on the Skree corpse.</summary>
    internal static readonly RgbColor BodyDark = new(41, 24, 8);
    /// <summary>The corpse palette's first word, copied as a standalone transparent-slot compatibility payload.
    /// The shifted auxiliary copy uses its first word as visible ink1 and is excluded.</summary>
    internal static readonly RgbColor TransparentSlot = new(0, 0, 115);
    /// <summary>Ink2 is the selected nearest three-quarter pale-limb shade.</summary>
    internal const int LimbShadeNumerator = 3, LimbShadeDenominator = 4;
    /// <summary>Ink7 is the selected floor two-thirds dark-body outline/mouth shade.</summary>
    internal const int OutlineNumerator = 2, OutlineDenominator = 3;
}
