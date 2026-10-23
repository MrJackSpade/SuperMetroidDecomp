using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>Captures the shared menu compositor without retaining menu state.</summary>
internal static class MenuRenderSnapshotCapture
{
    /// <summary>Captures the menu screen's artwork, tilemaps, palette and sprite entries with the established BG2, BG1, OBJ layer order and current brightness.</summary>
    internal static LayeredScene Capture(MenuScreen screen, ushort bg1VerticalScroll, byte brightness)
    {
        // These menus currently compose entire BG2, BG1 and OBJ planes in that order.
        // Preserve that established path here; do not silently replace it with the
        // gameplay priority ladder while moving composition to another backend.
        // Both planes draw from the menus' pre-coloured tiles (#1392).
        ScreenLayerPictures tiles = screen.MenuTiles;
        RenderLayer[] layers =
        [
            new TileRenderLayer(ScreenTileMap.Of(screen.Tilemaps.Bg2, new Extent16<TilemapCells>(32, 32), tiles), new Position16<BgPlanePixels>(0, 0), null),
            new TileRenderLayer(ScreenTileMap.Of(screen.Tilemaps.Bg1, new Extent16<TilemapCells>(32, 32), tiles), new Position16<BgPlanePixels>(0, bg1VerticalScroll), null),
            new ObjRenderLayer(),
        ];
        return new(screen.Paint(), layers, brightness);
    }
}
