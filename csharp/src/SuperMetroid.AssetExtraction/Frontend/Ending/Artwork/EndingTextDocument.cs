using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Editable <c>ending-text.json</c> schema for post-credit panels and typewriter labels; sequence timing, actual item percentage, and subtitle visibility remain runtime behavior.</summary>
public sealed record EndingTextDocument
{
    /// <summary>Schema revision; loading requires <see cref="EndingTextDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Nine-row result-credit tile template and its independently configurable producer label.</summary>
    public required EndingResultPanel ResultPanel { get; init; }
    /// <summary>One through four large-font characters at row 0, column 9 of the copyright panel; uppercase letters, digits, and spaces are supported.</summary>
    public required string CopyrightYear { get; init; }
    /// <summary>One through eight large-font characters at row 0, column 15 of the copyright panel; uppercase letters, digits, and spaces are supported.</summary>
    public required string CopyrightCompany { get; init; }
    /// <summary>One through thirteen uppercase letters or spaces beginning at row 10, column 9; drawn before the detail line in the item-percentage typewriter sequence.</summary>
    public required string PercentageHeading { get; init; }
    /// <summary>One through nineteen uppercase letters or spaces beginning at row 12, column 6; the runtime draws the actual inventory percentage after this label.</summary>
    public required string PercentageDetail { get; init; }
    /// <summary>Exactly 64 tiles in two 32-cell rows, retaining the native Japanese subtitle glyphs or independently edited tiles.</summary>
    public required BackgroundTile[] JapaneseSubtitle { get; init; }
    /// <summary>One through twenty uppercase letters or spaces beginning at row 2, column 6; compiled into two-row large glyphs for the final typewriter message.</summary>
    public required string FinalMessage { get; init; }
}
