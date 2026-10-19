using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>JSON wrapper for an independently editable SNES BG tilemap cell, rather than a character in a compiled label.</summary>
public sealed record EndingTextCell
{
    /// <summary>The cell's background tilemap entry: character, palette selection, priority and reflection.</summary>
    /// <remarks>The document stores the entry as its numeric <c>raw</c> value, read and written by <see cref="EndingTextCellWordJsonConverter"/>.</remarks>
    [JsonPropertyName("raw")]
    [JsonConverter(typeof(EndingTextCellWordJsonConverter))]
    public required SnesBgTilemapWord Word { get; init; }
}
