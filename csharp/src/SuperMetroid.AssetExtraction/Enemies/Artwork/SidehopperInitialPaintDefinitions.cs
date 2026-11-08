using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>Standalone Sidehopper target paints.
/// Reviewed categorical paints and material composition: ink numbers select painted pixel classes, not a measured lighting value. The shifted fifteen-color auxiliary copy has a separate disposition.</summary>
internal static class SidehopperInitialPaintDefinitions
{
    /// <summary>Cyan shell/leg edge highlights.</summary>
    internal static readonly RgbColor BodyHighlight = new(0, 189, 173);
    /// <summary>Cyan shell/leg surfaces with equal green/blue.</summary>
    internal static readonly RgbColor BodySurface = new(0, 99, 99);
    /// <summary>Upper-head glint.</summary>
    internal static readonly RgbColor HeadGlint = new(0, 247, 214);
    /// <summary>Installed inner-detail light endpoint.
    /// Its exact visible pixels are unidentified in the three alive compositions.</summary>
    internal static readonly RgbColor DetailLight = new(0, 181, 189);
    /// <summary>Dark inner head/limb details with equal green/blue.</summary>
    internal static readonly RgbColor DetailDark = new(0, 107, 107);
    /// <summary>Yellow mouth/claw light with equal red/green.</summary>
    internal static readonly RgbColor ClawLight = new(231, 231, 0);
    /// <summary>Brown mouth/claw shadow.</summary>
    internal static readonly RgbColor ClawDark = new(140, 49, 0);
    /// <summary>Standalone target ink0 copied by EF92-EF9B;
    /// Sprite ink 0 is transparent. This does not cover shifted auxiliary visible ink1.</summary>
    internal static readonly RgbColor TransparentSlot = new(0, 0, 115);
    /// <summary>Saturated-yellow paired head features.</summary>
    internal static readonly RgbColor HeadYellow = new(255, 255, 0);
}
