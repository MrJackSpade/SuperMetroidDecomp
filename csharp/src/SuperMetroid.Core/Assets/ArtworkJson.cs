using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>The serializer settings shared by installed artwork resources of every domain.</summary>
public static class ArtworkJson
{
    /// <summary>Emits stable camel-case artwork resources and rejects unknown authoring fields.</summary>
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };
}
