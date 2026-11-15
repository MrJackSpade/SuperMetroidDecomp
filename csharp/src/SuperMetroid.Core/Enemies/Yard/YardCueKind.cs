namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// What a signal cue of the Yard's animations marks: a turn, a hide or a flight beginning, or a pose shown again while
/// hiding or by chance.
/// </summary>
internal enum YardCueKind
{
    /// <summary>While hiding, or on a 50% chance, shows the previous pose again; otherwise runs on.</summary>
    RepeatPreviousPoseUnlessContinuing,
    /// <summary>The Yard begins turning a corner, holding still until the turn's animation starts its crawl.</summary>
    BeginTurn,
    /// <summary>The Yard begins hiding in its shell, watching whether its surface still holds it.</summary>
    BeginHiding,
    /// <summary>The Yard's shell begins its flight, falling, bouncing and slowing until it lands.</summary>
    BeginFlight,
}
