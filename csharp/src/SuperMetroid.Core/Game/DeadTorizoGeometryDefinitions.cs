namespace SuperMetroid.Core.Game;

/// <summary>One cropped tile row copied from the corpse sheet to its rotting surface.</summary>
internal readonly record struct DeadTorizoGraphicsCopy(int SourceOffset, int DestinationOffset, int Length);

/// <summary>
/// Tile geometry shared by $A9:DE18-DEBF initialization and E272/E38B row motion.
/// The chosen stepped silhouette remains unresolved artwork geometry under issue1165;
/// these operations derive its storage addresses and column clip boundaries only.
/// </summary>
internal static class DeadTorizoGeometryDefinitions
{
    /// <summary>$A9:DE18: twelve rows from the sixteen-tile-wide $B7:A800 source.</summary>
    internal const int Rows = 12;
    /// <summary>$7E:2000 corpse surface packs ten 4bpp tiles per row.</summary>
    internal const int Columns = 10;
    private const int TileBytes = 32;

    // Independent silhouette choices: top two rows omit three left/one right tiles;
    // middle rows omit two left tiles; the lower left edge steps outward at rows9/10.
    // These choices are explicitly pending, not claimed to follow from tile packing.
    private static int LeftColumn(int row) => row < 2 ? 3 : Math.Clamp(10 - row, 0, 2);
    private static int EndColumn(int row) => row < 2 ? Columns - 1 : Columns;

    /// <summary>Calculate each native MVN's source, destination and length from the shared tile crop.</summary>
    internal static DeadTorizoGraphicsCopy InitialCopy(int row)
    {
        if ((uint)row >= Rows) throw new IndexOutOfRangeException();
        int left = LeftColumn(row);
        return new((row * 16 + left + 6) * TileBytes,
            (row * Columns + left) * TileBytes, (EndColumn(row) - left) * TileBytes);
    }

    /// <summary>$A9:E27F-E383/E398-E458: each tile column advances sixteen planar words.</summary>
    internal static int ColumnWordOffset(int column)
    {
        if ((uint)column >= Columns) throw new IndexOutOfRangeException();
        return column * TileBytes / sizeof(ushort);
    }

    /// <summary>First pixel row present in a column of the same crop used by initialization.</summary>
    internal static int ColumnMinimumY(int column)
    {
        if ((uint)column >= Columns) throw new IndexOutOfRangeException();
        for (int row = 0; row < Rows; row++)
            if (column >= LeftColumn(row) && column < EndColumn(row)) return row * 8;
        throw new InvalidOperationException("Dead Torizo crop omits a complete column.");
    }
}
