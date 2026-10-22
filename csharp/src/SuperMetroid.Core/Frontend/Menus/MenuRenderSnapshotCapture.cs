using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>Captures the shared menu compositor without retaining menu state.</summary>
internal static class MenuRenderSnapshotCapture
{
    /// <summary>Captures the menu screen's artwork, tilemaps, palette and sprite entries with the established BG2, BG1, OBJ layer order and current brightness.</summary>
    [PpuLayoutBoundary("codec: shared menu render packet layers")]
    internal static LayeredRenderSnapshot Capture(MenuScreen screen, ushort bg1VerticalScroll, byte brightness)
    {
        // These menus currently compose entire BG2, BG1 and OBJ planes in that order.
        // Preserve that established path here; do not silently replace it with the
        // gameplay priority ladder while moving composition to another backend.
        // Both planes draw from the menus' pre-coloured tiles (#1392).
        ScreenLayerPictures tiles = screen.FileSelectTiles.Pictures(0, BgBitDepth.FourBpp);
        RenderLayer[] layers =
        [
            new Bg4BppRenderLayer(MenuVramLayout.Bg2TilemapWord, 0, new Position16<BgPlanePixels>(0, 0), new Extent16<TilemapCells>(32, 32), null, tiles),
            new Bg4BppRenderLayer(MenuVramLayout.Bg1TilemapWord, 0, new Position16<BgPlanePixels>(0, bg1VerticalScroll), new Extent16<TilemapCells>(32, 32), null, tiles),
            new ObjRenderLayer(),
        ];
        return new(MenuScreenImage.Capture(screen), layers, MenuRenderDefinitions.ObjectSelection, brightness);
    }
}
