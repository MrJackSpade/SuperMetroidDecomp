using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Reads the installed pre-rendered audio clips (#1305, #1410) into the game's <see cref="ClipAudioLibrary"/>. The clip
/// index, <c>audio-clips.json</c>, names each song by its data set and the song it is (#1748), and each sound by its name
/// (#1693). The audio folder holds this index and the clips it lists, and nothing else.
/// Loading checks the index's format, contained paths and every file's size; <see cref="InstalledClipFiles"/> checks
/// each clip's encoding and rate as it reads it.
/// </summary>
public static class InstalledAudioClips
{
    /// <summary>The clip index's file name in the audio root.</summary>
    public const string ManifestFileName = "audio-clips.json";

    /// <summary>
    /// Serializer settings of the installed audio files: names in camel case and enums by name. The clip index's
    /// reader adds the spelling of its sound identities to these.
    /// </summary>
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>
    /// The clip index's serializer settings: the installed audio files' settings plus each sound's name.
    /// </summary>
    public static JsonSerializerOptions ManifestOptions { get; } = new(JsonOptions)
    {
        Converters = { new SoundEffectIdJsonConverter() },
    };

    /// <summary>
    /// Loads the installed clips beneath <paramref name="audioDirectory"/>, as the player may have edited them in place:
    /// reads the clip index and checks its format and every clip file's containment and size. The catalog checks the
    /// index's meaning.
    /// </summary>
    /// <exception cref="FileNotFoundException">The manifest or a clip file is missing.</exception>
    /// <exception cref="InvalidDataException">The format, rate, an identity, a sound, a path, a duplicate key, loop bounds or a file size is invalid.</exception>
    public static InstalledClipAudioLibrary Load(string audioDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audioDirectory);
        string root = Path.GetFullPath(audioDirectory);
        string manifestPath = Path.Combine(root, ManifestFileName);
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException($"Audio clip manifest was not found at '{manifestPath}'.", manifestPath);
        byte[] manifestBytes = File.ReadAllBytes(manifestPath);
        AudioClipManifest manifest = JsonSerializer.Deserialize<AudioClipManifest>(manifestBytes, ManifestOptions)
            ?? throw new InvalidDataException($"Audio clip manifest '{manifestPath}' deserialized to null.");
        if (manifest.FormatVersion != AudioClipManifest.CurrentFormatVersion)
        {
            throw new InvalidDataException(
                $"Audio clip manifest '{manifestPath}' uses format {manifest.FormatVersion}; expected {AudioClipManifest.CurrentFormatVersion}.");
        }
        foreach (MusicClipEntry entry in manifest.Music)
            InstalledClipFiles.ValidateFileSize(root, entry.Clip);
        foreach (InstalledSoundEffectLayer layer in manifest.SoundEffects.SelectMany(entry => entry.Layers))
            InstalledClipFiles.ValidateFileSize(root, layer.Clip);
        return new InstalledClipAudioLibrary(
            manifest,
            Convert.ToHexString(SHA256.HashData(manifestBytes)),
            new InstalledClipFiles(root, manifest.SampleRate));
    }
}
