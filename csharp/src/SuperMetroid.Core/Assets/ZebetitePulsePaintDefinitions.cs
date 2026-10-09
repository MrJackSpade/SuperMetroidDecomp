namespace SuperMetroid.Core.Assets;

/// <summary>
/// Selected red barrier-core paint at $A6:FD87-FDA6, copied to OBJ palette2 inksC/D
/// by $A6:FD7C-82. Native $A6:FE08 and the Rinka sheet $AE:B800 use these inks for
/// adjacent bright/dark core facets. Exact hue, shade ratio and symmetric pulse
/// are chosen illumination content; no health, collision or cadence is defined here.
/// </summary>
internal static class ZebetitePulsePaintDefinitions
{
    /// <summary>Maximum value of one five-bit SNES palette channel.</summary>
    private const int ChannelMaximum = 31;
    /// <summary>$A6:FD87/$A2:BA73: warm-green bias2 on the saturated-red core.
    /// The neighboring darker facet uses half this green bias.</summary>
    private const int WarmGreenBias = 2;
    /// <summary>$A6:FD89/$A2:BA75: the darker facet begins at floor(3/4 of red31)=23.
    /// Three-quarter shade is the selected material contrast, not a health ratio.</summary>
    private const int DarkShadeNumerator = 3;
    /// <summary>Divisor paired with <see cref="DarkShadeNumerator"/> to set the darker red facet to three quarters of full intensity.</summary>
    private const int DarkShadeDenominator = 4;

    /// <summary>Calculates one of the two packed RGB5 paints along the selected symmetric barrier-core pulse.</summary>
    /// <param name="frame">The zero-based pulse row, from zero through <see cref="ZebetiteColorFormat.FrameCount"/> minus one.</param>
    /// <param name="color">Selects the bright red facet (0) or its darker adjacent facet (1).</param>
    internal static ushort Color(int frame, int color)
    {
        int intervals = ZebetiteColorFormat.FrameCount / 2;
        int phase = Math.Min(frame, ZebetiteColorFormat.FrameCount - frame);
        int redStart = color == 0 ? ChannelMaximum : ChannelMaximum * DarkShadeNumerator / DarkShadeDenominator;
        int greenStart = color == 0 ? WarmGreenBias : WarmGreenBias / 2;
        int red = redStart + (ChannelMaximum - redStart) * phase / intervals;
        int green = greenStart * (intervals - phase) / intervals;
        return (ushort)(red | green << 5);
    }
}
