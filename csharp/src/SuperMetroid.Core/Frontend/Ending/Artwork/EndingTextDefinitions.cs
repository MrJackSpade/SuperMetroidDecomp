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

    // Odd spare columns leave the extra cell on the right, matching native tilemaps.
    /// <summary>Centers a field within 32 cells, leaving an odd spare column on the right.</summary>
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

    /// <summary>Native bank-$8C identifiers used only by extraction and parity tests.</summary>
    public static class Native
    {
        /// <summary>Bank containing every post-credit text resource in this catalog.</summary>
        public const byte Bank = 0x8c;
        /// <summary>$8C:DC9B, nine-row producer/result panel.</summary>
        public const ushort ResultPanel = 0xdc9b;
        /// <summary>$8C:DEDB, two-row 1994 Nintendo panel.</summary>
        public const ushort CopyrightPanel = 0xdedb;
        /// <summary>$8C:DF5B, two-row alternate-language percentage subtitle.</summary>
        public const ushort JapaneseSubtitle = 0xdf5b;
        /// <summary>$8C:DFDB, item-percentage typewriter stream.</summary>
        public const ushort ItemPercentage = 0xdfdb;
        /// <summary>$8C:E0AF, final-message typewriter stream.</summary>
        public const ushort FinalMessage = 0xe0af;
        /// <summary>$8B:E627, draws the dynamic percentage and optional subtitle.</summary>
        public const ushort DrawPercentageOpcode = 0xe627;
        /// <summary>$8B:E769, installs the alternate-language percentage subtitle.</summary>
        public const ushort DrawSubtitleOpcode = 0xe769;
        /// <summary>$8B:E780, clears the subtitle and requests the ending scroll.</summary>
        public const ushort ClearSubtitleOpcode = 0xe780;
        /// <summary>$8B:9698, deletes the cinematic background object.</summary>
        public const ushort DeleteOpcode = 0x9698;
        /// <summary>$8B:88B7, draws a bounded ending-text rectangle.</summary>
        public const ushort DrawFunction = 0x88b7;
        /// <summary>$8B:8849, no-op marker payload.</summary>
        public const ushort DoNothingFunction = 0x8849;
        /// <summary>$8C:E12F, shared no-op marker record.</summary>
        public const ushort MarkerData = 0xe12f;
        /// <summary>Packed origin (0,0) used by both no-op marker records.</summary>
        public const ushort MarkerPackedPosition = 0x0000;
        /// <summary>Low-byte X component of packed ending-text positions.</summary>
        public const ushort PackedPositionXMask = 0x00ff;
        /// <summary>Width in tile cells of every native ending-text glyph.</summary>
        public const byte GlyphWidth = 1;
        /// <summary>Height in tile cells of a small native glyph.</summary>
        public const byte SmallGlyphHeight = 1;
        /// <summary>Height in tile cells of a large native glyph.</summary>
        public const byte LargeGlyphHeight = 2;
        /// <summary>High bit distinguishing an instruction word from a frame delay.</summary>
        public const ushort CommandBit = 0x8000;
        /// <summary>Safety limit on records decoded from one native text stream.</summary>
        public const int MaximumRecords = 128;
    }
}
