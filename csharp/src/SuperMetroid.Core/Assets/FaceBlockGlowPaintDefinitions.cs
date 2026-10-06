namespace SuperMetroid.Core.Assets;

/// <summary>Reviewed eye/mouth glow composition at $A8:E7CC-E80B, copied to inks9..12 by E893-E8AA.
/// Six selected intensity roles specify this painted glow; timers, Morph gate and blue-face artwork are excluded.</summary>
internal static class FaceBlockGlowPaintDefinitions
{
    /// <summary>$A8:E7CE/E7EA: resting middle red and brightest accent both use18.</summary>
    private const int MiddleRestRed = 18;
    /// <summary>$A8:E7D0: resting red shadow.</summary>
    private const int ShadowRestRed = 10;
    /// <summary>$A8:E7E4: main glow's strongest green channel.</summary>
    private const int PeakGreen = 25;
    /// <summary>$A8:E7CC/E7D4/E7DC/E7E4: main-glow blue advances one unit per light depth.</summary>
    private const int BlueDepthStep = 1;
    /// <summary>$A8:E7D2: resting accent adds one red/green unit to the red shadow.</summary>
    private const int RestWarmAddition = 1;
    /// <summary>$A8:E7DA: first active accent red, separate from its warm resting paint.</summary>
    private const int ActiveAccentRed = 7;
    /// <summary>$A8:E7CC-E80B: four light depths followed by their reverse, including both repeated ends.</summary>
    private const int PeakDepth = 3;

    internal static bool Matches(ushort[][] frames)
    {
        for (int frame = 0; frame < 2 * (PeakDepth + 1); frame++)
        for (int color = 0; color < 4; color++)
            if (frames[frame][color] != Color(frame, color)) return false;
        return true;
    }

    internal static ushort Color(int frame, int color)
    {
        if ((uint)frame >= 2 * (PeakDepth + 1) || (uint)color >= 4) throw new ArgumentOutOfRangeException(nameof(frame));
        int depth = Math.Min(frame, 2 * PeakDepth + 1 - frame);
        int green = Interpolate(0, PeakGreen, depth, PeakDepth);
        return color switch
        {
            0 => (ushort)(31 | green << 5 | depth * BlueDepthStep << 10),
            1 => (ushort)(Interpolate(MiddleRestRed, 31, depth, PeakDepth) | (green + 1) / 2 << 5),
            2 => (ushort)Interpolate(ShadowRestRed, 31, depth, PeakDepth),
            3 when depth == 0 => (ushort)(ShadowRestRed + RestWarmAddition | RestWarmAddition << 5),
            3 => (ushort)Interpolate(ActiveAccentRed, MiddleRestRed, depth - 1, PeakDepth - 1),
            _ => throw new ArgumentOutOfRangeException(nameof(color)),
        };
    }

    private static int Interpolate(int first, int last, int depth, int intervals) =>
        (first * (intervals - depth) + last * depth + intervals / 2) / intervals;
}
