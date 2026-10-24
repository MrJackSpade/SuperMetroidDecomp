namespace SuperMetroid.Core.Audio;

/// <summary>The sizes and update counts of the game's music queue and its three sound queues.</summary>
public static class AudioQueueDefinitions
{
    /// <summary>Music requests the music queue holds, each with its own delay.</summary>
    public const int MusicCapacity = 8;

    /// <summary>Wrap mask for a music queue position, 0 through <see cref="MusicCapacity"/> - 1.</summary>
    public const int MusicIndexMask = MusicCapacity - 1;

    /// <summary>The sound libraries, each with its own sound queue.</summary>
    public const int SoundLibraryCount = SoundClockTiming.LibraryCount;

    /// <summary>Positions in each sound queue's ring, including the one kept empty to tell a full ring from an empty one.</summary>
    public const int SoundCapacity = 16;

    /// <summary>Wrap and occupancy mask for a sound queue position, 0 through <see cref="SoundCapacity"/> - 1.</summary>
    public const int SoundIndexMask = SoundCapacity - 1;

    /// <summary>The most unread requests a sound queue can hold: fifteen.</summary>
    public const int MaximumSoundOccupancy = SoundCapacity - 1;

    /// <summary>The shortest music delay, in music-handler calls; shorter requested delays are lengthened to it.</summary>
    public const int MinimumMusicDelayFrames = 8;

    /// <summary>Sound-handler calls after a track change or a data set change during which every library's request is withdrawn.</summary>
    public const int MusicChangeSoundDowntimeCalls = 8;

    /// <summary>Music-handler calls a permanent item's fanfare plays before the queued stop; the room's track follows eight calls later.</summary>
    public const int PermanentItemFanfareFrames = 360;

    /// <summary>The track of the loaded data set a permanent item's fanfare plays.</summary>
    public const int PermanentItemTrack = 2;
}
