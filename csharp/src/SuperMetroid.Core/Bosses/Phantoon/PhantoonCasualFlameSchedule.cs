namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// One burst of Phantoon's casual flames: the waits before each flame in the order the mouth spends them, then the
/// cooldown after the last flame before the next burst is chosen.
/// </summary>
public sealed record PhantoonCasualFlameSchedule
{
    /// <summary>Mouth-scheduler updates waited after the burst's last flame before a new burst is chosen.</summary>
    public required int Cooldown { get; init; }

    /// <summary>
    /// Mouth-scheduler updates waited before each flame, first flame first; the burst spits one flame per entry.
    /// </summary>
    public required IReadOnlyList<int> Intervals { get; init; }

    /// <summary>Number of flames the burst spits.</summary>
    public int FlameCount => Intervals.Count;

    /// <summary>
    /// The wait before the next flame when <paramref name="remainingFlames"/> flames of the burst are still to come:
    /// the first interval when the whole burst remains, the last when one flame remains.
    /// </summary>
    /// <param name="remainingFlames">Flames still to come, from one to <see cref="FlameCount"/>.</param>
    public int IntervalBefore(int remainingFlames) =>
        Intervals[FlameCount - Ensure.BetweenInclusive(1, FlameCount, remainingFlames)];
}
