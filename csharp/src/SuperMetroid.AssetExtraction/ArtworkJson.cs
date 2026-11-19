using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction;

/// <summary>The serializer settings shared by installed artwork resources of every domain.</summary>
public static class ArtworkJson
{
    /// <summary>
    /// Emits stable camel-case artwork resources, rejects unknown authoring fields, and rejects a null where a document
    /// declares its member non-nullable, so a required member is never read back absent (#2554).
    /// </summary>
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectNullableAnnotations = true,
    };
}
