using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Map;

/// <summary>Shared exploration projection for $82:943D pause and $82:9517 room-select maps.</summary>
public static class AreaMapTilemapBuilder
{
    /// <summary>
    /// Projects immutable cartridge cells into native two-page VRAM order. The caller
    /// supplies its screen's hidden-cell character; reveal overrides never modify SRAM.
    /// </summary>
    public static BackgroundTile[] Build(IAreaMapView map, SharedGameState system,
        MapTileWord hiddenCell, MapRevealMode revealMode = MapRevealMode.None)
    {
        Ensure.NotNull(map);
        Ensure.NotNull(system);
        var tiles = new BackgroundTile[AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles];
        bool downloaded = system.HasAreaMap(map.Area);
        for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
        for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
        {
            var cell = new Position16<MapTiles>(unchecked((ushort)x), unchecked((ushort)y));
            MapTileWord word = map.GetTile(cell);
            bool explored = system.IsMapTileExplored(map.Area, cell);
            if (explored)
                word = word.AsExplored();
            else if (!AreaMapVisibility.IsVisible(explored, downloaded,
                         map.IsRevealedByMapStation(cell), map.IsDiscoverable(cell), revealMode))
                word = hiddenCell;
            tiles[AreaMapLayout.GetTilemapWordIndex(cell)] = word.ToTile();
        }
        return tiles;
    }
}
