namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Whether a sound request joins its library's request queue, and how full the queue was when it was made (#1410,
/// #1478). The rule (<see cref="For"/>): a request joins while the queue holds fewer waiting requests than its
/// <see cref="SoundRequestPriority"/> tolerates. A request that joins counts as admitted even when sounds are suppressed
/// and nothing is queued.
/// </summary>
/// <remarks>
/// Consulted every time a game system requests a sound, at the point in the update where it requests it, before
/// suppression is tested. The outcome reaches gameplay: the Speed Booster's stage selection reads the occupancy, or the
/// rejection, of its echo request (see <see cref="UnauthoredBoostCadence"/>), and the door transition waits for the
/// queues the admitted requests fill to empty. Evidence: the original's sound-queue routines (the occupancy test against
/// the routine's limit before the disable-sounds test, with sixteen-position rings keeping one position empty), the
/// Speed Booster's echo selection after its library-three request (<c>$90:85A6</c>), and the SMV replays whose
/// running cadences depend on it (the Ridley full movie and the 13% movie).
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
