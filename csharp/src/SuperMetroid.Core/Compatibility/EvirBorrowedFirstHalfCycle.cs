namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// An Evir body's first bobbing half-cycle (#1542, #2044). Instead of half its own direction-change period, the body takes
/// half a value of the enemy its room lists at a place fixed by its bobbing speed, whatever that enemy is as the
/// population loads. Every Evir in the game bobs at half or three quarters of a pixel an update: the slower reads the
/// second listed enemy's direction-change period and halves it, rounding down; the faster reads the third listed enemy's
/// colours, which for the second Evir of either room that holds Evirs are the first Evir's spit's and give a half-cycle of one
/// update. An enemy not yet loaded, still the empty place the room load left, gives zero.
/// </summary>
/// <remarks>
/// Consulted once, by each Evir body's initialization as the room's population loads. Evidence: the initialization at
/// <c>$A8:8811</c> indexes its direction-change period, the upper eight bits of the record's second parameter, by the
/// speed's row offset in place of the body's own record, so the word it halves lies in a fixed other enemy's record; the
/// Evir initial-timer check loads East Sand Hall and compares every body's first half-cycle with the value at that place.
/// </remarks>
[CompatibilityService("an Evir body's first bobbing half-cycle is half a value of the enemy its room lists at a place fixed by its bobbing speed")]
public static class EvirBorrowedFirstHalfCycle
{
    /// <summary>The slower bobbing speed the game's Evirs bob at: half a pixel an update.</summary>
    private static readonly SignedSixteenSixteen SlowerSpeed = SignedSixteenSixteen.FromPixelFraction(1, 2);

    /// <summary>The zero-based place, in the room's list, of the enemy whose direction-change period a slower body halves.</summary>
    private const int SlowerLenderPlace = 1;

    /// <summary>The faster bobbing speed the game's Evirs bob at: three quarters of a pixel an update.</summary>
    private static readonly SignedSixteenSixteen FasterSpeed = SignedSixteenSixteen.FromPixelFraction(3, 4);

    /// <summary>The zero-based place, in the room's list, of the enemy whose colours a faster body reads.</summary>
    private const int FasterLenderPlace = 2;

    /// <summary>The half-cycle an Evir spit's colours give, in updates, in both rooms holding Evirs (#1512).</summary>
    private const int SpitColoursHalfCycle = 1;

    /// <summary>
    /// The first bobbing half-cycle, in updates, of a body bobbing at <paramref name="bobbingSpeed"/> in a room whose
    /// enemies, as its population has loaded them so far, are <paramref name="listed"/> in its list's order.
    /// </summary>
    /// <typeparam name="TEnemy">An enemy place.</typeparam>
    /// <param name="bobbingSpeed">The body's bobbing speed.</param>
    /// <param name="listed">The room's enemy places, in the order its population lists them.</param>
    /// <param name="lender">What the half-cycle can read of an enemy place.</param>
    /// <exception cref="InvalidDataException">
    /// No Evir the game places bobs at <paramref name="bobbingSpeed"/>, or the faster body's lender is an enemy other than an Evir
    /// spit, which no room places there.
    /// </exception>
    public static int Of<TEnemy>(SignedSixteenSixteen bobbingSpeed, IReadOnlyList<TEnemy> listed, IEvirHalfCycleLender<TEnemy> lender)
    {
        Ensure.NotNull(listed);
        Ensure.NotNull(lender);
        if (bobbingSpeed == SlowerSpeed)
        {
            TEnemy enemy = ListedAt(listed, SlowerLenderPlace);
            return lender.IsEmpty(enemy) ? 0 : lender.DirectionChangePeriod(enemy) / 2;
        }
        if (bobbingSpeed == FasterSpeed)
        {
            TEnemy enemy = ListedAt(listed, FasterLenderPlace);
            if (lender.IsEmpty(enemy))
                return 0;
            return lender.IsEvirSpit(enemy)
                ? SpitColoursHalfCycle
                : throw new InvalidDataException("An Evir body reads the colours of an enemy other than an Evir spit, which no room places where it reads.");
        }
        throw new InvalidDataException(
            $"An Evir bobbing at {bobbingSpeed.WholePixels} pixels and {bobbingSpeed.Fraction} 65536ths per update takes its first half-cycle from a value no Evir the game places reads.");
    }

    /// <summary>The enemy the room lists at <paramref name="place"/>, as the population has loaded it so far.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="place"/> is not one of the room's places.</exception>
    private static TEnemy ListedAt<TEnemy>(IReadOnlyList<TEnemy> listed, int place) =>
        listed[Ensure.BetweenInclusive(0, listed.Count - 1, place)];
}
