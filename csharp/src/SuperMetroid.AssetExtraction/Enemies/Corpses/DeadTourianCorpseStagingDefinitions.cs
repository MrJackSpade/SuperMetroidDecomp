using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// The dead Zoomer, Ripper and Skree corpses' staged art layouts (#1406): where each variant's characters are copied from
/// the shared corpse sheet into work memory, and the columns its pixel movers walk. Only the extraction reads them.
/// </summary>
[EncodingOwner("bank-$A9 Tourian corpse work-memory layout")]
internal static class DeadTourianCorpseStagingDefinitions
{
    /// <summary>Dead Zoomer's three art variants.</summary>
    private static readonly DeadTourianCorpseVariant[] ZoomerVariants =
        CorpseLayout(columns: 3, rows: 2, workBufferOffset: 0x0940, sheetSources: SheetRun(0x0a60, 0x0060, 3));

    /// <summary>Dead Ripper's two art variants.</summary>
    private static readonly DeadTourianCorpseVariant[] RipperVariants =
        CorpseLayout(columns: 3, rows: 2, workBufferOffset: 0x0b80, sheetSources: SheetRun(0x0a00, 0x0180, 2));

    /// <summary>Dead Skree's three art variants.</summary>
    private static readonly DeadTourianCorpseVariant[] SkreeVariants =
        CorpseLayout(columns: 2, rows: 4, workBufferOffset: 0x0640, sheetSources: [0x02a0, 0x00e0, 0x01c0]);

    /// <summary>The art variants of <paramref name="species"/>'s corpse, by variant index.</summary>
    internal static IReadOnlyList<DeadTourianCorpseVariant> Variants(DeadTourianCorpseSpecies species) => species switch
    {
        DeadTourianCorpseSpecies.Zoomer => ZoomerVariants,
        DeadTourianCorpseSpecies.Ripper => RipperVariants,
        DeadTourianCorpseSpecies.Skree => SkreeVariants,
        _ => throw new ArgumentOutOfRangeException(nameof(species), species, null),
    };

    /// <summary>The species whose character-row layout <paramref name="species"/>'s corpse rows follow.</summary>
    internal static RottingCorpseSpecies RowLayout(DeadTourianCorpseSpecies species) => species switch
    {
        DeadTourianCorpseSpecies.Zoomer => RottingCorpseSpecies.Zoomer,
        DeadTourianCorpseSpecies.Ripper => RottingCorpseSpecies.Ripper,
        DeadTourianCorpseSpecies.Skree => RottingCorpseSpecies.Skree,
        _ => throw new ArgumentOutOfRangeException(nameof(species), species, null),
    };

    /// <summary>
    /// Builds a corpse's variants from its tile layout. Each corpse is a block of
    /// <paramref name="columns"/> by <paramref name="rows"/> 4bpp 8x8 tiles; successive variants
    /// stack in the dead-monster work buffer from <paramref name="workBufferOffset"/>. Each art
    /// row copies from the installed tile sheet (one $200-byte sheet row per tile row) at the
    /// variant's authored sheet position. Column word offsets are the block's tile columns, and
    /// the rot depth is the block height less two pixels.
    /// </summary>
    private static DeadTourianCorpseVariant[] CorpseLayout(int columns, int rows, int workBufferOffset, ReadOnlySpan<int> sheetSources)
    {
        const int tileBytes = 32, sheetRowBytes = 0x200, tileWords = tileBytes / 2;
        var variants = new DeadTourianCorpseVariant[sheetSources.Length];
        for (int variant = 0; variant < variants.Length; variant++)
        {
            int block = workBufferOffset + variant * columns * rows * tileBytes;
            var columnWords = new ushort[columns];
            for (int column = 0; column < columns; column++)
                columnWords[column] = (ushort)(block / 2 + tileWords * column);
            var copies = new CorpseGraphicsCopy[rows];
            for (int row = 0; row < rows; row++)
                copies[row] = new(sheetSources[variant] + sheetRowBytes * row,
                    block + columns * tileBytes * row, columns * tileBytes);
            variants[variant] = new(8 * rows - 2, columnWords, copies);
        }
        return variants;
    }

    /// <summary>Sheet positions spaced evenly across one tile-sheet row.</summary>
    private static int[] SheetRun(int first, int step, int count)
    {
        var sources = new int[count];
        for (int index = 0; index < count; index++) sources[index] = first + step * index;
        return sources;
    }
}
