using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Progression;

/// <summary>
/// Locates a cell of one 64-by-32 area map in its two stored 32-by-32 pages (<see cref="AreaMapLayout"/>): the byte and
/// bit of the presence and exploration planes, and the tilemap word.
/// </summary>
public static class AreaMapPages
{
    /// <summary>Map tiles per page, arranged as 32 rows of 32 cells; the second page begins after these 1024 tiles.</summary>
    private const int TilemapWordsPerPage = 1024;

    /// <summary>The bit of a presence or exploration byte that holds its first, leftmost cell; the planes store cells most-significant bit first.</summary>
    private const int FirstCellBit = 1 << 7;

    /// <summary>Returns the byte of the paged exploration bits that contains one map bit.</summary>
    public static int GetBitByteIndex(Position16<MapTiles> cell)
    {
        ValidateCoordinate(cell);
        int pageOffset = cell.X >= AreaMapLayout.PageWidthInTiles ? AreaMapLayout.BitPlaneBytesPerPage : 0;
        int byteColumnWithinPage = (cell.X & (AreaMapLayout.PageWidthInTiles - 1)) >> 3;
        return pageOffset + cell.Y * 4 + byteColumnWithinPage;
    }

    /// <summary>Returns the index of one map tile in the paged map layout.</summary>
    public static int GetTilemapWordIndex(Position16<MapTiles> cell)
    {
        ValidateCoordinate(cell);
        int pageOffset = cell.X >= AreaMapLayout.PageWidthInTiles ? TilemapWordsPerPage : 0;
        return pageOffset + cell.Y * AreaMapLayout.PageWidthInTiles + (cell.X & (AreaMapLayout.PageWidthInTiles - 1));
    }

    /// <summary>Returns the MSB-first mask used by both one-bit map planes.</summary>
    public static int GetBitMask(int mapX)
    {
        if ((uint)mapX >= AreaMapLayout.WidthInTiles)
            throw new ArgumentOutOfRangeException(nameof(mapX));
        return WordWrap.LowByte(FirstCellBit >> (mapX & 7));
    }

    /// <summary>Rejects coordinates outside the two 32-by-32 map pages.</summary>
    private static void ValidateCoordinate(Position16<MapTiles> cell)
    {
        if ((uint)cell.X >= AreaMapLayout.WidthInTiles)
            throw new ArgumentOutOfRangeException(nameof(cell), "Map X lies outside the 64-tile area map.");
        if ((uint)cell.Y >= AreaMapLayout.HeightInTiles)
            throw new ArgumentOutOfRangeException(nameof(cell), "Map Y lies outside the 32-tile area map.");
    }
}
