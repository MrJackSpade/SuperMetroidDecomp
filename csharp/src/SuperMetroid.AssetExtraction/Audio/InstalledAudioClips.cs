using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Reads the installed pre-rendered audio clips (#1305, #1410) into the game's <see cref="ClipAudioLibrary"/>. The clip
/// manifest, <c>audio-clips.json</c>, names each sound by its library and request number, the cartridge's numbering;
/// this reader decodes them into the game's sounds, and the exporter encodes them back, so the file keeps its format.
/// Loading checks the manifest's format, contained paths and every file's size; each clip's SHA-256 is checked as
/// <see cref="InstalledClipFiles"/> reads it.
/// </summary>
public static class InstalledAudioClips
{
    /// <summary>The clip manifest's file name in the audio root.</summary>
    public const string ManifestFileName = "audio-clips.json";

    /// <summary>
    /// Serializer settings of the installed audio files: names in camel case and enums by name. The clip manifest's
    /// reader adds the decoding of its sound identities to these.
    /// </summary>
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>
    /// The clip manifest's serializer settings: the installed audio files' settings plus the decoding of each sound
    /// entry's library and request number.
    /// </summary>
    public static JsonSerializerOptions ManifestOptions { get; } = new(JsonOptions)
    {
        Converters = { new SoundEffectClipEntryJsonConverter(), new UnrenderableSoundEffectJsonConverter() },
    };

    /// <summary>The validated stock clips: their audio root, their manifest and the manifest file's identity.</summary>
    /// <param name="Root">The audio root the manifest's paths are relative to.</param>
    /// <param name="Manifest">The stock manifest, its files checked.</param>
    /// <param name="Identity">SHA-256 of the stock manifest file, which names every clip's hash.</param>
    internal sealed record StockClips(string Root, AudioClipManifest Manifest, string Identity);

    /// <summary>Loads and validates the stock clips beneath <paramref name="audioDirectory"/>.</summary>
    /// <exception cref="FileNotFoundException">The manifest or a clip file is missing.</exception>
    /// <exception cref="InvalidDataException">The format, rate, an identity, a sound, a path, a duplicate key, loop bounds or a file size is invalid.</exception>
    public static InstalledClipAudioLibrary Load(string audioDirectory) => ClipAudioOverrides.Load(audioDirectory, overrideDirectory: null);

    /// <summary>Loads the catalog and reads every clip, checking every file's SHA-256: the installer's stock validation.</summary>
    /// <exception cref="InvalidDataException">The manifest or any clip is invalid.</exception>
    public static void ValidateStock(string audioDirectory)
    {
        InstalledClipAudioLibrary library = Load(audioDirectory);
        foreach (MusicClipEntry entry in library.Manifest.Music)
            _ = library.ReadClip(entry.Clip);
        foreach (SoundEffectClipLayer layer in library.Manifest.SoundEffects.SelectMany(entry => entry.Layers))
            _ = library.ReadClip(layer.Clip);
    }

    /// <summary>
    /// Reads the stock clip manifest beneath <paramref name="audioDirectory"/> and checks its format and every clip
    /// file's containment and size. The manifest names each sound by its library and request number;
    /// <see cref="ManifestOptions"/> decode them into sounds. The catalog checks the manifest's meaning.
    /// </summary>
    /// <exception cref="FileNotFoundException">The manifest or a clip file is missing.</exception>
    /// <exception cref="InvalidDataException">The format, a path or a file size is invalid.</exception>
    internal static StockClips ReadStock(string audioDirectory)
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
        foreach (SoundEffectClipLayer layer in manifest.SoundEffects.SelectMany(entry => entry.Layers))
            InstalledClipFiles.ValidateFileSize(root, layer.Clip);
        return new StockClips(root, manifest, Convert.ToHexString(SHA256.HashData(manifestBytes)));
    }
}
