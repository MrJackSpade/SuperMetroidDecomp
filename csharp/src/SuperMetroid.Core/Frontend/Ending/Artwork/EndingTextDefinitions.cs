namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Schema, layout, and native extraction identities for post-credit text.</summary>
public static class EndingTextDefinitions
{
    /// <summary>Current ending-text asset schema version.</summary>
    public const int Version = 1;
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
    /// <summary>Blank tilemap word used by the small result panel.</summary>
    public const ushort ResultBlankWord = 0x004f;
    /// <summary>Unattributed blank tilemap word used by large copyright glyphs.</summary>
    public const ushort LargeBlankWord = 0x007f;
    /// <summary>Blank tilemap word with the attributes used by installed large text.</summary>
    public const ushort InstalledLargeBlankWord = 0x207f;
    /// <summary>Blank tilemap word used by the item-percentage typewriter.</summary>
    public const ushort PercentageBlankWord = 0x207f;
    /// <summary>Base tilemap word for the item-percentage small alphabet.</summary>
    public const ushort PercentageLetterBase = 0x3c00;
    /// <summary>Tilemap attributes applied to final-message large letters.</summary>
    public const ushort FinalLetterAttributes = 0x2000;
    /// <summary>Top-half tile index of copyright digit zero.</summary>
    public const ushort CopyrightDigitTopBase = 0x0060;
    /// <summary>Base tile index of the small result alphabet.</summary>
    public const ushort ResultLetterBase = 0x0000;
    /// <summary>Top-half tile index of the first large-letter group.</summary>
    public const ushort LargeFirstGroupTopBase = 0x0020;
    /// <summary>Top-half tile index of the second large-letter group.</summary>
    public const ushort LargeSecondGroupTopBase = 0x0040;
    /// <summary>Tile-index delta from a large glyph's top half to its bottom half.</summary>
    public const ushort LargeBottomOffset = 0x0010;
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
    /// <returns>The packed SNES BG tilemap word for the glyph.</returns>
    public static ushort CompileGlyph(char character, EndingTextStyle style, bool bottom = false)
    {
        if (character == ' ')
        {
            return style switch
            {
                EndingTextStyle.ResultSmall => ResultBlankWord,
                EndingTextStyle.CopyrightLarge => LargeBlankWord,
                EndingTextStyle.PercentageSmall => PercentageBlankWord,
                EndingTextStyle.FinalLarge => InstalledLargeBlankWord,
                _ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
            };
        }

        if (style == EndingTextStyle.CopyrightLarge && character is >= '0' and <= '9')
            return unchecked((ushort)(CopyrightDigitTopBase + character - '0' +
                (bottom ? LargeBottomOffset : 0)));
        if (character is not (>= 'A' and <= 'Z'))
            throw new ArgumentOutOfRangeException(nameof(character), character,
                "Ending text supports spaces and A-Z; the copyright year also supports digits.");
        int letter = character - 'A';
        return style switch
        {
            EndingTextStyle.ResultSmall => unchecked((ushort)(ResultLetterBase + letter)),
            EndingTextStyle.PercentageSmall => unchecked((ushort)(PercentageLetterBase + letter)),
            EndingTextStyle.CopyrightLarge => CompileLarge(letter, 0, bottom),
            EndingTextStyle.FinalLarge => CompileLarge(letter, FinalLetterAttributes, bottom),
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
        };
    }

    /// <summary>Decodes a supported ending-text tilemap word into its editable character.</summary>
    /// <param name="word">The packed SNES BG tilemap word.</param>
    /// <param name="style">The text style that owns the word.</param>
    /// <param name="bottom">Whether the supplied word is a large glyph's bottom half.</param>
    /// <returns>The decoded space, letter, or copyright digit.</returns>
    public static char DecodeGlyph(ushort word, EndingTextStyle style, bool bottom = false)
    {
        ushort blank = CompileGlyph(' ', style, bottom);
        if (word == blank) return ' ';
        if (style == EndingTextStyle.CopyrightLarge)
        {
            int digit = word - CopyrightDigitTopBase - (bottom ? LargeBottomOffset : 0);
            if ((uint)digit <= 9) return (char)('0' + digit);
        }
        int letter;
        if (style == EndingTextStyle.ResultSmall)
            letter = word - ResultLetterBase;
        else if (style == EndingTextStyle.PercentageSmall)
            letter = word - PercentageLetterBase;
        else
        {
            int tile = word - (style == EndingTextStyle.FinalLarge ? FinalLetterAttributes : 0)
                - (bottom ? LargeBottomOffset : 0);
            int first = tile - LargeFirstGroupTopBase;
            int second = tile - LargeSecondGroupTopBase;
            letter = (uint)first < LargeSecondGroupFirstLetter ? first :
                (uint)second < 26 - LargeSecondGroupFirstLetter ? second + LargeSecondGroupFirstLetter : -1;
        }
        if ((uint)letter < 26) return (char)('A' + letter);
        throw new InvalidDataException(
            $"Ending {style} tile word ${word:X4} has no safe UTF-8 mapping.");
    }

    /// <summary>Maps A-Z into the split large-font tile ranges and optionally selects the bottom half.</summary>
    private static ushort CompileLarge(int letter, ushort attributes, bool bottom)
    {
        int tile = letter < LargeSecondGroupFirstLetter
            ? LargeFirstGroupTopBase + letter
            : LargeSecondGroupTopBase + letter - LargeSecondGroupFirstLetter;
        if (bottom) tile += LargeBottomOffset;
        return unchecked((ushort)(attributes | tile));
    }

    /// <summary>Height in tile cells of a small native glyph.</summary>
        public const byte SmallGlyphHeight = 1;
    /// <summary>Height in tile cells of a large native glyph.</summary>
        public const byte LargeGlyphHeight = 2;
}
