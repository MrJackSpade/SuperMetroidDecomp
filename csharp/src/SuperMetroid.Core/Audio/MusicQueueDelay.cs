namespace SuperMetroid.Core.Audio;

/// <summary>
/// How long a request waits on the music queue before it plays (#1410): the music-queue passes its countdown runs.
/// No request waits fewer than <see cref="AudioQueueDefinitions.MinimumMusicDelayFrames"/> updates; a shorter request is
/// lengthened to that.
/// </summary>
public readonly record struct MusicQueueDelay
{
    /// <summary>A delay of <paramref name="updates"/> music-queue passes, already lengthened to the shortest one.</summary>
    /// <param name="updates">Music-queue passes, zero or more.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is negative.</exception>
    internal MusicQueueDelay(int updates) => Updates = Ensure.AtLeastZero(updates);

    /// <summary>
    /// Music-queue passes the request waits before it plays, zero or more. The queue's wait starts at this count
    /// when it fetches the request and dispatches the request on the pass that brings it to zero.
    /// The longest delay the game requests is a fanfare's 365 updates.
    /// </summary>
    public int Updates { get; }

    /// <summary>The shortest delay, which most queued requests use.</summary>
    public static MusicQueueDelay Shortest { get; } = new(AudioQueueDefinitions.MinimumMusicDelayFrames);

    /// <summary>A delay of <paramref name="requestedUpdates"/> music-queue passes, lengthened to <see cref="Shortest"/> when shorter.</summary>
    /// <param name="requestedUpdates">The updates asked for, zero or more.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="requestedUpdates"/> is negative.</exception>
    public static MusicQueueDelay AtLeastShortest(int requestedUpdates) =>
        new(Math.Max(Ensure.AtLeastZero(requestedUpdates), AudioQueueDefinitions.MinimumMusicDelayFrames));

    /// <inheritdoc />
    public override string ToString() => $"{Updates} updates";
}
