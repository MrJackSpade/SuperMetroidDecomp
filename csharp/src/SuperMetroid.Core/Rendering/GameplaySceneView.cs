using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// How a frame views the gameplay scene below the HUD: the room pixels the foreground and the background show at the
/// screen's top-left corner, the lines the background covers, which layers show and how the background is pixelated.
/// Every screen pixel shows the layer pixel that far from the corner, with no further offset (#1514). What the background
/// shows says how far it reaches before it repeats.
/// </summary>
/// <param name="ForegroundScroll">The foreground pixel the screen's top-left corner shows.</param>
/// <param name="BackgroundScroll">The background pixel the screen's top-left corner shows, on every line that has no scroll of its own.</param>
/// <param name="ShownLayers">The room's layers and the actors the frame shows, on every line no band of the scene shows its own.</param>
/// <param name="BackgroundFirstLine">The first screen line the background is drawn on, within the gameplay area.</param>
/// <param name="BackgroundEndLine">The screen line after the last one the background is drawn on.</param>
/// <param name="BackgroundPixelation">How the background is pixelated.</param>
public readonly record struct GameplaySceneView(
    Position16<RoomPixels> ForegroundScroll, Position16<BackgroundPixels> BackgroundScroll,
    ScreenLayers ShownLayers,
    int BackgroundFirstLine = 32, int BackgroundEndLine = 224,
    BackgroundPixelation BackgroundPixelation = default)
{
    /// <summary>
    /// The foreground pixel the screen's top-left corner shows while the foreground's camera is at
    /// <paramref name="camera"/>: the camera's column, on the row below the camera row the screen hides
    /// (<see cref="ScreenGeometry.LayerRowsHiddenAboveScreen"/>).
    /// </summary>
    public static Position16<RoomPixels> LayerPixelAtScreenTop(Position16<RoomPixels> camera) =>
        camera + new Offset16<RoomPixels>(0, ScreenGeometry.LayerRowsHiddenAboveScreen);

    /// <summary>
    /// The background pixel the screen's top-left corner shows while the background's camera is at
    /// <paramref name="camera"/>: the camera's column, on the row below the camera row the screen hides
    /// (<see cref="ScreenGeometry.LayerRowsHiddenAboveScreen"/>).
    /// </summary>
    public static Position16<BackgroundPixels> LayerPixelAtScreenTop(Position16<BackgroundPixels> camera) =>
        camera + new Offset16<BackgroundPixels>(0, ScreenGeometry.LayerRowsHiddenAboveScreen);

    /// <summary>
    /// The background row the screen's top row shows while the background's camera is at row <paramref name="cameraRow"/>:
    /// the row below the camera row the screen hides (<see cref="ScreenGeometry.LayerRowsHiddenAboveScreen"/>), for a line
    /// scroll given in the background's camera rows.
    /// </summary>
    public static int LayerRowAtScreenTop(int cameraRow) => CoordinateAxis.Wrap(cameraRow + ScreenGeometry.LayerRowsHiddenAboveScreen);
}
