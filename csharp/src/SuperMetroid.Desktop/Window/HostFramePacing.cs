namespace SuperMetroid.Desktop.Window;

/// <summary>How the host spaces the frames a driver supplies (#1511).</summary>
internal enum HostFramePacing
{
    /// <summary>At the game's frame rate, as live play and recorded playback run: wall-clock time decides how many frames each tick runs.</summary>
    WallClock,

    /// <summary>Back to back: each tick runs frames for one timer interval and presents the last, as a verification replay runs.</summary>
    Unpaced,
}
