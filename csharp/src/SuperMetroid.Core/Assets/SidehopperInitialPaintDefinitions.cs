using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Standalone Sidehopper target paints at $A9:F8C6-F8E4.
/// Reviewed categorical paints and material composition: ink numbers select painted pixel classes, not a measured lighting value. The shifted fifteen-color auxiliary copy has a separate disposition.</summary>
internal static class SidehopperInitialPaintDefinitions
{
    /// <summary>$A9:F8CA, ink2: cyan shell/leg edge highlights.</summary>
    private static readonly Bgr555 BodyHighlight = new(0, 23, 21);
    /// <summary>$A9:F8CC, ink3: cyan shell/leg surfaces with equal green/blue.</summary>
    private static readonly Bgr555 BodySurface = new(0, 12, 12);
    /// <summary>$A9:F8D0, ink5: upper-head glint.</summary>
    private static readonly Bgr555 HeadGlint = new(0, 30, 26);
    /// <summary>$A9:F8D2, ink6: installed inner-detail light endpoint.
    /// Its exact visible pixels are unidentified in the three native alive compositions.</summary>
    private static readonly Bgr555 DetailLight = new(0, 22, 23);
    /// <summary>$A9:F8D6, ink8: dark inner head/limb details with equal green/blue.</summary>
    private static readonly Bgr555 DetailDark = new(0, 13, 13);
    /// <summary>$A9:F8DA, ink10: yellow mouth/claw light with equal red/green.</summary>
    private static readonly Bgr555 ClawLight = new(28, 28, 0);
    /// <summary>$A9:F8DE, ink12: brown mouth/claw shadow.</summary>
    private static readonly Bgr555 ClawDark = new(17, 6, 0);
    /// <summary>$A9:F8C6, standalone target ink0 copied by EF92-EF9B;
    /// OBJ ink0 is transparent. This does not cover shifted auxiliary visible ink1.</summary>
    private static readonly Bgr555 TransparentSlot = new(0, 0, 14);
    /// <summary>$A9:F8E0, ink13: saturated-yellow paired head features.</summary>
    private static readonly Bgr555 HeadYellow = new(31, 31, 0);

    internal static Bgr555 Color(int color) => color switch
    {
        0 => TransparentSlot,
        1 or 9 => Bgr555.White,
        2 => BodyHighlight,
        3 => BodySurface,
        4 => SidehopperInitialPalette.BodyShadow(BodySurface),
        5 => HeadGlint,
        6 => DetailLight,
        7 => SidehopperInitialPalette.DetailMidpoint(DetailLight, DetailDark),
        8 => DetailDark,
        10 => ClawLight,
        11 or 14 => SidehopperInitialPalette.ClawMidpoint(ClawLight, ClawDark),
        12 or 15 => ClawDark,
        13 => HeadYellow,
        _ => throw new ArgumentOutOfRangeException(nameof(color)),
    };
}
