namespace SuperMetroid.Core.Bosses;

/// <summary>
/// A countdown of a boss fight (#2212, #2233, #2253, #2267, #2311): Mother Brain's fight, the Baby Metroid's cutscene, the
/// Rinkas' and the Shitroid's waits, both Ridleys' phases and breakup fragments, the Golden Torizo's eggs, and Botwoon's spit
/// cooldown, pre-fall wait and wall explosions. It holds the game's count and runs out on the update whose step takes the
/// count below zero, so loaded with a count it lasts one update more than that count, and a count of zero runs out on its
/// first step. Every such wait of theirs is one of these.
/// </summary>
public sealed class BossCountdown
{
    /// <summary>The count before the step that runs the countdown out: the updates it lasts are one more.</summary>
    public int Count { get; private set; }

    /// <summary>A countdown started to run out on the step of its <paramref name="updates"/>th update.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The countdown would last no update.</exception>
    internal static BossCountdown Lasting(int updates)
    {
        var countdown = new BossCountdown();
        countdown.LastFor(updates);
        return countdown;
    }

    /// <summary>Starts the countdown so that it runs out on the step of its <paramref name="updates"/>th update.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The countdown would last no update.</exception>
    internal void LastFor(int updates) => Count = Ensure.BetweenInclusive(1, int.MaxValue, updates) - 1;

    /// <summary>Steps the countdown one update: whether it ran out on this step.</summary>
    internal bool StepRunsOut() => --Count < 0;

    /// <summary>Steps the countdown one update: whether it is still running after this step.</summary>
    internal bool StepKeepsRunning() => !StepRunsOut();
}
