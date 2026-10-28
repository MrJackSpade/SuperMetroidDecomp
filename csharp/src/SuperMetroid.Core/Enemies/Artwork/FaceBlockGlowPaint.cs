using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Enemies.Artwork.FaceBlockGlowPaintDefinitions;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Paints the face block's eye and mouth glow as it rises and falls through its light depths.</summary>
internal static class FaceBlockGlowPaint
{
    /// <summary>Accepts the supplied eight-frame, four-ink glow only when it matches the reviewed rise-and-fall calculation.</summary>
    internal static bool Matches(RgbColor[][] frames)
    {
        for (int frame = 0; frame < 2 * (PeakDepth + 1); frame++)
        for (int color = 0; color < 4; color++)
            if (frames[frame][color] != Color(frame, color)) return false;
        return true;
    }

    /// <summary>Calculates one eye/mouth glow ink at its mirrored light depth, including the distinct resting and active accents.</summary>
    internal static RgbColor Color(int frame, int color)
    {
        if ((uint)frame >= 2 * (PeakDepth + 1) || (uint)color >= 4) throw new ArgumentOutOfRangeException(nameof(frame));
        int depth = Math.Min(frame, 2 * PeakDepth + 1 - frame);
        int green = Interpolate(0, PeakGreen, depth, PeakDepth);
        return color switch
        {
            0 => new RgbColor(31, green, depth * BlueDepthStep),
            1 => new RgbColor(Interpolate(MiddleRestRed, 31, depth, PeakDepth), (green + 1) / 2, 0),
            2 => new RgbColor(Interpolate(ShadowRestRed, 31, depth, PeakDepth), 0, 0),
            3 when depth == 0 => new RgbColor(ShadowRestRed + RestWarmAddition, RestWarmAddition, 0),
            3 => new RgbColor(Interpolate(ActiveAccentRed, MiddleRestRed, depth - 1, PeakDepth - 1), 0, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(color)),
        };
    }

    /// <summary>Rounds a channel to the nearest RGB5 level between selected glow endpoints.</summary>
    private static int Interpolate(int first, int last, int depth, int intervals) =>
        (first * (intervals - depth) + last * depth + intervals / 2) / intervals;
}
