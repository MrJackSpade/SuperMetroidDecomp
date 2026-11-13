using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Progression;

/// <summary>
/// Locates a cell of one 64-by-32 area map (<see cref="AreaMapSize"/>) in its two stored 32-by-32 pages: the byte and
/// bit of the presence and exploration planes, and the tilemap word. The cartridge stores the map tiles and both one-bit
/// planes as two adjacent pages rather than as 64-column rows.
/// </summary>
public static class AreaMapPages
{
    /// <summary>Width of each 32-by-32 page; X coordinates 32..63 select the second page rather than extending a 64-cell row.</summary>
    public const int PageWidthInTiles = 32;

    /// <summary>Bytes per presence or exploration page: 32 rows of four most-significant-bit-first bytes, one bit per map cell.</summary>
    public const int BitPlaneBytesPerPage = 128;

    /// <summary>Map tiles per page, arranged as 32 rows of 32 cells; the second page begins after these 1024 tiles.</summary>
    private const int TilemapWordsPerPage = 1024;

    /// <summary>The bit of a presence or exploration byte that holds its first, leftmost cell; the planes store cells most-significant bit first.</summary>
    private const int FirstCellBit = 1 << 7;

    /// <summary>Returns the byte of the paged exploration bits that contains one map bit.</summary>
    public static int GetBitByteIndex(Position16<MapTiles> cell)
    {
        ValidateCoordinate(cell);
        int pageOffset = cell.X >= PageWidthInTiles ? BitPlaneBytesPerPage : 0;
        int byteColumnWithinPage = (cell.X & (PageWidthInTiles - 1)) >> 3;
        return pageOffset + cell.Y * 4 + byteColumnWithinPage;
    }

    /// <summary>Returns the index of one map tile in the paged map layout.</summary>
    public static int GetTilemapWordIndex(Position16<MapTiles> cell)
    {
        ValidateCoordinate(cell);
        int pageOffset = cell.X >= PageWidthInTiles ? TilemapWordsPerPage : 0;
        return pageOffset + cell.Y * PageWidthInTiles + (cell.X & (PageWidthInTiles - 1));
    }

    /// <summary>Returns the MSB-first mask used by both one-bit map planes.</summary>
    public static int GetBitMask(int mapX)
    {
        if ((uint)mapX >= AreaMapSize.WidthInMapTiles)
            throw new ArgumentOutOfRangeException(nameof(mapX));
        return FirstCellBit >> (mapX & 7);
    }

    /// <summary>Rejects coordinates outside the two 32-by-32 map pages.</summary>
    private static void ValidateCoordinate(Position16<MapTiles> cell)
    {
        if ((uint)cell.X >= AreaMapSize.WidthInMapTiles)
            throw new ArgumentOutOfRangeException(nameof(cell), "Map X lies outside the 64-tile area map.");
        if ((uint)cell.Y >= AreaMapSize.HeightInMapTiles)
            throw new ArgumentOutOfRangeException(nameof(cell), "Map Y lies outside the 32-tile area map.");
    }
}
