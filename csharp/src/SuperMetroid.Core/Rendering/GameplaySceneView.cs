using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// How a frame views the gameplay scene below the HUD: the room pixels the foreground and the background show at the
/// screen's top-left corner, which layers show and how the background is pixelated. The background covers every line of the
/// gameplay area below the HUD.
/// Every screen pixel shows the layer pixel that far from the corner, with no further offset (#1514); where that corner lies
/// against the camera is <see cref="RoomView"/>'s (#2573). What the background
/// shows says how far it reaches before it repeats: each of its pictures repeats by its own size, or shows nothing beyond
/// its edges (#2305).
/// </summary>
/// <param name="ForegroundScroll">
/// The foreground pixel the screen's top-left corner shows, as a signed position (#2384): a scroll left of or above the
/// room's origin is negative, and the room's pictures, which do not repeat, show nothing there.
/// </param>
/// <param name="BackgroundScroll">
/// The background pixel the screen's top-left corner shows, on every line that has no scroll of its own, as a signed
/// position (#2305): a scroll left of or above the background's origin is negative, and a repeating picture shows its last
/// columns or rows there.
/// </param>
/// <param name="ShownLayers">The room's layers and the actors the frame shows, on every line no band of the scene shows its own.</param>
public readonly record struct GameplaySceneView(
    Offset16<RoomPixels> ForegroundScroll, Offset16<BackgroundPixels> BackgroundScroll,
    ScreenLayers ShownLayers);
