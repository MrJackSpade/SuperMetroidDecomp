using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The pause screen's HUD plane as the cartridge's BG3 tilemap words hold it (#1466): the retained HUD's images laid out
/// from their tiles over its top rows, and the cleared cell everywhere else, so verification can compare it with native
/// VRAM.
/// </summary>
internal static class PauseScreenHudPlane
{
    /// <summary>Cells in the pause screen's two BG3 screens.</summary>
    private const int PlaneCells = 2 * ScreenGeometry.TilemapScreenCellCount;

    /// <summary>The pause screen's HUD plane.</summary>
    extension(PauseScreen screen)
    {
        /// <summary>The HUD plane's tiles across both screens, row by row.</summary>
        internal BackgroundTile[] Bg3Tiles
        {
            get
            {
                var tiles = new BackgroundTile[PlaneCells];
                tiles.AsSpan().Fill(PauseHudPlaneDefinitions.ClearedCell);
                GameplayHudArtwork.Of(screen.Artwork.GameplayHud).RowsOver(screen.Hud, PauseHudPlaneDefinitions.ClearedCell).CopyTo(tiles, 0);
                return tiles;
            }
        }
    }
}
