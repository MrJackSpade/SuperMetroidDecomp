using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The installed auxiliary enemy colour rows: which family a palette names, how a colour resolves, and their identity.</summary>
internal static class EnemyAuxiliaryColors
{
    /// <summary>Hashes each palette's rows, tagged by its place in the definitions' face-block, corpse, body and belly order.</summary>
    internal static string ContentIdentity(EnemyAuxiliaryColorCatalog colors) =>
        SelectedPresentationHash.Create("enemy-auxiliary-colors-v1", content =>
        {
            Span<RgbColor> row = stackalloc RgbColor[16];
            for (int tag = 0; tag < EnemyAuxiliaryColorDefinitions.All.Length; tag++)
            {
                EnemyAuxiliaryPaletteRows rows = RowsFor(colors, EnemyAuxiliaryColorDefinitions.All[tag].Id);
                content.Append("palette", tag);
                content.Append("frames", rows.FrameCount);
                for (int frame = 0; frame < rows.FrameCount; frame++)
                {
                    for (int color = 0; color < rows.ColorCount; color++) row[color] = rows.Color(frame, color);
                    content.AppendColors("row", row[..rows.ColorCount]);
                }
            }
        });

    /// <summary>Selects one of the four installed auxiliary color families by its defined identity.</summary>
    private static EnemyAuxiliaryPaletteRows RowsFor(EnemyAuxiliaryColorCatalog colors, EnemyAuxiliaryPalette palette) =>
        colors.rows.TryGetValue(palette, out EnemyAuxiliaryPaletteRows? rows)
            ? rows
            : throw new ArgumentOutOfRangeException(nameof(palette));

    /// <summary>Resolves one selected RGB5 word, rejecting a row or colour outside the family.</summary>
    internal static RgbColor Resolve(EnemyAuxiliaryColorCatalog colors, EnemyAuxiliaryPalette palette, int frame, int color)
    {
        EnemyAuxiliaryPaletteRows rows = RowsFor(colors, palette);
        if ((uint)frame >= rows.FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= rows.ColorCount) throw new ArgumentOutOfRangeException(nameof(color));
        return rows.Color(frame, color);
    }
}
