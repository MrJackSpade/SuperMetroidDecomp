using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// How far an area map's revealed cells reach (#1975), in map pixels: the left pixels of its leftmost and rightmost
/// revealed columns and the top pixels of its top and bottom revealed rows. A map screen frames the map from these and
/// scrolls it only while the reach still runs past the screen's edges (<see cref="MapFraming"/>).
/// </summary>
public readonly record struct MapScrollLimits
{
    /// <summary>A value of its parts; only its own operations make one.</summary>
    /// <param name="left">The leftmost revealed column's left pixel.</param>
    /// <param name="right">The rightmost revealed column's left pixel.</param>
    /// <param name="top">The top revealed row's top pixel.</param>
    /// <param name="bottom">The bottom revealed row's top pixel.</param>
    private MapScrollLimits(int left, int right, int top, int bottom)
    {
        Left = left;
        Right = right;
        Top = top;
        Bottom = bottom;
    }

    /// <summary>The leftmost revealed column's left pixel.</summary>
    public int Left { get; init; }

    /// <summary>The rightmost revealed column's left pixel.</summary>
    public int Right { get; init; }

    /// <summary>The top revealed row's top pixel.</summary>
    public int Top { get; init; }

    /// <summary>The bottom revealed row's top pixel.</summary>
    public int Bottom { get; init; }

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
        return Scan(cell => downloaded ? map.IsRevealedByMapStation(cell) : system.IsMapTileExplored(map.Area, cell));
    }

    /// <summary>
    /// The reach of the cells <paramref name="isRevealed"/> reveals, scanning every column and every row of the map. Where
    /// nothing is revealed the reach takes the default columns and rows (<see cref="MapFramingDefinitions"/>).
    /// </summary>
    /// <param name="isRevealed">Whether a map cell is revealed.</param>
    public static MapScrollLimits Scan(Func<Position16<MapTiles>, bool> isRevealed)
    {
        Ensure.NotNull(isRevealed);
        int left = MapFramingDefinitions.UnrevealedLeftColumn, right = MapFramingDefinitions.UnrevealedRightColumn;
        int top = MapFramingDefinitions.UnrevealedTopRow, bottom = MapFramingDefinitions.UnrevealedBottomRow;
        for (int x = 0; x < AreaMapSize.WidthInMapTiles; x++)
            if (ColumnRevealed(x)) { left = x; break; }
        for (int x = AreaMapSize.WidthInMapTiles - 1; x >= 0; x--)
            if (ColumnRevealed(x)) { right = x; break; }
        for (int y = 0; y < AreaMapSize.HeightInMapTiles; y++)
            if (RowRevealed(y)) { top = y; break; }
        for (int y = AreaMapSize.HeightInMapTiles - 1; y >= 0; y--)
            if (RowRevealed(y)) { bottom = y; break; }
        const int cell = MapScreenConversions.PixelsPerMapTile;
        return new(left * cell, right * cell, top * cell, bottom * cell);

        bool ColumnRevealed(int x) => Enumerable.Range(0, AreaMapSize.HeightInMapTiles).Any(y => isRevealed(new(x, y)));
        bool RowRevealed(int y) => Enumerable.Range(0, AreaMapSize.WidthInMapTiles).Any(x => isRevealed(new(x, y)));
    }
}
