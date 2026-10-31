namespace SuperMetroid.Core.Audio;

/// <summary>
/// How crowded its library's sound queue may be for a sound request to still join it (#1410, #1478). Each request
/// states its priority: a request is dropped when the queue already holds as many unread requests as its priority
/// allows, so a common sound gives way in a busy queue while a rarer one still gets in.
/// </summary>
public readonly record struct SoundRequestPriority
{
    /// <summary>A priority that admits a request while fewer than <paramref name="requests"/> requests are unread.</summary>
    /// <param name="requests">The unread requests at which a new request is dropped, 1 through <see cref="AudioQueueDefinitions.MaximumSoundOccupancy"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="requests"/> is outside 1 through <see cref="AudioQueueDefinitions.MaximumSoundOccupancy"/>.</exception>
    public SoundRequestPriority(int requests) =>
        Requests = Ensure.BetweenInclusive(1, AudioQueueDefinitions.MaximumSoundOccupancy, requests);

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
    /// The unread requests at which the queue drops a new request, 1 through
    /// <see cref="AudioQueueDefinitions.MaximumSoundOccupancy"/>: a queue keeps one position empty, so fifteen admits a
    /// request into any queue that is not full.
    /// </summary>
    public int Requests { get; }

    /// <inheritdoc />
    public override string ToString() => $"joins below {Requests} queued";
}
