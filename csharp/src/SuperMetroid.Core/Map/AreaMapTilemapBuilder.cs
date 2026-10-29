using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Map;

/// <summary>Shared exploration projection for the pause and room-select maps.</summary>
public static class AreaMapTilemapBuilder
{
    /// <summary>
    /// Places each cell of <paramref name="map"/> on a map screen's two-page layer: an explored cell in its explored
    /// colour variant, a revealed one as it is, and any other as the screen's <paramref name="hiddenCell"/>. Reveal
    /// overrides never modify saved exploration.
    /// </summary>
    public static BackgroundTile[] Build(IAreaMapView map, SharedGameState system,
        MapCell hiddenCell, MapRevealMode revealMode = MapRevealMode.None)
    {
        Ensure.NotNull(map);
        Ensure.NotNull(system);
        var tiles = new BackgroundTile[AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles];
        bool downloaded = system.HasAreaMap(map.Area);
        for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
        for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
        {
            var cell = new Position16<MapTiles>(unchecked((ushort)x), unchecked((ushort)y));
            bool explored = system.IsMapTileExplored(map.Area, cell);
            BackgroundTile tile = explored ? map.Cell(cell).ExploredTile
                : AreaMapVisibility.IsVisible(explored, downloaded, map.IsRevealedByMapStation(cell), map.IsDiscoverable(cell), revealMode)
                    ? map.Cell(cell).UnexploredTile
                    : hiddenCell.UnexploredTile;
            tiles[AreaMapPages.GetTilemapWordIndex(cell)] = tile;
        }
        return tiles;
    }
}
