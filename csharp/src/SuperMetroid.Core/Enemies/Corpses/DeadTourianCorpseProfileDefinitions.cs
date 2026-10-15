namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The dead Zoomer, Ripper and Skree corpses' native AI function pointers and installed art layouts.</summary>
internal static class DeadTourianCorpseProfileDefinitions
{
    /// <summary>The random bits that spread a corpse's dust clouds across its width ($1A: 0-26 pixels, even).</summary>
    internal const int DustXRandomBits = 0x001a;

    /// <summary>The shared inert AI function selected after all rotting rows finish.</summary>
    internal const ushort NoOperationFunction = 0xda63;

    /// <summary>Dead Zoomer's callback addresses and three installed art variants.</summary>
    internal static readonly DeadTourianBehavior.DeadTourianCorpseProfile DeadZoomerProfile = new(
        DeadTourianCorpseSpecies.Zoomer,
        WaitFunction: 0xda69,
        PreRotFunction: 0xda94,
        RottingFunction: 0xdad0,
        TouchAndShotFunction: 0xdcf8,
        PowerBombFunction: 0xdced,
        Variants: CorpseLayout(columns: 3, rows: 2, workBufferOffset: 0x0940, sheetSources: SheetRun(0x0a60, 0x0060, 3)));

    /// <summary>Dead Ripper's callback addresses and two installed art variants.</summary>
    internal static readonly DeadTourianBehavior.DeadTourianCorpseProfile DeadRipperProfile = new(
        DeadTourianCorpseSpecies.Ripper,
        WaitFunction: 0xda73,
        PreRotFunction: 0xda99,
        RottingFunction: 0xdae6,
        TouchAndShotFunction: 0xdd08,
        PowerBombFunction: 0xdcfd,
        Variants: CorpseLayout(columns: 3, rows: 2, workBufferOffset: 0x0b80, sheetSources: SheetRun(0x0a00, 0x0180, 2)));

    /// <summary>Dead Skree's callback addresses and three installed art variants.</summary>
    internal static readonly DeadTourianBehavior.DeadTourianCorpseProfile DeadSkreeProfile = new(
        DeadTourianCorpseSpecies.Skree,
        WaitFunction: 0xda6e,
        PreRotFunction: 0xda9e,
        RottingFunction: 0xdafc,
        TouchAndShotFunction: 0xdd18,
        PowerBombFunction: 0xdd0d,
        Variants: CorpseLayout(columns: 2, rows: 4, workBufferOffset: 0x0640, sheetSources: [0x02a0, 0x00e0, 0x01c0]));

    /// <summary>
    /// Builds a corpse's variants from its tile layout. Each corpse is a block of
    /// <paramref name="columns"/> by <paramref name="rows"/> 4bpp 8x8 tiles; successive variants
    /// stack in the dead-monster work buffer from <paramref name="workBufferOffset"/>. Each art
    /// row copies from the installed tile sheet (one $200-byte sheet row per tile row) at the
    /// variant's authored sheet position. Column word offsets are the block's tile columns, and
    /// the rot depth is the block height less two pixels.
    /// </summary>
    private static DeadTourianBehavior.DeadTourianCorpseVariant[] CorpseLayout(
        int columns, int rows, int workBufferOffset, ReadOnlySpan<int> sheetSources)
    {
        const int tileBytes = 32, sheetRowBytes = 0x200, tileWords = tileBytes / 2;
        var variants = new DeadTourianBehavior.DeadTourianCorpseVariant[sheetSources.Length];
        for (int variant = 0; variant < variants.Length; variant++)
        {
            int block = workBufferOffset + variant * columns * rows * tileBytes;
            var columnWords = new ushort[columns];
            for (int column = 0; column < columns; column++)
                columnWords[column] = (ushort)(block / 2 + tileWords * column);
            var copies = new DeadTourianBehavior.DeadTourianCorpseGraphicsCopy[rows];
            for (int row = 0; row < rows; row++)
                copies[row] = new(sheetSources[variant] + sheetRowBytes * row,
                    block + columns * tileBytes * row, columns * tileBytes);
            variants[variant] = new((ushort)(8 * rows - 2), columnWords, copies);
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
