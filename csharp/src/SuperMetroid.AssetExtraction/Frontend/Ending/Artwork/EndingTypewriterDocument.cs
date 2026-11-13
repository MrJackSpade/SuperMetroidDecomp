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
    /// <summary>One through thirteen uppercase letters or spaces beginning at row 10, column 9; typed before the detail line.</summary>
    public required string PercentageHeading { get; init; }
    /// <summary>One through nineteen uppercase letters or spaces beginning at row 12, column 6; the actual inventory percentage follows it.</summary>
    public required string PercentageDetail { get; init; }
    /// <summary>One through twenty uppercase letters or spaces beginning at row 2, column 6, typed in two-row large glyphs.</summary>
    public required string FinalMessage { get; init; }
}
