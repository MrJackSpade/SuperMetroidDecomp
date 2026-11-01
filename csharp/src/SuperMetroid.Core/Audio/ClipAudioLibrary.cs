namespace SuperMetroid.Core.Audio;

/// <summary>
/// The pre-rendered audio clips (#1305): a validated manifest's songs, sound effects and acceptance policies, with
/// each clip's samples supplied by an <see cref="IAudioClipSource"/>. Construction checks the manifest's rate,
/// identities, loop bounds and tempo timelines; <see cref="ReadClip"/> checks each clip's length as it is read, so a
/// host reads only the clips it plays.
/// </summary>
public sealed class ClipAudioLibrary
{
    /// <summary>Game updates per second; the clips' rate is one update's samples this many times.</summary>
    private const int UpdatesPerSecond = 60;

    /// <summary>Supplies each clip's samples.</summary>
    private readonly IAudioClipSource source;
    /// <summary>Songs by clip set and track.</summary>
    private readonly Dictionary<MusicTrackKey, MusicClipEntry> music;
    /// <summary>Sound-effect entries by clip set and sound.</summary>
    private readonly Dictionary<SoundEffectClipKey, SoundEffectClipEntry> soundEffects;
    /// <summary>Each sound's acceptance policy, the same in every data set.</summary>
    private readonly Dictionary<SoundEffectId, SoundEffectClipPolicy> policies = [];
    /// <summary>Effects with no clip in a clip set, by clip set and sound.</summary>
    private readonly Dictionary<SoundEffectClipKey, UnrenderableSoundEffect> unrenderable;

    /// <summary>
    /// Creates a catalog over <paramref name="manifest"/>, whose clips <paramref name="source"/> supplies;
    /// <paramref name="contentIdentity"/> identifies the selected audio.
    /// </summary>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="contentIdentity"/> is blank.</exception>
    /// <exception cref="InvalidDataException">
    /// The rate, an identity, a clip's length, loop bounds or a tempo timeline is invalid, or the manifest repeats an identity.
    /// </exception>
    public ClipAudioLibrary(AudioClipManifest manifest, string contentIdentity, IAudioClipSource source)
    {
        Ensure.NotNull(manifest);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentIdentity);
        this.source = Ensure.NotNull(source);
        Validate(manifest);
        Manifest = manifest;
        ContentIdentity = contentIdentity;
        music = [];
        foreach (MusicClipEntry entry in manifest.Music)
        {
            if (!music.TryAdd(new MusicTrackKey(entry.AudioSet, entry.Track), entry))
                throw new InvalidDataException($"Clip manifest repeats {entry.AudioSet} track {entry.Track}.");
        }
        soundEffects = [];
        foreach (SoundEffectClipEntry entry in manifest.SoundEffects)
        {
            if (!soundEffects.TryAdd(new SoundEffectClipKey(entry.Context, entry.Sound), entry))
                throw new InvalidDataException($"Clip manifest repeats {entry.Sound} in {ContextName(entry.Context)}.");
            if (!policies.TryAdd(entry.Sound, entry.Policy) && policies[entry.Sound] != entry.Policy)
                throw new InvalidDataException($"Clip manifest gives {entry.Sound} different policies by context.");
        }
        unrenderable = [];
        foreach (UnrenderableSoundEffect entry in manifest.Unrenderable)
        {
            if (soundEffects.ContainsKey(new SoundEffectClipKey(entry.Context, entry.Sound)) ||
                !unrenderable.TryAdd(new SoundEffectClipKey(entry.Context, entry.Sound), entry))
            {
                throw new InvalidDataException(
                    $"Clip manifest lists {entry.Sound} in {ContextName(entry.Context)} twice.");
            }
        }
    }

    /// <summary>The validated manifest.</summary>
    public AudioClipManifest Manifest { get; }

    /// <summary>
    /// The selected audio's identity, supplied by the content reader: any audible change alters it.
    /// </summary>
    public string ContentIdentity { get; }

    /// <summary>The song <paramref name="track"/> in <paramref name="clipSet"/>'s clip set.</summary>
    /// <exception cref="KeyNotFoundException">The clip set has no such song.</exception>
    public MusicClipEntry GetMusic(MusicDataSet clipSet, MusicTrack track) =>
        music.TryGetValue(new MusicTrackKey(clipSet, track.Number), out MusicClipEntry? entry)
            ? entry
            : throw new KeyNotFoundException($"No song for {clipSet} {track}.");

    /// <summary>The sound effect's entry in <paramref name="context"/>'s clip set (null for only the shared sounds).</summary>
    /// <exception cref="InvalidDataException">The effect plays samples the clip set lacks, so it has no clip there.</exception>
    /// <exception cref="KeyNotFoundException">The library has no such effect.</exception>
    public SoundEffectClipEntry GetSoundEffect(MusicDataSet? context, SoundEffectId sound)
    {
        if (soundEffects.TryGetValue(new SoundEffectClipKey(context, sound), out SoundEffectClipEntry? entry))
            return entry;
        if (unrenderable.TryGetValue(new SoundEffectClipKey(context, sound), out UnrenderableSoundEffect? missing))
        {
            throw new InvalidDataException(
                $"{sound} has no clip in {ContextName(context)}: the clip set lacks " +
                $"{string.Join("; ", missing.MissingSources)}.");
        }
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

    /// <summary>Checks the manifest's rate, source identity, clip lengths, loop bounds and tempo timelines.</summary>
    private static void Validate(AudioClipManifest manifest)
    {
        if (manifest.SampleRate != SoundClockTiming.SamplesPerUpdate * UpdatesPerSecond)
            throw new InvalidDataException($"Audio clip manifest has rate {manifest.SampleRate}.");
        if (string.IsNullOrWhiteSpace(manifest.SourceContentIdentity))
            throw new InvalidDataException("Audio clip manifest names no source content identity.");
        foreach (MusicClipEntry entry in manifest.Music)
        {
            ValidateClip(entry.Clip);
            if (entry.LoopStart is long loop && (loop < 0 || loop >= entry.Clip.Frames))
                throw new InvalidDataException($"{entry.AudioSet} track {entry.Track} loops at {loop} of {entry.Clip.Frames} frames.");
            if (entry.TempoSegments.Count == 0 || entry.TempoSegments[0].Frame != 0)
                throw new InvalidDataException($"{entry.AudioSet} track {entry.Track} has no tempo at frame zero.");
        }
        foreach (SoundEffectClipEntry entry in manifest.SoundEffects)
        {
            if (entry.Layers.Count == 0 && entry.ActiveFrames is null)
                throw new InvalidDataException($"Never-ending {entry.Sound} in {ContextName(entry.Context)} has no layers.");
            foreach (SoundEffectClipLayer layer in entry.Layers)
            {
                ValidateClip(layer.Clip);
                if ((layer.LoopStart is null) != (entry.ActiveFrames is not null) ||
                    layer.LoopStart is long loop && (loop < 0 || loop >= layer.Clip.Frames))
                {
                    throw new InvalidDataException(
                        $"{entry.Sound} in {ContextName(entry.Context)} has inconsistent loop bounds.");
                }
            }
        }
    }

    /// <summary>Checks a clip has frames and a content hash.</summary>
    private static void ValidateClip(AudioClipFile clip)
    {
        if (clip.Frames <= 0 || string.IsNullOrWhiteSpace(clip.Sha256))
            throw new InvalidDataException($"Audio clip '{clip.Path}' has {clip.Frames} frames or no hash.");
    }

    /// <summary>A data set's name in diagnostics.</summary>
    private static string ContextName(MusicDataSet? context) => context?.ToString() ?? "the shared sounds";
}
