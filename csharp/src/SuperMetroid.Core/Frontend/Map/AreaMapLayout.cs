namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// Native coordinate addressing for one 64-by-32 area map. Both the tilemap and one-bit
/// presence/exploration planes are stored as two adjacent 32-by-32 pages rather than as
/// conventional 64-column rows.
/// </summary>
public static class AreaMapLayout
{
    /// <summary>Logical area-map width of 64 cells, with the left and right halves stored in separate native pages.</summary>
    public const int WidthInTiles = 64;
    /// <summary>Logical area-map height of 32 cells, shared by both horizontal pages.</summary>
    public const int HeightInTiles = 32;
    /// <summary>Width of each 32-by-32 native page; X coordinates 32..63 select the second page rather than extending a 64-word physical row.</summary>
    public const int PageWidthInTiles = 32;
    /// <summary>$80 bytes per presence or exploration page: 32 rows of four MSB-first bit bytes, one bit per map cell.</summary>
    public const int BitPlaneBytesPerPage = 0x80;
    /// <summary>$0400 BG tile words per page, arranged as 32 rows of 32 cells; the second page begins after these 1024 words.</summary>
    public const int TilemapWordsPerPage = 0x400;

    /// <summary>The logical 64-by-32 area-map size.</summary>
    public static Extent16<MapTiles> Size => new(WidthInTiles, HeightInTiles);

    /// <summary>Returns the native byte offset containing one map bit.</summary>
    public static int GetBitByteIndex(Position16<MapTiles> cell)
    {
        ValidateCoordinate(cell);
        int pageOffset = cell.X >= PageWidthInTiles ? BitPlaneBytesPerPage : 0;
        int byteColumnWithinPage = (cell.X & (PageWidthInTiles - 1)) >> 3;
        return pageOffset + cell.Y * 4 + byteColumnWithinPage;
    }

    /// <summary>Returns the native word offset containing one map tile.</summary>
    public static int GetTilemapWordIndex(Position16<MapTiles> cell)
    {
        ValidateCoordinate(cell);
        int pageOffset = cell.X >= PageWidthInTiles ? TilemapWordsPerPage : 0;
        return pageOffset + cell.Y * PageWidthInTiles + (cell.X & (PageWidthInTiles - 1));
    }

    /// <summary>Returns the MSB-first mask used by both cartridge one-bit planes.</summary>
    public static byte GetBitMask(int mapX)
    {
        if ((uint)mapX >= WidthInTiles)
            throw new ArgumentOutOfRangeException(nameof(mapX));
        return unchecked((byte)(0x80 >> (mapX & 7)));
    }

    /// <summary>Rejects coordinates outside the two 32-by-32 native map pages.</summary>
    private static void ValidateCoordinate(Position16<MapTiles> cell)
    {
        if ((uint)cell.X >= WidthInTiles)
            throw new ArgumentOutOfRangeException(nameof(cell), "Map X lies outside the 64-tile area map.");
        if ((uint)cell.Y >= HeightInTiles)
            throw new ArgumentOutOfRangeException(nameof(cell), "Map Y lies outside the 32-tile area map.");
    }
}
