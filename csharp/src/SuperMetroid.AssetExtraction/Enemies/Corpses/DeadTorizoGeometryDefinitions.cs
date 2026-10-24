namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// Tile geometry shared by $A9:DE18-DEBF initialization and E272/E38B row motion.
/// All crop edges derive from the immutable stationary composition's tile union.
/// Its artwork ownership remains explicit in DeadTorizoStationaryCompositionDefinitions.
/// </summary>
internal static class DeadTorizoGeometryDefinitions
{
    /// <summary>Planar bytes and pixels contributed by one corpse-sheet tile.</summary>
    private const int TileBytes = 32, TilePixels = 8;
    /// <summary>Height in tiles of the stationary corpse composition's occupied bounds.</summary>
    internal static int Rows => Bounds.Bottom - Bounds.Top;
    /// <summary>Width in tiles of the stationary corpse composition's occupied bounds.</summary>
    internal static int Columns => Bounds.Right - Bounds.Left;

    /// <summary>Unions the tile extents of stationary sprite parts to define the corpse crop.</summary>
    private static DeadTorizoBounds Bounds
    {
        get
        {
            int left = int.MaxValue, top = int.MaxValue, right = 0, bottom = 0;
            for (int index = 0; index < DeadTorizoStationaryCompositionDefinitions.Count; index++)
            {
                var part = SourcePart(index);
                left = Math.Min(left, part.Corner.X); top = Math.Min(top, part.Corner.Y);
                right = Math.Max(right, part.Corner.X + part.Extent); bottom = Math.Max(bottom, part.Corner.Y + part.Extent);
            }
            return new(left, top, right, bottom);
        }
    }

    /// <summary>Converts an indexed stationary sprite part into source-sheet tile coordinates.</summary>
    private static DeadTorizoSourcePart SourcePart(int index)
    {
        DeadTorizoStockPart part = DeadTorizoStationaryCompositionDefinitions.Part(index);
        int tile = part.Tile - DeadTorizoStationaryCompositionDefinitions.SourceTileBase;
        return new(new((ushort)(tile % DeadTorizoStationaryCompositionDefinitions.SourceColumns),
            (ushort)(tile / DeadTorizoStationaryCompositionDefinitions.SourceColumns)), part.TileExtent);
    }

    /// <summary>Calculate each native MVN's source, destination and length from the stock sprite tile union.</summary>
    internal static CorpseGraphicsCopy InitialCopy(int row)
    {
        var bounds = Bounds;
        if ((uint)row >= bounds.Bottom - bounds.Top) throw new ArgumentOutOfRangeException(nameof(row));
        int sourceRow = bounds.Top + row, left = int.MaxValue, right = 0;
        for (int index = 0; index < DeadTorizoStationaryCompositionDefinitions.Count; index++)
        {
            var part = SourcePart(index);
            if (sourceRow < part.Corner.Y || sourceRow >= part.Corner.Y + part.Extent) continue;
            left = Math.Min(left, part.Corner.X); right = Math.Max(right, part.Corner.X + part.Extent);
        }
        return new((sourceRow * DeadTorizoStationaryCompositionDefinitions.SourceColumns + left) * TileBytes,
            (row * (bounds.Right - bounds.Left) + left - bounds.Left) * TileBytes, (right - left) * TileBytes);
    }

    /// <summary>$A9:E27F-E383/E398-E458: each tile column advances sixteen planar words.</summary>
    internal static int ColumnWordOffset(int column)
    {
        if ((uint)column >= Columns) throw new ArgumentOutOfRangeException(nameof(column));
        return column * TileBytes / sizeof(ushort);
    }

    /// <summary>First pixel row covered by a stock sprite in the given packed tile column.</summary>
    internal static int ColumnMinimumY(int column)
    {
        var bounds = Bounds;
        if ((uint)column >= bounds.Right - bounds.Left) throw new ArgumentOutOfRangeException(nameof(column));
        int sourceColumn = bounds.Left + column, firstRow = bounds.Bottom;
        for (int index = 0; index < DeadTorizoStationaryCompositionDefinitions.Count; index++)
        {
            var part = SourcePart(index);
            if (sourceColumn >= part.Corner.X && sourceColumn < part.Corner.X + part.Extent)
                firstRow = Math.Min(firstRow, part.Corner.Y);
        }
        return (firstRow - bounds.Top) * TilePixels;
    }
}
