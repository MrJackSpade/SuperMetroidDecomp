using System.Collections.Frozen;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using static SuperMetroid.AssetExtraction.Frontend.Intro.IntroNarrationGlyphTileDefinitions;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// The tiles the cartridge types the opening narration's characters with (#1467): the import decodes the cartridge's
/// glyph records through them and paints each letter's pictures from them.
/// </summary>
public static class IntroNarrationGlyphTiles
{
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
