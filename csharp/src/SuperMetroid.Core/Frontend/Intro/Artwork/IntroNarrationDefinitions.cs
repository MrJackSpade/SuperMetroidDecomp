using System.Collections.Frozen;
using SuperMetroid.Core.Assets;

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
    /// <summary>Tile of uppercase A, from <c>IndirectInstructions_IntroText_A</c> at <c>$8C:D685</c>; A-Z use consecutive tiles in the upper priority tier.</summary>
    public const int UppercaseATile = 0x000;
    /// <summary>Tile of digit zero, from the glyph record at <c>$8C:D721</c>; zero through nine use consecutive upper-priority tiles.</summary>
    public const int DigitZeroTile = 0x01a;
    /// <summary>Blank-space tile from <c>IndirectInstructions_IntroText_Space</c> at <c>$8C:D67D</c>, without the nonblank glyphs' priority.</summary>
    private const int BlankTile = 0x02f;
    /// <summary>Period tile from <c>IndirectInstructions_IntroText_Period</c> at <c>$8C:D75D</c>, distinct from the unused decimal-point glyph.</summary>
    private const int PeriodTile = 0x024;
    /// <summary>Comma tile from <c>IndirectInstructions_IntroText_Comma</c> at <c>$8C:D763</c>.</summary>
    private const int CommaTile = 0x025;
    /// <summary>Apostrophe tile from <c>IndirectInstructions_IntroText_Apostrophe</c> at <c>$8C:D76F</c>.</summary>
    private const int ApostropheTile = 0x027;
    /// <summary>Exclamation tile from <c>IndirectInstructions_IntroText_ExclamationPoint</c> at <c>$8C:D77B</c>.</summary>
    private const int ExclamationTile = 0x02a;

    /// <summary>Compiles one supported narration character into the exact tile that shows it, in the glyphs' priority tier.</summary>
    /// <param name="character">Space, uppercase A-Z, digit 0-9, period, comma, straight apostrophe, or exclamation mark; no case or punctuation normalization is performed.</param>
    /// <returns>The tile that shows the character, not a Unicode code point or glyph-record pointer.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="character"/> is outside the supported narration alphabet.</exception>
    public static BackgroundTile CompileGlyph(char character) => character switch
    {
        ' ' => new(BlankTile, 0, highPriority: false),
        >= 'A' and <= 'Z' => Glyph(UppercaseATile + character - 'A'),
        >= '0' and <= '9' => Glyph(DigitZeroTile + character - '0'),
        '.' => Glyph(PeriodTile),
        ',' => Glyph(CommaTile),
        '\'' => Glyph(ApostropheTile),
        '!' => Glyph(ExclamationTile),
        _ => throw new ArgumentOutOfRangeException(nameof(character), character,
            "Opening narration supports space, A-Z, 0-9, period, comma, apostrophe and exclamation mark."),
    };

    /// <summary>Decodes one exact narration tile to its editable text character.</summary>
    /// <param name="cell">The placed tile; colour, priority, or mirroring variations outside the canonical glyph tiles are not accepted.</param>
    /// <returns>The supported space, uppercase letter, digit, or punctuation character the tile shows.</returns>
    /// <exception cref="InvalidDataException"><paramref name="cell"/> has no supported, exact narration glyph mapping.</exception>
    public static char DecodeGlyph(BackgroundTile cell) =>
        GlyphsByTile.TryGetValue(cell, out char character)
            ? character
            : throw new InvalidDataException($"{cell} is not an opening-narration glyph tile.");

    /// <summary>A nonblank glyph's tile: colour variant zero, upper priority tier, unmirrored.</summary>
    private static BackgroundTile Glyph(int tile) => new(tile, 0, highPriority: true);

    /// <summary>Checks whether a character can be compiled without changing case or substituting punctuation.</summary>
    /// <param name="character">The UTF-16 character from an editable narration line.</param>
    /// <returns>True only for the space, A-Z, 0-9, period, comma, straight-apostrophe, and exclamation alphabet accepted by <see cref="CompileGlyph"/>.</returns>
    public static bool IsSupportedGlyph(char character) => Alphabet.Contains(character, StringComparison.Ordinal);

    /// <summary>Every character the narration supports, which <see cref="CompileGlyph"/> accepts.</summary>
    private const string Alphabet = " ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.,'!";

    /// <summary>Each supported character keyed by the tile <see cref="CompileGlyph"/> gives it.</summary>
    private static readonly FrozenDictionary<BackgroundTile, char> GlyphsByTile =
        Alphabet.ToFrozenDictionary(CompileGlyph, character => character);
}
