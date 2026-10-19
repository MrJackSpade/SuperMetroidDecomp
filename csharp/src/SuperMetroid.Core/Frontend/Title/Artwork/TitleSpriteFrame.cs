using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>One installed title OBJ composition; all parts inherit the active title palette.</summary>
public sealed record TitleSpriteFrame
{
    /// <summary>Gets the title composition this frame draws, stored in the document by its name.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter<TitleSprite>))]
    public required TitleSprite Sprite { get; init; }
    /// <summary>Gets the ordered OBJ parts composing the frame.</summary>
    public required SpriteVisualPart[] Parts { get; init; }
}
