namespace SuperMetroid.Core.Progression;

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

    /// <summary>The bit of a presence or exploration byte that holds its first, leftmost cell; the planes store cells most-significant bit first.</summary>
    public const int FirstCellBit = 0x80;
}
