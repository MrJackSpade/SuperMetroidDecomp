using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

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
    /// <summary>Updates held before a typewriter sequence begins.</summary>
    public const ushort InitialDelayFrames = 64;
    /// <summary>Updates between consecutive typewriter characters.</summary>
    public const ushort CharacterDelayFrames = 4;
    /// <summary>Updates that the completed item-percentage text remains visible.</summary>
    public const ushort PercentageHoldFrames = 128;
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

    /// <summary>Font3's small alphabet occupies tiles00..19. Large tops occupy20..2F
    /// and40..49 with bottoms16 tiles later; copyright digits occupy60..69/70..79.
    /// Native8C ending text records supply each style's attributes and blank identity.</summary>
    /// <param name="character">The supported character to encode.</param>
    /// <param name="style">The destination text style.</param>
    /// <param name="bottom">Whether to encode the bottom half of a large glyph.</param>
    /// <returns>The tile that shows the glyph.</returns>
    public static BackgroundTile CompileGlyph(char character, EndingTextStyle style, bool bottom = false)
    {
        if (character == ' ')
        {
            return style switch
            {
                EndingTextStyle.ResultSmall => ResultBlankTile,
                EndingTextStyle.CopyrightLarge => LargeBlankTile,
                EndingTextStyle.PercentageSmall => PercentageBlankTile,
                EndingTextStyle.FinalLarge => InstalledLargeBlankTile,
                _ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
            };
        }

        if (style == EndingTextStyle.CopyrightLarge && character is >= '0' and <= '9')
            return new(CopyrightDigitTopBase + character - '0' + (bottom ? LargeBottomOffset : 0), 0, highPriority: false);
        if (character is not (>= 'A' and <= 'Z'))
            throw new ArgumentOutOfRangeException(nameof(character), character,
                "Ending text supports spaces and A-Z; the copyright year also supports digits.");
        int letter = character - 'A';
        return style switch
        {
            EndingTextStyle.ResultSmall => new(ResultLetterBase + letter, 0, highPriority: false),
            EndingTextStyle.PercentageSmall => new(letter, PercentageLetterColourVariant, highPriority: true),
            EndingTextStyle.CopyrightLarge => new(LargeTile(letter, bottom), 0, highPriority: false),
            EndingTextStyle.FinalLarge => new(LargeTile(letter, bottom), 0, highPriority: true),
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
        };
    }

    /// <summary>
    /// Decodes a supported ending-text tile into its editable character: the one character of <paramref name="style"/>
    /// whose <see cref="CompileGlyph"/> tile is exactly <paramref name="cell"/>.
    /// </summary>
    /// <param name="cell">The tile shown.</param>
    /// <param name="style">The text style that owns the tile.</param>
    /// <param name="bottom">Whether the supplied tile is a large glyph's bottom half.</param>
    /// <returns>The decoded space, letter, or copyright digit.</returns>
    public static char DecodeGlyph(BackgroundTile cell, EndingTextStyle style, bool bottom = false)
    {
        if (cell == CompileGlyph(' ', style, bottom)) return ' ';
        if (style == EndingTextStyle.CopyrightLarge)
        {
            for (char digit = '0'; digit <= '9'; digit++)
                if (cell == CompileGlyph(digit, style, bottom)) return digit;
        }
        for (char letter = 'A'; letter <= 'Z'; letter++)
            if (cell == CompileGlyph(letter, style, bottom)) return letter;
        throw new InvalidDataException(
            $"Ending {style} tile {cell} has no safe UTF-8 mapping.");
    }

    /// <summary>Maps A-Z into the split large-font tile ranges and optionally selects the bottom half.</summary>
    private static int LargeTile(int letter, bool bottom)
    {
        int tile = letter < LargeSecondGroupFirstLetter
            ? LargeFirstGroupTopBase + letter
            : LargeSecondGroupTopBase + letter - LargeSecondGroupFirstLetter;
        return bottom ? tile + LargeBottomOffset : tile;
    }

    /// <summary>Height in tile cells of a small native glyph.</summary>
        public const byte SmallGlyphHeight = 1;
    /// <summary>Height in tile cells of a large native glyph.</summary>
        public const byte LargeGlyphHeight = 2;
}
