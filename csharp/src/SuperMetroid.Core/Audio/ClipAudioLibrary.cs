namespace SuperMetroid.Core.Audio;

/// <summary>
/// The pre-rendered audio clips (#1305): the songs, sound effects and acceptance policies of a manifest the import
/// pipeline validated (#1502), with each clip's samples supplied by an <see cref="IAudioClipSource"/>. Each sound effect
/// has one clip, which plays whichever music data set is loaded (#2057). Construction rejects a repeated identity;
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

    /// <summary>Creates a catalog over the import's songs and sound effects, whose clips <paramref name="source"/> supplies.</summary>
    /// <param name="initialBeat">How long the music's beat lasts before any song has played.</param>
    /// <param name="music">One clip per song of each clip set.</param>
    /// <param name="soundEffects">One entry per sound effect.</param>
    /// <param name="source">Supplies each clip's samples.</param>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="InvalidDataException">The songs or sound effects repeat an identity.</exception>
    internal ClipAudioLibrary(BeatLength initialBeat, IReadOnlyList<MusicClipEntry> music, IReadOnlyList<SoundEffectClipEntry> soundEffects,
        IAudioClipSource source)
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
