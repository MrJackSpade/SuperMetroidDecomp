using SuperMetroid.Core;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Compatibility;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// The installed pre-rendered audio clips as the import pipeline reads them (#1502, #1616): the songs, sound effects and
/// acceptance policies the game plays, which the game receives alone, beside the validated clip manifest they came from
/// and the identity of the selected audio.
/// </summary>
public sealed class InstalledClipAudioLibrary
{
    /// <summary>Game updates per second; the clips' rate is one update's samples this many times.</summary>
    private const int UpdatesPerSecond = 60;

    /// <summary>
    /// Creates a catalog over <paramref name="manifest"/>, whose clips <paramref name="source"/> supplies;
    /// <paramref name="contentIdentity"/> identifies the selected audio.
    /// </summary>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="contentIdentity"/> is blank.</exception>
    /// <exception cref="InvalidDataException">
    /// The rate, an identity, a clip's length, loop bounds or a tempo timeline is invalid, or the manifest repeats an identity.
    /// </exception>
    internal InstalledClipAudioLibrary(AudioClipManifest manifest, string contentIdentity, IAudioClipSource source)
    {
        Clips = new ClipAudioLibrary(Validated(manifest).Music, [.. manifest.SoundEffects.Select(entry => entry.PlayedEntry())],
            manifest.Unrenderable, source);
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

    /// <summary>Checks the manifest's rate, clip lengths, loop bounds and tempo timelines.</summary>
    /// <returns><paramref name="manifest"/>, once checked.</returns>
    private static AudioClipManifest Validated(AudioClipManifest manifest)
    {
        Ensure.NotNull(manifest);
        if (manifest.SampleRate != SoundClockTiming.SamplesPerUpdate * UpdatesPerSecond)
            throw new InvalidDataException($"Audio clip manifest has rate {manifest.SampleRate}.");
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
            if (entry.Layers.Count == 0 && entry.ActiveFrames is null)
                throw new InvalidDataException($"Never-ending {entry.Sound} in {ContextName(entry.Context)} has no layers.");
            foreach (InstalledSoundEffectLayer layer in entry.Layers)
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
        return manifest;
    }

    /// <summary>Checks a clip has frames.</summary>
    private static void ValidateClip(AudioClipFile clip)
    {
        if (clip.Frames <= 0)
            throw new InvalidDataException($"Audio clip '{clip.Path}' has {clip.Frames} frames.");
    }

    /// <summary>A data set's name in diagnostics.</summary>
    private static string ContextName(MusicDataSet? context) => context?.ToString() ?? "the shared sounds";
}
