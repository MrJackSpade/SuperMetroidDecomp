namespace SuperMetroid.Core.Audio;

/// <summary>
/// How full its library's sound queue may be for a sound request to join it (#1410). Each caller of the original's
/// sound queue chose its limit (the queue's Max1 through Max15 entry points): a request is dropped when the queue
/// already holds that many unread requests, so a common sound can give way while a rarer one still gets in.
/// </summary>
public readonly record struct SoundQueueLimit
{
    /// <summary>A limit of <paramref name="requests"/> unread requests.</summary>
    /// <param name="requests">The unread requests at which a new request is dropped, 1 through <see cref="AudioQueueDefinitions.MaximumSoundOccupancy"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="requests"/> is outside 1 through <see cref="AudioQueueDefinitions.MaximumSoundOccupancy"/>.</exception>
    public SoundQueueLimit(int requests) =>
        Requests = Ensure.BetweenInclusive(1, AudioQueueDefinitions.MaximumSoundOccupancy, requests);

    /// <summary>
    /// The unread requests at which the queue drops a new request, 1 through
    /// <see cref="AudioQueueDefinitions.MaximumSoundOccupancy"/>: a queue keeps one position empty, so fifteen admits a
    /// request into any queue that is not full.
    /// </summary>
    public int Requests { get; }

    /// <inheritdoc />
    public override string ToString() => $"at most {Requests} queued";
}
