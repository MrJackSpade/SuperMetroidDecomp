using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Reads and writes one <c>samus-body.json</c> sprite part's Y byte as the plain JSON number the document format
/// stores, so a loaded part holds a typed vertical offset while the editable file keeps its numeric schema.
/// </summary>
internal sealed class SamusSpritePartYJsonConverter : JsonConverter<SnesSpritemapYByte>
{
    /// <summary>Accepts only an unsigned eight-bit JSON number; anything else is a malformed document.</summary>
    [ExternalContract("System.Text.Json's JsonConverter<T>.Read fixes the signature.")]
    public override SnesSpritemapYByte Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number || !reader.TryGetByte(out byte y))
            throw new JsonException("A Samus sprite part's Y byte must be a number from 0 through 255.");
        return new SnesSpritemapYByte(y);
    }

    /// <summary>Writes the encoded Y byte as the document's numeric value.</summary>
    public override void Write(Utf8JsonWriter writer, SnesSpritemapYByte value, JsonSerializerOptions options)
    {
        Ensure.NotNull(writer);
        writer.WriteNumberValue(value.Raw);
    }
}
