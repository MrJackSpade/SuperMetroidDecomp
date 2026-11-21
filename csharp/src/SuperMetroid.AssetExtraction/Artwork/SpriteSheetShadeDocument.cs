using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>One shade's manifest entry (#1512): the inks it recolours and each one's colour.</summary>
public sealed record SpriteSheetShadeDocument
{
    /// <summary>The inks recoloured, in rising order.</summary>
    public required int[] Inks { get; init; }

    /// <summary>Each ink's colour.</summary>
    public required RgbColor[] Colours { get; init; }

    /// <summary>Whether a draw showing the shade takes part in colour math over every part.</summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public bool ColourMath { get; init; }
}
