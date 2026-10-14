using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Schema, native source identities, and fixed layout limits for opening narration.</summary>
public static class IntroNarrationDefinitions
{
    /// <summary>Required schema revision of the six-page editable English narration document.</summary>
    public const int Version = 1;
    /// <summary>JSON presentation-resource filename containing named pages and their ordered UTF-8 text lines.</summary>
    public const string FileName = "intro-narration.json";
    /// <summary>Zero-based BG tilemap column at which every narration line begins: column one.</summary>
    public const int FirstColumn = 1;
    /// <summary>Maximum glyph count per line, 29, occupying tile columns one through 29 including spaces.</summary>
    public const int MaximumColumns = 29;
    /// <summary>First permitted text row in BG tile cells, four; presentation lines use even rows with two-row spacing.</summary>
    public const int FirstTextRow = 4;
    /// <summary>Last permitted text row in BG tile cells, sixteen, inclusive.</summary>
    public const int LastTextRow = 16;
    /// <summary>Five cinematic background-object updates between character records, including spaces; independent of hardware lag or host elapsed time.</summary>
    public const ushort CharacterDelayFrames = 5;
    /// <summary>One-update duration of the no-op marker at the beginning of each native page stream.</summary>
    public const ushort InitialMarkerDelayFrames = 1;
    /// <summary>128 cinematic background-object updates held after the final page starts caret blinking and before its finish callback.</summary>
    public const ushort FinalPageHoldFrames = 128;
    /// <summary>BG word $2000 for uppercase A, from <c>IndirectInstructions_IntroText_A</c> at <c>$8C:D685</c>; A-Z use consecutive tile indices with high BG priority.</summary>
    public const ushort UppercaseAWord = 0x2000;
    /// <summary>BG word $201A for digit zero, from the glyph record at <c>$8C:D721</c>; zero through nine use consecutive high-priority tile indices.</summary>
    public const ushort DigitZeroWord = 0x201a;

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

    /// <summary>Native source and callback boundaries for one mutually exclusive narration page.</summary>
    /// <param name="page">One of the six defined English story-page identities.</param>
    /// <returns>The bank-relative stock instruction pointer and expected begin/finish callback opcodes used by extraction.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="page"/> is not a defined narration page.</exception>
    public static IntroNarrationNativePage NativePage(IntroNarrationPageId page) => page switch
    {
        IntroNarrationPageId.Page1 => Page1Native,
        IntroNarrationPageId.Page2 => Page2Native,
        IntroNarrationPageId.Page3 => Page3Native,
        IntroNarrationPageId.Page4 => Page4Native,
        IntroNarrationPageId.Page5 => Page5Native,
        IntroNarrationPageId.Page6 => Page6Native,
        _ => throw new ArgumentOutOfRangeException(nameof(page), page, "Unknown narration page."),
    };

    /// <summary>Enumerates the six stock stream descriptors in narrative order, page one through page six.</summary>
    public static IEnumerable<IntroNarrationNativePage> Pages
    {
        get
        {
            for (int page = (int)IntroNarrationPageId.Page1; page <= (int)IntroNarrationPageId.Page6; page++)
                yield return NativePage((IntroNarrationPageId)page);
        }
    }
    /// <summary>Compiles one supported narration character into its exact native BG tilemap word, preserving glyph priority bits.</summary>
    /// <param name="character">Space, uppercase A-Z, digit 0-9, period, comma, straight apostrophe, or exclamation mark; no case or punctuation normalization is performed.</param>
    /// <returns>The corresponding complete sixteen-bit tilemap word, not a Unicode code point or glyph-record pointer.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="character"/> is outside the supported narration alphabet.</exception>
    public static ushort CompileGlyph(char character) => character switch
    {
        ' ' => (ushort)IntroNarrationSymbolWord.Blank,
        >= 'A' and <= 'Z' => unchecked((ushort)(UppercaseAWord + character - 'A')),
        >= '0' and <= '9' => unchecked((ushort)(DigitZeroWord + character - '0')),
        '.' => (ushort)IntroNarrationSymbolWord.Period,
        ',' => (ushort)IntroNarrationSymbolWord.Comma,
        '\'' => (ushort)IntroNarrationSymbolWord.Apostrophe,
        '!' => (ushort)IntroNarrationSymbolWord.Exclamation,
        _ => throw new ArgumentOutOfRangeException(nameof(character), character,
            "Opening narration supports space, A-Z, 0-9, period, comma, apostrophe and exclamation mark."),
    };

    /// <summary>Decodes one exact native narration tilemap word to its editable text character.</summary>
    /// <param name="word">Complete tilemap word; palette, priority, or flip variations outside the canonical glyph words are not accepted.</param>
    /// <returns>The supported space, uppercase letter, digit, or punctuation character represented by the word.</returns>
    /// <exception cref="InvalidDataException"><paramref name="word"/> has no supported, exact narration glyph mapping.</exception>
    public static char DecodeGlyph(ushort word) => word switch
    {
        >= UppercaseAWord and <= UppercaseAWord + 25 =>
            (char)('A' + word - UppercaseAWord),
        >= DigitZeroWord and <= DigitZeroWord + 9 =>
            (char)('0' + word - DigitZeroWord),
        _ => ClosedNativeWords.Decode<IntroNarrationSymbolWord>(word, "opening-narration glyph word") switch
        {
            IntroNarrationSymbolWord.Blank => ' ',
            IntroNarrationSymbolWord.Period => '.',
            IntroNarrationSymbolWord.Comma => ',',
            IntroNarrationSymbolWord.Apostrophe => '\'',
            IntroNarrationSymbolWord.Exclamation => '!',
            _ => throw new InvalidOperationException($"Undefined narration symbol ${word:X4}."),
        },
    };

    /// <summary>Checks whether a character can be compiled without changing case or substituting punctuation.</summary>
    /// <param name="character">The UTF-16 character from an editable narration line.</param>
    /// <returns>True only for the space, A-Z, 0-9, period, comma, straight-apostrophe, and exclamation alphabet accepted by <see cref="CompileGlyph"/>.</returns>
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
