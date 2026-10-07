using System.Runtime.InteropServices;
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
                ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal, descendArrays: true,
                name => new JsonException($"Duplicate presentation property '{name}'."));
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
    /// Rejects a repeated property name in any object of an already-parsed document, reading
    /// the element's raw UTF-8 in place. <paramref name="descendArrays"/> false leaves objects
    /// reached through an array unchecked; <paramref name="error"/> builds the failure.
    /// </summary>
    internal static void RejectDuplicateProperties(JsonElement element, StringComparer comparer,
        Func<string, Exception> error, bool descendArrays = true)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        ArgumentNullException.ThrowIfNull(error);
        RejectDuplicates(JsonMarshal.GetRawUtf8Value(element), comparer, descendArrays, error);
    }

    /// <summary>
    /// One forward pass over the document with the same strictness as <see cref="JsonDocument.Parse(Stream, JsonDocumentOptions)"/>
    /// defaults. Names are compared as raw UTF-8 and decoded only when escaped or non-ASCII,
    /// so validating thousands of small cell objects allocates no per-object sets or strings.
    /// </summary>
    private static void RejectDuplicates(ReadOnlySpan<byte> json, StringComparer comparer, bool descendArrays,
        Func<string, Exception> error)
    {
        bool ignoreCase = ReferenceEquals(comparer, StringComparer.OrdinalIgnoreCase);
        var reader = new Utf8JsonReader(json, new JsonReaderOptions());
        var objects = new List<ObjectNames>();
        int depth = -1, arrays = 0;
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    depth++;
                    if (depth == objects.Count) objects.Add(new ObjectNames());
                    else objects[depth].Clear();
                    break;
                case JsonTokenType.EndObject:
                    depth--;
                    break;
                case JsonTokenType.StartArray:
                    arrays++;
                    break;
                case JsonTokenType.EndArray:
                    arrays--;
                    break;
                case JsonTokenType.PropertyName:
                    if (!descendArrays && arrays > 0) break;
                    var name = reader.ValueIsEscaped
                        ? new PropertyName(0, 0, reader.GetString())
                        : new PropertyName(checked((int)reader.TokenStartIndex) + 1, reader.ValueSpan.Length, null);
                    if (objects[depth].Contains(json, name, ignoreCase, comparer))
                        throw error(Text(json, name));
                    objects[depth].Add(json, name, ignoreCase);
                    break;
            }
        }
    }

    /// <summary>
    /// The names seen in one object. Plain ASCII names are indexed by a case-folded FNV-1a hash
    /// of their raw bytes, so large objects (named frame tables) check each name in constant
    /// time; escaped or non-ASCII names, whose folding the hash cannot mirror, are compared
    /// directly against every name.
    /// </summary>
    private sealed class ObjectNames
    {
        private readonly List<PropertyName> names = [];
        private readonly Dictionary<ulong, int> firstByHash = [];
        private readonly List<int> irregular = [];

        internal void Clear()
        {
            names.Clear();
            firstByHash.Clear();
            irregular.Clear();
        }

        internal bool Contains(ReadOnlySpan<byte> json, PropertyName name, bool ignoreCase, StringComparer comparer)
        {
            if (!TryHash(json, name, ignoreCase, out ulong hash))
            {
                foreach (PropertyName existing in names)
                    if (Same(json, existing, name, ignoreCase, comparer)) return true;
                return false;
            }
            foreach (int index in irregular)
                if (Same(json, names[index], name, ignoreCase, comparer)) return true;
            if (!firstByHash.TryGetValue(hash, out int first)) return false;
            if (Same(json, names[first], name, ignoreCase, comparer)) return true;
            // A genuine hash collision: confirm against every name rather than assume.
            foreach (PropertyName existing in names)
                if (Same(json, existing, name, ignoreCase, comparer)) return true;
            return false;
        }

        internal void Add(ReadOnlySpan<byte> json, PropertyName name, bool ignoreCase)
        {
            names.Add(name);
            if (TryHash(json, name, ignoreCase, out ulong hash)) firstByHash.TryAdd(hash, names.Count - 1);
            else irregular.Add(names.Count - 1);
        }

        private static bool TryHash(ReadOnlySpan<byte> json, PropertyName name, bool ignoreCase, out ulong hash)
        {
            hash = 14695981039346656037;
            if (name.Decoded is not null) return false;
            foreach (byte value in json.Slice(name.Start, name.Length))
            {
                if (value >= 0x80) return false;
                byte folded = ignoreCase && value is >= (byte)'A' and <= (byte)'Z' ? (byte)(value | 0x20) : value;
                hash = (hash ^ folded) * 1099511628211;
            }
            return true;
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
