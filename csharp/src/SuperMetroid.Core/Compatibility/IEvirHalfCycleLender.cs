namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What an Evir body's first half-cycle (<see cref="EvirBorrowedFirstHalfCycle"/>) reads of the enemy its room lists at
/// the place its bobbing speed fixes (#2044).
/// </summary>
/// <typeparam name="TEnemy">An enemy place.</typeparam>
/// <remarks>
/// Consulted by <see cref="EvirBorrowedFirstHalfCycle"/> once, as each Evir body initializes while the room's population
/// loads. Evidence: the initialization at <c>$A8:8811</c> reads the lender's direction-change period, or for the faster
/// speed its colours, from the record at the place the speed fixes, whatever enemy holds it; an empty place reads zero.
/// </remarks>
[CompatibilityService("what an Evir body's first bobbing half-cycle reads of the enemy its room lists at the place its bobbing speed fixes: whether the place has loaded, its direction-change period, and whether it is an Evir's spit")]
public interface IEvirHalfCycleLender<in TEnemy>
{
    /// <summary>Whether <paramref name="enemy"/> is a place the population has not loaded yet.</summary>
    bool IsEmpty(TEnemy enemy);

    /// <summary>
    /// The direction-change period, in updates, the record of <paramref name="enemy"/> holds: an Evir body's or its arms',
    /// or zero for a place the population has not loaded yet, whose record is cleared.
    /// </summary>
    int DirectionChangePeriod(TEnemy enemy);

    /// <summary>Whether <paramref name="enemy"/> is an Evir's spit.</summary>
    bool IsEvirSpit(TEnemy enemy);
}
