namespace SuperMetroid.Core.Progression;

/// <summary>
/// Coordinate layout of one 64-by-32 area map. Both the map tiles and the one-bit
/// presence/exploration planes are stored as two adjacent 32-by-32 pages rather than as
/// conventional 64-column rows.
/// </summary>
public static class AreaMapLayout
{
    /// <summary>Logical area-map width of 64 cells, with the left and right halves stored in separate pages.</summary>
    public const int WidthInTiles = 64;
    /// <summary>Logical area-map height of 32 cells, shared by both horizontal pages.</summary>
    public const int HeightInTiles = 32;
    /// <summary>Width of each 32-by-32 page; X coordinates 32..63 select the second page rather than extending a 64-cell row.</summary>
    public const int PageWidthInTiles = 32;
    /// <summary>$80 bytes per presence or exploration page: 32 rows of four MSB-first bit bytes, one bit per map cell.</summary>
    public const int BitPlaneBytesPerPage = 128;

    /// <summary>The logical 64-by-32 area-map size.</summary>
    public static Extent16<MapTiles> Size => new(WidthInTiles, HeightInTiles);
}
