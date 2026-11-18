namespace SuperMetroid.Core.Audio;

/// <summary>
/// How long a request waits on the music queue before it plays (#1410): the music-queue passes its countdown runs.
/// No request waits fewer than <see cref="AudioQueueDefinitions.MinimumMusicDelayPasses"/> passes: each delay the game
/// asks for is named, at the length it waits, in the catalog of what asks for it (#2057).
/// </summary>
public readonly record struct MusicQueueDelay
{
    /// <summary>A delay of <paramref name="updates"/> music-queue passes.</summary>
    /// <param name="updates">Music-queue passes, at least the shortest delay's.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is shorter than the shortest delay.</exception>
    internal MusicQueueDelay(int updates) =>
        Updates = Ensure.BetweenInclusive(AudioQueueDefinitions.MinimumMusicDelayPasses, int.MaxValue, updates);

    /// <summary>
    /// Music-queue passes the request waits before it plays, at least the shortest delay's. The queue's wait starts at
    /// this count when it fetches the request and dispatches the request on the pass that brings it to zero.
    /// The longest delay the game requests is the room track's wait behind a saved game's fanfare
    /// (<see cref="SuperMetroid.Core.Frontend.SavedGameAppearanceMusicDefinitions.RoomTrackDelay"/>).
    /// </summary>
    public int Updates { get; }

    /// <summary>The shortest delay, which most queued requests use.</summary>
    public static MusicQueueDelay Shortest { get; } = new(AudioQueueDefinitions.MinimumMusicDelayPasses);

    /// <inheritdoc />
    public override string ToString() => $"{Updates} updates";
}
