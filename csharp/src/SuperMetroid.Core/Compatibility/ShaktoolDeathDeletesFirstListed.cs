namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which enemies a Shaktool chain's death deletes (#1592): the seven enemies the room lists first, not the chain's own
/// segments by name. A kill empties the struck segment's record, which leaves its chain's owner as the first enemy, and
/// the death deletes the seven enemies from there. Both retail Shaktool room states list the chain first, so the death
/// deletes its own segments; a chain listed anywhere else would delete unrelated enemies, so such a population is
/// refused when the chain is placed (<see cref="DeletesOwnChain{TEnemy}"/>).
/// </summary>
/// <remarks>
/// Consulted when each segment is placed, to refuse a chain its death would not delete, and once per kill by the chain's
/// fatal-shot reaction after the common death, before the rest of the update runs (<see cref="KilledShaktoolSegment{TPlace, TSegment}"/>).
/// Evidence: the original fatal-shot reaction reads the chain owner's offset from the killed segment's record after the
/// common death cleared it, and deletes the seven records from that offset.
/// </remarks>
[CompatibilityService("a Shaktool chain's death deletes the seven enemies the room lists first")]
public static class ShaktoolDeathDeletesFirstListed
{
    /// <summary>The enemies a chain's death deletes, in the order the room lists them.</summary>
    /// <typeparam name="TEnemy">An enemy place.</typeparam>
    /// <param name="listedEnemies">The room's enemy places, in the order the room lists them.</param>
    /// <param name="segmentCount">How many segments the chain has.</param>
    /// <exception cref="ArgumentOutOfRangeException">The room lists fewer enemies than the chain has segments.</exception>
    public static IReadOnlyList<TEnemy> Deleted<TEnemy>(IReadOnlyList<TEnemy> listedEnemies, int segmentCount)
    {
        Ensure.NotNull(listedEnemies);
        _ = Ensure.BetweenInclusive(1, listedEnemies.Count, segmentCount);
        return [.. listedEnemies.Take(segmentCount)];
    }

    /// <summary>Whether the death of the chain led by <paramref name="leadingSaw"/> deletes the chain itself: the room lists it first.</summary>
    /// <typeparam name="TEnemy">An enemy place.</typeparam>
    /// <param name="listedEnemies">The room's enemy places, in the order the room lists them.</param>
    /// <param name="leadingSaw">The chain's leading saw.</param>
    public static bool DeletesOwnChain<TEnemy>(IReadOnlyList<TEnemy> listedEnemies, TEnemy leadingSaw)
        where TEnemy : class
    {
        Ensure.NotNull(listedEnemies);
        Ensure.NotNull(leadingSaw);
        return listedEnemies.Count != 0 && ReferenceEquals(listedEnemies[0], leadingSaw);
    }
}
