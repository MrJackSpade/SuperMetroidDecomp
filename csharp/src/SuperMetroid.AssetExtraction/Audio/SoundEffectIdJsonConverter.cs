using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Reads and writes a sound in the clip index by its name, its library's sound type and member as
/// <see cref="SoundEffectId.ToString"/> spells them (for example <c>SamusSound.UnchargedPowerBeam</c>), so the index keys
/// each sound by the game's identity rather than the library's request number (#1693).
/// </summary>
internal sealed class SoundEffectIdJsonConverter : JsonConverter<SoundEffectId>
{
    /// <summary>Every sound of every library by its name.</summary>
    private static readonly FrozenDictionary<string, SoundEffectId> SoundsByName =
        Enum.GetValues<SamusSound>().Select(sound => (SoundEffectId)sound)
            .Concat(Enum.GetValues<EnemySound>().Select(sound => (SoundEffectId)sound))
            .Concat(Enum.GetValues<RoomSound>().Select(sound => (SoundEffectId)sound))
            .ToFrozenDictionary(sound => sound.ToString(), StringComparer.Ordinal);

    /// <inheritdoc />
    /// <exception cref="InvalidDataException">The value is not a string naming a sound.</exception>
    public override SoundEffectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new InvalidDataException($"A clip index sound is a {reader.TokenType}, not a sound's name.");
        string name = reader.GetString() ?? throw new InvalidDataException("A clip index sound has no name.");
        return SoundsByName.TryGetValue(name, out SoundEffectId sound)
            ? sound
            : throw new InvalidDataException($"The clip index names '{name}', which is no sound.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SoundEffectId value, JsonSerializerOptions options)
    {
        Ensure.NotNull(writer);
        writer.WriteStringValue(value.ToString());
    }
}
