using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>One lossless native BG tilemap word used by authored message borders and templates, not an atlas coordinate or controller instruction.</summary>
public sealed record GameplayMessageTitleCell
{
    /// <summary>Complete 16-bit word: character index in bits 0..9, palette in 10..12, priority in 13 and horizontal/vertical reflections in 14/15.</summary>
    [JsonPropertyName("raw"), JsonConverter(typeof(GameplayMessageCellWordJsonConverter))]
    public required SnesBgTilemapWord Word { get; init; }
}
