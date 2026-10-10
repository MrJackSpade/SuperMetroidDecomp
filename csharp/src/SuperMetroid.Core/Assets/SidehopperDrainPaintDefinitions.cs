using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Exact seven15-color shifted drain images at $A9:EBCC-EC8C.
/// Reviewed source-prefix paint application, three final paints and selected shade progression. Source3800 is visible dark-blue ink1 here, not transparent; this particular source-prefix application is separately reviewed from the standalone targets.</summary>
internal static class SidehopperDrainPaintDefinitions
{
    /// <summary>$A9:EC6E, source slot1 becomes visible ink2: pale body/limb edge paint.</summary>
    private static readonly Bgr555 BodyLight = new(31, 27, 22);
    /// <summary>$A9:EC76, source slot5 becomes visible ink6: installed detail light endpoint.
    /// Its precise visible pixels are not identified in the three alive maps.</summary>
    private static readonly Bgr555 DetailLight = new(27, 24, 19);
    /// <summary>$A9:EC7C, source slot8 becomes visible ink9: dark detail/mouth endpoint.</summary>
    private static readonly Bgr555 DetailDark = new(12, 10, 7);
    /// <summary>$A9:EBCC-EC6C contains six temporal levels including both endpoints.</summary>
    private const int DrainIntervals = 5;
    /// <summary>$A9:EC8C is the seventh separately selected corpse-prefix image.</summary>
    private const int CorpseFrame = 6;

    internal static bool Matches(Bgr555[][] frames)
    {
        for (int frame = 0; frame <= CorpseFrame; frame++)
        for (int color = 0; color < 15; color++)
            if (frames[frame][color] != Color(frame, color)) return false;
        return true;
    }

    internal static Bgr555 Color(int frame, int color)
    {
        if ((uint)frame > CorpseFrame || (uint)color >= 15) throw new ArgumentOutOfRangeException(nameof(frame));
        if (frame == CorpseFrame) return SidehopperCorpsePaintDefinitions.Color(color);
        Bgr555 start = SidehopperInitialPaintDefinitions.Color(color), end = Endpoint(color);
        return start.Zip(end, (_, a, b) => ((a * (DrainIntervals - frame) + b * frame + DrainIntervals / 2) / DrainIntervals));
    }

    private static Bgr555 Endpoint(int color) => color switch
    {
        0 => SidehopperInitialPaintDefinitions.Color(0),
        >= 1 and <= 8 => SidehopperDrainShadeDefinitions.Resolve(color, BodyLight, DetailLight, DetailDark, SidehopperCorpsePaintDefinitions.Color(15)),
        >= 9 and <= 12 => SidehopperCorpsePaintDefinitions.Color(color - 8),
        13 or 14 => SidehopperCorpsePaintDefinitions.Color(color - 10),
        _ => throw new ArgumentOutOfRangeException(nameof(color)),
    };
}
