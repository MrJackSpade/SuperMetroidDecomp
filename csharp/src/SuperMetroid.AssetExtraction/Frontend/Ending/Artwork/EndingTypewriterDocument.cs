namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// The installed ending typewriter text (#1638): the item-percentage labels and the final message the game types. The
/// panels' tiles and the Japanese subtitle, whose entries are the original's packed tile placements, are import data
/// (<see cref="EndingTextDefinitions.ArtworkFileName"/>).
/// </summary>
public sealed record EndingTypewriterDocument
{
    /// <summary>Revision; loading requires <see cref="EndingTextPresentation.TypewriterVersion"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Uppercase letters or spaces in the field whose width and place <see cref="EndingTextDefinitions.PercentageHeading"/> holds; typed before the detail line.</summary>
    public required string PercentageHeading { get; init; }
    /// <summary>Uppercase letters or spaces in the field whose width and place <see cref="EndingTextDefinitions.PercentageDetail"/> holds; the actual inventory percentage follows it.</summary>
    public required string PercentageDetail { get; init; }
    /// <summary>Uppercase letters or spaces in the field whose width and place <see cref="EndingTextDefinitions.FinalMessage"/> holds, typed in large glyphs.</summary>
    public required string FinalMessage { get; init; }
}
