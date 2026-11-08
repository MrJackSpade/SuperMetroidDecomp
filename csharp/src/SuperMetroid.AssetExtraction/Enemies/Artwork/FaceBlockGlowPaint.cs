using SuperMetroid.Core.Assets;
using static SuperMetroid.AssetExtraction.Enemies.Artwork.FaceBlockGlowPaintDefinitions;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>Paints the face block's eye and mouth glow as it rises and falls through its light depths.</summary>
internal static class FaceBlockGlowPaint
{
    /// <summary>Calculates one eye/mouth glow ink at its mirrored light depth, including the distinct resting and active accents.</summary>
    internal static RgbColor Color(int frame, int color)
    {
        if ((uint)frame >= 2 * (PeakDepth + 1) || (uint)color >= 4) throw new ArgumentOutOfRangeException(nameof(frame));
        int depth = Math.Min(frame, 2 * PeakDepth + 1 - frame);
        int green = Interpolate(0, PeakGreen, depth, PeakDepth);
        return color switch
        {
            0 => new FadeLevelColour(31, green, depth * BlueDepthStep).ToColour(),
            1 => new FadeLevelColour(Interpolate(MiddleRestRed, 31, depth, PeakDepth), (green + 1) / 2, 0).ToColour(),
            2 => new FadeLevelColour(Interpolate(ShadowRestRed, 31, depth, PeakDepth), 0, 0).ToColour(),
            3 when depth == 0 => new FadeLevelColour(ShadowRestRed + RestWarmAddition, RestWarmAddition, 0).ToColour(),
            3 => new FadeLevelColour(Interpolate(ActiveAccentRed, MiddleRestRed, depth - 1, PeakDepth - 1), 0, 0).ToColour(),
            _ => throw new ArgumentOutOfRangeException(nameof(color)),
        };
    }

    /// <summary>Rounds a channel to the nearest RGB5 level between selected glow endpoints.</summary>
    private static int Interpolate(int first, int last, int depth, int intervals) =>
        (first * (intervals - depth) + last * depth + intervals / 2) / intervals;
}
