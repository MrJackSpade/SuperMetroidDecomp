using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Bosses.MotherBrain.Artwork.ZebetitePulsePaintDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Paints the two Zebetite core facets of each pulse frame from the catalog's selected red, green bias and shade.</summary>
internal static class ZebetitePulsePaint
{
    /// <summary>Computes one of two symmetric red/green core facets for a pulse frame, leaving blue at zero.</summary>
    internal static RgbColor Color(int frame, int color)
    {
        int intervals = ZebetiteColorFormat.FrameCount / 2;
        int phase = Math.Min(frame, ZebetiteColorFormat.FrameCount - frame);
        int redStart = color == 0 ? ChannelMaximum : ChannelMaximum * DarkShadeNumerator / DarkShadeDenominator;
        int greenStart = color == 0 ? WarmGreenBias : WarmGreenBias / 2;
        int red = redStart + (ChannelMaximum - redStart) * phase / intervals;
        int green = greenStart * (intervals - phase) / intervals;
        return new RgbColor(red, green, 0);
    }
}
