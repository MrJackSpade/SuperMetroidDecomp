namespace SuperMetroid.Core.Audio;

/// <summary>
/// What one sound-queue call did: whether its library's ring was below the call's threshold, and how many unread
/// requests the ring held when it was called (<see cref="AudioQueues.QueueSound(SoundEffectId, int, bool)"/>). A request below the
/// threshold counts as admitted even when sounds are suppressed and nothing is queued.
/// </summary>
public readonly record struct SoundQueueAdmission
{
    /// <summary>Whether the ring held fewer unread requests than the threshold.</summary>
    public bool IsAdmitted { get; private init; }

    /// <summary>The unread requests the ring held, 0 through 15.</summary>
    public int Occupancy { get; private init; }

    /// <summary>A call below its threshold, made with <paramref name="occupancy"/> unread requests.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="occupancy"/> is outside 0 through 15.</exception>
    public static SoundQueueAdmission Admitted(int occupancy) => new()
    {
        IsAdmitted = true,
        Occupancy = Ensure.BetweenInclusive(0, AudioQueueDefinitions.MaximumSoundOccupancy, occupancy),
    };

    /// <summary>A call rejected at its threshold, made with <paramref name="occupancy"/> unread requests.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="occupancy"/> is outside 0 through 15.</exception>
    public static SoundQueueAdmission Rejected(int occupancy) => new()
    {
        IsAdmitted = false,
        Occupancy = Ensure.BetweenInclusive(0, AudioQueueDefinitions.MaximumSoundOccupancy, occupancy),
    };
}
