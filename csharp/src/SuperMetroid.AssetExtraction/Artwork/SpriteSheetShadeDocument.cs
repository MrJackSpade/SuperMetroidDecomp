using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>One shade's manifest entry (#1512): the colour each ink it recolours shows, by ink.</summary>
public sealed partial record SpriteSheetShadeDocument
{
    /// <summary>The colour each recoloured ink shows, by ink in rising order.</summary>
    public required SortedDictionary<int, RgbColor> InkColours { get; init; }

    /// <summary>Whether a draw showing the shade takes part in colour math over every part.</summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public bool ColourMath { get; init; }
}
