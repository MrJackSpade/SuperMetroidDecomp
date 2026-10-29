using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The tilemaps the pause screen shows (#1358): the map and equipment pages' 64x32 BG1 and the frame's BG2 with its
/// button-label rows. The screen owns their words; its capture places them. The retained gameplay HUD is the screen's
/// own images (#1466), not a tilemap.
/// </summary>
[NumberedIdentities("BG1 and BG2 are the PPU's distinct background layers, each with its own pause-screen role and size: the 64x32 map or equipment page and the 32x32 frame. The number names the hardware layer, not a position.")]
internal sealed class PauseTilemaps
{
    /// <summary>Words in one 32x32 screen.</summary>
    internal const int ScreenWords = ScreenGeometry.TilemapScreenCellCount;

    /// <summary>The BG2 row the two button-label rows start on.</summary>
    private const int ButtonRowsFirstRow = 25;

    /// <summary>BG1's two screens.</summary>
    private readonly BackgroundTile[] bg1 = new BackgroundTile[2 * ScreenWords];

    /// <summary>BG2's screen.</summary>
    private readonly BackgroundTile[] bg2 = new BackgroundTile[ScreenWords];

    /// <summary>The map or equipment page.</summary>
    internal Span<BackgroundTile> Bg1 => bg1;

    /// <summary>The pause frame.</summary>
    internal Span<BackgroundTile> Bg2 => bg2;

    /// <summary>The frame's two button-label rows, which the pause routines rewrite as labels change.</summary>
    internal Span<BackgroundTile> ButtonRows => bg2.AsSpan(ButtonRowsFirstRow * Layer3Plane.Columns, 2 * Layer3Plane.Columns);
}
