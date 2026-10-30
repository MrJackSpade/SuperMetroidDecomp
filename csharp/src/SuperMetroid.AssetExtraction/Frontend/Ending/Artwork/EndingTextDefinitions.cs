using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Schema, layout, and native extraction identities for post-credit text.</summary>
public static class EndingTextDefinitions
{
    /// <summary>Current ending-text asset schema version.</summary>
    public const int Version = 2;
    /// <summary>Installed filename of the editable ending-text asset.</summary>
    public const string FileName = "ending-text.json";
    /// <summary>Width of each ending tilemap in cells.</summary>
    public const int TilemapWidth = 32;
    /// <summary>Height of the result panel in rows.</summary>
    public const int ResultPanelRows = 9;
    /// <summary>Height of the copyright panel in rows.</summary>
    public const int CopyrightRows = 2;
    /// <summary>Height of the alternate-language subtitle in rows.</summary>
    public const int JapaneseSubtitleRows = 2;
    /// <summary>The blank tile of the small result panel.</summary>
    public static BackgroundTile ResultBlankTile { get; } = new(0x04f, 0, highPriority: false);
    /// <summary>The blank tile of large copyright glyphs, in colour variant zero and the lower priority tier.</summary>
    public static BackgroundTile LargeBlankTile { get; } = new(0x07f, 0, highPriority: false);
    /// <summary>The blank tile of installed large text, in the upper priority tier.</summary>
    public static BackgroundTile InstalledLargeBlankTile { get; } = new(0x07f, 0, highPriority: true);
    /// <summary>The blank tile of the item-percentage typewriter.</summary>
    public static BackgroundTile PercentageBlankTile { get; } = new(0x07f, 0, highPriority: true);
    /// <summary>The colour variant of the item-percentage small alphabet, which is drawn in the upper priority tier.</summary>
    public const int PercentageLetterColourVariant = 7;
    /// <summary>Top-half tile index of copyright digit zero.</summary>
    public const int CopyrightDigitTopBase = 0x0060;
    /// <summary>Base tile index of the small result alphabet.</summary>
    public const int ResultLetterBase = 0x0000;
    /// <summary>Top-half tile index of the first large-letter group.</summary>
    public const int LargeFirstGroupTopBase = 0x0020;
    /// <summary>Top-half tile index of the second large-letter group.</summary>
    public const int LargeSecondGroupTopBase = 0x0040;
    /// <summary>Tile-index delta from a large glyph's top half to its bottom half.</summary>
    public const int LargeBottomOffset = 0x0010;
    /// <summary>Alphabet index of the first letter stored in the second large group.</summary>
    public const int LargeSecondGroupFirstLetter = 16;

    /// <summary>Eleven-cell producer line centered on result-panel row zero.</summary>
    public static EndingTextRegionDefinition ResultProducedBy => Centered(0, 11, EndingTextStyle.ResultSmall);
    /// <summary>Thirteen-cell percentage heading centered on tile row ten.</summary>
    public static EndingTextRegionDefinition PercentageHeading => Centered(10, 13, EndingTextStyle.PercentageSmall);
    /// <summary>Nineteen-cell percentage detail centered on tile row twelve.</summary>
    public static EndingTextRegionDefinition PercentageDetail => Centered(12, 19, EndingTextStyle.PercentageSmall);
    /// <summary>Twenty-cell two-row message centered from tile row two.</summary>
    public static EndingTextRegionDefinition FinalMessage => Centered(2, 20, EndingTextStyle.FinalLarge);

    /// <summary>Native copyright spacing: four year glyphs, two blank cells, and eight company glyphs.</summary>
    private const int CopyrightYearWidth = 4, CopyrightCompanyWidth = 8, CopyrightGap = 2;
    /// <summary>First column of the centered year-and-company composition.</summary>
    private static int CopyrightStart => CenteredColumn(CopyrightYearWidth + CopyrightGap + CopyrightCompanyWidth);
    /// <summary>Four large year digits beginning at the centered copyright origin.</summary>
    public static EndingTextRegionDefinition CopyrightYear => new(0, CopyrightStart, CopyrightYearWidth, EndingTextStyle.CopyrightLarge);
    /// <summary>Eight large company glyphs after the year and two-cell gap.</summary>
    public static EndingTextRegionDefinition CopyrightCompany => new(0, CopyrightStart + CopyrightYearWidth + CopyrightGap,
        CopyrightCompanyWidth, EndingTextStyle.CopyrightLarge);

    /// <summary>Centers a field within 32 cells, leaving an odd spare column on the right.</summary>
    /// <remarks>Odd spare columns leave the extra cell on the right, matching native tilemaps.</remarks>
    private static int CenteredColumn(int width) => (TilemapWidth - width) / 2;
    /// <summary>Places an ending-text field at a native centered column with the selected font style.</summary>
    private static EndingTextRegionDefinition Centered(int row, int width, EndingTextStyle style) =>
        new(row, CenteredColumn(width), width, style);

    /// <summary>Height in tile cells of a small native glyph.</summary>
        public const byte SmallGlyphHeight = 1;
    /// <summary>Height in tile cells of a large native glyph.</summary>
        public const byte LargeGlyphHeight = 2;
}
