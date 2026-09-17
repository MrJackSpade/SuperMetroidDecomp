namespace SuperMetroid.Core.Assets;

/// <summary>Schema, layout, and native extraction identities for post-credit text.</summary>
public static class EndingTextDefinitions
{
    public const int Version = 1;
    public const string FileName = "ending-text.json";
    public const int TilemapWidth = 32;
    public const int ResultPanelRows = 9;
    public const int CopyrightRows = 2;
    public const int JapaneseSubtitleRows = 2;
    public const ushort InitialDelayFrames = 64;
    public const ushort CharacterDelayFrames = 4;
    public const ushort PercentageHoldFrames = 128;
    public const ushort ResultBlankWord = 0x004f;
    public const ushort LargeBlankWord = 0x007f;
    public const ushort InstalledLargeBlankWord = 0x207f;
    public const ushort PercentageBlankWord = 0x207f;
    public const ushort PercentageLetterBase = 0x3c00;
    public const ushort FinalLetterAttributes = 0x2000;
    public const ushort CopyrightDigitTopBase = 0x0060;
    public const ushort ResultLetterBase = 0x0000;
    public const ushort LargeFirstGroupTopBase = 0x0020;
    public const ushort LargeSecondGroupTopBase = 0x0040;
    public const ushort LargeBottomOffset = 0x0010;
    public const int LargeSecondGroupFirstLetter = 16;

    public static readonly EndingTextRegionDefinition ResultProducedBy =
        new(0, 10, 11, EndingTextStyle.ResultSmall);
    public static readonly EndingTextRegionDefinition CopyrightYear =
        new(0, 9, 4, EndingTextStyle.CopyrightLarge);
    public static readonly EndingTextRegionDefinition CopyrightCompany =
        new(0, 15, 8, EndingTextStyle.CopyrightLarge);
    public static readonly EndingTextRegionDefinition PercentageHeading =
        new(10, 9, 13, EndingTextStyle.PercentageSmall);
    public static readonly EndingTextRegionDefinition PercentageDetail =
        new(12, 6, 19, EndingTextStyle.PercentageSmall);
    public static readonly EndingTextRegionDefinition FinalMessage =
        new(2, 6, 20, EndingTextStyle.FinalLarge);

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

    public static char DecodeGlyph(ushort word, EndingTextStyle style, bool bottom = false)
    {
        ushort blank = CompileGlyph(' ', style, bottom);
        if (word == blank) return ' ';
        if (style == EndingTextStyle.CopyrightLarge)
        {
            int digit = word - CopyrightDigitTopBase - (bottom ? LargeBottomOffset : 0);
            if ((uint)digit <= 9) return (char)('0' + digit);
        }
        for (char character = 'A'; character <= 'Z'; character++)
            if (word == CompileGlyph(character, style, bottom)) return character;
        throw new InvalidDataException(
            $"Ending {style} tile word ${word:X4} has no safe UTF-8 mapping.");
    }

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
        public const byte GlyphWidth = 1;
        public const byte SmallGlyphHeight = 1;
        public const byte LargeGlyphHeight = 2;
        public const ushort CommandBit = 0x8000;
        public const int MaximumRecords = 128;
    }
}

public enum EndingTextStyle : byte
{
    ResultSmall,
    CopyrightLarge,
    PercentageSmall,
    FinalLarge,
}

public enum EndingTextSequence : byte
{
    ItemPercentage,
    FinalMessage,
}

public readonly record struct EndingTextRegionDefinition(
    int Row, int Column, int Width, EndingTextStyle Style);

public readonly record struct EndingTextCharacter(
    int Row, int Column, ushort TopWord, ushort? BottomWord);
