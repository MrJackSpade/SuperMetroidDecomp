using SuperMetroid.Core;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// The installed pre-rendered audio clips as the import pipeline reads them (#1502, #1616): the songs, sound effects and
/// acceptance policies the game plays, which the game receives alone, beside the validated clip manifest they came from
/// and the identity of the selected audio.
/// </summary>
public sealed partial class InstalledClipAudioLibrary
{
    /// <summary>
    /// Creates a catalog over <paramref name="manifest"/>, whose clips <paramref name="source"/> supplies;
    /// <paramref name="contentIdentity"/> identifies the selected audio.
    /// </summary>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="contentIdentity"/> is blank.</exception>
    /// <exception cref="InvalidDataException">
    /// The rate, the power-on beat, an identity, a clip's length, loop bounds or a tempo timeline is invalid, or the
    /// manifest repeats an identity.
    /// </exception>
    internal InstalledClipAudioLibrary(AudioClipManifest manifest, string contentIdentity, IAudioClipSource source)
    {
        Clips = new ClipAudioLibrary(Validated(manifest).InitialBeat, manifest.Music, [.. manifest.SoundEffects.Select(entry => entry.PlayedEntry())],
            [.. manifest.SoundEffects.SelectMany(entry => entry.PlayedSetEntries())], source);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentIdentity);
        Manifest = manifest;
        ContentIdentity = contentIdentity;
    }

    /// <summary>The songs, sound effects and acceptance policies the game plays, whose samples the installed clips supply.</summary>
    public ClipAudioLibrary Clips { get; }

    /// <summary>The validated clip index.</summary>
    public AudioClipManifest Manifest { get; }

    /// <summary>
    /// The selected audio's identity, supplied by the content reader: any audible change alters it.
    /// </summary>
    public string ContentIdentity { get; }

    /// <summary>Checks the manifest's rate, power-on beat, clip lengths, loop bounds and tempo timelines.</summary>
    /// <returns><paramref name="manifest"/>, once checked.</returns>
    private static AudioClipManifest Validated(AudioClipManifest manifest)
    {
        Ensure.NotNull(manifest);
        if (manifest.SampleRate != AudioSession.SampleRate)
            throw new InvalidDataException($"Audio clip manifest has rate {manifest.SampleRate}.");
        if (manifest.InitialBeat is null)
            throw new InvalidDataException("Audio clip manifest gives no beat length before any song has played.");
        foreach (MusicClipEntry entry in manifest.Music)
        {
            ValidateClip(entry.Clip);
            if (entry.LoopStart is long loop && (loop < 0 || loop >= entry.Clip.Frames))
                throw new InvalidDataException($"{entry.AudioSet} track {entry.Track} loops at {loop} of {entry.Clip.Frames} frames.");
            if (entry.TempoSegments.Count == 0 || entry.TempoSegments[0].Frame != 0)
                throw new InvalidDataException($"{entry.AudioSet} track {entry.Track} has no tempo at frame zero.");
        }
        foreach (InstalledSoundEffectClip entry in manifest.SoundEffects)
        {
            ValidateShape(entry.Sound, entry.Shape);
            foreach (InstalledSoundEffectSetShape set in entry.ByDataSet)
                ValidateShape(entry.Sound, set.Shape);
        }
        return manifest;
    }

    /// <summary>Checks one shape of <paramref name="sound"/>: its clips have frames and its loops lie within them.</summary>
    private static void ValidateShape(SoundEffectId sound, InstalledSoundEffectShape shape)
    {
        {
            switch (shape)
            {
                case InstalledOneShotSoundEffect { Clip: { } clip }:
                    ValidateClip(clip);
                    break;
                case InstalledOneShotSoundEffect:
                    break;
                case InstalledLoopingSoundEffect { Layers.Count: 0 }:
                    throw new InvalidDataException($"Never-ending {sound} has no layers.");
                case InstalledLoopingSoundEffect looping:
                    foreach (InstalledLoopingSoundEffectLayer layer in looping.Layers)
                    {
                        ValidateClip(layer.Clip);
                        if (layer.LoopStart < 0 || layer.LoopStart >= layer.Clip.Frames)
                            throw new InvalidDataException($"{sound} has inconsistent loop bounds.");
                    }
                    break;
                default:
                    throw new InvalidDataException($"{sound} has no shape.");
            }
        }
    }

    /// <summary>Checks a clip has frames.</summary>
    private static void ValidateClip(AudioClipFile clip)
    {
        if (clip.Frames <= 0)
            throw new InvalidDataException($"Audio clip '{clip.Path}' has {clip.Frames} frames.");
    }
}
