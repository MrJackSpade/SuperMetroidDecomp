namespace SuperMetroid.Core.Assets;

/// <summary>Standalone Sidehopper target paints at $A9:F8C6-F8E4.
/// Reviewed categorical paints and material composition: ink numbers select painted pixel classes, not a measured lighting value. The shifted fifteen-color auxiliary copy has a separate disposition.</summary>
internal static class SidehopperInitialPaintDefinitions
{
    /// <summary>$A9:F8CA, ink2: cyan shell/leg edge highlights.</summary>
    private const ushort BodyHighlight = (23 << 5) | (21 << 10);
    /// <summary>$A9:F8CC, ink3: cyan shell/leg surfaces with equal green/blue.</summary>
    private const ushort BodySurface = (12 << 5) | (12 << 10);
    /// <summary>$A9:F8D0, ink5: upper-head glint.</summary>
    private const ushort HeadGlint = (30 << 5) | (26 << 10);
    /// <summary>$A9:F8D2, ink6: installed inner-detail light endpoint.
    /// Its exact visible pixels are unidentified in the three native alive compositions.</summary>
    private const ushort DetailLight = (22 << 5) | (23 << 10);
    /// <summary>$A9:F8D6, ink8: dark inner head/limb details with equal green/blue.</summary>
    private const ushort DetailDark = (13 << 5) | (13 << 10);
    /// <summary>$A9:F8DA, ink10: yellow mouth/claw light with equal red/green.</summary>
    private const ushort ClawLight = 28 | (28 << 5);
    /// <summary>$A9:F8DE, ink12: brown mouth/claw shadow.</summary>
    private const ushort ClawDark = 17 | (6 << 5);
    /// <summary>$A9:F8C6, standalone target ink0 copied by EF92-EF9B;
    /// OBJ ink0 is transparent. This does not cover shifted auxiliary visible ink1.</summary>
    private const ushort TransparentSlot = 14 << 10;
    /// <summary>$A9:F8E0, ink13: saturated-yellow paired head features.</summary>
    private const ushort HeadYellow = 31 | (31 << 5);

    /// <summary>Returns the standalone target's painted BGR555 value for one four-bit Sidehopper palette index.</summary>
    /// <param name="color">Palette ink index from 0 through 15.</param>
    /// <returns>The native 15-bit color word associated with the selected painted ink.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The ink index is outside the palette's 0–15 range.</exception>
    internal static ushort Color(int color) => color switch
    {
        0 => TransparentSlot,
        1 or 9 => 31 | (31 << 5) | (31 << 10),
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
