namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// A countdown of Mother Brain's fight, the Baby Metroid's cutscene, the Rinkas' and the Shitroid's waits (#2212, #2253).
/// It holds the game's count and runs out on the update whose step takes the count below zero, so loaded with a count it
/// lasts one update more than that count, and a count of zero runs out on its first step. Every wait of theirs that counts
/// down past zero is one of these.
/// </summary>
public sealed class MotherBrainCountdown
{
    /// <summary>The count before the step that runs the countdown out: the updates it lasts are one more.</summary>
    public int Count { get; internal set; }

    /// <summary>Steps the countdown one update: whether it ran out on this step.</summary>
    internal bool StepRunsOut() => --Count < 0;

    /// <summary>Steps the countdown one update: whether it is still running after this step.</summary>
    internal bool StepKeepsRunning() => !StepRunsOut();
}
