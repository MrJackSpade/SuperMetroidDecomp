namespace SuperMetroid.Core.Bosses;

/// <summary>
/// A countdown of a boss fight (#2212, #2233, #2253): Mother Brain's fight, the Baby Metroid's cutscene, the Rinkas' and the
/// Shitroid's waits, both Ridleys' phases and breakup fragments, and the Golden Torizo's eggs. It holds the game's count and runs out on the update whose step
/// takes the count below zero, so loaded with a count it lasts one update more than that count, and a count of zero runs
/// out on its first step. Every such wait of theirs is one of these.
/// </summary>
public sealed class BossCountdown
{
    /// <summary>The count before the step that runs the countdown out: the updates it lasts are one more.</summary>
    public int Count { get; internal set; }

    /// <summary>Steps the countdown one update: whether it ran out on this step.</summary>
    internal bool StepRunsOut() => --Count < 0;

    /// <summary>Steps the countdown one update: whether it is still running after this step.</summary>
    internal bool StepKeepsRunning() => !StepRunsOut();
}
