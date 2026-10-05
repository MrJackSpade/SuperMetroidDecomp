using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>One cropped tile row copied from the corpse sheet to its rotting surface.</summary>
internal readonly record struct DeadTorizoGraphicsCopy(int SourceOffset, int DestinationOffset, int Length);

/// <summary>
/// Tile geometry shared by $A9:DE18-DEBF initialization and E272/E38B row motion.
/// All crop edges derive from the immutable stationary composition's tile union.
/// Its artwork ownership remains explicit in DeadTorizoStationaryCompositionDefinitions.
/// </summary>
internal static class DeadTorizoGeometryDefinitions
{
    private const int TileBytes = 32, TilePixels = 8;
    internal static int Rows => Bounds.Bottom - Bounds.Top;
    internal static int Columns => Bounds.Right - Bounds.Left;

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
