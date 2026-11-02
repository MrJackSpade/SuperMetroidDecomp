using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

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
}
