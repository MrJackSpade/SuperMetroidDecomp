using System.Text.Json;
using SuperMetroid.Core;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Reads and writes a clip manifest's unrenderable sound in its installed form, <c>{context, library, command,
/// missingSources}</c>, decoding the library and request number into the sound (#1410).
/// </summary>
internal sealed class UnrenderableSoundEffectJsonConverter : JsonConverter<UnrenderableSoundEffect>
{
    /// <inheritdoc />
    /// <exception cref="InvalidDataException">The entry names no sound of its library.</exception>
    public override UnrenderableSoundEffect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        InstalledEntry entry = JsonSerializer.Deserialize<InstalledEntry>(ref reader, options)
            ?? throw new InvalidDataException("A clip manifest unrenderable entry is null.");
        return new UnrenderableSoundEffect(entry.Context, SoundEffectCommands.Decode(entry.Library, entry.Command), entry.MissingSources);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, UnrenderableSoundEffect value, JsonSerializerOptions options)
    {
        Ensure.NotNull(value);
        JsonSerializer.Serialize(writer, new InstalledEntry(
            value.Context, value.Sound.Library, SoundEffectCommands.Command(value.Sound), value.MissingSources), options);
    }

    /// <summary>The entry as the manifest stores it.</summary>
    /// <param name="Context">The music data set loaded, or null for only the shared sounds.</param>
    /// <param name="Library">The sound's library.</param>
    /// <param name="Command">The sound's request number in its library.</param>
    /// <param name="MissingSources">Each instrument whose source the context lacks.</param>
    private sealed record InstalledEntry(
        MusicDataSet? Context,
        SoundEffectLibrary Library,
        int Command,
        IReadOnlyList<string> MissingSources);
}
