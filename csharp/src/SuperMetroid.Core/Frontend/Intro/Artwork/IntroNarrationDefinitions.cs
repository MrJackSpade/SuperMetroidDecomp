namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Schema, glyph encoding, and fixed layout limits for opening narration.</summary>
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
    /// <summary>Blank-space BG word $002F from <c>IndirectInstructions_IntroText_Space</c> at <c>$8C:D67D</c>, without the nonblank glyphs' priority bit.</summary>
    private const ushort BlankWord = 0x002f;
    /// <summary>High-priority period BG word $2024 from <c>IndirectInstructions_IntroText_Period</c> at <c>$8C:D75D</c>, distinct from the unused decimal-point glyph.</summary>
    private const ushort PeriodWord = 0x2024;
    /// <summary>High-priority comma BG word $2025 from <c>IndirectInstructions_IntroText_Comma</c> at <c>$8C:D763</c>.</summary>
    private const ushort CommaWord = 0x2025;
    /// <summary>High-priority apostrophe BG word $2027 from <c>IndirectInstructions_IntroText_Apostrophe</c> at <c>$8C:D76F</c>.</summary>
    private const ushort ApostropheWord = 0x2027;
    /// <summary>High-priority exclamation BG word $202A from <c>IndirectInstructions_IntroText_ExclamationPoint</c> at <c>$8C:D77B</c>.</summary>
    private const ushort ExclamationWord = 0x202a;

    /// <summary>Compiles one supported narration character into its exact native BG tilemap word, preserving glyph priority bits.</summary>
    /// <param name="character">Space, uppercase A-Z, digit 0-9, period, comma, straight apostrophe, or exclamation mark; no case or punctuation normalization is performed.</param>
    /// <returns>The corresponding complete sixteen-bit tilemap word, not a Unicode code point or glyph-record pointer.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="character"/> is outside the supported narration alphabet.</exception>
    public static ushort CompileGlyph(char character) => character switch
    {
        ' ' => BlankWord,
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
        >= UppercaseAWord and <= UppercaseAWord + 25 =>
            (char)('A' + word - UppercaseAWord),
        >= DigitZeroWord and <= DigitZeroWord + 9 =>
            (char)('0' + word - DigitZeroWord),
        BlankWord => ' ',
        PeriodWord => '.',
        CommaWord => ',',
        ApostropheWord => '\'',
        ExclamationWord => '!',
        _ => throw new InvalidDataException($"${word:X4} is not an opening-narration glyph word."),
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
