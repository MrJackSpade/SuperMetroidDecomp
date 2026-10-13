using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace SuperMetroid.Tooling;

/// <summary>
/// JSON reads whose document must be present (#623). System.Text.Json returns null for the literal <c>null</c>
/// document; a fixture or asset that must hold a value fails here with a <see cref="JsonException"/> naming the
/// expected type instead of carrying a null onward.
/// </summary>
public static class RequiredJson
{
    /// <summary>Deserializes a required <typeparamref name="T"/> from JSON text.</summary>
    public static T Deserialize<T>(string json, JsonSerializerOptions? options = null) =>
        JsonSerializer.Deserialize<T>(json, options) ?? throw Missing<T>();

    /// <summary>Deserializes a required <typeparamref name="T"/> from JSON text with source-generated metadata.</summary>
    public static T Deserialize<T>(string json, JsonTypeInfo<T> typeInfo) =>
        JsonSerializer.Deserialize(json, typeInfo) ?? throw Missing<T>();

    /// <summary>Deserializes a required <typeparamref name="T"/> from UTF-8 JSON.</summary>
    public static T Deserialize<T>(ReadOnlySpan<byte> utf8Json, JsonSerializerOptions? options = null) =>
        JsonSerializer.Deserialize<T>(utf8Json, options) ?? throw Missing<T>();

    /// <summary>Deserializes a required <typeparamref name="T"/> from a UTF-8 JSON stream.</summary>
    public static T Deserialize<T>(Stream utf8Json, JsonSerializerOptions? options = null) =>
        JsonSerializer.Deserialize<T>(utf8Json, options) ?? throw Missing<T>();

    /// <summary>Deserializes a required <typeparamref name="T"/> from a parsed node.</summary>
    public static T Deserialize<T>(JsonNode? node, JsonSerializerOptions? options = null) =>
        node.Deserialize<T>(options) ?? throw Missing<T>();

    /// <summary>Deserializes a required <typeparamref name="T"/> from a parsed document.</summary>
    public static T Deserialize<T>(JsonDocument document, JsonSerializerOptions? options = null) =>
        document.Deserialize<T>(options) ?? throw Missing<T>();

    /// <summary>Deserializes a required <typeparamref name="T"/> from a JSON element.</summary>
    public static T Deserialize<T>(JsonElement element, JsonSerializerOptions? options = null) =>
        element.Deserialize<T>(options) ?? throw Missing<T>();

    /// <summary>Parses JSON text whose root must not be the <c>null</c> literal.</summary>
    public static JsonNode Parse(string json) => JsonNode.Parse(json) ?? throw MissingNode();

    /// <summary>Parses UTF-8 JSON whose root must not be the <c>null</c> literal.</summary>
    public static JsonNode Parse(ReadOnlySpan<byte> utf8Json) => JsonNode.Parse(utf8Json) ?? throw MissingNode();

    /// <summary>Parses a UTF-8 JSON stream whose root must not be the <c>null</c> literal.</summary>
    public static JsonNode Parse(Stream utf8Json) => JsonNode.Parse(utf8Json) ?? throw MissingNode();

    /// <summary>The failure for a null document where a value was required.</summary>
    private static JsonException Missing<T>() => new($"The JSON document is null; a {typeof(T).Name} is required.");

    /// <summary>The failure for a null root node.</summary>
    private static JsonException MissingNode() => new("The JSON document's root is null; a value is required.");
}
