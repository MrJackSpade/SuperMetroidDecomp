using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// How a frame views the gameplay scene below the HUD: the room pixels the foreground and the background show at the
/// screen's top-left corner, which layers show and how the background is pixelated. The background covers every line of the
/// gameplay area below the HUD.
/// Every screen pixel shows the layer pixel that far from the corner, with no further offset (#1514). What the background
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
/// <param name="BackgroundPixelation">How the background is pixelated.</param>
public readonly record struct GameplaySceneView(
    Offset16<RoomPixels> ForegroundScroll, Offset16<BackgroundPixels> BackgroundScroll,
    ScreenLayers ShownLayers,
    BackgroundPixelation BackgroundPixelation = default)
{
    /// <summary>
    /// The foreground pixel the screen's top-left corner shows while the foreground's camera is at the signed position
    /// <paramref name="camera"/>: the camera's column, on the row below the camera row the screen hides
    /// (<see cref="ScreenGeometry.LayerRowsHiddenAboveScreen"/>).
    /// </summary>
    public static Offset16<RoomPixels> LayerPixelAtScreenTop(Offset16<RoomPixels> camera) =>
        camera.WithY(camera.Y + ScreenGeometry.LayerRowsHiddenAboveScreen);

    /// <summary>
    /// The background pixel the screen's top-left corner shows while the background's camera is at the signed position
    /// <paramref name="camera"/>: the camera's column, on the row below the camera row the screen hides
    /// (<see cref="ScreenGeometry.LayerRowsHiddenAboveScreen"/>).
    /// </summary>
    public static Offset16<BackgroundPixels> LayerPixelAtScreenTop(Offset16<BackgroundPixels> camera) =>
        camera.WithY(camera.Y + ScreenGeometry.LayerRowsHiddenAboveScreen);

    /// <summary>
    /// The background row the screen's top row shows while the background's camera is at the signed row
    /// <paramref name="cameraRow"/>: the row below the camera row the screen hides
    /// (<see cref="ScreenGeometry.LayerRowsHiddenAboveScreen"/>), for a line scroll given in the background's camera rows. The
    /// row is signed, as the view's <see cref="BackgroundScroll"/> is (#2308): a row above the background's origin is
    /// negative, and the backend repeats each background picture by its own size, so no wrap is taken here.
    /// </summary>
    public static int LayerRowAtScreenTop(int cameraRow) => cameraRow + ScreenGeometry.LayerRowsHiddenAboveScreen;
}
