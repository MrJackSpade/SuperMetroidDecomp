using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Renames the frames an installed JSON document lists under one property, so a document or player override written
/// before its frames were renamed (#1465) reaches its loader under the current names; every other part of the document
/// is kept as written, for that loader to judge.
/// </summary>
internal static class JsonFrameNames
{
    /// <summary>Reads <paramref name="json"/> as a JSON object.</summary>
    /// <exception cref="InvalidDataException">The document is not valid JSON or not an object.</exception>
    internal static JsonObject Parse(byte[] json, string document)
    {
        Ensure.NotNull(json);
        try
        {
            return JsonNode.Parse(json) as JsonObject ?? throw new InvalidDataException($"The {document} is not a JSON object.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"The {document} is not valid JSON: {error.Message}", error);
        }
    }

    /// <summary>Renames each frame <paramref name="root"/> lists under <paramref name="property"/> by <paramref name="rename"/>.</summary>
    /// <exception cref="InvalidDataException">The document has no frame object under <paramref name="property"/>, repeats a frame, or two frames take one name.</exception>
    internal static void Rename(JsonObject root, string property, Func<string, string> rename, string document)
    {
        Ensure.NotNull(root);
        Ensure.NotNull(rename);
        try
        {
            if (root[property] is not JsonObject frames)
                throw new InvalidDataException($"The {document} has no {property} object.");
            var renamed = new JsonObject();
            foreach ((string name, JsonNode? value) in frames.ToList())
            {
                frames.Remove(name);
                string current = rename(name);
                if (renamed.ContainsKey(current))
                    throw new InvalidDataException($"The {document} names frame {current} twice.");
                renamed.Add(current, value);
            }
            root[property] = renamed;
        }
        catch (ArgumentException error)
        {
            // A JSON object repeating a property name fails as it is first read.
            throw new InvalidDataException($"The {document} repeats a property: {error.Message}", error);
        }
    }

    /// <summary>The document's UTF-8 JSON.</summary>
    internal static byte[] Write(JsonObject root) => Encoding.UTF8.GetBytes(Ensure.NotNull(root).ToJsonString());
}
