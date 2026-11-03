using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// Draygon's installed colours at work: the white hurt flash and health bands resolved from their edits, the opening,
/// hurt and health-band installs, and the identity of the selected colours.
/// </summary>
internal static class DraygonPalette
{
    /// <summary>Gets one of the 16 selected white-flash colors, preserving transparent entry zero.</summary>
    internal static RgbColor WhiteFlash(Dictionary<ColourCell, RgbColor> edits, int color)
    {
        if ((uint)color >= DraygonColorDefinitions.WhiteFlashCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(new ColourCell(0, color), out RgbColor selected) ? selected : StockWhiteFlash(color);
    }

    /// <summary>Gets one of the four selected colors of one of Draygon's eight health bands.</summary>
    internal static RgbColor HealthBand(DraygonHealthPaint healthBands, int band, int color)
    {
        _ = CheckBand(band);
        if ((uint)color >= DraygonColorDefinitions.HealthBandColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return healthBands.Color(band, color);
    }

    /// <summary>The white flash preserves the backdrop; all visible inks are white.</summary>
    internal static RgbColor StockWhiteFlash(int color) => color == 0 ? DraygonMaterialPaint.ClearTarget : DraygonColorCatalog.FlashInk;

    /// <summary>Validates a health band before resolving one of its colors.</summary>
    private static int CheckBand(int band) =>
        (uint)band < DraygonColorDefinitions.HealthBandCount
            ? band
            : throw new ArgumentOutOfRangeException(nameof(band));
}
