using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// One front-end dispatcher step's output: its status and the semantic audio it published. Its picture is the
/// published render snapshot (<see cref="CapturedFrontendFrame.Snapshot"/>), which every backend draws.
/// </summary>
/// <param name="GameState">Front-end dispatcher state reported with this emitted frame.</param>
/// <param name="Phase">Human-readable scene or runtime phase label for host diagnostics.</param>
/// <param name="FrameNumber">The number of updates the game has run, not elapsed time or a recorded movie's video-frame number.</param>
/// <param name="Audio">
/// The update's ordered semantic audio events after sound acceptance. The session has already rendered them into the
/// update's PCM (<see cref="SuperMetroidGame.CopyAudioFrame"/>); a host that plays audio itself plays these instead.
/// </param>
public readonly record struct FrontendFrame(
    SuperMetroidGameState GameState,
    string Phase,
    int FrameNumber,
    AudioFrameEvents Audio)
{
    /// <summary>Visible non-overscan frame height in pixels: 224 rows.</summary>
    public const int Height = ScreenGeometry.ScreenHeightPixels;
}
