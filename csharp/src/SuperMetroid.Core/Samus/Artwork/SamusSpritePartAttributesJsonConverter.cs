using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Reads and writes one <c>samus-body.json</c> sprite part's OBJ attributes as the plain JSON number the document
/// format stores, so a loaded part holds typed attributes while the editable file keeps its numeric schema.
/// </summary>
internal sealed class SamusSpritePartAttributesJsonConverter : JsonConverter<SnesObjAttributeWord>
{
    /// <summary>Accepts only an unsigned sixteen-bit JSON number; anything else is a malformed document.</summary>
    [ExternalContract("System.Text.Json's JsonConverter<T>.Read fixes the signature.")]
    public override SnesObjAttributeWord Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number || !reader.TryGetInt32(out int attributes) || attributes is < 0 or > ushort.MaxValue)
            throw new JsonException("A Samus sprite part's attributes must be a number from 0 through 65535.");
        return new SnesObjAttributeWord((ushort)attributes);
    }

    /// <summary>Writes the complete attribute word as the document's numeric value.</summary>
    public override void Write(Utf8JsonWriter writer, SnesObjAttributeWord value, JsonSerializerOptions options)
    {
        Ensure.NotNull(writer);
        writer.WriteNumberValue(value.Raw);
    }
}
