namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How crowded its library's sound queue may be for a sound request to still join it (#1410, #1478). Each request
/// states its priority: a request is turned away when the queue already holds as many waiting requests as its priority
/// tolerates, so a common sound gives way in a busy queue while a rarer one still gets in. Each priority's tolerance is
/// part of <see cref="SoundQueueAdmission"/>'s rule, so the mapping from priority to waiting requests is owned here,
/// beside the rule that reads it.
/// </summary>
/// <remarks>
/// Evidence: the original requested every sound through one of a family of queue routines, each comparing the
/// library's waiting requests with its own fixed limit (one, three, six, nine or fifteen); the five priorities are those
/// limits, and #1478 replaced the per-call routines with them at every requester.
/// </remarks>
[CompatibilityService("how many waiting requests a sound request tolerates in its library's queue")]
public readonly record struct SoundRequestPriority
{
    /// <summary>A priority that admits a request while fewer than <paramref name="requests"/> requests wait.</summary>
    /// <param name="requests">The waiting requests at which a new request is turned away, 1 through <see cref="SoundQueueAdmission.MaximumOccupancy"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="requests"/> is outside 1 through <see cref="SoundQueueAdmission.MaximumOccupancy"/>.</exception>
    private SoundRequestPriority(int requests) =>
        Requests = Ensure.BetweenInclusive(1, SoundQueueAdmission.MaximumOccupancy, requests);

    /// <summary>Joins only a queue with nothing waiting.</summary>
    public static SoundRequestPriority WhenIdle { get; } = new(1);

    /// <summary>Joins a queue holding at most two waiting requests.</summary>
    public static SoundRequestPriority Low { get; } = new(3);

    /// <summary>Joins a queue holding at most five waiting requests: the usual priority.</summary>
    public static SoundRequestPriority Normal { get; } = new(6);

    /// <summary>Joins a queue holding at most eight waiting requests.</summary>
    public static SoundRequestPriority High { get; } = new(9);

    /// <summary>Joins any queue that is not full.</summary>
    public static SoundRequestPriority Essential { get; } = new(15);

    /// <summary>
    /// The waiting requests at which the queue turns a new request away, 1 through
    /// <see cref="SoundQueueAdmission.MaximumOccupancy"/>: a queue holds at most fifteen, so fifteen admits a request into
    /// any queue that is not full.
    /// </summary>
    public int Requests { get; }

    /// <inheritdoc />
    public override string ToString() => $"joins below {Requests} queued";
}
