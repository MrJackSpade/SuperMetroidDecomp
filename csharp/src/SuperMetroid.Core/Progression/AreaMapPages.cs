namespace SuperMetroid.Core.Progression;

/// <summary>
/// Locates a cell of one 64-by-32 area map in its two stored 32-by-32 pages (<see cref="AreaMapLayout"/>): the byte and
/// bit of the presence and exploration planes, and the tilemap word.
/// </summary>
public static class AreaMapPages
{
    /// <summary>Returns the native byte offset containing one map bit.</summary>
    public static int GetBitByteIndex(Position16<MapTiles> cell)
    {
        ValidateCoordinate(cell);
        int pageOffset = cell.X >= AreaMapLayout.PageWidthInTiles ? AreaMapLayout.BitPlaneBytesPerPage : 0;
        int byteColumnWithinPage = (cell.X & (AreaMapLayout.PageWidthInTiles - 1)) >> 3;
        return pageOffset + cell.Y * 4 + byteColumnWithinPage;
    }

    /// <summary>Returns the native word offset containing one map tile.</summary>
    public static int GetTilemapWordIndex(Position16<MapTiles> cell)
    {
        ValidateCoordinate(cell);
        int pageOffset = cell.X >= AreaMapLayout.PageWidthInTiles ? AreaMapLayout.TilemapWordsPerPage : 0;
        return pageOffset + cell.Y * AreaMapLayout.PageWidthInTiles + (cell.X & (AreaMapLayout.PageWidthInTiles - 1));
    }

    /// <summary>Returns the MSB-first mask used by both cartridge one-bit planes.</summary>
    public static byte GetBitMask(int mapX)
    {
        if ((uint)mapX >= AreaMapLayout.WidthInTiles)
            throw new ArgumentOutOfRangeException(nameof(mapX));
        return unchecked((byte)(AreaMapLayout.FirstCellBit >> (mapX & 7)));
    }

    /// <summary>Rejects coordinates outside the two 32-by-32 native map pages.</summary>
    private static void ValidateCoordinate(Position16<MapTiles> cell)
    {
        if ((uint)cell.X >= AreaMapLayout.WidthInTiles)
            throw new ArgumentOutOfRangeException(nameof(cell), "Map X lies outside the 64-tile area map.");
        if ((uint)cell.Y >= AreaMapLayout.HeightInTiles)
            throw new ArgumentOutOfRangeException(nameof(cell), "Map Y lies outside the 32-tile area map.");
    }
}
