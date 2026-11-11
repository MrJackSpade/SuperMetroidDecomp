namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// Tile geometry of the $A9:DE18-DEBF initialization.
/// All crop edges derive from the immutable stationary composition's tile union.
/// Its artwork ownership remains explicit in DeadTorizoStationaryCompositionDefinitions.
/// </summary>
internal static class DeadTorizoGeometryDefinitions
{
    /// <summary>Planar bytes and pixels contributed by one corpse-sheet tile.</summary>
    private const int TileBytes = 32;
    /// <summary>Height in tiles of the stationary corpse composition's occupied bounds.</summary>
    internal static int Rows => Bounds.Bottom - Bounds.Top;
    /// <summary>Unions the tile extents of stationary sprite parts to define the corpse crop.</summary>
    private static DeadTorizoBounds Bounds
    {
        get
        {
            int left = int.MaxValue, top = int.MaxValue, right = 0, bottom = 0;
            for (int index = 0; index < DeadTorizoStationaryCompositionDefinitions.Count; index++)
            {
                var part = SourcePart(index);
                left = Math.Min(left, part.Column); top = Math.Min(top, part.Row);
                right = Math.Max(right, part.Column + part.Extent); bottom = Math.Max(bottom, part.Row + part.Extent);
            }
            return new(left, top, right, bottom);
        }
    }

    /// <summary>Converts an indexed stationary sprite part into source-sheet tile coordinates.</summary>
    private static DeadTorizoSourcePart SourcePart(int index)
    {
        DeadTorizoStockPart part = DeadTorizoStationaryCompositionDefinitions.Part(index);
        int tile = part.Tile - DeadTorizoStationaryCompositionDefinitions.SourceTileBase;
        return new(tile % DeadTorizoStationaryCompositionDefinitions.SourceColumns,
            tile / DeadTorizoStationaryCompositionDefinitions.SourceColumns, part.TileExtent);
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
            if (sourceRow < part.Row || sourceRow >= part.Row + part.Extent) continue;
            left = Math.Min(left, part.Column); right = Math.Max(right, part.Column + part.Extent);
        }
        return new((sourceRow * DeadTorizoStationaryCompositionDefinitions.SourceColumns + left) * TileBytes,
            (row * (bounds.Right - bounds.Left) + left - bounds.Left) * TileBytes, (right - left) * TileBytes);
    }

}
