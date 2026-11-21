namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>One shade's manifest entry (#1512): the colour each ink it recolours shows, as a packed colour word, by ink.</summary>
public sealed record SpriteSheetShadeDocument
{
    /// <summary>The colour each recoloured ink shows, as a packed colour word, by ink in rising order.</summary>
    public required SortedDictionary<int, ushort> InkColours { get; init; }

    /// <summary>Whether a draw showing the shade takes part in colour math over every part.</summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public bool ColourMath { get; init; }
}
