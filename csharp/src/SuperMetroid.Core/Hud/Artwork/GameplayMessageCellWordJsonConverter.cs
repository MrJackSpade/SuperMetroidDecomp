using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>
/// Reads and writes one gameplay message border or template cell as the plain JSON number the document format
/// stores, so a loaded cell holds a typed background tilemap entry while the editable file keeps its numeric schema.
/// </summary>
internal sealed class GameplayMessageCellWordJsonConverter : JsonConverter<SnesBgTilemapWord>
{
    /// <summary>Accepts only an unsigned sixteen-bit JSON number; anything else is a malformed document.</summary>
    public override SnesBgTilemapWord Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number || !reader.TryGetUInt16(out ushort entry))
            throw new JsonException("A gameplay message tilemap cell must be a number from 0 through 65535.");
        return new SnesBgTilemapWord(entry);
    }

    /// <summary>Writes the complete tilemap entry as the document's numeric cell value.</summary>
    public override void Write(Utf8JsonWriter writer, SnesBgTilemapWord value, JsonSerializerOptions options)
    {
        Ensure.NotNull(writer);
        writer.WriteNumberValue(value.Raw);
    }
}
