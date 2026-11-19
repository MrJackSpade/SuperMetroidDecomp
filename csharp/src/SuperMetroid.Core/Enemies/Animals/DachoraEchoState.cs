using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The state of one of a Dachora's four speed-boost echoes (#2147): an afterimage its body places along the shinespark's
/// trail, which flickers on its updates' parity for a short life and is hidden otherwise.
/// </summary>
public sealed class DachoraEchoState
{
    /// <summary>The animation the echo plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<DachoraSegment> Playback { get; set; }

    /// <summary>
    /// The updates the echo stays visible, counted once each update it shows; once it is over the echo is free for the next
    /// trail position.
    /// </summary>
    public UpdateCountdown Lifetime { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>
    /// Whether the echo shows on odd updates rather than even ones while it is visible. Its body sets it each time it places
    /// the echo, alternating along the trail so neighbouring echoes flicker in turn.
    /// </summary>
    internal bool ShowsOnOddFrames { get; set; }
}
