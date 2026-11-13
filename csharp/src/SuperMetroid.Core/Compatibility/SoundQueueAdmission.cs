namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Whether a sound request joins its library's request queue, and how full the queue was when it was made (#1410,
/// #1478, #1620). The rule: a request is admitted while the queue holds fewer waiting requests than its
/// <see cref="SoundRequestPriority"/> tolerates (<see cref="For"/>), and an admitted request joins the queue unless the
/// game turns sounds away as it is made (<see cref="Joins"/>, <see cref="SoundSuppression"/>): its sounds are disabled,
/// an attract demo is playing or a power bomb is exploding. A request turned away after its admission still counts as
/// admitted, and nothing is queued.
/// </summary>
/// <remarks>
/// Every sound request the game makes, gameplay's and the menus' alike, passes this one test at the point in the update
/// where it is made, so no requester tests the suppression itself. The outcome reaches gameplay: the Speed Booster's
/// stage selection reads the occupancy, or the rejection, of its echo request (see <see cref="UnauthoredBoostCadence"/>),
/// and the door transition waits for the queues the joined requests fill to empty. Evidence: the original's three
/// sound-queue routines (<c>$80:9051</c>, <c>$80:90D3</c> and <c>$80:9155</c>, one per library, which all fifteen
/// limit entries <c>$80:9021</c>-<c>$80:914D</c> call): each tests the occupancy against the entry's limit first, then
/// the disable-sounds switch, the game state (below the demo states) and the sign of the power-bomb explosion status
/// (set by the explosion's spawn and by Crystal Flash's, cleared by their cleanup), and returns without
/// queueing when any of them is set; sixteen-position rings keep one position empty. The music queue routines
/// (<c>$80:8FC1</c>, <c>$80:8FF7</c>) test only the game state, so a power bomb never turns music away. The Speed
/// Booster's echo selection after its room-library request (<c>$90:85A6</c>) and the SMV replays whose running cadences
/// depend on it (the Ridley full movie and the 13% movie) confirm the occupancy.
/// </remarks>
[CompatibilityService("whether a sound request joins its library's queue, and how full the queue was")]
public readonly record struct SoundQueueAdmission
{
    /// <summary>The most waiting requests one library's queue holds.</summary>
    public const int MaximumOccupancy = 15;

    /// <summary>Whether the queue held fewer waiting requests than the request tolerates.</summary>
    public bool IsAdmitted { get; private init; }

    /// <summary>The waiting requests the queue held, 0 through <see cref="MaximumOccupancy"/>.</summary>
    public int Occupancy { get; private init; }

    /// <summary>The admission rule: a request of <paramref name="priority"/> made while <paramref name="occupancy"/> requests wait.</summary>
    /// <param name="occupancy">The waiting requests in the request's library queue, 0 through <see cref="MaximumOccupancy"/>.</param>
    /// <param name="priority">How many waiting requests the request tolerates.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="occupancy"/> is outside 0 through <see cref="MaximumOccupancy"/>.</exception>
    public static SoundQueueAdmission For(int occupancy, SoundRequestPriority priority) =>
        occupancy < priority.Requests ? Admitted(occupancy) : Rejected(occupancy);

    /// <summary>
    /// Whether the admitted request joins its queue: only when it was admitted and <paramref name="suppression"/>, the
    /// game's state as the request is made, turns no sound away.
    /// </summary>
    /// <param name="suppression">Whether the game turns sounds away as the request is made.</param>
    public bool Joins(SoundSuppression suppression) => IsAdmitted && !suppression.TurnsSoundsAway;

    /// <summary>A request that joined a queue holding <paramref name="occupancy"/> waiting requests.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="occupancy"/> is outside 0 through <see cref="MaximumOccupancy"/>.</exception>
    public static SoundQueueAdmission Admitted(int occupancy) => new()
    {
        IsAdmitted = true,
        Occupancy = Ensure.BetweenInclusive(0, MaximumOccupancy, occupancy),
    };

    /// <summary>A request turned away by a queue holding <paramref name="occupancy"/> waiting requests.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="occupancy"/> is outside 0 through <see cref="MaximumOccupancy"/>.</exception>
    public static SoundQueueAdmission Rejected(int occupancy) => new()
    {
        IsAdmitted = false,
        Occupancy = Ensure.BetweenInclusive(0, MaximumOccupancy, occupancy),
    };
}
