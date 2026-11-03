using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley;
namespace SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

/// <summary>
/// Ridley health colors, uniformly copied by the health-palette update.
/// The selected armor/eye color composition tints toward the existing middle
/// membrane paint, with strengths2/20,3/20,4/20. Recessed armor green
/// holds its healthy shade in the first band; middle plate blue holds the preceding
/// shade in the second band. Those two chosen material/phase memberships preserve
/// the painted shading, with channel magnitudes derived from their previous shade.
/// No new paint magnitudes, health thresholds, timing or pixels are retained here.
/// </summary>
internal sealed class CeresRidleyHealthPaint
{
    /// <summary>Source healthy body and eye paints from which the three damage bands are calculated.</summary>
    private readonly CeresRidleyFade body, eyes;
    /// <summary>Authored departures from the calculated health shades, keyed by band and color.</summary>
    internal readonly Dictionary<PaintColour, RgbColor> edits;

    /// <summary>Keeps the healthy paints and the input colors that differ from the calculated tint.</summary>
    /// <param name="body">The body fade supplying healthy armor and the membrane target.</param>
    /// <param name="eyes">The eye fade supplying healthy eyes.</param>
    /// <param name="edits">The supplied colors the import found differing from the tint.</param>
    internal CeresRidleyHealthPaint(CeresRidleyFade body, CeresRidleyFade eyes, Dictionary<PaintColour, RgbColor> edits)
    {
        this.body = body; this.eyes = eyes;
        this.edits = edits;
    }

    /// <summary>Uses an authored override where present, otherwise calculates the selected damage-band shade.</summary>
    internal RgbColor Resolve(int band, int color)
    {
        if ((uint)band >= CeresRidleyPaletteDefinitions.HealthBandCount) throw new ArgumentOutOfRangeException(nameof(band));
        if ((uint)color >= CeresRidleyPaletteDefinitions.HealthColorCount) throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(new PaintColour(band, color), out RgbColor edited) ? edited : Calculate(band, color);
    }

    /// <summary>Tints healthy armor or eyes toward middle membrane paint, preserving two chosen channel shades.</summary>
    private RgbColor Calculate(int band, int color)
    {
        RgbColor healthy = color < 11 ? body.Resolve(15, color) : eyes.Resolve(0, color - 11);
        RgbColor target = body.Resolve(15, 9);
        int strength = 2 + band;
        return healthy.Zip(target, (channel, original, toward) =>
        {
            if (band == 0 && color == 2 && channel == ColorChannel.Green) return original;
            if (band == 1 && color == 5 && channel == ColorChannel.Blue) return Calculate(0, color).Blue;
            return (original * (20 - strength) + toward * strength) / 20;
        });
    }
}
