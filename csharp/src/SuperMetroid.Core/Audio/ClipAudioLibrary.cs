namespace SuperMetroid.Core.Audio;

/// <summary>
/// The pre-rendered audio clips (#1305): the songs, sound effects and acceptance policies of a manifest the import
/// pipeline validated (#1502), with each clip's samples supplied by an <see cref="IAudioClipSource"/>. Construction
/// rejects a repeated identity; <see cref="ReadClip"/> checks each clip's length as it is read, so a host reads only
/// the clips it plays. The manifest itself and the audio's identity are the import's (#1616).
/// </summary>
public sealed class ClipAudioLibrary
{
    /// <summary>Supplies each clip's samples.</summary>
    private readonly IAudioClipSource source;
    /// <summary>Songs by clip set and track.</summary>
    private readonly Dictionary<MusicTrackKey, MusicClipEntry> music;
    /// <summary>Sound-effect entries by clip set and sound.</summary>
    private readonly Dictionary<SoundEffectClipKey, SoundEffectClipEntry> soundEffects;
    /// <summary>Each sound's acceptance policy, the same in every data set.</summary>
    private readonly Dictionary<SoundEffectId, SoundEffectClipPolicy> policies = [];
    /// <summary>Effects with no clip in a clip set, by clip set and sound.</summary>
    private readonly HashSet<SoundEffectClipKey> unrenderable;

    /// <summary>Creates a catalog over the import's songs and sound effects, whose clips <paramref name="source"/> supplies.</summary>
    /// <param name="music">One clip per song of each clip set.</param>
    /// <param name="soundEffects">One entry per renderable (data set, effect).</param>
    /// <param name="unrenderable">The (data set, effect) pairs with no clip.</param>
    /// <param name="source">Supplies each clip's samples.</param>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="InvalidDataException">The songs or sound effects repeat an identity.</exception>
    internal ClipAudioLibrary(IReadOnlyList<MusicClipEntry> music, IReadOnlyList<SoundEffectClipEntry> soundEffects,
        IReadOnlyList<UnrenderableSoundEffect> unrenderable, IAudioClipSource source)
    {
        Ensure.NotNull(music);
        Ensure.NotNull(soundEffects);
        Ensure.NotNull(unrenderable);
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
            if (!this.soundEffects.TryAdd(new SoundEffectClipKey(entry.Context, entry.Sound), entry))
                throw new InvalidDataException($"Clip manifest repeats {entry.Sound} in {ContextName(entry.Context)}.");
            if (!policies.TryAdd(entry.Sound, entry.Policy) && policies[entry.Sound] != entry.Policy)
                throw new InvalidDataException($"Clip manifest gives {entry.Sound} different policies by context.");
        }
        this.unrenderable = [];
        foreach (UnrenderableSoundEffect entry in unrenderable)
        {
            if (this.soundEffects.ContainsKey(new SoundEffectClipKey(entry.Context, entry.Sound)) ||
                !this.unrenderable.Add(new SoundEffectClipKey(entry.Context, entry.Sound)))
            {
                throw new InvalidDataException(
                    $"Clip manifest lists {entry.Sound} in {ContextName(entry.Context)} twice.");
            }
        }
    }

    /// <summary>The song <paramref name="track"/> in <paramref name="clipSet"/>'s clip set.</summary>
    /// <exception cref="KeyNotFoundException">The clip set has no such song.</exception>
    public MusicClipEntry GetMusic(MusicDataSet clipSet, MusicTrack track) =>
        music.TryGetValue(new MusicTrackKey(clipSet, track), out MusicClipEntry? entry)
            ? entry
            : throw new KeyNotFoundException($"No song for {clipSet} {track}.");

    /// <summary>The sound effect's entry in <paramref name="context"/>'s clip set (null for only the shared sounds).</summary>
    /// <exception cref="InvalidDataException">The effect plays samples the clip set lacks, so it has no clip there.</exception>
    /// <exception cref="KeyNotFoundException">The library has no such effect.</exception>
    public SoundEffectClipEntry GetSoundEffect(MusicDataSet? context, SoundEffectId sound)
    {
        if (soundEffects.TryGetValue(new SoundEffectClipKey(context, sound), out SoundEffectClipEntry? entry))
            return entry;
        if (unrenderable.Contains(new SoundEffectClipKey(context, sound)))
            throw new InvalidDataException($"{sound} has no clip in {ContextName(context)}: the clip set lacks a sample it plays.");
        throw new KeyNotFoundException($"No sound effect {sound} in {ContextName(context)}.");
    }

    /// <summary>The sound's acceptance policy, which does not depend on the data set.</summary>
    /// <exception cref="KeyNotFoundException">No context renders the effect.</exception>
    public SoundEffectClipPolicy GetPolicy(SoundEffectId sound) =>
        policies.TryGetValue(sound, out SoundEffectClipPolicy? policy)
            ? policy
            : throw new KeyNotFoundException($"No sound effect {sound} in any data set.");

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

    /// <summary>A data set's name in diagnostics.</summary>
    private static string ContextName(MusicDataSet? context) => context?.ToString() ?? "the shared sounds";
}
