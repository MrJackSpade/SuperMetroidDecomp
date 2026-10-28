using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// How a frame views the gameplay scene below the HUD: the foreground's and background's scrolls, the background's map
/// size and the lines it covers, which layers show and how the background is pixelated.
/// </summary>
/// <param name="ForegroundScroll">The foreground's scroll in pixels, before its map wraps.</param>
/// <param name="BackgroundScroll">The background's scroll in pixels, on every line that has no scroll of its own.</param>
/// <param name="BackgroundMapSize">The background's map size in eight-pixel cells: 32 or 64 on each axis.</param>
/// <param name="MainScreenLayers">The layers the frame shows, replaced rather than combined where a per-line choice of layers is captured.</param>
/// <param name="BackgroundFirstLine">The first screen line the background is drawn on, within the gameplay area.</param>
/// <param name="BackgroundEndLine">The screen line after the last one the background is drawn on.</param>
/// <param name="BackgroundPixelation">How the background is pixelated.</param>
public readonly record struct GameplaySceneView(
    Position16<RoomPixels> ForegroundScroll, Position16<BackgroundPixels> BackgroundScroll,
    Extent16<TilemapCells> BackgroundMapSize,
    ScreenLayers MainScreenLayers,
    int BackgroundFirstLine = 32, int BackgroundEndLine = 224,
    BackgroundPixelation BackgroundPixelation = default);
