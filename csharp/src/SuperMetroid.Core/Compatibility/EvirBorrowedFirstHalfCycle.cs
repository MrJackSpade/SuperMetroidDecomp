namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which enemy an Evir body borrows its first bobbing half-cycle from (#1542): the enemy its room lists at a place fixed
/// by the body's bobbing speed, whatever that enemy is as the population loads. Instead of half its own
/// direction-change period, the body takes half the upper eight bits of one of that enemy's values: the slower retail
/// speed reads the second enemy's direction-change period, the faster the third enemy's colours. For the second Evir of
/// either retail room the third enemy is the first Evir's spit, already carrying its body's colours, so the first
/// half-cycle is not zero; an enemy not yet loaded gives zero. The Evir's own definitions name the place and the value
/// for each speed.
/// </summary>
/// <remarks>
/// Consulted once, by each Evir body's initialization as the room's population loads. Evidence: the initialization at
/// <c>$A8:8811</c> indexes its direction-change period by the speed's row offset in place of the body's own record, so
/// the word it halves lies in a fixed other enemy's record; the Evir initial-timer check loads East Sand Hall and
/// compares every body's first half-cycle with the value at that place.
/// </remarks>
[CompatibilityService("an Evir body's first bobbing half-cycle is half a value of the enemy its room lists at a place fixed by its bobbing speed")]
public static class EvirBorrowedFirstHalfCycle
{
    /// <summary>The enemy the room lists at <paramref name="lenderPlace"/>, as the population has loaded it so far.</summary>
    /// <typeparam name="TEnemy">An enemy place.</typeparam>
    /// <param name="listed">The room's enemy places, in the order its population lists them.</param>
    /// <param name="lenderPlace">The zero-based place the body's speed fixes.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="lenderPlace"/> is not one of the room's places.</exception>
    public static TEnemy Lender<TEnemy>(IReadOnlyList<TEnemy> listed, int lenderPlace)
    {
        Ensure.NotNull(listed);
        return listed[Ensure.BetweenInclusive(0, listed.Count - 1, lenderPlace)];
    }
}
