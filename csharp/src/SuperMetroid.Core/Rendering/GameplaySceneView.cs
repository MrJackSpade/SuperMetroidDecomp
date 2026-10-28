using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// How a frame views the gameplay scene below the HUD: BG1's and BG2's scrolls, BG2's map size and the lines it covers,
/// which layers show and BG2's mosaic.
/// </summary>
/// <param name="ForegroundScroll">BG1 scroll-register values in pixels, before tilemap wrapping and without the renderer's first-visible-background-scanline adjustment.</param>
/// <param name="BackgroundScroll">Fallback BG2 scroll-register values when no per-line replacement is captured, without the first-visible-background-scanline adjustment.</param>
/// <param name="BackgroundMapSize">BG2 tilemap size in 8-pixel cells: 32 or 64 on each axis.</param>
/// <param name="MainScreenLayers">The layers the frame shows, replaced rather than combined where a per-line choice of layers is captured.</param>
/// <param name="BackgroundFirstLine">Inclusive physical output scanline at which BG2 drawing starts, intersected with the gameplay region.</param>
/// <param name="BackgroundEndLine">Exclusive physical output scanline at which BG2 drawing ends.</param>
/// <param name="BackgroundPixelation">Captured BG2 mosaic sampling configuration, applied by rendering without advancing simulation state.</param>
public readonly record struct GameplaySceneView(
    Position16<RoomPixels> ForegroundScroll, Position16<BackgroundPixels> BackgroundScroll,
    Extent16<TilemapCells> BackgroundMapSize,
    ScreenLayers MainScreenLayers,
    int BackgroundFirstLine = 32, int BackgroundEndLine = 224,
    BackgroundPixelation BackgroundPixelation = default);
