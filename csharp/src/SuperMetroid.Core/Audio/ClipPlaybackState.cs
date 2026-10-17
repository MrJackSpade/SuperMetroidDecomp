namespace SuperMetroid.Core.Audio;

/// <summary>
/// The clip backend's playback state (#1305), kept by the session so a restored session continues
/// its audio: the bank context, the music port and tempo timer, the music clip and its start, each
/// sound library's sound, priority and low-health mode, and the clips still releasing. Positions
/// are absolute native stereo frames since the session's first update.
/// </summary>
public sealed class ClipPlaybackState
{
    /// <summary>The three sound libraries, in queue order.</summary>
    internal readonly ClipLibraryState[] Libraries = [new(), new(), new()];

    /// <summary>Clips cut by a newer sound, a track change or an upload, fading over one key-off release.</summary>
    internal readonly List<ReleasingClip> Releasing = [];

    /// <summary>Native stereo frames rendered so far.</summary>
    internal long Sample;

    /// <summary>The music bank uploaded last, or null while only the common upload is installed.</summary>
    internal AudioBank? Context;

    /// <summary>The track the music port holds, 0 through 255; zero when stopped or after a track ended.</summary>
    internal int MusicPort;

    /// <summary>The music-port write the driver consumes at its next music tick, or null when none is pending.</summary>
    internal int? MusicLatch;

    /// <summary>The tempo accumulator; each overflow is a music tick.</summary>
    internal TickAccumulator TempoAccumulator;

    /// <summary>The tempo in force while no track plays, 0 through 255: the driver's power-on tempo, then each track's last.</summary>
    internal int Tempo = SpcPortDefinitions.InitialTempo;

    /// <summary>The playing music clip, or null.</summary>
    internal MusicClipEntry? Music;

    /// <summary>Absolute frame of the music clip's frame zero.</summary>
    internal long MusicStart;
}
