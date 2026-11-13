using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Map;

/// <summary>Shared exploration projection for the pause and room-select maps.</summary>
public static class AreaMapProjection
{
    /// <summary>
    /// How a map screen shows each cell of <paramref name="map"/> (#1513): an explored cell as explored, a revealed one as
    /// it is, and any other as the empty cell, which shows nothing. Reveal overrides never modify saved exploration.
    /// </summary>
    public static ShownAreaMap Project(IAreaMapView map, SharedGameState system, MapRevealMode revealMode = MapRevealMode.None)
    {
        Ensure.NotNull(map);
        Ensure.NotNull(system);
        var cells = new ShownMapCell[AreaMapSize.WidthInMapTiles * AreaMapSize.HeightInMapTiles];
        bool downloaded = system.HasAreaMap(map.Area);
        for (int y = 0; y < AreaMapSize.HeightInMapTiles; y++)
        for (int x = 0; x < AreaMapSize.WidthInMapTiles; x++)
        {
            var cell = new Position16<MapTiles>(CoordinateAxis.Wrap(x), CoordinateAxis.Wrap(y));
            bool explored = system.IsMapTileExplored(map.Area, cell);
            cells[y * AreaMapSize.WidthInMapTiles + x] = explored ? ShownMapCell.Explored(map.Cell(cell))
                : AreaMapVisibility.IsVisible(explored, downloaded, map.IsRevealedByMapStation(cell), map.IsDiscoverable(cell), revealMode)
                    ? ShownMapCell.Unexplored(map.Cell(cell))
                    : ShownMapCell.Unexplored(MapCell.Empty);
        }
        return new ShownAreaMap([.. cells]);
    }
}
