using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Encoded X/size word, signed-wrap Y byte, and the part's OBJ attributes (tile, palette, priority and flips).</summary>
/// <param name="X">Spritemap X word: signed nine-bit offset and the large-size bit.</param>
/// <param name="Y">Signed Y offset byte.</param>
/// <param name="Attributes">Tile, palette, priority and flips; the installed document stores them as one number.</param>
public readonly record struct SamusSpritePart(
    [property: JsonRequired] ushort X,
    [property: JsonRequired] byte Y,
    [property: JsonRequired, JsonConverter(typeof(SamusSpritePartAttributesJsonConverter))] SnesObjAttributeWord Attributes);
