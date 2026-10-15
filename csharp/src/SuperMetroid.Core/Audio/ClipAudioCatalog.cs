using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Audio;

/// <summary>
/// The installed pre-rendered audio clips (#1305). Loading validates the manifest's format, rate,
/// identities and contained paths and every file's size; <see cref="ReadClip"/> also checks a
/// file's SHA-256 as it reads it, so a host reads only the clips it plays.
/// </summary>
public sealed class ClipAudioCatalog
{
    /// <summary>The clip manifest's file name in the audio root.</summary>
    public const string ManifestFileName = "audio-clips.json";

    /// <summary>Bytes of the minimal RIFF/WAVE header the exporter writes before the samples.</summary>
    internal const int WaveHeaderBytes = 44;

    /// <summary>Bytes per 16-bit stereo frame.</summary>
    internal const int BytesPerFrame = PcmSampleFormat.StereoChannelCount * sizeof(short);

    /// <summary>Serializer settings shared by the exporter and the loader.</summary>
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>The audio root the manifest's paths are relative to.</summary>
    private readonly string root;
    /// <summary>Music clips by (bank, track).</summary>
    private readonly Dictionary<(AudioBank Bank, int Track), MusicClipEntry> music;
    /// <summary>Sound-effect entries by (bank context, library, command).</summary>
    private readonly Dictionary<(AudioBank? Context, SoundEffectLibrary Library, byte Command), SoundEffectClipEntry> soundEffects;
    /// <summary>Effects with no clip in a context, by (bank context, library, command).</summary>
    private readonly Dictionary<(AudioBank? Context, SoundEffectLibrary Library, byte Command), UnrenderableSoundEffect> unrenderable;

    /// <summary>Creates a catalog over a validated manifest.</summary>
    private ClipAudioCatalog(string root, AudioClipManifest manifest, string contentIdentity)
    {
        this.root = root;
        Manifest = manifest;
        ContentIdentity = contentIdentity;
        music = [];
        foreach (MusicClipEntry entry in manifest.Music)
        {
            if (!music.TryAdd((entry.Bank, entry.Track), entry))
                throw new InvalidDataException($"Clip manifest repeats {entry.Bank} track {entry.Track}.");
        }
        soundEffects = [];
        foreach (SoundEffectClipEntry entry in manifest.SoundEffects)
        {
            if (!soundEffects.TryAdd((entry.Context, entry.Library, entry.Command), entry))
                throw new InvalidDataException($"Clip manifest repeats {entry.Library} ${entry.Command:X2} in {ContextName(entry.Context)}.");
        }
        unrenderable = [];
        foreach (UnrenderableSoundEffect entry in manifest.Unrenderable)
        {
            if (soundEffects.ContainsKey((entry.Context, entry.Library, entry.Command)) ||
                !unrenderable.TryAdd((entry.Context, entry.Library, entry.Command), entry))
            {
                throw new InvalidDataException(
                    $"Clip manifest lists {entry.Library} ${entry.Command:X2} in {ContextName(entry.Context)} twice.");
            }
        }
    }

    /// <summary>The validated manifest.</summary>
    public AudioClipManifest Manifest { get; }

    /// <summary>SHA-256 of the manifest file: it names every clip's hash, so any audible change alters it.</summary>
    public string ContentIdentity { get; }

