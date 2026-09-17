namespace SuperMetroid.Core.Assets;

/// <summary>Schema, native source identities, and fixed layout limits for opening narration.</summary>
public static class IntroNarrationDefinitions
{
    public const int Version = 1;
    public const string FileName = "intro-narration.json";
    public const int FirstColumn = 1;
    public const int MaximumColumns = 29;
    public const int FirstTextRow = 4;
    public const int LastTextRow = 16;
    public const ushort CharacterDelayFrames = 5;
    public const ushort InitialMarkerDelayFrames = 1;
    public const ushort FinalPageHoldFrames = 128;
    public const ushort BlankCharacterWord = 0x002f;
    public const ushort UppercaseAWord = 0x2000;
    public const ushort DigitZeroWord = 0x201a;
    public const ushort PeriodWord = 0x2024;
    public const ushort CommaWord = 0x2025;
    public const ushort ApostropheWord = 0x2027;
    public const ushort ExclamationWord = 0x202a;

    /// <summary>Native bank-$8C script identities consumed only while extracting stock content.</summary>
    public static class Native
    {
        /// <summary>Bank containing the intro background-object streams and glyph records.</summary>
        public const byte ScriptBank = 0x8c;
        /// <summary>Interpreter command-bit mask distinguishing callbacks from timed records.</summary>
        public const ushort InstructionCommandBit = 0x8000;
        /// <summary><c>CinematicBGObject_Instruction_Delete</c> at $8B:9698.</summary>
        public const ushort DeleteOpcode = 0x9698;
        /// <summary><c>Instruction_SetCaretToBlink</c> at $8B:ADD4.</summary>
        public const ushort SetCaretBlinkingOpcode = 0xadd4;
        /// <summary><c>IndirectInstructionFunction_DrawTextCharacter</c> at $8B:884D.</summary>
        public const ushort DrawCharacterFunction = 0x884d;
        /// <summary><c>IndirectInstructionFunction_DoNothing</c> at $8B:8849.</summary>
        public const ushort DoNothingFunction = 0x8849;
        /// <summary>$8C:D683, the no-op marker record shared by each page boundary.</summary>
        public const ushort MarkerDataPointer = 0xd683;
        /// <summary>Packed tile position (1,1) used by the no-op marker.</summary>
        public const ushort MarkerPackedPosition = 0x0101;
        /// <summary>One-column-by-one-row dimensions required by each narration glyph payload.</summary>
        public const ushort SingleTileDimensions = 0x0101;
        /// <summary>Low-byte X component of each packed narration tile position.</summary>
        public const ushort PackedPositionXMask = 0x00ff;
        /// <summary>Malformed-stream bound; retail's longest page is well below this count.</summary>
        public const int MaximumRecords = 512;
    }

    /// <summary>Page-one list $8C:C383, bounded by native callbacks $AE43/$AE5B.</summary>
    public static readonly IntroNarrationNativePage Page1Native =
        new(IntroNarrationPageId.Page1, 0xc383, 0xae43, 0xae5b);
    /// <summary>Page-two list $8C:C797, bounded by native callbacks $AE79/$AE91.</summary>
    public static readonly IntroNarrationNativePage Page2Native =
        new(IntroNarrationPageId.Page2, 0xc797, 0xae79, 0xae91);
    /// <summary>Page-three list $8C:CB45, bounded by native callbacks $B074/$B08C.</summary>
    public static readonly IntroNarrationNativePage Page3Native =
        new(IntroNarrationPageId.Page3, 0xcb45, 0xb074, 0xb08c);
    /// <summary>Page-four list $8C:CE33, bounded by native callbacks $B0B3/$B0CB.</summary>
    public static readonly IntroNarrationNativePage Page4Native =
        new(IntroNarrationPageId.Page4, 0xce33, 0xb0b3, 0xb0cb);
    /// <summary>Page-five list $8C:D15D, bounded by native callbacks $B19B/$B1B3.</summary>
    public static readonly IntroNarrationNativePage Page5Native =
        new(IntroNarrationPageId.Page5, 0xd15d, 0xb19b, 0xb1b3);
    /// <summary>Page-six list $8C:D511, bounded by native callbacks $B228/$B240.</summary>
    public static readonly IntroNarrationNativePage Page6Native =
        new(IntroNarrationPageId.Page6, 0xd511, 0xb228, 0xb240);

    private static readonly IntroNarrationNativePage[] NativePages =
    [
        Page1Native,
        Page2Native,
        Page3Native,
        Page4Native,
        Page5Native,
        Page6Native,
    ];

    public static ReadOnlySpan<IntroNarrationNativePage> Pages => NativePages;

    public static ushort CompileGlyph(char character) => character switch
    {
        ' ' => BlankCharacterWord,
        >= 'A' and <= 'Z' => unchecked((ushort)(UppercaseAWord + character - 'A')),
        >= '0' and <= '9' => unchecked((ushort)(DigitZeroWord + character - '0')),
        '.' => PeriodWord,
        ',' => CommaWord,
        '\'' => ApostropheWord,
        '!' => ExclamationWord,
        _ => throw new ArgumentOutOfRangeException(nameof(character), character,
            "Opening narration supports space, A-Z, 0-9, period, comma, apostrophe and exclamation mark."),
    };

    public static char DecodeGlyph(ushort word) => word switch
    {
        BlankCharacterWord => ' ',
        >= UppercaseAWord and <= UppercaseAWord + 25 =>
            (char)('A' + word - UppercaseAWord),
        >= DigitZeroWord and <= DigitZeroWord + 9 =>
            (char)('0' + word - DigitZeroWord),
        PeriodWord => '.',
        CommaWord => ',',
        ApostropheWord => '\'',
        ExclamationWord => '!',
        _ => throw new InvalidDataException(
            $"Opening-narration tile word ${word:X4} has no safe UTF-8 glyph mapping."),
    };

    public static bool IsSupportedGlyph(char character)
    {
        try
        {
            _ = CompileGlyph(character);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }
}

/// <summary>One named English narration page exposed to presentation content.</summary>
public enum IntroNarrationPageId : byte
{
    Page1 = 1,
    Page2,
    Page3,
    Page4,
    Page5,
    Page6,
}

/// <summary>Native bank-$8C stream boundaries used only by extraction and parity tests.</summary>
public readonly record struct IntroNarrationNativePage(
    IntroNarrationPageId Id,
    ushort InstructionPointer,
    ushort BeginOpcode,
    ushort FinishOpcode);
