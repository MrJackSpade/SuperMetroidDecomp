using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Reads and writes one <c>samus-body.json</c> sprite part's X word as the plain JSON number the document format
/// stores, so a loaded part holds a typed offset-and-size word while the editable file keeps its numeric schema.
/// </summary>
internal sealed class SamusSpritePartXJsonConverter : JsonConverter<SnesSpritemapXWord>
{
    /// <summary>Accepts only an unsigned sixteen-bit JSON number; anything else is a malformed document.</summary>
    [ExternalContract("System.Text.Json's JsonConverter<T>.Read fixes the signature.")]
    public override SnesSpritemapXWord Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number || !reader.TryGetUInt16(out ushort x))
            throw new JsonException("A Samus sprite part's X word must be a number from 0 through 65535.");
        return new SnesSpritemapXWord(x);
    }

    /// <summary>Writes the complete X word as the document's numeric value.</summary>
    public override void Write(Utf8JsonWriter writer, SnesSpritemapXWord value, JsonSerializerOptions options)
    {
        Ensure.NotNull(writer);
        writer.WriteNumberValue(value.Raw);
    }
}
