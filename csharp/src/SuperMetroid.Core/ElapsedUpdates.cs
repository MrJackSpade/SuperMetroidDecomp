namespace SuperMetroid.Core;

/// <summary>
/// The updates counted since something began, with no length of its own (#2561): a phase that ends on a condition rather
/// than after a number of updates, whose cadence reads its age. Its owner counts it once on each of its updates; a wait
/// with a length is an <see cref="UpdateCountdown"/> instead.
/// </summary>
public sealed class ElapsedUpdates
{
    /// <summary>The updates counted since the count began.</summary>
    [IntegerQuantity("updates counted since the count began")]
    public int Age { get; internal set; }

    /// <summary>Counts one update.</summary>
    internal void Count() => Age++;

    /// <summary>Begins the count again from none.</summary>
    internal void Restart() => Age = 0;
}
