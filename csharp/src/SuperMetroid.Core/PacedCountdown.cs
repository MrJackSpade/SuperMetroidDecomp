namespace SuperMetroid.Core;

/// <summary>
/// A countdown that each update counts a pace off, rather than one update (#2561): it runs out on the update that takes it
/// below zero, and its owner starts it again. Ridley's wing beat counts the wait for his wings' next pose so, at the pace
/// his flight speed sets.
/// </summary>
public sealed partial class PacedCountdown
{
    /// <summary>The count left: it runs out once a pace takes it below zero.</summary>
    [IntegerQuantity("the count left before the countdown runs below zero")]
    internal int Left { get; set; }

    /// <summary>Starts the countdown again at <paramref name="count"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    internal void Restart(int count) => Left = Ensure.AtLeastZero(count);

    /// <summary>Counts one update's <paramref name="pace"/> off the countdown.</summary>
    /// <returns>Whether this update took the countdown below zero.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pace"/> is negative.</exception>
    internal bool CountOff(int pace)
    {
        Left -= Ensure.AtLeastZero(pace);
        return Left < 0;
    }
}
