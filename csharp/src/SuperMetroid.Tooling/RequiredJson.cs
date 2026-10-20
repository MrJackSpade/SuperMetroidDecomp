using System.Text.Json;
using System.Text.Json.Nodes;

namespace SuperMetroid.Tooling;

/// <summary>
/// JSON reads whose document must be present (#623). System.Text.Json returns null for the literal <c>null</c>
/// document; a fixture or asset that must hold a value fails here with a <see cref="JsonException"/> naming the
/// expected type instead of carrying a null onward.
/// </summary>
public static class RequiredJson
{
    /// <summary>Deserializes a required <typeparamref name="T"/> from UTF-8 JSON.</summary>
    public static T Deserialize<T>(ReadOnlySpan<byte> utf8Json, JsonSerializerOptions? options = null) =>
        JsonSerializer.Deserialize<T>(utf8Json, options) ?? throw Missing<T>();

    /// <summary>Parses UTF-8 JSON whose root must not be the <c>null</c> literal.</summary>
    public static JsonNode Parse(ReadOnlySpan<byte> utf8Json) => JsonNode.Parse(utf8Json) ?? throw MissingNode();

    /// <summary>The failure for a null document where a value was required.</summary>
    internal static JsonException Missing<T>() => new($"The JSON document is null; a {typeof(T).Name} is required.");

    /// <summary>The failure for a null root node.</summary>
    internal static JsonException MissingNode() => new("The JSON document's root is null; a value is required.");
}
