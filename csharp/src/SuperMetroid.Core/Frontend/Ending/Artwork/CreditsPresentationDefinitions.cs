using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Compiled layout identities and timing boundaries for the scrolling staff credits.</summary>
public static class CreditsPresentationDefinitions
{
    /// <summary>Required schema revision of the editable ordered staff-credit line document.</summary>
    public const int Version = 1;
    /// <summary>JSON presentation filename containing selected wording, starting columns, and palettes for the fixed credit roles.</summary>
    public const string FileName = "ending-credits.json";
    /// <summary>Number of BG tile cells in each compiled credit row; text placement must fit within columns zero through 31.</summary>
    public const int TilemapWidth = 32;
    /// <summary>Eight blank tile rows preceding the first staff heading; this is a row count, not a frame delay.</summary>
    public const int InitialBlankRows = 8;
    /// <summary>Sixteen blank tile rows before a new section heading, except the initial heading and paired sound-effects heading.</summary>
    public const int SectionBlankRows = 16;
    /// <summary>One blank tile row before each non-heading line, separating large names within a section.</summary>
    public const int InterLineBlankRows = 1;
    /// <summary>Thirty-five blank tile rows following the final general-manager name, retaining the native scrolling tail.</summary>
    public const int TrailingBlankRows = 35;
    /// <summary>Total expanded scrolling stream length, 520 tile rows including headings, two-row names, and blank spacing; not the 128-row source tilemap size.</summary>
    public const int ExpectedCompiledRows = 520;
    /// <summary>The Font-3 blank tile, used for empty rows and spaces without applying a line's colour variant.</summary>
    public static BackgroundTile BlankTile { get; } = new(0x07f, 0, highPriority: false);
    /// <summary>Largest accepted colour variant, seven; this is a palette-row identity, not an RGB value.</summary>
    public const int MaximumPalette = 7;
    /// <summary>Font-3 tile used by the upper half of the credits ampersand.</summary>
    public const int LargeAmpersandTop = 0x007b;
    /// <summary>Font-3 tile used by the lower half of the credits ampersand.</summary>
    public const int LargeAmpersandBottom = 0x007c;
    /// <summary>Font-3 tile used by the upper half of the credits period.</summary>
    public const int LargePeriodTop = 0x007d;
    /// <summary>Font-3 tile used by the lower half of the credits period.</summary>
    public const int LargePeriodBottom = 0x007e;
}
