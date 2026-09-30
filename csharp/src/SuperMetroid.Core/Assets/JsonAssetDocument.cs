using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>Rejects ambiguous authored properties before deserializing installed presentation data.</summary>
internal static class JsonAssetDocument
{
    /// <summary>Uses the document type as the diagnostic identity for older presentation compilers.</summary>
    internal static T Read<T>(Stream json, JsonSerializerOptions options) =>
        Read<T>(json, options, typeof(T).Name);

    /// <summary>Validates already-buffered presentation bytes with the same property rules as streams.</summary>
    internal static T Read<T>(byte[] json, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(json);
        using var stream = new MemoryStream(json, writable: false);
        return Read<T>(stream, options, typeof(T).Name);
    }

    internal static T Read<T>(Stream json, JsonSerializerOptions options, string description)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            // Match the serializer's name resolution. Historical mixed casing remains
            // readable, but two spellings of the same field must not silently overwrite.
            RejectDuplicates(parsed.RootElement, options.PropertyNameCaseInsensitive
                ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            return parsed.RootElement.Deserialize<T>(options)
                ?? throw new InvalidDataException($"{description} JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid {description} JSON.", error);
        }
    }

    private static void RejectDuplicates(JsonElement element, StringComparer comparer)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(comparer);
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new JsonException($"Duplicate presentation property '{property.Name}'.");
                RejectDuplicates(property.Value, comparer);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
            foreach (JsonElement item in element.EnumerateArray())
                RejectDuplicates(item, comparer);
    }
}
