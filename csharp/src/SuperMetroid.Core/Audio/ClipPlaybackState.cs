namespace SuperMetroid.Core.Audio;

/// <summary>
/// The clip player's playback state (#1305, #1478), kept by the session so a restored session continues its audio: the
/// clip set loaded, the song playing and from when, the song change waiting for the music's next beat, how far the beat
/// has advanced and the tempo between songs, each sound library's sound, priority and low-health mode, and the clips
/// still fading out. Positions are absolute clip frames since the session's first update. Presentation only: no
/// gameplay update reads it.
/// </summary>
public sealed class ClipPlaybackState
{
    /// <summary>The parts one beat is divided into; a song's tempo counts the parts each clock step advances.</summary>
    public const int BeatParts = 256;

    /// <summary>
    /// The tempo the beat advances at before any song has played: the rate the songs' tempo maps were rendered against
    /// before a song set its own. Each song's tempo map starts at its first frame, so it does not record this one.
    /// </summary>
    internal const int InitialTempo = 16;

    /// <summary>The three sound libraries, in queue order.</summary>
    internal readonly ClipLibraryState[] Libraries = [new(), new(), new()];

    /// <summary>Clips cut by a newer sound, a song change or a clip-set switch, fading out.</summary>
    internal readonly List<ReleasingClip> Releasing = [];

    /// <summary>Clip frames played so far.</summary>
    internal long Sample { get; private set; }

    /// <summary>The clip set loaded last, named by its music data set, or null while only the shared sounds are loaded.</summary>
    internal MusicDataSet? ClipSet { get; private set; }

    /// <summary>The song playing, or null when stopped or after a song ended.</summary>
    internal MusicClipEntry? Song { get; private set; }

    /// <summary>The track of the song playing, or null when none plays.</summary>
    internal MusicTrack? PlayingTrack { get; private set; }

    /// <summary>Absolute frame of the song's frame zero.</summary>
    internal long SongStart { get; private set; }

    /// <summary>The song change waiting for the music's next beat, or null when none waits.</summary>
    internal SongChange? PendingSongChange { get; private set; }

    /// <summary>Parts of the current beat already played, 0 through <see cref="BeatParts"/> - 1.</summary>
    internal int Beat { get; private set; }

    /// <summary>The tempo while no song plays, 0 through <see cref="BeatParts"/> - 1: <see cref="InitialTempo"/>, then each song's last.</summary>
    internal int Tempo { get; private set; } = InitialTempo;

    /// <summary>Records that playback has reached absolute frame <paramref name="sample"/>.</summary>
    internal void RenderedTo(long sample) => Sample = sample;

    /// <summary>Holds <paramref name="change"/> until the music's next beat, replacing any change already waiting.</summary>
    internal void RequestSongChange(SongChange change) => PendingSongChange = change;

    /// <summary>Takes the song change waiting for this beat, leaving none.</summary>
    internal SongChange? TakeSongChange()
    {
        SongChange? change = PendingSongChange;
        PendingSongChange = null;
        return change;
    }

    /// <summary>Switching to <paramref name="clipSet"/>'s clips stops the song and drops the waiting song change.</summary>
    internal void SwitchClipSet(MusicDataSet? clipSet)
    {
        StopSong();
        PendingSongChange = null;
        ClipSet = clipSet;
    }

    /// <summary>Holds <paramref name="tempo"/> while no song plays.</summary>
    internal void HoldTempo(int tempo) => Tempo = tempo;

    /// <summary>The song played to its end: the music keeps the song's last tempo and no song plays.</summary>
    internal void EndSong(int lastTempo)
    {
        Tempo = lastTempo;
        StopSong();
    }

    /// <summary>Advances the beat by <paramref name="parts"/> and reports whether that completed it; the overshoot starts the next beat.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="parts"/> is negative.</exception>
    internal bool AdvanceBeat(int parts)
    {
        int reached = Beat + Ensure.AtLeastZero(parts);
        Beat = reached % BeatParts;
        return reached >= BeatParts;
    }

    /// <summary>Plays <paramref name="song"/>, the clip of <paramref name="track"/>, from <paramref name="now"/>.</summary>
    internal void PlaySong(MusicTrack track, MusicClipEntry song, long now)
    {
        PlayingTrack = track;
        Song = song;
        SongStart = now;
    }

    /// <summary>Stops the song; none plays until the next change.</summary>
    internal void StopSong()
    {
        PlayingTrack = null;
        Song = null;
    }
}
