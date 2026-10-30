using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>The staff credits document's schema and the Font-3 tiles its glyphs are pictured from; the roll's row layout is <see cref="CreditsRollDefinitions"/>.</summary>
public static class CreditsPresentationDefinitions
{
    /// <summary>Required schema revision of the editable ordered staff-credit line document.</summary>
    public const int Version = 1;
    /// <summary>JSON presentation filename containing selected wording, starting columns, and palettes for the fixed credit roles.</summary>
    public const string FileName = "ending-credits.json";
    /// <summary>Number of BG tile cells in each compiled credit row; text placement must fit within columns zero through 31.</summary>
    public const int TilemapWidth = 32;
    /// <summary>The Font-3 blank tile, used for empty rows and spaces without applying a line's colour variant.</summary>
    public static BackgroundTile BlankTile { get; } = new(0x07f, 0, highPriority: false);
    /// <summary>Font-3 tile used by the upper half of the credits ampersand.</summary>
    public const int LargeAmpersandTop = 0x007b;
    /// <summary>Font-3 tile used by the lower half of the credits ampersand.</summary>
    public const int LargeAmpersandBottom = 0x007c;
    /// <summary>Font-3 tile used by the upper half of the credits period.</summary>
    public const int LargePeriodTop = 0x007d;
    /// <summary>Font-3 tile used by the lower half of the credits period.</summary>
    public const int LargePeriodBottom = 0x007e;
}
