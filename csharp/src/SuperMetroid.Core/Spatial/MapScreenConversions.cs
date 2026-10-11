namespace SuperMetroid.Core.Spatial;

/// <summary>Named conversions between the area-map grids and the screen showing them.</summary>
public static class MapScreenConversions
{
    /// <summary>Map-plane pixels per map tile on each axis.</summary>
    public const int PixelsPerTile = 8;

    /// <summary>The top-left map-plane pixel of a map tile (<c>tile × 8</c>), wrapping each axis at sixteen bits.</summary>
    public static Position16<MapPixels> ToMapPixels(this Point32<MapTiles> tile) =>
        new(unchecked((ushort)(tile.X * PixelsPerTile)), unchecked((ushort)(tile.Y * PixelsPerTile)));

    /// <summary>
    /// The screen position of a map-plane pixel under the map's BG scroll (<c>position - scroll</c>),
    /// wrapping each axis at sixteen bits exactly as the native sprite origins do.
    /// </summary>
    public static Position16<ScreenPixels> ToScreen(this Position16<MapPixels> position, Position16<MapPixels> scroll) =>
        new(unchecked((ushort)(position.X - scroll.X)), unchecked((ushort)(position.Y - scroll.Y)));
}
