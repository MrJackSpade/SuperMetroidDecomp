using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// How a frame views the gameplay scene below the HUD: the foreground's and background's scrolls, the lines the background
/// covers, which layers show and how the background is pixelated. What the background shows says how far it reaches before
/// it repeats.
/// </summary>
/// <param name="ForegroundScroll">The foreground's scroll in pixels.</param>
/// <param name="BackgroundScroll">The background's scroll in pixels, on every line that has no scroll of its own.</param>
/// <param name="ShownLayers">The room's layers and the actors the frame shows, on every line no band of the scene shows its own.</param>
/// <param name="BackgroundFirstLine">The first screen line the background is drawn on, within the gameplay area.</param>
/// <param name="BackgroundEndLine">The screen line after the last one the background is drawn on.</param>
/// <param name="BackgroundPixelation">How the background is pixelated.</param>
public readonly record struct GameplaySceneView(
    Position16<RoomPixels> ForegroundScroll, Position16<BackgroundPixels> BackgroundScroll,
    ScreenLayers ShownLayers,
    int BackgroundFirstLine = 32, int BackgroundEndLine = 224,
    BackgroundPixelation BackgroundPixelation = default);