    /// <summary>Loads and validates <see cref="ManifestFileName"/> beneath <paramref name="audioDirectory"/>.</summary>
    /// <exception cref="FileNotFoundException">The manifest or a clip file is missing.</exception>
    /// <exception cref="InvalidDataException">The format, rate, an identity, a path, a duplicate key, loop bounds or a file size is invalid.</exception>
    public static ClipAudioCatalog Load(string audioDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audioDirectory);
        string root = Path.GetFullPath(audioDirectory);
        string manifestPath = Path.Combine(root, ManifestFileName);
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException($"Audio clip manifest was not found at '{manifestPath}'.", manifestPath);
        byte[] manifestBytes = File.ReadAllBytes(manifestPath);
        AudioClipManifest manifest = JsonSerializer.Deserialize<AudioClipManifest>(manifestBytes, JsonOptions)
            ?? throw new InvalidDataException($"Audio clip manifest '{manifestPath}' deserialized to null.");
        if (manifest.FormatVersion != AudioClipManifest.CurrentFormatVersion)
        {
            throw new InvalidDataException(
                $"Audio clip manifest '{manifestPath}' uses format {manifest.FormatVersion}; expected {AudioClipManifest.CurrentFormatVersion}.");
        }
        if (manifest.SampleRate != ManagedSnesDsp.NativeStereoFramesPerVideoFrame * 60)
            throw new InvalidDataException($"Audio clip manifest '{manifestPath}' has rate {manifest.SampleRate}.");
        if (string.IsNullOrWhiteSpace(manifest.SourceContentIdentity))
            throw new InvalidDataException($"Audio clip manifest '{manifestPath}' names no source content identity.");
        foreach (MusicClipEntry entry in manifest.Music)
        {
            ValidateFile(root, entry.Clip);
            if (entry.LoopStart is long loop && (loop < 0 || loop >= entry.Clip.Frames))
                throw new InvalidDataException($"{entry.Bank} track {entry.Track} loops at {loop} of {entry.Clip.Frames} frames.");
            if (entry.TempoSegments.Count == 0 || entry.TempoSegments[0].Frame != 0)
                throw new InvalidDataException($"{entry.Bank} track {entry.Track} has no tempo at frame zero.");
        }
        foreach (SoundEffectClipEntry entry in manifest.SoundEffects)
        {
            if (entry.Layers.Count == 0 && entry.ActiveFrames is null)
                throw new InvalidDataException($"Never-ending {entry.Library} ${entry.Command:X2} in {ContextName(entry.Context)} has no layers.");
            foreach (SoundEffectClipLayer layer in entry.Layers)
            {
                ValidateFile(root, layer.Clip);
                if ((layer.LoopStart is null) != (entry.ActiveFrames is not null) ||
                    layer.LoopStart is long loop && (loop < 0 || loop >= layer.Clip.Frames))
                {
                    throw new InvalidDataException(
                        $"{entry.Library} ${entry.Command:X2} in {ContextName(entry.Context)} has inconsistent loop bounds.");
                }
            }
        }
        return new ClipAudioCatalog(root, manifest, Convert.ToHexString(SHA256.HashData(manifestBytes)));
    }

    /// <summary>Loads the catalog and reads every clip, checking every file's SHA-256: the installer's stock validation.</summary>
    /// <exception cref="InvalidDataException">The manifest or any clip is invalid.</exception>
    public static void ValidateStock(string audioDirectory)
    {
        ClipAudioCatalog catalog = Load(audioDirectory);
        foreach (MusicClipEntry entry in catalog.Manifest.Music)
            _ = catalog.ReadClip(entry.Clip);
        foreach (SoundEffectClipLayer layer in catalog.Manifest.SoundEffects.SelectMany(entry => entry.Layers))
            _ = catalog.ReadClip(layer.Clip);
    }

    /// <summary>The music clip for <paramref name="track"/> in <paramref name="bank"/>.</summary>
    /// <exception cref="KeyNotFoundException">The bank has no such track.</exception>
    public MusicClipEntry GetMusic(AudioBank bank, int track) =>
        music.TryGetValue((bank, track), out MusicClipEntry? entry)
            ? entry
            : throw new KeyNotFoundException($"No music clip for {bank} track {track}.");

    /// <summary>The sound effect's entry in <paramref name="context"/> (null for the common upload alone).</summary>
    /// <exception cref="InvalidDataException">The effect keys sources the context does not upload, so it has no clip there.</exception>
    /// <exception cref="KeyNotFoundException">The library has no such effect.</exception>
    public SoundEffectClipEntry GetSoundEffect(AudioBank? context, SoundEffectLibrary library, byte command)
    {
        if (soundEffects.TryGetValue((context, library, command), out SoundEffectClipEntry? entry))
            return entry;
        if (unrenderable.TryGetValue((context, library, command), out UnrenderableSoundEffect? missing))
        {
            throw new InvalidDataException(
                $"{library} ${command:X2} has no clip in {ContextName(context)}: the upload lacks " +
                $"{string.Join("; ", missing.MissingSources)}.");
        }
        throw new KeyNotFoundException($"No sound effect {library} ${command:X2} in {ContextName(context)}.");
    }

    /// <summary>Reads a clip's interleaved stereo samples, verifying its SHA-256, rate and length.</summary>
    /// <exception cref="InvalidDataException">The file's hash, encoding, rate or length does not match the manifest.</exception>
    public short[] ReadClip(AudioClipFile clip)
    {
        Ensure.NotNull(clip);
        string path = ResolveContainedPath(root, clip.Path);
        byte[] bytes = File.ReadAllBytes(path);
        string hash = Convert.ToHexString(SHA256.HashData(bytes));
        if (!hash.Equals(clip.Sha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Audio clip '{path}' has SHA-256 {hash}, expected {clip.Sha256}.");
        (int rate, short[] samples) = PcmWaveFile.ReadStereoPcm16(bytes, path);
        if (rate != Manifest.SampleRate || samples.Length != clip.Frames * PcmSampleFormat.StereoChannelCount)
            throw new InvalidDataException($"Audio clip '{path}' holds {samples.Length / 2} frames at {rate} Hz, expected {clip.Frames} at {Manifest.SampleRate}.");
        return samples;
    }

    /// <summary>Checks a clip file exists beneath the root with the size its frame count implies.</summary>
    private static void ValidateFile(string root, AudioClipFile clip)
    {
        if (clip.Frames <= 0 || string.IsNullOrWhiteSpace(clip.Sha256))
            throw new InvalidDataException($"Audio clip '{clip.Path}' has {clip.Frames} frames or no hash.");
        string path = ResolveContainedPath(root, clip.Path);
        long expected = WaveHeaderBytes + clip.Frames * BytesPerFrame;
        long actual = new FileInfo(path).Length;
        if (actual != expected)
            throw new InvalidDataException($"Audio clip '{path}' is {actual} bytes, expected {expected}.");
    }

    /// <summary>Resolves a manifest path beneath the root, rejecting escapes and missing files.</summary>
    private static string ResolveContainedPath(string root, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
            throw new InvalidDataException($"Audio clip path '{relativePath}' must be relative.");
        string path = Path.GetFullPath(Path.Combine(root, relativePath));
        string prefix = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Audio clip path '{relativePath}' escapes '{root}'.");
        if (!File.Exists(path))
            throw new FileNotFoundException($"Audio clip '{path}' is missing.", path);
        return path;
    }

    /// <summary>A bank context's name in diagnostics.</summary>
    private static string ContextName(AudioBank? context) => context?.ToString() ?? "the common upload";
}
