using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// One front-end dispatcher step's status. Its picture is the published render snapshot
/// (<see cref="CapturedFrontendFrame.Snapshot"/>), which every backend draws.
/// </summary>
/// <param name="GameState">Front-end dispatcher state reported with this emitted frame.</param>
/// <param name="Phase">Human-readable scene or runtime phase label for host diagnostics.</param>
/// <param name="FrameNumber">Wrapping 16-bit front-end step counter, not elapsed time or a native movie video-frame number.</param>
public readonly record struct FrontendFrame(
    SuperMetroidGameState GameState,
    string Phase,
    int FrameNumber)
{
    /// <summary>Visible frame width in pixels: 256 columns in row-major screen order.</summary>
    public const int Width = ScreenGeometry.ScreenWidthPixels;
    /// <summary>Visible non-overscan frame height in pixels: 224 scanlines.</summary>
    public const int Height = ScreenGeometry.ScreenHeightPixels;
}
