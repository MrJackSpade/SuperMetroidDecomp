using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The tilemaps the pause screen shows (#1358): the map and equipment pages' 64x32 BG1, the frame's BG2 with its
/// button-label rows, and the BG3 plane holding the retained gameplay HUD rows over the cleared FX rows. The screen owns
/// their words; its capture places them.
/// </summary>
internal sealed class PauseTilemaps
{
    /// <summary>Words in one 32x32 screen.</summary>
    private const int ScreenWords = SnesPpuLayout.TilemapPageWordCount;

    /// <summary>The BG2 row the two button-label rows start on.</summary>
    private const int ButtonRowsFirstRow = 25;

    /// <summary>The first BG3 word below the four HUD rows.</summary>
    private const int FxRowsWord = Layer3Plane.HudRows * Layer3Plane.Columns;

    /// <summary>BG1's two screens.</summary>
    private readonly ushort[] bg1 = new ushort[2 * ScreenWords];

    /// <summary>BG2's screen.</summary>
    private readonly ushort[] bg2 = new ushort[ScreenWords];

    /// <summary>BG3's two screens: the HUD rows, the rest of the first screen and the screen below it.</summary>
    private readonly ushort[] bg3 = new ushort[2 * ScreenWords];

    /// <summary>The map or equipment page.</summary>
    internal Span<ushort> Bg1 => bg1;

    /// <summary>The pause frame.</summary>
    internal Span<ushort> Bg2 => bg2;

    /// <summary>The frame's two button-label rows, which the pause routines rewrite as labels change.</summary>
    internal Span<ushort> ButtonRows => bg2.AsSpan(ButtonRowsFirstRow * Layer3Plane.Columns, 2 * Layer3Plane.Columns);

    /// <summary>The HUD and FX plane.</summary>
    internal ReadOnlySpan<ushort> Bg3 => bg3;

    /// <summary>
    /// Keeps the first screen of gameplay's BG3 plane resident: <c>SetupPPUForPauseMenu</c> moves BG3SC without uploading
    /// a tilemap, so the HUD gameplay displayed stays (#1351).
    /// </summary>
    internal void RetainHud(Layer3Plane gameplay) => Ensure.NotNull(gameplay).Words[..ScreenWords].CopyTo(bg3);

    /// <summary>Mirrors the three mutable HUD rows of the plane gameplay displayed, keeping the static top row.</summary>
    internal void MirrorAcceptedHud(Layer3Plane gameplay) =>
        Ensure.NotNull(gameplay).Words[Layer3Plane.Columns..FxRowsWord].CopyTo(bg3.AsSpan(Layer3Plane.Columns));

    /// <summary>Fills every BG3 row below the HUD with <paramref name="tile"/>, as <c>QueueClearingOfFxTilemap</c> ($80:A211) does.</summary>
    internal void ClearFx(ushort tile) => bg3.AsSpan(FxRowsWord).Fill(tile);
}
