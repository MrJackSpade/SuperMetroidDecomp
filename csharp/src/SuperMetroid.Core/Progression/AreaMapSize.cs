namespace SuperMetroid.Core.Progression;

/// <summary>The size of one area's map: 64 map tiles across and 32 down.</summary>
public static class AreaMapSize
{
    /// <summary>Map tiles across one area's map.</summary>
    public const int WidthInMapTiles = 64;
    /// <summary>Map tiles down one area's map.</summary>
    public const int HeightInMapTiles = 32;

    /// <summary>One area's map, <see cref="WidthInMapTiles"/> by <see cref="HeightInMapTiles"/> map tiles.</summary>
    public static Extent16<MapTiles> Extent => new(WidthInMapTiles, HeightInMapTiles);

    /// <summary>
    /// The map column <paramref name="column"/> lands in: the area map wraps across its left and right edges, so a column
    /// past the right edge is the column as far from the left edge, and one before the left edge is as far from the right
    /// edge. It is gameplay-observable (#1832): a room whose screens run past the right edge has the minimap explore, show
    /// and save the wrapped columns at the left.
    /// </summary>
    public static int WrapColumn(int column) => ((column % WidthInMapTiles) + WidthInMapTiles) % WidthInMapTiles;

    /// <summary>
    /// Whether <paramref name="row"/> lies on the area map. The map does not wrap down its top and bottom edges: the
    /// minimap explores nothing while Samus's screen lies above or below the map, and shows its rows there hidden.
    /// </summary>
    public static bool ContainsRow(int row) => row is >= 0 and < HeightInMapTiles;
}
