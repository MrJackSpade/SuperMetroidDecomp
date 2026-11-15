using System.Text.Json.Serialization;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>
/// One installed title card frame (#1979): the card's text as far as one typing step shows it, whole; all parts inherit the
/// active title palette.
/// </summary>
public sealed record TitleCardFrame
{
    /// <summary>Gets the phase whose card the frame shows, stored in the document by its name.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter<TitleSequencePhase>))]
    public required TitleSequencePhase Card { get; init; }
    /// <summary>Gets how many of the card's letters the frame shows typed.</summary>
    public required int Letters { get; init; }
    /// <summary>Gets the ordered OBJ parts composing the frame.</summary>
    public required SpriteVisualPart[] Parts { get; init; }

    /// <summary>The typing step the frame shows.</summary>
    [JsonIgnore]
    public TitleCardTyping Typed => new(Card, Letters);
}
