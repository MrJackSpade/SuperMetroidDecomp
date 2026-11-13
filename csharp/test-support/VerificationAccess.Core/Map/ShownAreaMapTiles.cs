using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Progression;

namespace SuperMetroid.Core.Map;

/// <summary>
/// Lays a shown area map out as the cartridge places it on the map screens' two-page layer, each cell as the tile the
/// import pictures it from (#1513), so verification can draw a projection from VRAM characters. The screens themselves show
/// a grid of the cells' pictures (#1467).
/// </summary>
internal static class ShownAreaMapTiles
{
    /// <summary>The tiles the cartridge places for <paramref name="map"/>, in the layer's two-page order.</summary>
    internal static BackgroundTile[] Of(ShownAreaMap map)
    {
        Ensure.NotNull(map);
        var tiles = new BackgroundTile[AreaMapSize.WidthInMapTiles * AreaMapSize.HeightInMapTiles];
        for (int y = 0; y < AreaMapSize.HeightInMapTiles; y++)
        for (int x = 0; x < AreaMapSize.WidthInMapTiles; x++)
        {
            var cell = new Position16<MapTiles>(unchecked((ushort)x), unchecked((ushort)y));
            ShownMapCell shown = map.Cells[y * AreaMapSize.WidthInMapTiles + x];
            tiles[AreaMapPages.GetTilemapWordIndex(cell)] = MapCellCharacters.Tile(shown);
        }
        return tiles;
    }
}
