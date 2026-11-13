using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// One front-end dispatcher step's status. Its picture is the published render snapshot
/// (<see cref="CapturedFrontendFrame.Snapshot"/>), which every backend draws.
/// </summary>
/// <param name="GameState">Front-end dispatcher state reported with this emitted frame.</param>
/// <param name="Phase">Human-readable scene or runtime phase label for host diagnostics.</param>
/// <param name="FrameNumber">The number of updates the game has run, not elapsed time or a recorded movie's video-frame number.</param>
public readonly record struct FrontendFrame(
    SuperMetroidGameState GameState,
    string Phase,
    int FrameNumber)
{
    /// <summary>Visible non-overscan frame height in pixels: 224 rows.</summary>
    public const int Height = ScreenGeometry.ScreenHeightPixels;
}
