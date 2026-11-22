using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's installed eye colours (#1512): his three eyes' colours under the hurt flash and under each of his eight health
/// bands (#2297). Gameplay reads them back through <see cref="KraidEyeGlow"/>, which shows a band's eyes, starts each glow from
/// them and fades back to them, and times his roar from the eyes reaching them; the rest of his colours belong to the
/// import, which pre-colours his pictures.
/// </summary>
public sealed partial class KraidEyeColours
{
    /// <summary>The eyes' colours under the hurt flash.</summary>
    private readonly RgbColor[] hurt;

    /// <summary>The eyes' colours under each health band.</summary>
    private readonly Dictionary<KraidHealthBand, RgbColor[]> bands;

    /// <summary>Takes the eyes' colours under the hurt flash and under each health band, a colour for each
    /// eye.</summary>
    /// <param name="hurt">The eyes' colours under the hurt flash.</param>
    /// <param name="healthBands">The eyes' colours under each of the eight health bands.</param>
    /// <exception cref="ArgumentException">A set does not hold one colour per eye, or a band has none.</exception>
    internal KraidEyeColours(IReadOnlyList<RgbColor> hurt, IReadOnlyDictionary<KraidHealthBand, IReadOnlyList<RgbColor>> healthBands)
    {
        Ensure.NotNull(hurt);
        Ensure.NotNull(healthBands);
        this.hurt = Eyes(hurt, nameof(hurt));
        bands = new Dictionary<KraidHealthBand, RgbColor[]>(ReferenceEqualityComparer.Instance);
        foreach (KraidHealthBand band in KraidHealthBandDefinitions.HighestFirst)
        {
            bands[band] = healthBands.TryGetValue(band, out IReadOnlyList<RgbColor>? colours)
                ? Eyes(colours, nameof(healthBands))
                : throw new ArgumentException($"Kraid's eye colours hold none for his {band} band.", nameof(healthBands));
        }
        if (healthBands.Count != bands.Count)
            throw new ArgumentException($"Kraid has {bands.Count} health bands, not {healthBands.Count}.", nameof(healthBands));
    }

    /// <summary>The eyes' colours under the hurt flash.</summary>
    internal ReadOnlySpan<RgbColor> Hurt => hurt;

    /// <summary>
    /// The colours every shade of his body shows his eyes in, his most hurt band's: his eye glow shows the eyes' colours
    /// now in their place, so the eyes show what the glow holds whichever band or flash his body shows.
    /// </summary>
    internal ReadOnlySpan<RgbColor> Shaded => bands[KraidHealthBandDefinitions.BelowQuarter];

    /// <summary>The eyes' colours under health band <paramref name="band"/>.</summary>
    internal ReadOnlySpan<RgbColor> Of(KraidHealthBand band) => bands[Ensure.NotNull(band)];

    /// <summary>A copy of one eye colour set, which must hold one colour per eye.</summary>
    private static RgbColor[] Eyes(IReadOnlyList<RgbColor> colours, string parameter)
    {
        Ensure.NotNull(colours);
        if (colours.Count != KraidEyeGlow.Eyes)
            throw new ArgumentException($"Kraid's eye colours hold one colour for each of his {KraidEyeGlow.Eyes} eyes, not {colours.Count}.", parameter);
        return [.. colours];
    }
}
