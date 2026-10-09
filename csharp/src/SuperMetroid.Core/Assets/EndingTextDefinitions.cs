namespace SuperMetroid.Core.Assets;

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

    /// <summary>Gets the centered region containing the result's producer line.</summary>
    public static EndingTextRegionDefinition ResultProducedBy => Centered(0, 11, EndingTextStyle.ResultSmall);
    /// <summary>Gets the centered region containing the item-percentage heading.</summary>
    public static EndingTextRegionDefinition PercentageHeading => Centered(10, 13, EndingTextStyle.PercentageSmall);
    /// <summary>Gets the centered region containing the item-percentage detail line.</summary>
    public static EndingTextRegionDefinition PercentageDetail => Centered(12, 19, EndingTextStyle.PercentageSmall);
    /// <summary>Gets the centered region containing the final large message.</summary>
    public static EndingTextRegionDefinition FinalMessage => Centered(2, 20, EndingTextStyle.FinalLarge);

    /// <summary>Copyright layout uses four year columns, eight company columns, and a two-column gap.</summary>
    private const int CopyrightYearWidth = 4, CopyrightCompanyWidth = 8, CopyrightGap = 2;

    /// <summary>Centered starting column shared by the year and company regions in the copyright panel.</summary>
    private static int CopyrightStart => CenteredColumn(CopyrightYearWidth + CopyrightGap + CopyrightCompanyWidth);
    /// <summary>Gets the large-glyph region containing the copyright year.</summary>
    public static EndingTextRegionDefinition CopyrightYear => new(0, CopyrightStart, CopyrightYearWidth, EndingTextStyle.CopyrightLarge);
    /// <summary>Gets the large-glyph region containing the copyright company name.</summary>
    public static EndingTextRegionDefinition CopyrightCompany => new(0, CopyrightStart + CopyrightYearWidth + CopyrightGap,
        CopyrightCompanyWidth, EndingTextStyle.CopyrightLarge);

    // Odd spare columns leave the extra cell on the right, matching native tilemaps.
    /// <summary>Chooses the left column for a centered field, assigning an odd spare column to the right.</summary>
    /// <param name="width">Field width in tile cells.</param>
    /// <returns>The zero-based starting column within the 32-cell tilemap.</returns>
    private static int CenteredColumn(int width) => (TilemapWidth - width) / 2;

    /// <summary>Builds a centered text region with the requested row, width, and glyph style.</summary>
    /// <param name="row">Zero-based destination tile row.</param>
    /// <param name="width">Region width in glyph cells.</param>
    /// <param name="style">Glyph layout and tile attributes used in the region.</param>
    /// <returns>The region's row, centered column, width, and style.</returns>
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

    /// <summary>Maps an uppercase letter to the split large-font tile groups and applies its row and attribute bits.</summary>
    /// <param name="letter">Zero-based uppercase alphabet index from 0 through 25.</param>
    /// <param name="attributes">Tilemap attribute bits applied to the selected tile.</param>
    /// <param name="bottom">Whether to select the glyph's lower tile row.</param>
    /// <returns>The packed tilemap word for the selected half of the large glyph.</returns>
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

/// <summary>Supported native font layouts and tile attributes for ending text.</summary>
public enum EndingTextStyle : byte
{
    /// <summary>Single-tile alphabet used by the producer/result panel.</summary>
    ResultSmall,
    /// <summary>Two-row alphabet and digits used by the copyright panel.</summary>
    CopyrightLarge,
    /// <summary>Single-tile attributed alphabet used by the percentage sequence.</summary>
    PercentageSmall,
    /// <summary>Two-row attributed alphabet used by the final message.</summary>
    FinalLarge,
}

/// <summary>Editable native typewriter sequences shown after the credits.</summary>
public enum EndingTextSequence : byte
{
    /// <summary>The item-collection percentage sequence.</summary>
    ItemPercentage,
    /// <summary>The final mission message sequence.</summary>
    FinalMessage,
}

/// <summary>Rectangular placement and font style of an ending-text field.</summary>
/// <param name="Row">Zero-based destination tile row.</param>
/// <param name="Column">Zero-based destination tile column.</param>
/// <param name="Width">Field width in glyphs.</param>
/// <param name="Style">Font layout and attributes used by the field.</param>
public readonly record struct EndingTextRegionDefinition(
    int Row, int Column, int Width, EndingTextStyle Style);

/// <summary>Compiled placement and tile words for one ending-text character.</summary>
/// <param name="Row">Zero-based destination tile row.</param>
/// <param name="Column">Zero-based destination tile column.</param>
/// <param name="TopWord">Tilemap word for the glyph's top or only tile.</param>
/// <param name="BottomWord">Optional tilemap word for the glyph's bottom half.</param>
public readonly record struct EndingTextCharacter(
    int Row, int Column, ushort TopWord, ushort? BottomWord);
