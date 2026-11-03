using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;

/// <summary>
/// Compares Kraid's five supplied colour sources with the stock paint and keeps only their deviations, the bands the game
/// resolves its colours from; the secondary bands share the health bands' storage when the supplied colours match.
/// </summary>
internal static class KraidColorEdits
{
    /// <summary>Compiles five authored sources, sharing health-band storage only when secondary colors match exactly.</summary>
    internal static Dictionary<KraidPaletteSource, IReadOnlyList<RgbColor>> Sources(RgbColor[] roomBackdrop,
        RgbColor[] initialTarget, RgbColor[] health, RgbColor[] secondary, RgbColor[] deathArm)
    {
        KraidHealthBands healthBands = HealthBands(health);
        return new()
        {
            [KraidPaletteSource.RoomBackdrop] = StockBand(KraidPaletteSource.RoomBackdrop, roomBackdrop),
            [KraidPaletteSource.InitialTarget] = StockBand(KraidPaletteSource.InitialTarget, initialTarget),
            [KraidPaletteSource.Health] = healthBands,
            [KraidPaletteSource.Secondary] = secondary.SequenceEqual(healthBands) ? healthBands : HealthBands(secondary),
            [KraidPaletteSource.DeathArm] = StockBand(KraidPaletteSource.DeathArm, deathArm),
        };
    }

    /// <summary>Records independent edits to one sixteen-color source.</summary>
    private static KraidStockBand StockBand(KraidPaletteSource source, RgbColor[] supplied)
    {
        var deviations = new Dictionary<ColourCell, RgbColor>();
        for (int index = 0; index < supplied.Length; index++)
            if (KraidPaint.Color(source, index) != supplied[index])
                deviations.Add(new ColourCell(0, index), supplied[index]);
        return new KraidStockBand(source, deviations);
    }

    /// <summary>
    /// Records endpoint edits before comparing interior colors against those selected endpoints: interior interpolation
    /// reads the (possibly edited) endpoints, so the bands compare each interior colour once the endpoints are in place.
    /// </summary>
    private static KraidHealthBands HealthBands(RgbColor[] supplied)
    {
        var deviations = new Dictionary<ColourCell, RgbColor>();
        for (int index = 0; index < supplied.Length; index++)
            if (KraidPaint.HealthAuthored(index) is RgbColor stock && stock != supplied[index])
                deviations.Add(new ColourCell(0, index), supplied[index]);
        var bands = new KraidHealthBands(deviations);
        var interior = new Dictionary<ColourCell, RgbColor>();
        for (int index = 0; index < supplied.Length; index++)
            if (KraidPaint.HealthAuthored(index) is null && bands[index] != supplied[index])
                interior.Add(new ColourCell(0, index), supplied[index]);
        foreach ((ColourCell cell, RgbColor colour) in interior)
            deviations.Add(cell, colour);
        return bands;
    }
}
