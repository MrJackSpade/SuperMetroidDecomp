using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Map;

/// <summary>
/// Lays a shown area map out as the cartridge places it on the map screens' two-page layer, so verification can compare a
/// projection with the cartridge's map words. The screens themselves show a grid of the cells' pictures (#1467).
/// </summary>
internal static class ShownAreaMapTiles
{
    /// <summary>The tiles the cartridge places for <paramref name="map"/>, in the layer's two-page order.</summary>
    internal static BackgroundTile[] Of(ShownAreaMap map)
    {
        Ensure.NotNull(map);
        var tiles = new BackgroundTile[AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles];
        for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
        for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
        {
            var cell = new Position16<MapTiles>(unchecked((ushort)x), unchecked((ushort)y));
            ShownMapCell shown = map[cell];
            tiles[AreaMapPages.GetTilemapWordIndex(cell)] =
                new BackgroundTile(shown.Cell.Shape.Picture, shown.ColourVariant, shown.Cell.InFront, shown.Cell.Mirroring);
        }
        return tiles;
    }
}
