using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Saves;

/// <summary>
/// Writes an identity into a save as its installed key in the save's camel case, and reads it back from one ignoring case
/// as saves have always been read (#2520): the save's form of the identity's <see cref="InstalledKeys{TIdentity}"/>, so
/// renaming a member never changes a save or stops one loading.
/// </summary>
/// <typeparam name="TIdentity">The identity the converter reads and writes.</typeparam>
internal partial class SaveKeyJsonConverter<TIdentity> : JsonConverter<TIdentity>
    where TIdentity : struct, Enum
{
    /// <summary>Each member's name in a save.</summary>
    private readonly FrozenDictionary<TIdentity, string> names;

    /// <summary>Each name's member, its case ignored.</summary>
    private readonly FrozenDictionary<string, TIdentity> identities;

    /// <summary>Spells each member of <typeparamref name="TIdentity"/> in a save by its key in <paramref name="keys"/>.</summary>
    /// <param name="keys">The identity's written-out keys.</param>
    /// <exception cref="ArgumentException">Two members' keys differ only in case.</exception>
    internal SaveKeyJsonConverter(InstalledKeys<TIdentity> keys)
    {
        Ensure.NotNull(keys);
        names = Enum.GetValues<TIdentity>().ToFrozenDictionary(identity => identity,
            identity => JsonNamingPolicy.CamelCase.ConvertName(keys.Of(identity)));
        identities = names.ToFrozenDictionary(entry => entry.Value, entry => entry.Key, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    /// <exception cref="JsonException">The value is not a string naming the identity.</exception>
    [SuppressMessage("Design", "SME6426", Justification = "System.Text.Json's JsonConverter contract fixes the signature; a converter of one identity needs neither the type it converts nor the options.")]
    public override TIdentity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.String
            ? Named(reader.GetString() ?? throw new JsonException($"A save's {typeof(TIdentity).Name} has no name."))
            : throw new JsonException($"A save's {typeof(TIdentity).Name} is a {reader.TokenType}, not a string naming one.");

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TIdentity value, JsonSerializerOptions options) =>
        Ensure.NotNull(writer).WriteStringValue(NameOf(value));

    /// <summary>The save's name of <paramref name="value"/>.</summary>
    /// <exception cref="JsonException">The value is no member of the identity.</exception>
    private protected virtual string NameOf(TIdentity value) => names.TryGetValue(value, out string? name) ? name
        : throw new JsonException($"{value} is no {typeof(TIdentity).Name} a save can name.");

    /// <summary>The value the save's <paramref name="text"/> names.</summary>
    /// <exception cref="JsonException">The text names no member.</exception>
    private protected virtual TIdentity Named(string text) => TryMember(text, out TIdentity identity) ? identity
        : throw new JsonException($"'{text}' names no {typeof(TIdentity).Name}.");

    /// <summary>The save's name of a member, or null for a value that is no member.</summary>
    private protected string? MemberName(TIdentity value) => names.TryGetValue(value, out string? name) ? name : null;

    /// <summary>Whether a save's name names a member, its case ignored, and the <paramref name="identity"/> it names.</summary>
    private protected bool TryMember(string text, out TIdentity identity) => identities.TryGetValue(text, out identity);
}
