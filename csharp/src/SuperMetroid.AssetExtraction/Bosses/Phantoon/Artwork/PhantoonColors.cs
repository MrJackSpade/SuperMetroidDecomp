using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Phantoon;

namespace SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork;

/// <summary>
/// Phantoon's installed colours: how each health, fade-out and ship-power colour resolves from its edit or the
/// calculated paint, and the identity of all three.
/// </summary>
internal static class PhantoonColors
{
    /// <summary>Resolves one health-band colour from its edit or the calculated shade.</summary>
    internal static RgbColor Health(Dictionary<ColourCell, RgbColor> edits, int band, int color)
    {
        _ = CheckBand(band);
        if ((uint)color >= PhantoonColorDefinitions.HealthBandColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        var key = new ColourCell(band, color);
        return edits.TryGetValue(key, out RgbColor edit) ? edit : PhantoonHealthPaint.Color(band, color);
    }

    /// <summary>Resolves one fade-out target: its edit, or black.</summary>
    internal static RgbColor FadeOut(Dictionary<ColourCell, RgbColor> edits, int color)
    {
        if ((uint)color >= PhantoonColorDefinitions.FadeOutCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return edits.GetValueOrDefault(new ColourCell(0, color));
    }

    /// <summary>Resolves one powered-ship colour of a faded part from its edit or the calculated paint.</summary>
    internal static RgbColor PowerOn(Dictionary<WreckedShipPoweredColour, RgbColor> edits, WreckedShipPoweredPart part, int color)
    {
        if (!PhantoonColorDefinitions.PoweredParts.Contains(part))
            throw new ArgumentOutOfRangeException(nameof(part), part, "The Wrecked Ship's power-on does not fade this part.");
        if ((uint)color >= WreckedShipPowerPaintDefinitions.PartColourCount) throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(new WreckedShipPoweredColour(part, color), out RgbColor edit) ? edit : WreckedShipPowerPaint.Color(part, color);
    }

    /// <summary>Rejects a health palette index outside the eight Phantoon bands.</summary>
    private static int CheckBand(int band) =>
        (uint)band < PhantoonColorDefinitions.HealthBandCount
            ? band
            : throw new ArgumentOutOfRangeException(nameof(band));
}
