namespace SuperMetroid.Core.Fx;

/// <summary>
/// A clock that falls due once every <see cref="Interval"/> updates it runs (#2367): it counts the updates left until it
/// falls due, and on the update it does it starts its next interval. Lava keeps one for its bubbling.
/// </summary>
/// <param name="Interval">The updates between the clock's falls, at least one.</param>
/// <param name="UpdatesLeft">The updates the clock runs before it next falls due, the falling one included.</param>
internal readonly partial record struct IntervalClock(int Interval, int UpdatesLeft)
{
    /// <summary>A clock that falls due on the <paramref name="interval"/>th update it runs, and every interval after.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The interval is not one update or more.</exception>
    internal static IntervalClock Starting(int interval) =>
        new(Ensure.BetweenInclusive(1, int.MaxValue, interval), interval);

    /// <summary>
    /// The clock one update on: one update fewer left, or, on the update it falls due, its next whole interval left.
    /// </summary>
    /// <param name="due">Whether the clock fell due on this update.</param>
    /// <exception cref="InvalidOperationException">The clock was never started, so it has no interval to run.</exception>
    internal IntervalClock Run(out bool due)
    {
        if (UpdatesLeft < 1)
            throw new InvalidOperationException("A clock that was never started has no interval to run.");
        due = UpdatesLeft == 1;
        return this with { UpdatesLeft = due ? Interval : UpdatesLeft - 1 };
    }
}
