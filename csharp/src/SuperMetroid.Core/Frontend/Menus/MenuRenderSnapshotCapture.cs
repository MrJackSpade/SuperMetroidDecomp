using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>Captures the shared menu compositor without retaining menu state.</summary>
internal static class MenuRenderSnapshotCapture
{
    /// <summary>Captures the menu screen's artwork, tilemaps, palette and sprite entries with the established BG2, BG1, OBJ layer order and current brightness.</summary>
    /// <param name="screen">The menu screen captured.</param>
    /// <param name="bg1VerticalScroll">BG1's vertical scroll; only its low sixteen bits position the plane.</param>
    /// <param name="brightness">The screen brightness.</param>
    internal static LayeredScene Capture(MenuScreen screen, int bg1VerticalScroll, int brightness)
    {
        // These menus currently compose entire BG2, BG1 and OBJ planes in that order.
        // Preserve that established path here; do not silently replace it with the
        // gameplay priority ladder while moving composition to another backend.
        // Both planes draw from the menus' pre-coloured tiles (#1392).
        ScreenLayerPictures tiles = screen.MenuTiles;
        DisplayLayer[] layers =
        [
            new ScreenTilesLayer(ScreenTileMap.Of(screen.Tilemaps.Bg2, new Extent16<TilemapCells>(32, 32), tiles), new Position16<TilePlanePixels>(0, 0), null),
            new ScreenTilesLayer(ScreenTileMap.Of(screen.Tilemaps.Bg1, new Extent16<TilemapCells>(32, 32), tiles), new Position16<TilePlanePixels>(0, unchecked((ushort)bg1VerticalScroll)), null),
            new SpritesLayer(),
        ];
        return new(screen.Paint(), layers, brightness);
    }
}
