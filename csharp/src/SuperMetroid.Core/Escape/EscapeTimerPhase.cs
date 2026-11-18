namespace SuperMetroid.Core.Escape;

/// <summary>Which step of its sequence the escape timer runs on its next update.</summary>
public enum EscapeTimerPhase
{
    /// <summary>Not running: an update leaves the timer unchanged and reports no expiry.</summary>
    Inactive,
    /// <summary>
    /// A start was requested: the next update clears the timer, begins the requested countdown and enters
    /// <see cref="InitialDelay"/>.
    /// </summary>
    Starting,
    /// <summary>
    /// Shown at the screen's centre without counting down, for <see cref="EscapeTimerDefinitions.InitialDelayFrames"/>
    /// updates.
    /// </summary>
    InitialDelay,
    /// <summary>
    /// Counting down at the screen's centre for <see cref="EscapeTimerDefinitions.CentreCountingFrames"/> updates, until
    /// <see cref="EscapeTimerDefinitions.MovementDelayFrames"/> updates have passed since the start.
    /// </summary>
    CountingAtCentre,
    /// <summary>Counting down while moving toward its corner, until both axes reach it.</summary>
    MovingIntoPlace,
    /// <summary>Counting down in its corner.</summary>
    InPlace,
    /// <summary>
    /// The countdown has run out (#2499): the timer is only drawn, at zero where it stopped, and an update neither counts
    /// it nor reports its expiry again.
    /// </summary>
    Expired,
    /// <summary>
    /// The time-up's black-out has stopped the expired timer (#2499): it is no longer drawn, and keeps its countdown and
    /// place.
    /// </summary>
    StoppedAtTimeUp,
}
