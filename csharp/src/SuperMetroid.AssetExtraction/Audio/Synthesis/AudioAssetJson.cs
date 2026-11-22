using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Serializes large audio manifests with exact byte lists and low-allocation music instruction records.</summary>
internal static partial class AudioAssetJson
{
    /// <summary>Camel-case manifest settings with converters that preserve the authored JSON structure.</summary>
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new ExactByteListConverter(), new MusicInstructionConverter(), new MusicTrackJsonConverter() },
    };

    /// <summary>
    /// Reads each music instruction straight into its record. The generic constructor path
    /// allocated argument state per instruction across thousands of instructions per load.
    /// Semantics match the default record binding: unknown members are skipped, the last
    /// duplicate wins and absent members take their defaults. The operation is read through the
    /// written-out key codecs (#2548): an effect's key names an effect command, which its opcode must
    /// name too, and every other key names its kind.
    /// </summary>
    private sealed partial class MusicInstructionConverter : JsonConverter<AudioMusicInstructionMetadata>
    {
        /// <summary>Reads an authored instruction directly, retaining default and last-duplicate JSON behavior.</summary>
        /// <exception cref="JsonException">The operation names no command, or names an effect its opcode does not.</exception>
        public override AudioMusicInstructionMetadata Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected a music instruction object.");
            MusicOperation? operation = null;
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
            MusicOperation named = operation ?? throw new JsonException("Music instruction requires an operation.");
            if (named.Effect is { } effect && !NamesEffect(opcode, effect))
                throw new JsonException($"Music instruction opcode ${opcode:X2} is no '{SpcMusicEffectKeys.Installed.Of(effect)}' command.");
            return new AudioMusicInstructionMetadata(
                named.Kind,
                opcode,
                timing ?? throw new JsonException("Music instruction requires its timing bytes."),
                arguments ?? throw new JsonException("Music instruction requires its argument bytes."));
        }

        /// <summary>Whether <paramref name="opcode"/> is the opcode of <paramref name="effect"/>.</summary>
        private static bool NamesEffect(int opcode, SpcMusicEffect effect) =>
            opcode >= SpcDriverDefinitions.Music.FirstEffect && opcode - SpcDriverDefinitions.Music.FirstEffect < SpcMusicDefinitionCodec.EffectCount &&
            SpcMusicDefinitionCodec.DecodeEffect(opcode) == effect;

        /// <summary>The command an operation's key names, read through the effects' and the kinds' key codecs without allocating.</summary>
        /// <exception cref="JsonException">The operation is no string, or names no command; the effect kind's own key names none.</exception>
        private static MusicOperation Operation(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("Music instruction operation must be a string.");
            int length = reader.HasValueSequence ? checked((int)reader.ValueSequence.Length) : reader.ValueSpan.Length;
            Span<char> text = length <= 128 ? stackalloc char[length] : new char[length];
            ReadOnlySpan<char> key = text[..reader.CopyString(text)];
            if (SpcMusicEffectKeys.Installed.TryParse(key, out SpcMusicEffect effect))
                return new(AudioMusicInstructionKind.Effect, effect);
            return AudioMusicInstructionKindKeys.Installed.TryParse(key, out AudioMusicInstructionKind kind) && kind != AudioMusicInstructionKind.Effect
                ? new(kind, null)
                : throw new JsonException($"Music instruction operation '{key}' names no command.");
        }

        /// <summary>Writes the four authored music instruction fields in the canonical manifest order: an effect by its effect's key, any other command by its kind's.</summary>
        public override void Write(Utf8JsonWriter writer, AudioMusicInstructionMetadata value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("operation"u8, value.Kind == AudioMusicInstructionKind.Effect
                ? SpcMusicEffectKeys.Installed.Of(SpcMusicDefinitionCodec.DecodeEffect(value.Opcode))
                : AudioMusicInstructionKindKeys.Installed.Of(value.Kind));
            writer.WriteNumber("opcode"u8, value.Opcode);
            writer.WritePropertyName("timing"u8);
            JsonSerializer.Serialize(writer, value.Timing, global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::System.Collections.Generic.IReadOnlyList<byte>>(options));
            writer.WritePropertyName("arguments"u8);
            JsonSerializer.Serialize(writer, value.Arguments, global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::System.Collections.Generic.IReadOnlyList<byte>>(options));
            writer.WriteEndObject();
        }

        /// <summary>The command an operation's key names.</summary>
        /// <param name="Kind">What the command is.</param>
        /// <param name="Effect">The effect an effect command's key names; null for any other command.</param>
        private readonly partial record struct MusicOperation(AudioMusicInstructionKind Kind, SpcMusicEffect? Effect);
    }

    /// <summary>
    /// Reads every instruction's timing/argument bytes into one exact array instead of a
    /// growing list. Writes the same JSON number array the default collection writer emits.
    /// </summary>
    private sealed partial class ExactByteListConverter : JsonConverter<IReadOnlyList<byte>>
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
