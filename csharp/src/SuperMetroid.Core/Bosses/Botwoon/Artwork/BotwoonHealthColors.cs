using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Botwoon.Artwork;

/// <summary>Reads Botwoon's installed health-band colours: the authored range of each band and colour, and the content identity.</summary>
internal static class BotwoonHealthColors
{
    /// <summary>Hashes every health band's sixteen colours in band order.</summary>
    internal static string ContentIdentity(BotwoonColorCatalog colors) =>
        SelectedPresentationHash.Create("BotwoonColorCatalog-v1", content =>
        {
            content.Append("health", BotwoonHealthPaletteDefinitions.PaletteCount);
            Span<RgbColor> row = stackalloc RgbColor[BotwoonHealthPaletteDefinitions.ColorsPerPalette];
            for (int band = 0; band < BotwoonHealthPaletteDefinitions.PaletteCount; band++)
            {
                for (int color = 0; color < row.Length; color++) row[color] = colors.HealthColor(band, color);
                content.AppendColors("row", row);
            }
        });

    /// <summary>Reads one colour of one health band, rejecting an index outside the authored images.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Either index is outside its authored range.</exception>
    internal static RgbColor Read(BotwoonHealthPaint health, int band, int color)
    {
        if ((uint)band >= BotwoonHealthPaletteDefinitions.PaletteCount ||
            (uint)color >= BotwoonHealthPaletteDefinitions.ColorsPerPalette)
            throw new ArgumentOutOfRangeException(nameof(band),
                $"Botwoon health band {band}, color {color} is outside the authored images.");
        return health.Resolve(band, color);
    }
}
