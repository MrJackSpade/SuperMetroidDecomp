using SuperMetroid.Core.Assets;
using static SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.CreditsPresentationDefinitions;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// Encodes and decodes the credits' Font-3 glyphs (<see cref="CreditsPresentationDefinitions"/>) in their small and large styles.
/// </summary>
public static class CreditsGlyphs
{
    /// <summary>The Font-3 tile of one supported credits glyph, in colour variant zero before the line's variant is applied.</summary>
    /// <param name="character">Space or uppercase A-Z; large text additionally accepts ampersand and period. No case conversion or punctuation substitution is performed.</param>
    /// <param name="style">Small heading or large name/credit style.</param>
    /// <param name="bottom">Selects the lower tile row of a large glyph; nonblank small glyphs require false.</param>
    /// <returns>The character's tile, or <see cref="BlankTile"/> for a space; no colour variant, priority, or mirroring is added.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The nonblank character, style, or requested glyph half has no supported credits encoding.</exception>
    public static BackgroundTile CompileGlyph(char character, CreditsLineStyle style,
        bool bottom = false) => new CreditsGlyphKey(style, character, bottom) switch
    {
        (_, ' ', _) => BlankTile,
        (CreditsLineStyle.Small, >= 'A' and <= 'Z', false) => Plain(character - 'A'),
        (CreditsLineStyle.Large, '&', false) => Plain(LargeAmpersandTop),
        (CreditsLineStyle.Large, '&', true) => Plain(LargeAmpersandBottom),
        (CreditsLineStyle.Large, '.', false) => Plain(LargePeriodTop),
        (CreditsLineStyle.Large, '.', true) => Plain(LargePeriodBottom),
        (CreditsLineStyle.Large, >= 'A' and <= 'Z', _) =>
            EndingTextGlyphs.CompileGlyph(
                character, EndingTextStyle.CopyrightLarge, bottom),
        _ => throw new ArgumentOutOfRangeException(nameof(character), character,
            style == CreditsLineStyle.Small
                ? "Small credits text supports spaces and A-Z."
                : "Large credits text supports spaces, A-Z, ampersand, and period."),
    };

    /// <summary>Decodes the Font-3 tile a credits cell shows, ignoring its colour variant, priority and mirroring.</summary>
    /// <param name="cell">The placed tile.</param>
    /// <param name="style">The one-row small or two-row large font layout owning the selector.</param>
    /// <param name="bottom">Whether the selector belongs to the lower row of a large glyph; nonblank small glyphs require false.</param>
    /// <returns>The represented glyph; large decoding also recognizes Font-3 copyright digits through the shared ending-text decoder, although authored credits compilation excludes digits.</returns>
    /// <exception cref="InvalidDataException">The selector does not have a supported mapping for the requested style and glyph half.</exception>
    public static char DecodeGlyph(BackgroundTile cell, CreditsLineStyle style,
        bool bottom = false)
    {
        int tile = cell.Tile;
        if (tile == BlankTile.Tile)
            return ' ';
        if (style == CreditsLineStyle.Small && !bottom && tile <= 25)
            return (char)('A' + tile);
        if (style == CreditsLineStyle.Large)
        {
            if (tile == (bottom ? LargeAmpersandBottom : LargeAmpersandTop))
                return '&';
            if (tile == (bottom ? LargePeriodBottom : LargePeriodTop))
                return '.';
            return EndingTextGlyphs.DecodeGlyph(
                Plain(tile), EndingTextStyle.CopyrightLarge, bottom);
        }
        throw new InvalidDataException(
            $"Credits {style} tile {cell} has no safe UTF-8 mapping.");
    }

    /// <summary>Font-3 tile <paramref name="tile"/> in colour variant zero, the lower priority tier and unmirrored.</summary>
    private static BackgroundTile Plain(int tile) => new(tile, 0, highPriority: false);
}
