using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Editable <c>ending-text.json</c> schema for post-credit panels and typewriter labels; sequence timing, actual item percentage, and subtitle visibility remain runtime behavior.</summary>
public sealed partial record EndingTextDocument
{
    /// <summary>Schema revision; loading requires <see cref="EndingTextDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Nine-row result-credit tile template and its independently configurable producer label.</summary>
    public required EndingResultPanel ResultPanel { get; init; }
    /// <summary>Large-font characters in the copyright panel's year field, whose width and place <see cref="EndingTextDefinitions.CopyrightYear"/> holds; uppercase letters, digits, and spaces are supported.</summary>
    public required string CopyrightYear { get; init; }
    /// <summary>Large-font characters in the copyright panel's company field, whose width and place <see cref="EndingTextDefinitions.CopyrightCompany"/> holds; uppercase letters, digits, and spaces are supported.</summary>
    public required string CopyrightCompany { get; init; }
    /// <summary>Uppercase letters or spaces in the field whose width and place <see cref="EndingTextDefinitions.PercentageHeading"/> holds; drawn before the detail line in the item-percentage typewriter sequence.</summary>
    public required string PercentageHeading { get; init; }
    /// <summary>Uppercase letters or spaces in the field whose width and place <see cref="EndingTextDefinitions.PercentageDetail"/> holds; the runtime draws the actual inventory percentage after this label.</summary>
    public required string PercentageDetail { get; init; }
    /// <summary>Exactly 64 tiles in two 32-cell rows, retaining the native Japanese subtitle glyphs or independently edited tiles.</summary>
    public required BackgroundTile[] JapaneseSubtitle { get; init; }
    /// <summary>Uppercase letters or spaces in the field whose width and place <see cref="EndingTextDefinitions.FinalMessage"/> holds; compiled into large glyphs for the final typewriter message.</summary>
    public required string FinalMessage { get; init; }
}
