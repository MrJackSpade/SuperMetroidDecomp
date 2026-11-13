using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's installed eye colours (#1512): his three eyes' colours under the hurt flash and under each of his eight health
/// bands. Gameplay reads them back through <see cref="KraidEyeGlow"/>, which shows a band's eyes, starts each glow from
/// them and fades back to them, and times his roar from the eyes reaching them; the rest of his colours belong to the
/// import, which pre-colours his pictures.
/// </summary>
public sealed class KraidEyeColours
{
    /// <summary>The eyes' colours under the hurt flash.</summary>
    private readonly RgbColor[] hurt;

    /// <summary>The eyes' colours under each health band, lowest band first.</summary>
    private readonly RgbColor[][] bands;

    /// <summary>Takes the eyes' colours under the hurt flash and under each health band, three colours to each.</summary>
    /// <param name="hurt">The eyes' colours under the hurt flash.</param>
    /// <param name="healthBands">The eyes' colours under each of the eight health bands, lowest band first.</param>
    /// <exception cref="ArgumentException">A set does not hold one colour per eye, or the bands are not eight.</exception>
    internal KraidEyeColours(IReadOnlyList<RgbColor> hurt, IReadOnlyList<IReadOnlyList<RgbColor>> healthBands)
    {
        Ensure.NotNull(hurt);
        Ensure.NotNull(healthBands);
        if (healthBands.Count != KraidHealthThresholdDefinitions.EighthCount)
            throw new ArgumentException($"Kraid has {KraidHealthThresholdDefinitions.EighthCount} health bands, not {healthBands.Count}.", nameof(healthBands));
        this.hurt = Eyes(hurt, nameof(hurt));
        bands = [.. healthBands.Select(band => Eyes(band, nameof(healthBands)))];
    }

    /// <summary>The eyes' colours under the hurt flash.</summary>
    internal ReadOnlySpan<RgbColor> Hurt => hurt;

    /// <summary>
    /// The colours every shade of his body shows his eyes in, his first health band's: his eye glow shows the eyes' colours
    /// now in their place, so the eyes show what the glow holds whichever band or flash his body shows.
    /// </summary>
    internal ReadOnlySpan<RgbColor> Shaded => bands[0];

    /// <summary>The eyes' colours under <paramref name="band"/>: a health band's, or the hurt flash's.</summary>
    internal ReadOnlySpan<RgbColor> Of(KraidColourBand band) => band.HealthBand is { } health ? HealthBand(health) : Hurt;

    /// <summary>The eyes' colours under health band <paramref name="band"/>, zero through seven.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The band is not one of the eight.</exception>
    internal ReadOnlySpan<RgbColor> HealthBand(int band) =>
        bands[Ensure.BetweenInclusive(0, KraidHealthThresholdDefinitions.EighthCount - 1, band)];

    /// <summary>A copy of one eye colour set, which must hold one colour per eye.</summary>
    private static RgbColor[] Eyes(IReadOnlyList<RgbColor> colours, string parameter)
    {
        Ensure.NotNull(colours);
        if (colours.Count != KraidEyeGlow.Eyes)
            throw new ArgumentException($"Kraid's eye colours hold one colour for each of his {KraidEyeGlow.Eyes} eyes, not {colours.Count}.", parameter);
        return [.. colours];
    }
}
