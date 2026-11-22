namespace SuperMetroid.Core.Audio;

/// <summary>
/// The clip player's playback state (#1305, #1478), kept by the session so a restored session continues its audio: the
/// clip set loaded, the song playing and from when, the song change waiting for the music's next beat, the music's beat
/// (#1725), each sound library's sound, priority and low-health mode, and the clips still fading out. Positions are
/// absolute clip frames since the session's first update. Presentation only: no gameplay update reads it.
/// </summary>
public sealed partial class ClipPlaybackState
{
    /// <summary>Creates a <see cref="ClipPlaybackState"/>.</summary>
    /// <param name="initialBeat">How long the music's beat lasts before any song has played: the clip set's <see cref="ClipAudioLibrary.InitialBeat"/>.</param>
    public ClipPlaybackState(BeatLength initialBeat)
    {
        this.Beat = new(initialBeat);
    }

    /// <summary>Each sound library's clip playback, kept by its library (#1755).</summary>
    internal readonly Dictionary<SoundEffectLibrary, ClipLibraryState> Libraries =
        SoundEffectLibraries.InQueueOrder.ToDictionary(library => library, _ => new ClipLibraryState());

    /// <summary>Clips cut by a newer sound, a song change or a clip-set switch, fading out.</summary>
    internal readonly List<ReleasingClip> Releasing = [];

    /// <summary>Clip frames played so far.</summary>
    internal long Sample { get; private set; }

    /// <summary>
    /// The clip set the songs are asked of, named by its music data set: the one loaded last, and from power-on until the
    /// first set loads <see cref="AudioQueueDefinitions.PowerOnDataSet"/>.
    /// </summary>
    internal MusicDataSet ClipSet { get; private set; } = AudioQueueDefinitions.PowerOnDataSet;

    /// <summary>The song playing, or null when stopped or after a song ended.</summary>
    internal MusicClipEntry? Song { get; private set; }

    /// <summary>The track of the song playing, or null when none plays.</summary>
    internal MusicTrack? PlayingTrack => Song?.Track;

    /// <summary>Absolute frame of the song's frame zero.</summary>
    internal long SongStart { get; private set; }

    /// <summary>The song change waiting for the music's next beat, which the playback's step at that beat takes.</summary>
    internal SongChangeAtBeat<SongChange> SongChange { get; } = new();

    /// <summary>The music's beat: how much of it has played, and how long it lasts while no song plays.</summary>
    internal MusicBeat Beat { get; }

    /// <summary>Records that playback has reached absolute frame <paramref name="sample"/>.</summary>
    internal void RenderedTo(long sample) => Sample = sample;

    /// <summary>Switching to <paramref name="clipSet"/>'s clips stops the song and drops the waiting song change.</summary>
    internal void SwitchClipSet(MusicDataSet clipSet)
    {
        StopMusic();
        ClipSet = clipSet;
    }

    /// <summary>Installing the shared sounds stops the song and drops the waiting song change; the songs asked for next are of the set loaded next.</summary>
    internal void StopMusic()
    {
        StopSong();
        SongChange.Forget();
    }

    /// <summary>The song played to its end: the music keeps the song's last beat length and no song plays.</summary>
    internal void EndSong(BeatLength lastBeat)
    {
        Beat.Hold(lastBeat);
        StopSong();
    }

    /// <summary>Plays <paramref name="song"/> from <paramref name="now"/>.</summary>
    internal void PlaySong(MusicClipEntry song, long now)
    {
        Song = Ensure.NotNull(song);
        SongStart = now;
    }

    /// <summary>Stops the song; none plays until the next change.</summary>
    internal void StopSong() => Song = null;
}
