using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Serializes large audio manifests with exact byte lists and low-allocation music instruction records.</summary>
internal static class AudioAssetJson
{
    /// <summary>Camel-case manifest settings with converters that preserve the authored JSON structure.</summary>
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new ExactByteListConverter(), new MusicInstructionConverter() },
    };

    /// <summary>
    /// Reads each music instruction straight into its record. The generic constructor path
    /// allocated argument state per instruction across thousands of instructions per load.
    /// Semantics match the default record binding: unknown members are skipped, the last
    /// duplicate wins, absent members take their defaults, and the written JSON is identical.
    /// </summary>
    private sealed class MusicInstructionConverter : JsonConverter<AudioMusicInstructionMetadata>
    {
        /// <summary>Thread-local intern table sharing repeated operation names across music programs.</summary>
        [ThreadStatic] private static Dictionary<string, string>? operations;

        /// <summary>Reads an authored instruction directly, retaining default and last-duplicate JSON behavior.</summary>
        public override AudioMusicInstructionMetadata Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected a music instruction object.");
            string? operation = null;
            int opcode = 0;
            IReadOnlyList<byte>? timing = null, arguments = null;
            var bytes = (JsonConverter<IReadOnlyList<byte>>)options.GetConverter(typeof(IReadOnlyList<byte>));
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Expected a music instruction property.");
                if (reader.ValueTextEquals("operation"u8))
                {
                    reader.Read();
                    operation = reader.TokenType == JsonTokenType.Null ? null : Operation(ref reader);
                }
                else if (reader.ValueTextEquals("opcode"u8))
                {
                    reader.Read();
                    opcode = reader.GetByte();
                }
                else if (reader.ValueTextEquals("timing"u8))
                {
                    reader.Read();
                    timing = reader.TokenType == JsonTokenType.Null ? null : bytes.Read(ref reader, typeof(IReadOnlyList<byte>), options);
                }
                else if (reader.ValueTextEquals("arguments"u8))
                {
                    reader.Read();
                    arguments = reader.TokenType == JsonTokenType.Null ? null : bytes.Read(ref reader, typeof(IReadOnlyList<byte>), options);
                }
                else
                {
                    reader.Read();
                    reader.Skip();
                }
            }
            if (reader.TokenType != JsonTokenType.EndObject)
                throw new JsonException("Music instruction is not terminated.");
            return new AudioMusicInstructionMetadata(
                operation ?? throw new JsonException("Music instruction requires an operation."),
                opcode,
                timing ?? throw new JsonException("Music instruction requires its timing bytes."),
                arguments ?? throw new JsonException("Music instruction requires its argument bytes."));
        }

        /// <summary>Interns repeated operation strings using a span lookup before allocating a new name.</summary>
        private static string Operation(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("Music instruction operation must be a string.");
            int length = reader.HasValueSequence ? checked((int)reader.ValueSequence.Length) : reader.ValueSpan.Length;
            Span<char> text = length <= 128 ? stackalloc char[length] : new char[length];
            int written = reader.CopyString(text);
            var lookup = (operations ??= new(StringComparer.Ordinal)).GetAlternateLookup<ReadOnlySpan<char>>();
            if (lookup.TryGetValue(text[..written], out string? known))
                return known;
            string name = new(text[..written]);
            operations.Add(name, name);
            return name;
        }

        /// <summary>Writes the four authored music instruction fields in the canonical manifest order.</summary>
        public override void Write(Utf8JsonWriter writer, AudioMusicInstructionMetadata value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("operation"u8, value.Operation);
            writer.WriteNumber("opcode"u8, value.Opcode);
            writer.WritePropertyName("timing"u8);
            JsonSerializer.Serialize(writer, value.Timing, options);
            writer.WritePropertyName("arguments"u8);
            JsonSerializer.Serialize(writer, value.Arguments, options);
            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// Reads every instruction's timing/argument bytes into one exact array instead of a
    /// growing list. Writes the same JSON number array the default collection writer emits.
    /// </summary>
    private sealed class ExactByteListConverter : JsonConverter<IReadOnlyList<byte>>
    {
        /// <summary>Reads a byte array with stack storage for short operands and a pooled buffer for larger ones.</summary>
        public override IReadOnlyList<byte> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Expected a JSON array of bytes.");
            Span<byte> small = stackalloc byte[16];
            byte[]? rented = null;
            Span<byte> values = small;
            int count = 0;
            try
            {
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    if (count == values.Length)
                    {
                        byte[] larger = System.Buffers.ArrayPool<byte>.Shared.Rent(values.Length * 2);
                        values.CopyTo(larger);
                        if (rented is not null) System.Buffers.ArrayPool<byte>.Shared.Return(rented);
                        rented = larger;
                        values = larger;
                    }
                    values[count++] = reader.GetByte();
                }
                if (reader.TokenType != JsonTokenType.EndArray)
                    throw new JsonException("Byte array is not terminated.");
                return values[..count].ToArray();
            }
            finally
            {
                if (rented is not null) System.Buffers.ArrayPool<byte>.Shared.Return(rented);
            }
        }

        /// <summary>Writes each timing or argument byte as a JSON number without changing collection order.</summary>
        public override void Write(Utf8JsonWriter writer, IReadOnlyList<byte> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            for (int index = 0; index < value.Count; index++)
                writer.WriteNumberValue(value[index]);
            writer.WriteEndArray();
        }
    }
}
