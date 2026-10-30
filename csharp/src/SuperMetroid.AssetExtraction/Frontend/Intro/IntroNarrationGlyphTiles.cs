using System.Collections.Frozen;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// The tiles the cartridge types the opening narration's characters with (#1467): the import decodes the cartridge's
/// glyph records through them and paints each letter's pictures from them.
/// </summary>
public static class IntroNarrationGlyphTiles
{
    /// <summary>Tile of uppercase A, from <c>IndirectInstructions_IntroText_A</c> at <c>$8C:D685</c>; A-Z use consecutive tiles in the upper priority tier.</summary>
    private const int UppercaseATile = 0x000;
    /// <summary>Tile of digit zero, from the glyph record at <c>$8C:D721</c>; zero through nine use consecutive upper-priority tiles.</summary>
    private const int DigitZeroTile = 0x01a;
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

    /// <summary>Each supported character keyed by the tile <see cref="CompileGlyph"/> gives it.</summary>
    private static readonly FrozenDictionary<BackgroundTile, char> GlyphsByTile =
        IntroNarrationDefinitions.Alphabet.ToFrozenDictionary(CompileGlyph, character => character);
}
