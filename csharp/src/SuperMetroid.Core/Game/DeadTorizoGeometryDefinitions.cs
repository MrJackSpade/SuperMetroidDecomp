using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>One cropped tile row copied from the corpse sheet to its rotting surface.</summary>
/// <param name="SourceOffset">Byte offset of the row within the source graphics sheet.</param>
/// <param name="DestinationOffset">Byte offset where the row begins in the compact rotting-surface layout.</param>
/// <param name="Length">Number of contiguous source bytes copied for this row.</param>
internal readonly record struct DeadTorizoGraphicsCopy(int SourceOffset, int DestinationOffset, int Length);

/// <summary>
/// Tile geometry shared by $A9:DE18-DEBF initialization and E272/E38B row motion.
/// All crop edges derive from the immutable stationary composition's tile union.
/// Its artwork ownership remains explicit in DeadTorizoStationaryCompositionDefinitions.
/// </summary>
internal static class DeadTorizoGeometryDefinitions
{
    /// <summary>A SNES 4bpp tile occupies 32 bytes and spans 8 pixels per side.</summary>
    private const int TileBytes = 32, TilePixels = 8;

    /// <summary>Number of tile rows in the stationary corpse artwork's bounding union.</summary>
    internal static int Rows => Bounds.Bottom - Bounds.Top;

    /// <summary>Number of tile columns in the stationary corpse artwork's bounding union.</summary>
    internal static int Columns => Bounds.Right - Bounds.Left;

    /// <summary>Inclusive left/top and exclusive right/bottom tile bounds covering every stock corpse part.</summary>
    private static (int Left, int Top, int Right, int Bottom) Bounds
    {
        get
        {
            int left = int.MaxValue, top = int.MaxValue, right = 0, bottom = 0;
            for (int index = 0; index < DeadTorizoStationaryCompositionDefinitions.Count; index++)
            {
                var part = SourcePart(index);
                left = Math.Min(left, part.X); top = Math.Min(top, part.Y);
                right = Math.Max(right, part.X + part.Extent); bottom = Math.Max(bottom, part.Y + part.Extent);
            }
            return (left, top, right, bottom);
        }
    }

    /// <summary>Gets a stock corpse part's source tile coordinates and square extent.</summary>
    /// <param name="index">Zero-based index into the stationary composition's stock parts.</param>
    /// <returns>The part's source tile column, row, and tile extent.</returns>
    private static (int X, int Y, int Extent) SourcePart(int index)
    {
        DeadTorizoStockPart part = DeadTorizoStationaryCompositionDefinitions.Part(index);
        int tile = part.Tile - DeadTorizoStationaryCompositionDefinitions.SourceTileBase;
        return (tile % DeadTorizoStationaryCompositionDefinitions.SourceColumns,
            tile / DeadTorizoStationaryCompositionDefinitions.SourceColumns, part.TileExtent);
    }

    /// <summary>Calculate each native MVN's source, destination and length from the stock sprite tile union.</summary>
    internal static DeadTorizoGraphicsCopy InitialCopy(int row)
    {
        var bounds = Bounds;
        if ((uint)row >= bounds.Bottom - bounds.Top) throw new IndexOutOfRangeException();
        int sourceRow = bounds.Top + row, left = int.MaxValue, right = 0;
        for (int index = 0; index < DeadTorizoStationaryCompositionDefinitions.Count; index++)
        {
            var part = SourcePart(index);
            if (sourceRow < part.Y || sourceRow >= part.Y + part.Extent) continue;
            left = Math.Min(left, part.X); right = Math.Max(right, part.X + part.Extent);
        }
        return new((sourceRow * DeadTorizoStationaryCompositionDefinitions.SourceColumns + left) * TileBytes,
            (row * (bounds.Right - bounds.Left) + left - bounds.Left) * TileBytes, (right - left) * TileBytes);
    }

    /// <summary>$A9:E27F-E383/E398-E458: each tile column advances sixteen planar words.</summary>
    internal static int ColumnWordOffset(int column)
    {
        if ((uint)column >= Columns) throw new IndexOutOfRangeException();
        return column * TileBytes / sizeof(ushort);
    }

    /// <summary>First pixel row covered by a stock sprite in the given packed tile column.</summary>
    internal static int ColumnMinimumY(int column)
    {
        var bounds = Bounds;
        if ((uint)column >= bounds.Right - bounds.Left) throw new IndexOutOfRangeException();
        int sourceColumn = bounds.Left + column, firstRow = bounds.Bottom;
        for (int index = 0; index < DeadTorizoStationaryCompositionDefinitions.Count; index++)
        {
            var part = SourcePart(index);
            if (sourceColumn >= part.X && sourceColumn < part.X + part.Extent)
                firstRow = Math.Min(firstRow, part.Y);
        }
        return (firstRow - bounds.Top) * TilePixels;
    }
}
