namespace SuperMetroid.Core.Audio;

/// <summary>
/// How long a request waits on the music queue before it plays (#1410): the music-handler updates its countdown runs.
/// No request waits fewer than <see cref="AudioQueueDefinitions.MinimumMusicDelayFrames"/> updates; a shorter request is
/// lengthened to that.
/// </summary>
public readonly record struct MusicQueueDelay
{
    /// <summary>A delay of <paramref name="updates"/> music-handler updates, already lengthened to the shortest one.</summary>
    /// <param name="updates">Music-handler updates, 0 through 65,535; zero is an empty queue position's.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is outside 0 through 65,535.</exception>
    internal MusicQueueDelay(int updates) => Updates = Ensure.BetweenInclusive(0, ushort.MaxValue, updates);

    /// <summary>
    /// Music-handler updates the request waits before it plays, 0 through 65,535. The queue's countdown starts at this
    /// count; zero marks an empty queue position. The countdown is a sixteen-bit word in the original, which bounds it.
    /// </summary>
    public int Updates { get; }

    /// <summary>The shortest delay, which most queued requests use.</summary>
    public static MusicQueueDelay Shortest { get; } = new(AudioQueueDefinitions.MinimumMusicDelayFrames);

    /// <summary>A delay of <paramref name="requestedUpdates"/> music-handler updates, lengthened to <see cref="Shortest"/> when shorter.</summary>
    /// <param name="requestedUpdates">The updates asked for, 0 through 65,535.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="requestedUpdates"/> is outside 0 through 65,535.</exception>
    public static MusicQueueDelay AtLeastShortest(int requestedUpdates) =>
        new(Math.Max(Ensure.BetweenInclusive(0, ushort.MaxValue, requestedUpdates), AudioQueueDefinitions.MinimumMusicDelayFrames));

    /// <inheritdoc />
    public override string ToString() => $"{Updates} updates";
}
