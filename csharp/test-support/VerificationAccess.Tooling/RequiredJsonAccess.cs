using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace SuperMetroid.Tooling;

/// <summary>Verification access to <see cref="RequiredJson"/> reads development tools do not use.</summary>
internal static class RequiredJsonAccess
{
    /// <summary>Required-value JSON reads only verification performs.</summary>
    extension(RequiredJson)
    {
        /// <summary>Deserializes a required <typeparamref name="T"/> from JSON text.</summary>
        public static T Deserialize<T>(string json, JsonSerializerOptions? options = null) =>
            JsonSerializer.Deserialize<T>(json, options) ?? throw RequiredJson.Missing<T>();
        /// <summary>Deserializes a required <typeparamref name="T"/> from JSON text with source-generated metadata.</summary>
        public static T Deserialize<T>(string json, JsonTypeInfo<T> typeInfo) =>
            JsonSerializer.Deserialize(json, typeInfo) ?? throw RequiredJson.Missing<T>();
        /// <summary>Deserializes a required <typeparamref name="T"/> from a UTF-8 JSON stream.</summary>
        public static T Deserialize<T>(Stream utf8Json, JsonSerializerOptions? options = null) =>
            JsonSerializer.Deserialize<T>(utf8Json, options) ?? throw RequiredJson.Missing<T>();
        /// <summary>Deserializes a required <typeparamref name="T"/> from a parsed node.</summary>
        public static T Deserialize<T>(JsonNode? node, JsonSerializerOptions? options = null) =>
            node.Deserialize<T>(options) ?? throw RequiredJson.Missing<T>();
        /// <summary>Deserializes a required <typeparamref name="T"/> from a parsed document.</summary>
        public static T Deserialize<T>(JsonDocument document, JsonSerializerOptions? options = null) =>
            document.Deserialize<T>(options) ?? throw RequiredJson.Missing<T>();
        /// <summary>Deserializes a required <typeparamref name="T"/> from a JSON element.</summary>
        public static T Deserialize<T>(JsonElement element, JsonSerializerOptions? options = null) =>
            element.Deserialize<T>(options) ?? throw RequiredJson.Missing<T>();
        /// <summary>Parses JSON text whose root must not be the <c>null</c> literal.</summary>
        public static JsonNode Parse(string json) => JsonNode.Parse(json) ?? throw RequiredJson.MissingNode();
        /// <summary>Parses a UTF-8 JSON stream whose root must not be the <c>null</c> literal.</summary>
        public static JsonNode Parse(Stream utf8Json) => JsonNode.Parse(utf8Json) ?? throw RequiredJson.MissingNode();
    }
}
