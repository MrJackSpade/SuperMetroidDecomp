using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Standalone Sidehopper target paints.
/// Reviewed categorical paints and material composition: ink numbers select painted pixel classes, not a measured lighting value. The shifted fifteen-color auxiliary copy has a separate disposition.</summary>
internal static class SidehopperInitialPaintDefinitions
{
    /// <summary>Cyan shell/leg edge highlights.</summary>
    internal static readonly RgbColor BodyHighlight = new(0, 23, 21);
    /// <summary>Cyan shell/leg surfaces with equal green/blue.</summary>
    internal static readonly RgbColor BodySurface = new(0, 12, 12);
    /// <summary>Upper-head glint.</summary>
    internal static readonly RgbColor HeadGlint = new(0, 30, 26);
    /// <summary>Installed inner-detail light endpoint.
    /// Its exact visible pixels are unidentified in the three alive compositions.</summary>
    internal static readonly RgbColor DetailLight = new(0, 22, 23);
    /// <summary>Dark inner head/limb details with equal green/blue.</summary>
    internal static readonly RgbColor DetailDark = new(0, 13, 13);
    /// <summary>Yellow mouth/claw light with equal red/green.</summary>
    internal static readonly RgbColor ClawLight = new(28, 28, 0);
    /// <summary>Brown mouth/claw shadow.</summary>
    internal static readonly RgbColor ClawDark = new(17, 6, 0);
    /// <summary>Standalone target ink0 copied by EF92-EF9B;
    /// Sprite ink 0 is transparent. This does not cover shifted auxiliary visible ink1.</summary>
    internal static readonly RgbColor TransparentSlot = new(0, 0, 14);
    /// <summary>Saturated-yellow paired head features.</summary>
    internal static readonly RgbColor HeadYellow = new(31, 31, 0);
}
