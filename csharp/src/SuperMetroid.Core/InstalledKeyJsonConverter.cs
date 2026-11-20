using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core;

/// <summary>
/// Writes an identity into a JSON document as its installed key and reads it back from one (#2520), as a value or as a
/// dictionary's property name: the JSON form of the identity's <see cref="InstalledKeys{TIdentity}"/>, in place of a
/// converter that spells each member by its source name. Each identity a document holds names its converter, a sealed
/// subclass handing this one its keys.
/// </summary>
/// <typeparam name="TIdentity">The identity the converter reads and writes.</typeparam>
/// <param name="keys">The identity's written-out keys.</param>
public abstract class InstalledKeyJsonConverter<TIdentity>(InstalledKeys<TIdentity> keys) : JsonConverter<TIdentity>
    where TIdentity : struct, Enum
{
    /// <summary>The identity's written-out keys.</summary>
    private readonly InstalledKeys<TIdentity> keys = Ensure.NotNull(keys);

    /// <inheritdoc />
    /// <exception cref="JsonException">The value is not a string naming a member.</exception>
    [SuppressMessage("Design", "SME6426", Justification = "System.Text.Json's JsonConverter contract fixes the signature; a converter of one identity needs neither the type it converts nor the options.")]
    public override TIdentity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.String
            ? Named(reader.GetString())
            : throw new JsonException($"A {typeof(TIdentity).Name} is a {reader.TokenType}, not a string naming one.");

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TIdentity value, JsonSerializerOptions options) =>
        Ensure.NotNull(writer).WriteStringValue(keys.Of(value));

    /// <inheritdoc />
    /// <exception cref="JsonException">The property name names no member.</exception>
    [SuppressMessage("Design", "SME6426", Justification = "System.Text.Json's JsonConverter contract fixes the signature; a converter of one identity needs neither the type it converts nor the options.")]
    public override TIdentity ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Named(reader.GetString());

    /// <inheritdoc />
    public override void WriteAsPropertyName(Utf8JsonWriter writer, TIdentity value, JsonSerializerOptions options) =>
        Ensure.NotNull(writer).WritePropertyName(keys.Of(value));

    /// <summary>The member <paramref name="key"/> names.</summary>
    /// <exception cref="JsonException">The key names no member.</exception>
    private TIdentity Named(string? key) =>
        keys.TryParse(key, out TIdentity identity)
            ? identity
            : throw new JsonException($"'{key}' names no {typeof(TIdentity).Name}.");
}
