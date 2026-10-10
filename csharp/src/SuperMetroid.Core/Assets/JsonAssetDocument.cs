using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>Rejects ambiguous authored properties before deserializing installed presentation data.</summary>
internal static class JsonAssetDocument
{
    /// <summary>Deserializes a stream after rejecting duplicate properties, labeling failures with the target type name.</summary>
    /// <param name="json">JSON input consumed from its current position; the stream remains open.</param>
    /// <param name="options">Serializer rules applied after duplicate-property validation.</param>
    /// <returns>The deserialized value, or an invalid-data exception for malformed or null JSON.</returns>
    internal static T Read<T>(Stream json, JsonSerializerOptions options) =>
        Read<T>(json, options, typeof(T).Name);

    /// <summary>Deserializes buffered JSON bytes after applying the same duplicate-property checks used for streams.</summary>
    /// <param name="json">Complete JSON document to validate and deserialize.</param>
    /// <param name="options">Serializer rules applied after duplicate-property validation.</param>
    /// <returns>The deserialized value, or an invalid-data exception for malformed or null JSON.</returns>
    internal static T Read<T>(byte[] json, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(json);
        return Read<T>(json, options, typeof(T).Name);
    }

    /// <summary>Deserializes a stream after rejecting duplicate properties and labels invalid data with a caller-supplied description.</summary>
    /// <typeparam name="T">Document type to deserialize.</typeparam>
    /// <param name="json">JSON input consumed from its current position and left open.</param>
    /// <param name="options">Serializer rules applied after duplicate-property validation.</param>
    /// <param name="description">Human-readable document name included in validation failures.</param>
    /// <returns>The deserialized non-null document.</returns>
    internal static T Read<T>(Stream json, JsonSerializerOptions options, string description)
    {
        ArgumentNullException.ThrowIfNull(json);
        if (json is MemoryStream memory && memory.TryGetBuffer(out ArraySegment<byte> segment))
        {
            int position = checked((int)memory.Position);
            int length = checked((int)(memory.Length - memory.Position));
            memory.Position = memory.Length;
            return Read<T>(segment.AsSpan(position, length), options, description);
        }
        // Deserialization copies everything it keeps, so the document bytes are only borrowed.
        byte[] rented = Rent(json, out int byteCount);
        try
        {
            return Read<T>(rented.AsSpan(0, byteCount), options, description);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    /// <summary>Deserializes buffered UTF-8 JSON after rejecting duplicate properties under the serializer's name-comparison rules.</summary>
    /// <typeparam name="T">Document type to deserialize.</typeparam>
    /// <param name="json">Complete UTF-8 JSON document.</param>
    /// <param name="options">Serializer rules applied after duplicate-property validation.</param>
    /// <param name="description">Human-readable document name included in validation failures.</param>
    /// <returns>The deserialized non-null document.</returns>
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

    /// <summary>Reads the remaining stream contents into a rented array, growing it as needed for non-seekable input.</summary>
    /// <param name="json">Input stream whose remaining bytes are consumed.</param>
    /// <param name="byteCount">Receives the number of valid bytes written at the beginning of the rented array.</param>
    /// <returns>A pooled array that the caller must return to the shared array pool.</returns>
    private static byte[] Rent(Stream json, out int byteCount)
    {
        if (json.CanSeek)
        {
            // Files know their length: one exact read rather than a doubling copy.
            byteCount = checked((int)(json.Length - json.Position));
            byte[] exact = ArrayPool<byte>.Shared.Rent(byteCount);
            json.ReadExactly(exact.AsSpan(0, byteCount));
            return exact;
        }
        byte[] buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);
        byteCount = 0;
        while (true)
        {
            if (byteCount == buffer.Length)
            {
                byte[] larger = ArrayPool<byte>.Shared.Rent(checked(buffer.Length * 2));
                buffer.AsSpan(0, byteCount).CopyTo(larger);
                ArrayPool<byte>.Shared.Return(buffer);
                buffer = larger;
            }
            int read = json.Read(buffer, byteCount, buffer.Length - byteCount);
            if (read == 0) return buffer;
            byteCount += read;
        }
    }

    /// <summary>Identifies one property name either by its raw UTF-8 range or by decoded text when JSON escaping requires it.</summary>
    /// <param name="Start">Byte offset of the unescaped property name within the source JSON.</param>
    /// <param name="Length">Number of source bytes occupied by the unescaped property name.</param>
    /// <param name="Decoded">Decoded name for an escaped token; null when the source range can be compared directly.</param>
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
        /// <summary>Property-name descriptors in encounter order, used to confirm collisions and irregular names.</summary>
        private readonly List<PropertyName> names = [];
        /// <summary>First encountered index for each hashable ASCII name under the selected case rule.</summary>
        private readonly Dictionary<ulong, int> firstByHash = [];
        /// <summary>Indexes of escaped or non-ASCII names that cannot use the raw ASCII hash path.</summary>
        private readonly List<int> irregular = [];

        /// <summary>Clears all per-object lookup state before the scanner reuses this storage for another object.</summary>
        internal void Clear()
        {
            names.Clear();
            firstByHash.Clear();
            irregular.Clear();
        }

        /// <summary>Checks for an equivalent property name, confirming hash hits to handle collisions correctly.</summary>
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

        /// <summary>Records a property name and indexes it by hash when its bytes support the ASCII fast path.</summary>
        internal void Add(ReadOnlySpan<byte> json, PropertyName name, bool ignoreCase)
        {
            names.Add(name);
            if (TryHash(json, name, ignoreCase, out ulong hash)) firstByHash.TryAdd(hash, names.Count - 1);
            else irregular.Add(names.Count - 1);
        }

        /// <summary>Computes the case-adjusted hash for an unescaped ASCII name; returns false for decoded or non-ASCII names.</summary>
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

    /// <summary>Compares two names directly from UTF-8 when possible and otherwise applies the configured string comparer.</summary>
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

    /// <summary>Returns the decoded property name, decoding its source bytes only when no decoded value is already stored.</summary>
    private static string Text(ReadOnlySpan<byte> json, PropertyName name) =>
        name.Decoded ?? Encoding.UTF8.GetString(json.Slice(name.Start, name.Length));
}
