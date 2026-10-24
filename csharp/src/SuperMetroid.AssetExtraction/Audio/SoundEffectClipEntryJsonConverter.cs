using System.Text.Json;
using SuperMetroid.Core;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Reads and writes a clip manifest's sound entry in its installed form, <c>{context, library, command, policy,
/// activeFrames, layers}</c>, decoding the library and request number into the sound (#1410).
/// </summary>
internal sealed class SoundEffectClipEntryJsonConverter : JsonConverter<SoundEffectClipEntry>
{
    /// <inheritdoc />
    /// <exception cref="InvalidDataException">The entry names no sound of its library.</exception>
    public override SoundEffectClipEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        InstalledEntry entry = JsonSerializer.Deserialize<InstalledEntry>(ref reader, options)
            ?? throw new InvalidDataException("A clip manifest sound entry is null.");
        return new SoundEffectClipEntry(
            entry.Context,
            SoundEffectCommands.Decode(entry.Library, entry.Command),
            entry.Policy,
            entry.ActiveFrames,
            entry.Layers);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SoundEffectClipEntry value, JsonSerializerOptions options)
    {
        Ensure.NotNull(value);
        JsonSerializer.Serialize(writer, new InstalledEntry(
            value.Context,
            value.Sound.Library,
            SoundEffectCommands.Command(value.Sound),
            value.Policy,
            value.ActiveFrames,
            value.Layers), options);
    }

    /// <summary>The entry as the manifest stores it.</summary>
    /// <param name="Context">The music data set loaded, or null for only the shared sounds.</param>
    /// <param name="Library">The sound's library.</param>
    /// <param name="Command">The sound's request number in its library.</param>
    /// <param name="Policy">How the driver accepts and starts the request.</param>
    /// <param name="ActiveFrames">Frames until the library's logical end; null for a never-ending effect.</param>
    /// <param name="Layers">The entry's clip layers.</param>
    private sealed record InstalledEntry(
        MusicDataSet? Context,
        SoundEffectLibrary Library,
        int Command,
        SoundEffectClipPolicy Policy,
        long? ActiveFrames,
        IReadOnlyList<SoundEffectClipLayer> Layers);
}
