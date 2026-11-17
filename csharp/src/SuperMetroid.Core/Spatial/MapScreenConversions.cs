namespace SuperMetroid.Core.Spatial;

/// <summary>Named conversions between the area-map grids and the screen showing them.</summary>
public static class MapScreenConversions
{
    /// <summary>Map-plane pixels per map tile on each axis.</summary>
    public const int PixelsPerMapTile = 8;

    /// <summary>The top-left map-plane pixel of a map tile (<c>tile × 8</c>), wrapping each axis at 65536.</summary>
    public static Position16<MapPixels> ToMapPixels(this Position16<MapTiles> tile) =>
        Position16<MapPixels>.Wrapped(tile.X * PixelsPerMapTile, tile.Y * PixelsPerMapTile);

    /// <summary>
    /// A signed map-plane pixel offset rounded toward lower values to a whole number of map tiles: 13 gives 8 and -3
    /// gives -8.
    /// </summary>
    public static int RoundDownToMapTile(int mapPixels) =>
        mapPixels - (mapPixels % PixelsPerMapTile + PixelsPerMapTile) % PixelsPerMapTile;

    /// <summary>
    /// Where a map-plane pixel lands on the screen under the map's scroll, the screen's corner as an offset from the map's
    /// (<c>position - scroll</c>): a pixel left of or above the screen is negative.
    /// </summary>
    public static Offset16<ScreenPixels> RelativeToScroll(this Position16<MapPixels> position, Offset16<MapPixels> scroll) =>
        new(position.X - scroll.X, position.Y - scroll.Y);
}
