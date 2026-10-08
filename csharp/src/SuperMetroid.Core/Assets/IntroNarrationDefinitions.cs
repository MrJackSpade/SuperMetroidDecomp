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
    /// <summary>Blank-space BG word $002F from <c>IndirectInstructions_IntroText_Space</c> at <c>$8C:D67D</c>, without the nonblank glyphs' priority bit.</summary>
    public const ushort BlankCharacterWord = 0x002f;
    /// <summary>BG word $2000 for uppercase A, from <c>IndirectInstructions_IntroText_A</c> at <c>$8C:D685</c>; A-Z use consecutive tile indices with high BG priority.</summary>
    public const ushort UppercaseAWord = 0x2000;
    /// <summary>BG word $201A for digit zero, from the glyph record at <c>$8C:D721</c>; zero through nine use consecutive high-priority tile indices.</summary>
    public const ushort DigitZeroWord = 0x201a;
    /// <summary>High-priority period BG word $2024 from <c>IndirectInstructions_IntroText_Period</c> at <c>$8C:D75D</c>, distinct from the unused decimal-point glyph.</summary>
    public const ushort PeriodWord = 0x2024;
    /// <summary>High-priority comma BG word $2025 from <c>IndirectInstructions_IntroText_Comma</c> at <c>$8C:D763</c>.</summary>
    public const ushort CommaWord = 0x2025;
    /// <summary>High-priority apostrophe BG word $2027 from <c>IndirectInstructions_IntroText_Apostrophe</c> at <c>$8C:D76F</c>.</summary>
    public const ushort ApostropheWord = 0x2027;
    /// <summary>High-priority exclamation BG word $202A from <c>IndirectInstructions_IntroText_ExclamationPoint</c> at <c>$8C:D77B</c>.</summary>
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

    /// <summary>Decodes one exact native narration tilemap word to its editable text character.</summary>
    /// <param name="word">Complete tilemap word; palette, priority, or flip variations outside the canonical glyph words are not accepted.</param>
    /// <returns>The supported space, uppercase letter, digit, or punctuation character represented by the word.</returns>
    /// <exception cref="InvalidDataException"><paramref name="word"/> has no supported, exact narration glyph mapping.</exception>
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

/// <summary>One named English narration page exposed to presentation content.</summary>
public enum IntroNarrationPageId : byte
{
    /// <summary>First story page, recalling the Zebes battle and Mother Brain's plans; native <c>CinematicBGObjectInstLists_IntroTextPage1</c> at <c>$8C:C383</c>.</summary>
    Page1 = 1,
    /// <summary>Second story page, recalling SR388 and the surviving larva; native <c>CinematicBGObjectInstLists_IntroTextPage2</c> at <c>$8C:C797</c>.</summary>
    Page2,
    /// <summary>Third story page, describing the larva's delivery to Ceres for research; native <c>CinematicBGObjectInstLists_IntroTextPage3</c> at <c>$8C:CB45</c>.</summary>
    Page3,
    /// <summary>Fourth story page, describing the scientists' findings about Metroid energy; native <c>CinematicBGObjectInstLists_IntroTextPage4</c> at <c>$8C:CE33</c>.</summary>
    Page4,
    /// <summary>Fifth story page, describing Samus's departure and receipt of a distress signal; native <c>CinematicBGObjectInstLists_IntroTextPage5</c> at <c>$8C:D15D</c>.</summary>
    Page5,
    /// <summary>Final story page, announcing the attack on Ceres and using a timed final hold instead of the earlier pages' input wait; native <c>CinematicBGObjectInstLists_IntroTextPage6</c> at <c>$8C:D511</c>.</summary>
    Page6,
}

/// <summary>Native bank-$8C stream boundaries used only by extraction and parity tests.</summary>
/// <param name="Id">Named story-page identity, independent of the editable wording.</param>
/// <param name="InstructionPointer">Sixteen-bit offset of the stock background-object list in bank $8C, not a full SNES address.</param>
/// <param name="BeginOpcode">Bank-$8B callback offset expected at the list's start, establishing that page's subtitle state.</param>
/// <param name="FinishOpcode">Bank-$8B callback offset expected before deletion, establishing the next wait or scene transition.</param>
public readonly record struct IntroNarrationNativePage(
    IntroNarrationPageId Id,
    ushort InstructionPointer,
    ushort BeginOpcode,
    ushort FinishOpcode);
