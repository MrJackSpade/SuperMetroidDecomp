using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Botwoon;

namespace SuperMetroid.AssetExtraction.Bosses.Botwoon.Artwork;

/// <summary>Reads Botwoon's installed health-band colours: the authored range of each band and colour, and the content identity.</summary>
internal static class BotwoonHealthColors
{
    /// <summary>Reads one colour of one health band, rejecting an index outside the authored images.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Either index is outside its authored range.</exception>
    internal static RgbColor Read(BotwoonHealthPaint health, int band, int color)
    {
        if ((uint)band >= BotwoonHealthBandDefinitions.BandCount ||
            (uint)color >= BotwoonColorFormat.ColorsPerPalette)
            throw new ArgumentOutOfRangeException(nameof(band),
                $"Botwoon health band {band}, color {color} is outside the authored images.");
        return health.Resolve(band, color);
    }
}
