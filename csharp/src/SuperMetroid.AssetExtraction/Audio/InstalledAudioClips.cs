using System.Text.Json;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Reads the installed pre-rendered audio clips (#1305, #1410) into the game's <see cref="ClipAudioCatalog"/>. The clip
/// manifest, <c>audio-clips.json</c>, names each sound by its library and request number, the cartridge's numbering;
/// this reader decodes them into the game's sounds, and the exporter encodes them back, so the file keeps its format.
/// </summary>
public static class InstalledAudioClips
{
    /// <summary>
    /// The clip manifest's serializer settings: the installed audio files' settings plus the decoding of each sound
    /// entry's library and request number.
    /// </summary>
    public static JsonSerializerOptions ManifestOptions { get; } = new(ClipAudioCatalog.JsonOptions)
    {
        Converters = { new SoundEffectClipEntryJsonConverter(), new UnrenderableSoundEffectJsonConverter() },
    };

    /// <summary>Loads and validates the stock clips beneath <paramref name="audioDirectory"/>.</summary>
    /// <exception cref="FileNotFoundException">The manifest or a clip file is missing.</exception>
    /// <exception cref="InvalidDataException">The format, rate, an identity, a sound, a path, a duplicate key, loop bounds or a file size is invalid.</exception>
    public static ClipAudioCatalog Load(string audioDirectory) => ClipAudioOverrides.Load(audioDirectory, overrideDirectory: null);

    /// <summary>Loads the catalog and reads every clip, checking every file's SHA-256: the installer's stock validation.</summary>
    /// <exception cref="InvalidDataException">The manifest or any clip is invalid.</exception>
    public static void ValidateStock(string audioDirectory) => Load(audioDirectory).ReadEveryClip();
}
