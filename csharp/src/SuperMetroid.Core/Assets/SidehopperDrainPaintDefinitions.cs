namespace SuperMetroid.Core.Assets;

/// <summary>Exact seven15-color shifted drain images at $A9:EBCC-EC8C.
/// Reviewed source-prefix paint application, three final paints and selected shade progression. Source3800 is visible dark-blue ink1 here, not transparent; this particular source-prefix application is separately reviewed from the standalone targets.</summary>
internal static class SidehopperDrainPaintDefinitions
{
    /// <summary>$A9:EC6E, source slot1 becomes visible ink2: pale body/limb edge paint.</summary>
    private const ushort BodyLight = 31 | (27 << 5) | (22 << 10);
    /// <summary>$A9:EC76, source slot5 becomes visible ink6: installed detail light endpoint.
    /// Its precise visible pixels are not identified in the three alive maps.</summary>
    private const ushort DetailLight = 27 | (24 << 5) | (19 << 10);
    /// <summary>$A9:EC7C, source slot8 becomes visible ink9: dark detail/mouth endpoint.</summary>
    private const ushort DetailDark = 12 | (10 << 5) | (7 << 10);
    /// <summary>$A9:EBCC-EC6C contains six temporal levels including both endpoints.</summary>
    private const int DrainIntervals = 5;
    /// <summary>$A9:EC8C is the seventh separately selected corpse-prefix image.</summary>
    private const int CorpseFrame = 6;

    /// <summary>Checks the seven supplied 15-color frames against the compiled drain progression and corpse paint.</summary>
    /// <param name="frames">Frame-major RGB5 colors to compare with the reviewed artwork sequence.</param>
    /// <returns><see langword="true"/> when every color in each expected frame matches.</returns>
    internal static bool Matches(ushort[][] frames)
    {
        for (int frame = 0; frame <= CorpseFrame; frame++)
        for (int color = 0; color < 15; color++)
            if (frames[frame][color] != Color(frame, color)) return false;
        return true;
    }

    /// <summary>Returns one RGB5 color from the drain progression or its final corpse frame.</summary>
    /// <param name="frame">The sequence index, from the initial pose through the final corpse image.</param>
    /// <param name="color">The palette slot in the 15-color RGB5 image.</param>
    /// <returns>The interpolated drain color, or the corpse color for the final frame.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The frame or color index is outside the compiled sequence or palette.</exception>
    internal static ushort Color(int frame, int color)
    {
        if ((uint)frame > CorpseFrame || (uint)color >= 15) throw new ArgumentOutOfRangeException(nameof(frame));
        if (frame == CorpseFrame) return SidehopperCorpsePaintDefinitions.Color(color);
        ushort start = SidehopperInitialPaintDefinitions.Color(color), end = Endpoint(color);
        int result = 0;
        for (int shift = 0; shift < 15; shift += 5)
            result |= (((start >> shift & 31) * (DrainIntervals - frame) + (end >> shift & 31) * frame + DrainIntervals / 2) / DrainIntervals) << shift;
        return (ushort)result;
    }

    /// <summary>Selects the reviewed final RGB5 value for a palette slot before temporal interpolation.</summary>
    /// <param name="color">The palette slot whose drain endpoint is requested.</param>
    /// <returns>The initial color, a reviewed shade, or the corresponding corpse-frame color.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The color slot has no defined drain endpoint.</exception>
    private static ushort Endpoint(int color) => color switch
    {
        0 => SidehopperInitialPaintDefinitions.Color(0),
        >= 1 and <= 8 => SidehopperDrainShadeDefinitions.Resolve(color, BodyLight, DetailLight, DetailDark, SidehopperCorpsePaintDefinitions.Color(15)),
        >= 9 and <= 12 => SidehopperCorpsePaintDefinitions.Color(color - 8),
        13 or 14 => SidehopperCorpsePaintDefinitions.Color(color - 10),
        _ => throw new ArgumentOutOfRangeException(nameof(color)),
    };
}
