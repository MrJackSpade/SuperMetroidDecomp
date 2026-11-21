using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Compatibility;

namespace SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;

/// <summary>Reads Kraid's installed colour sources: one colour of a named source, and the identity of all five.</summary>
internal static class KraidColors
{
    /// <summary>
    /// Selects one of five named editable sources directly. Each source keeps its
    /// own loaded color bounds; unknown sources and out-of-range indices are rejected.
    /// </summary>
    internal static RgbColor Resolve(KraidColorCatalog colors, KraidPaletteSource source, int index)
    {
        if (!colors.sources.TryGetValue(source, out IReadOnlyList<RgbColor>? band))
            throw new ArgumentOutOfRangeException(nameof(source));
        if ((uint)index >= band.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        return band[index];
    }

    /// <summary>
    /// The eyes' colours under the hurt flash and under each health band, which gameplay's eye glow reads back (#1512):
    /// the health colours hold the hurt flash's colour group and then each band's, the eyes starting at the same colour
    /// of each.
    /// </summary>
    internal static KraidEyeColours Eyes(KraidColorCatalog colors)
    {
        Dictionary<PictureShade, RgbColor[]> byShade = KraidPalettes.HealthSequence.ToDictionary(band => band.Shade, band => EyeSet(colors, band));
        return new(byShade[KraidShades.HurtFlash],
            KraidHealthBandDefinitions.HighestFirst.ToDictionary(band => band, band => (IReadOnlyList<RgbColor>)byShade[KraidShades.Health(band)]));
    }

    /// <summary>The eyes' colours of the installed health colours' band <paramref name="band"/>.</summary>
    private static RgbColor[] EyeSet(KraidColorCatalog colors, KraidHealthSequenceBand band) =>
        [.. Enumerable.Range(band.FirstColour + KraidPalettes.FirstEyeColour, KraidEyeGlow.Eyes)
            .Select(colour => Resolve(colors, KraidPaletteSource.Health, colour))];
}
