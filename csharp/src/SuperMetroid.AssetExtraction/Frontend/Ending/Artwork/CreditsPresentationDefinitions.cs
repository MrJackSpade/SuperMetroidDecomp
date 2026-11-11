using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>The staff credits document's schema and the Font-3 tiles its glyphs are pictured from; the roll's layout in pixels is <see cref="CreditsRollDefinitions"/>.</summary>
public static class CreditsPresentationDefinitions
{
    /// <summary>Required schema revision of the editable ordered staff-credit line document.</summary>
    public const int Version = 1;
    /// <summary>JSON presentation filename containing selected wording, starting columns, and palettes for the fixed credit roles.</summary>
    public const string FileName = "ending-credits.json";
    /// <summary>Number of BG tile cells in each compiled credit row; text placement must fit within columns zero through 31.</summary>
    public const int TilemapWidth = 32;
    /// <summary>The pixels of the roll one row of the cartridge's credits row stream covers.</summary>
    public const int RowHeight = 8;
    /// <summary>The rows of the cartridge's credits row stream: one for each row's height of the roll's length; not the 128-row source tilemap size.</summary>
    public const int RowCount = CreditsRollDefinitions.Length / RowHeight;
    /// <summary>The Font-3 blank tile, used for empty rows and spaces without applying a line's colour variant.</summary>
    public static BackgroundTile BlankTile { get; } = new(127, 0, highPriority: false);
    /// <summary>Font-3 tile used by the upper half of the credits ampersand.</summary>
    public const int LargeAmpersandTop = 123;
    /// <summary>Font-3 tile used by the lower half of the credits ampersand.</summary>
    public const int LargeAmpersandBottom = 124;
    /// <summary>Font-3 tile used by the upper half of the credits period.</summary>
    public const int LargePeriodTop = 125;
    /// <summary>Font-3 tile used by the lower half of the credits period.</summary>
    public const int LargePeriodBottom = 126;
}
