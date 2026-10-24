namespace SuperMetroid.Core.Audio;

/// <summary>
/// The clip backend's playback state (#1305), kept by the session so a restored session continues
/// its audio: the data set, the music port and tempo timer, the music clip and its start, each
/// sound library's sound, priority and low-health mode, and the clips still releasing. Positions
/// are absolute native stereo frames since the session's first update.
/// </summary>
public sealed class ClipPlaybackState
{
    /// <summary>The three sound libraries, in queue order.</summary>
    internal readonly ClipLibraryState[] Libraries = [new(), new(), new()];

    /// <summary>Clips cut by a newer sound, a track change or a data set change, fading over one key-off release.</summary>
    internal readonly List<ReleasingClip> Releasing = [];

    /// <summary>Native stereo frames rendered so far.</summary>
    internal long Sample { get; private set; }

    /// <summary>The music data set loaded last, or null while only the shared sounds are loaded.</summary>
    internal MusicDataSet? Context { get; private set; }

    /// <summary>The track the music port holds, 0 through 255; zero when stopped or after a track ended.</summary>
    internal int MusicPort { get; private set; }

    /// <summary>The music-port write the driver consumes at its next music tick, or null when none is pending.</summary>
    internal int? MusicLatch { get; private set; }

    /// <summary>The tempo accumulator; each overflow is a music tick.</summary>
    internal TickAccumulator TempoAccumulator { get; private set; }

    /// <summary>The tempo in force while no track plays, 0 through 255: the driver's power-on tempo, then each track's last.</summary>
    internal int Tempo { get; private set; } = SpcPortDefinitions.InitialTempo;

    /// <summary>The playing music clip, or null.</summary>
    internal MusicClipEntry? Music { get; private set; }

    /// <summary>Absolute frame of the music clip's frame zero.</summary>
    internal long MusicStart { get; private set; }

    /// <summary>Records that rendering has reached absolute frame <paramref name="sample"/>.</summary>
    internal void RenderedTo(long sample) => Sample = sample;

    /// <summary>Holds a music-port write until the next music tick.</summary>
    internal void LatchMusic(int? command) => MusicLatch = command;

    /// <summary>Takes the latched music-port write, leaving none pending.</summary>
    internal int? TakeMusicLatch()
    {
        int? latched = MusicLatch;
        MusicLatch = null;
        return latched;
    }

    /// <summary>Loading <paramref name="context"/> stops the music, empties the port and drops the pending write.</summary>
    internal void Upload(MusicDataSet? context)
    {
        Music = null;
        MusicPort = 0;
        MusicLatch = null;
        Context = context;
    }

    /// <summary>Holds <paramref name="tempo"/> while no track plays.</summary>
    internal void HoldTempo(int tempo) => Tempo = tempo;

    /// <summary>The track reached its terminator: the driver keeps its last tempo and returns the port to zero.</summary>
    internal void EndTrack(int lastTempo)
    {
        Tempo = lastTempo;
        Music = null;
        MusicPort = 0;
    }

    /// <summary>Adds <paramref name="amount"/> to the tempo accumulator and reports whether it overflowed into a music tick.</summary>
    internal bool AdvanceTempo(int amount)
    {
        (TempoAccumulator, bool musicTick) = TempoAccumulator.Advance(amount);
        return musicTick;
    }

    /// <summary>The port takes <paramref name="command"/> and plays <paramref name="music"/> from <paramref name="now"/>, or stops on track zero.</summary>
    internal void SelectTrack(int command, MusicClipEntry? music, long now)
    {
        MusicPort = command;
        Music = music;
        if (music is not null)
            MusicStart = now;
    }
}
