namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// What a signal cue of the Yard's animations marks: a turn, a hide or a flight beginning, or the end of an update its
/// shell shows closed.
/// </summary>
internal enum YardCueKind
{
    /// <summary>
    /// An update of its closed shell has ended (#2180): while the Yard hides, or by chance (<see cref="YardDecisions.HoldsShellClosedAgain"/>) once it no longer does, the
    /// shell shows closed for another update; otherwise the hide plays on.
    /// </summary>
    ShellClosedUpdateEnds,
    /// <summary>The Yard begins turning a corner, holding still until the turn's animation starts its crawl.</summary>
    BeginTurn,
    /// <summary>The Yard begins hiding in its shell, watching whether its surface still holds it.</summary>
    BeginHiding,
    /// <summary>The Yard's shell begins its flight, falling, bouncing and slowing until it lands.</summary>
    BeginFlight,
}
