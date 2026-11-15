using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// How far an area map's revealed cells reach (#1975), in map pixels: the left pixels of its leftmost and rightmost
/// revealed columns and the top pixels of its top and bottom revealed rows. A map screen frames the map from these and
/// scrolls it only while the reach still runs past the screen's edges (<see cref="MapFraming"/>).
/// </summary>
/// <param name="Left">The leftmost revealed column's left pixel; Maridia's reach starts left of its column, below zero.</param>
/// <param name="Right">The rightmost revealed column's left pixel.</param>
/// <param name="Top">The top revealed row's top pixel.</param>
/// <param name="Bottom">The bottom revealed row's top pixel.</param>
public readonly record struct MapScrollLimits(int Left, int Right, int Top, int Bottom)
{
    /// <summary>
    /// The reach of the plane a saved map frames by: the downloaded map's station cells once <paramref name="map"/>'s area
    /// map has been downloaded, otherwise the explored cells. The game frames one plane, not their union, so an explored
    /// secret cell outside a downloaded map does not move the framing.
    /// </summary>
    /// <param name="map">The area map, which knows the cells its map station reveals.</param>
    /// <param name="system">The saved exploration and downloaded maps.</param>
    public static MapScrollLimits OfRevealedPlane(IAreaMapView map, SharedGameState system)
    {
        Ensure.NotNull(map);
        Ensure.NotNull(system);
        bool downloaded = system.HasAreaMap(map.Area);
        return Scan(map.Area, cell => downloaded ? map.IsRevealedByMapStation(cell) : system.IsMapTileExplored(map.Area, cell));
    }

    /// <summary>
    /// The reach of the cells <paramref name="isRevealed"/> reveals on <paramref name="area"/>'s map. Where nothing is
    /// revealed the reach takes the default columns and rows (<see cref="MapFramingDefinitions"/>).
    /// </summary>
    /// <param name="area">The area, whose map Maridia's reach extends left of.</param>
    /// <param name="isRevealed">Whether a map cell is revealed.</param>
    public static MapScrollLimits Scan(AreaId area, Func<Position16<MapTiles>, bool> isRevealed)
    {
        Ensure.NotNull(isRevealed);
        int left = MapFramingDefinitions.UnrevealedLeftColumn, right = MapFramingDefinitions.UnrevealedRightColumn;
        int top = MapFramingDefinitions.UnrevealedTopRow, bottom = MapFramingDefinitions.UnrevealedBottomRow;
        for (int x = 0; x < AreaMapSize.WidthInMapTiles; x++)
            if (ColumnRevealed(x)) { left = x; break; }
        for (int x = AreaMapSize.WidthInMapTiles - 1; x >= 0; x--)
            if (ColumnRevealed(x)) { right = x; break; }
        // The game's row scans stop before the last row in either direction: a map revealed only in its bottom row keeps
        // the default top, and one revealed only in its top row keeps the default bottom.
        for (int y = 0; y < AreaMapSize.HeightInMapTiles - 1; y++)
            if (RowRevealed(y)) { top = y; break; }
        for (int y = AreaMapSize.HeightInMapTiles - 1; y > 0; y--)
            if (RowRevealed(y)) { bottom = y; break; }
        const int cell = MapScreenConversions.PixelsPerMapTile;
        int leftReach = area == AreaId.Maridia ? MapFramingDefinitions.MaridiaLeftReachPixels : 0;
        return new(left * cell - leftReach, right * cell, top * cell, bottom * cell);

        bool ColumnRevealed(int x) => Enumerable.Range(0, AreaMapSize.HeightInMapTiles).Any(y => isRevealed(new(x, y)));
        bool RowRevealed(int y) => Enumerable.Range(0, AreaMapSize.WidthInMapTiles).Any(x => isRevealed(new(x, y)));
    }
}
