namespace SuperMetroid.Core.Assets;

/// <summary>Standalone shared-corpse target paints from $A9:F8A6-F8C4.
/// The six categorical inks, shade ratios and rounding specify this chosen corpse painting; ink numbers are not sampled physical quantities. Its shifted fifteen-color auxiliary image has an independent disposition.</summary>
internal static class SidehopperCorpsePaintDefinitions
{
    /// <summary>$A9:F8A8, ink1: pale yellow limb/ligament highlight.</summary>
    private const ushort LimbLight = 31 | (31 << 5) | (21 << 10);
    /// <summary>$A9:F8AC, ink3: medium olive head/limb details, equal red/green.</summary>
    private const ushort OliveMedium = 9 | (9 << 5) | (2 << 10);
    /// <summary>$A9:F8AE, ink4: dark olive head/limb contours.</summary>
    private const ushort OliveDark = 5 | (5 << 5);
    /// <summary>$A9:F8B2, ink6: warm gray eyes visible on the Zoomer corpse.</summary>
    private const ushort EyeGray = 17 | (17 << 5) | (16 << 10);
    /// <summary>$A9:F8B6, ink8: ochre shell/body light, visible on the Ripper corpse.</summary>
    private const ushort BodyLight = 31 | (25 << 5) | (18 << 10);
    /// <summary>$A9:F8C4, ink15: ochre shell/body darkest shade, visible on the Skree corpse.</summary>
    private const ushort BodyDark = 5 | (3 << 5) | (1 << 10);
    /// <summary>$A9:F8A6 copied by EFAC-EFB5: standalone transparent-slot compatibility payload.
    /// The shifted auxiliary copy uses its first word as visible ink1 and is excluded.</summary>
    private const ushort TransparentSlot = 14 << 10;
    /// <summary>$A9:F8AA, ink2 is the selected nearest three-quarter pale-limb shade.</summary>
    private const int LimbShadeNumerator = 3, LimbShadeDenominator = 4;
    /// <summary>$A9:F8B4, ink7 is the selected floor two-thirds dark-body outline/mouth shade.</summary>
    private const int OutlineNumerator = 2, OutlineDenominator = 3;

    /// <summary>Resolves one of the sixteen selected corpse-palette inks to its packed SNES color word.</summary>
    /// <param name="color">Zero-based ink index in the standalone corpse paint; indices 8 through 15 interpolate the body shades.</param>
    /// <returns>The BGR555 word for the requested ink.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The ink index is outside the palette's 0-through-15 range.</exception>
    internal static ushort Color(int color) => color switch
    {
        0 => TransparentSlot,
        1 => LimbLight,
        2 => Scale(LimbLight, LimbShadeNumerator, LimbShadeDenominator, LimbShadeDenominator / 2),
        3 => OliveMedium,
        4 => OliveDark,
        5 => 31 | (31 << 5) | (31 << 10),
        6 => EyeGray,
        7 => Scale(BodyDark, OutlineNumerator, OutlineDenominator, 0),
        >= 8 and <= 15 => SidehopperCorpsePalette.Interpolate(BodyLight, BodyDark, color - 8),
        _ => throw new ArgumentOutOfRangeException(nameof(color)),
    };

    /// <summary>Applies one rounded integer ratio independently to each five-bit BGR component.</summary>
    /// <param name="color">Packed BGR555 source color.</param>
    /// <param name="numerator">Multiplier applied to each component before division.</param>
    /// <param name="denominator">Divisor used to produce the scaled component.</param>
    /// <param name="rounding">Nonnegative adjustment added before division to select the desired integer rounding.</param>
    /// <returns>The scaled BGR555 color with each component kept in its original channel position.</returns>
    private static ushort Scale(ushort color, int numerator, int denominator, int rounding)
    {
        int result = 0;
        for (int shift = 0; shift < 15; shift += 5)
            result |= (((color >> shift & 31) * numerator + rounding) / denominator) << shift;
        return (ushort)result;
    }
}
