using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>Reads Kraid's installed colour sources: one colour of a named source, and the identity of all five.</summary>
internal static class KraidColors
{
    /// <summary>Hashes the five sources, each tagged by its position: room backdrop, initial target, health, secondary, death arm.</summary>
    internal static string ContentIdentity(KraidColorCatalog colors) =>
        SelectedPresentationHash.Create("enemy-kraid-colors-v1", content =>
        {
            IReadOnlyList<RgbColor>[] rows = [colors.roomBackdrop, colors.initialTarget, colors.health, colors.secondary, colors.deathArm];
            for (int tag = 0; tag < rows.Length; tag++)
            {
                content.Append("source", tag);
                content.AppendColors("colors", rows[tag].ToArray());
            }
        });

    /// <summary>
    /// Selects one of five named editable sources directly. Each source keeps its
    /// own loaded color bounds; unknown sources and out-of-range indices are rejected.
    /// </summary>
    internal static RgbColor Resolve(KraidColorCatalog colors, KraidPaletteSource source, int index)
    {
        IReadOnlyList<RgbColor> band = source switch
        {
            KraidPaletteSource.RoomBackdrop => colors.roomBackdrop,
            KraidPaletteSource.InitialTarget => colors.initialTarget,
            KraidPaletteSource.Health => colors.health,
            KraidPaletteSource.Secondary => colors.secondary,
            KraidPaletteSource.DeathArm => colors.deathArm,
            _ => throw new ArgumentOutOfRangeException(nameof(source)),
        };
        if ((uint)index >= band.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        return band[index];
    }
}
