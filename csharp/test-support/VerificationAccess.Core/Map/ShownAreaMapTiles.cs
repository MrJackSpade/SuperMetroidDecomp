using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Map;

/// <summary>
/// Lays a shown area map out as the map screens place it on their two-page layer, so verification can compare a projection
/// with the cartridge's map words.
/// </summary>
internal static class ShownAreaMapTiles
{
    /// <summary>The tiles the map screens place for <paramref name="map"/>, in the layer's two-page order.</summary>
    internal static BackgroundTile[] Of(ShownAreaMap map)
    {
        var tiles = new BackgroundTile[AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles];
        MenuTilemaps.LoadAreaMap(tiles, map);
        return tiles;
    }
}
