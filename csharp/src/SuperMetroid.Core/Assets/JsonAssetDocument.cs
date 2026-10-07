using System.Text;
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
        return Read<T>(json, options, typeof(T).Name);
    }

    internal static T Read<T>(Stream json, JsonSerializerOptions options, string description)
    {
        ArgumentNullException.ThrowIfNull(json);
        return Read<T>(Buffer(json), options, description);
    }

    private static T Read<T>(ReadOnlySpan<byte> json, JsonSerializerOptions options, string description)
    {
        try
        {
            // Match the serializer's name resolution. Historical mixed casing remains
            // readable, but two spellings of the same field must not silently overwrite.
            RejectDuplicates(json, options.PropertyNameCaseInsensitive
                ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            return JsonSerializer.Deserialize<T>(json, options)
                ?? throw new InvalidDataException($"{description} JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid {description} JSON.", error);
        }
    }

    private static ArraySegment<byte> Buffer(Stream json)
    {
        if (json is MemoryStream memory && memory.TryGetBuffer(out ArraySegment<byte> segment))
        {
            int position = checked((int)memory.Position);
            int length = checked((int)(memory.Length - memory.Position));
            memory.Position = memory.Length;
            return segment.Slice(position, length);
        }
        if (json.CanSeek)
        {
            // Files know their length: one exact read rather than a doubling copy.
            var exact = new byte[checked((int)(json.Length - json.Position))];
            json.ReadExactly(exact);
            return exact;
        }
        var copy = new MemoryStream();
        json.CopyTo(copy);
        return new ArraySegment<byte>(copy.GetBuffer(), 0, checked((int)copy.Length));
    }

    /// <summary>One property name: a raw UTF-8 range of the document, or its decoded text when escaped.</summary>
    private readonly record struct PropertyName(int Start, int Length, string? Decoded);

    /// <summary>
    /// One forward pass over the document with the same strictness as <see cref="JsonDocument.Parse(Stream, JsonDocumentOptions)"/>
    /// defaults. Names are compared as raw UTF-8 and decoded only when escaped or non-ASCII,
    /// so validating thousands of small cell objects allocates no per-object sets or strings.
    /// </summary>
    private static void RejectDuplicates(ReadOnlySpan<byte> json, StringComparer comparer)
    {
        bool ignoreCase = ReferenceEquals(comparer, StringComparer.OrdinalIgnoreCase);
        var reader = new Utf8JsonReader(json, new JsonReaderOptions());
        var objects = new List<List<PropertyName>>();
        int depth = -1;
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    depth++;
                    if (depth == objects.Count) objects.Add([]);
                    else objects[depth].Clear();
                    break;
                case JsonTokenType.EndObject:
                    depth--;
                    break;
                case JsonTokenType.PropertyName:
                    var name = reader.ValueIsEscaped
                        ? new PropertyName(0, 0, reader.GetString())
                        : new PropertyName(checked((int)reader.TokenStartIndex) + 1, reader.ValueSpan.Length, null);
                    List<PropertyName> siblings = objects[depth];
                    foreach (PropertyName sibling in siblings)
                        if (Same(json, sibling, name, ignoreCase, comparer))
                            throw new JsonException($"Duplicate presentation property '{Text(json, name)}'.");
                    siblings.Add(name);
                    break;
            }
        }
    }

    private static bool Same(ReadOnlySpan<byte> json, PropertyName left, PropertyName right, bool ignoreCase, StringComparer comparer)
    {
        if (left.Decoded is null && right.Decoded is null)
        {
            ReadOnlySpan<byte> a = json.Slice(left.Start, left.Length), b = json.Slice(right.Start, right.Length);
            if (!ignoreCase) return a.SequenceEqual(b);
            if (Ascii.IsValid(a) && Ascii.IsValid(b)) return Ascii.EqualsIgnoreCase(a, b);
        }
        return comparer.Equals(Text(json, left), Text(json, right));
    }

    private static string Text(ReadOnlySpan<byte> json, PropertyName name) =>
        name.Decoded ?? Encoding.UTF8.GetString(json.Slice(name.Start, name.Length));
}
