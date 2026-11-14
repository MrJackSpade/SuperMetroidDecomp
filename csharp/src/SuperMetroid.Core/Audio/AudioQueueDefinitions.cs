namespace SuperMetroid.Core.Audio;

/// <summary>The update counts of the game's music queue and its three sound queues.</summary>
public static class AudioQueueDefinitions
{
    /// <summary>The shortest music delay, in music-queue passes; shorter requested delays are lengthened to it.</summary>
    public const int MinimumMusicDelayFrames = 8;

    /// <summary>Sound-queue passes after a track change or a data set change during which every library's request is withdrawn.</summary>
    public const int MusicChangeSoundDowntimePasses = 8;

    /// <summary>Music-queue passes a permanent item's fanfare plays before the queued stop; the room's track follows eight passes later.</summary>
    public const int PermanentItemFanfareFrames = 360;

    /// <summary>The song a permanent item's fanfare plays.</summary>
    public const MusicTrack PermanentItemTrack = MusicTrack.ItemFanfare;
}
