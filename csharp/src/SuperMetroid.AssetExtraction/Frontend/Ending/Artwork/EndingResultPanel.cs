using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Editable result-credit tilemap based on $8C:DC9B, with a bounded producer label layered over the first row.</summary>
public sealed partial record EndingResultPanel
{
    /// <summary>Exactly 288 tiles in nine 32-cell rows; the producer-label region is cleared and replaced during loading.</summary>
    public required BackgroundTile[] Template { get; init; }
    /// <summary>Uppercase letters or spaces drawn in the small font in the result panel's field, whose width and place <see cref="EndingTextDefinitions.ResultProducedBy"/> holds.</summary>
    public required string Text { get; init; }
}
