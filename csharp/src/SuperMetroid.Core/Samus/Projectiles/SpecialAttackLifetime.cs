namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// How long a special attack particle's stage has left (#2529): the Ice particle's orbit before it turns outward, and the
/// Wave particle's homing before it ends. Each update the stage runs counts one down, and the stage ends on the update that
/// leaves none, so the count never passes zero.
/// </summary>
/// <param name="updates">The updates the stage lasts, one or more.</param>
/// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is below one.</exception>
internal sealed class SpecialAttackLifetime(int updates)
{
    /// <summary>The updates the stage has left, counting the update about to run; none once it has ended.</summary>
    [IntegerQuantity("updates a special attack stage has left")]
    internal int UpdatesLeft { get; private set; } = Ensure.BetweenInclusive(1, int.MaxValue, updates);

    /// <summary>Counts one update of the stage down: whether it was the stage's last.</summary>
    /// <exception cref="InvalidOperationException">The stage has already ended.</exception>
    internal bool CountsDownToEnd()
    {
        if (UpdatesLeft == 0)
            throw new InvalidOperationException("A special attack stage counted down after it ended.");
        return --UpdatesLeft == 0;
    }
}
