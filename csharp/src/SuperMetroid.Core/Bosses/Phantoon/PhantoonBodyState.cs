namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's body's own state: the encounter phase it dispatches, its phase timer, its starting flames, its
/// figure-eight and its death explosions.
/// </summary>
/// <remarks>
/// Every phase loads the values it reads when it starts: the starting-flame index, path cursor and explosion pass are
/// each set when their phase starts, and the starting-flame latch is cleared before the figure-eight first writes its
/// speed. Each count is never negative: every writer stores zero, a table position or a count it has stepped up.
/// </remarks>
public sealed class PhantoonBodyState
{
    /// <summary>The encounter phase.</summary>
    private PhantoonPhase? _phase;

    /// <summary>The body's next starting flame, 0 through 7.</summary>
    public int StartingFlameIndex { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>The body's figure-eight path cursor.</summary>
    public int PathCursor { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>The body's completed death-explosion passes.</summary>
    public int ExplosionPass { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>Whether the body's starting flames are still orbiting.</summary>
    public bool StartingFlamesActive { get; internal set; }

    /// <summary>
    /// The body's figure-eight speed in path entries per update: zero or more forward, zero or less while the eye runs
    /// the path backwards, and never more than seven entries from rest.
    /// </summary>
    public SignedSixteenSixteen FigureEightSpeed { get; internal set; }

    /// <summary>The stage of the figure-eight's speed change the body is in.</summary>
    public PhantoonFigureEightStage FigureEightStage { get; internal set; }

    /// <summary>The body's updates before its current phase moves on.</summary>
    public int PhaseTimer { get; internal set; }

    /// <summary>The encounter phase the body dispatches.</summary>
    public PhantoonPhase Phase
    {
        get => _phase ?? default;
        internal set => _phase = value;
    }

    /// <summary>Whether the body takes the flame-rain branch when the eye-close animation picks the next pattern.</summary>
    public bool FlameRainRequested { get; internal set; }

    /// <summary>
    /// Starts the body as a Phantoon part: no flame rain is requested. No reader consumes this value before a phase
    /// sets it (#1472).
    /// </summary>
    internal void StartPart() => FlameRainRequested = false;
}
