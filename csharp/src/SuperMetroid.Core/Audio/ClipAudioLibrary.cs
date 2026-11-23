namespace SuperMetroid.Core.Audio;

/// <summary>
/// The pre-rendered audio clips (#1305): the songs, sound effects and acceptance policies of a manifest the import
/// pipeline validated (#1502), with each clip's samples supplied by an <see cref="IAudioClipSource"/>. Each sound effect
/// has its clip, and its own clip in each music data set that redefines an instrument it plays (#2057, #2645). Construction rejects a repeated identity;
/// <see cref="ReadClip"/> checks each clip's length as it is read, so a host reads only the clips it plays. The manifest
/// itself and the audio's identity are the import's (#1616).
/// </summary>
public sealed partial class ClipAudioLibrary
{
    /// <summary>Supplies each clip's samples.</summary>
    private readonly IAudioClipSource source;
    /// <summary>Songs by clip set and track.</summary>
    private readonly Dictionary<MusicTrackKey, MusicClipEntry> music;
    /// <summary>Sound-effect entries by sound.</summary>
    private readonly Dictionary<SoundEffectId, SoundEffectClipEntry> soundEffects;
    /// <summary>Each sound's entry in the music data sets that play it differently, by set and sound (#2645).</summary>
    private readonly Dictionary<(MusicDataSet DataSet, SoundEffectId Sound), SoundEffectClipEntry> setSoundEffects = [];

    /// <summary>Creates a catalog over the import's songs and sound effects, whose clips <paramref name="source"/> supplies.</summary>
    /// <param name="initialBeat">How long the music's beat lasts before any song has played.</param>
    /// <param name="music">One clip per song of each clip set.</param>
    /// <param name="soundEffects">One entry per sound effect.</param>
    /// <param name="setSoundEffects">Each sound effect's entry in the music data sets that play it differently from its own.</param>
    /// <param name="source">Supplies each clip's samples.</param>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="InvalidDataException">The songs or sound effects repeat an identity.</exception>
    internal ClipAudioLibrary(BeatLength initialBeat, IReadOnlyList<MusicClipEntry> music, IReadOnlyList<SoundEffectClipEntry> soundEffects,
        IReadOnlyList<SoundEffectSetClipEntry> setSoundEffects, IAudioClipSource source)
    {
        InitialBeat = Ensure.NotNull(initialBeat);
        Ensure.NotNull(music);
        Ensure.NotNull(soundEffects);
        this.source = Ensure.NotNull(source);
        this.music = [];
        foreach (MusicClipEntry entry in music)
        {
            if (!this.music.TryAdd(new MusicTrackKey(entry.AudioSet, entry.Track), entry))
                throw new InvalidDataException($"Clip manifest repeats {entry.AudioSet} track {entry.Track}.");
        }
        this.soundEffects = [];
        foreach (SoundEffectClipEntry entry in soundEffects)
        {
            if (!this.soundEffects.TryAdd(entry.Sound, entry))
                throw new InvalidDataException($"Clip manifest repeats {entry.Sound}.");
        }
        foreach (SoundEffectSetClipEntry entry in Ensure.NotNull(setSoundEffects))
        {
            if (!this.soundEffects.ContainsKey(entry.Entry.Sound))
                throw new InvalidDataException($"Clip manifest gives {entry.Entry.Sound} a {entry.DataSet} clip but no clip of its own.");
            if (!this.setSoundEffects.TryAdd((entry.DataSet, entry.Entry.Sound), entry.Entry))
                throw new InvalidDataException($"Clip manifest repeats {entry.Entry.Sound} in {entry.DataSet}.");
        }
    }

    /// <summary>
    /// How long the music's beat lasts at power-on, before any song has played (#1725): the beat the first song change
    /// waits for, as the songs were rendered from.
    /// </summary>
    public BeatLength InitialBeat { get; }

    /// <summary>The song <paramref name="track"/> in <paramref name="clipSet"/>'s clip set.</summary>
    /// <exception cref="KeyNotFoundException">The clip set has no such song.</exception>
    public MusicClipEntry GetMusic(MusicDataSet clipSet, MusicTrack track) =>
        music.TryGetValue(new MusicTrackKey(clipSet, track), out MusicClipEntry? entry)
            ? entry
            : throw new KeyNotFoundException($"No song for {clipSet} {track}.");

    /// <summary>The sound effect's entry.</summary>
    /// <exception cref="KeyNotFoundException">The library has no such effect.</exception>
    public SoundEffectClipEntry GetSoundEffect(SoundEffectId sound) =>
        soundEffects.TryGetValue(sound, out SoundEffectClipEntry? entry)
            ? entry
            : throw new KeyNotFoundException($"No sound effect {sound}.");

    /// <summary>
    /// The sound effect's entry as <paramref name="clipSet"/>'s music data set plays it (#2645): its own there where the set
    /// redefines an instrument it plays, else its one entry.
    /// </summary>
    /// <exception cref="KeyNotFoundException">The library has no such effect.</exception>
    public SoundEffectClipEntry GetSoundEffect(MusicDataSet clipSet, SoundEffectId sound) =>
        setSoundEffects.TryGetValue((clipSet, sound), out SoundEffectClipEntry? entry) ? entry : GetSoundEffect(sound);

    /// <summary>A clip's interleaved stereo samples from the source, checked against the manifest's frame count.</summary>
    /// <exception cref="InvalidDataException">The source's samples do not match the manifest.</exception>
    public short[] ReadClip(AudioClipFile clip)
    {
        Ensure.NotNull(clip);
        short[] samples = source.ReadSamples(clip);
        if (samples.Length != clip.Frames * PcmSampleFormat.StereoChannelCount)
        {
            throw new InvalidDataException(
                $"Audio clip '{clip.Path}' holds {samples.Length / PcmSampleFormat.StereoChannelCount} frames, expected {clip.Frames}.");
        }
        return samples;
    }
}
