using static SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingTextDefinitions;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// Encodes and decodes the ending text's glyphs (<see cref="EndingTextDefinitions"/>) in each text style.
/// </summary>
public static class EndingTextGlyphs
{
    /// <summary>Font3's small alphabet occupies tiles00..19. Large tops occupy20..2F
    /// and40..49 with bottoms16 tiles later; copyright digits occupy60..69/70..79.
    /// Native8C ending text records supply each style's attributes and blank identity.</summary>
    /// <param name="character">The supported character to encode.</param>
    /// <param name="style">The destination text style.</param>
    /// <param name="bottom">Whether to encode the bottom half of a large glyph.</param>
    /// <returns>The tile that shows the glyph.</returns>
    public static BackgroundTile CompileGlyph(char character, EndingTextStyle style, bool bottom = false)
    {
        if (character == ' ')
        {
            return style switch
            {
                EndingTextStyle.ResultSmall => ResultBlankTile,
                EndingTextStyle.CopyrightLarge => LargeBlankTile,
                EndingTextStyle.PercentageSmall => PercentageBlankTile,
                EndingTextStyle.FinalLarge => InstalledLargeBlankTile,
                _ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
            };
        }

        if (style == EndingTextStyle.CopyrightLarge && character is >= '0' and <= '9')
            return new(CopyrightDigitTopBase + character - '0' + (bottom ? LargeBottomOffset : 0), 0, highPriority: false);
        if (character is not (>= 'A' and <= 'Z'))
            throw new ArgumentOutOfRangeException(nameof(character), character,
                "Ending text supports spaces and A-Z; the copyright year also supports digits.");
        int letter = character - 'A';
        return style switch
        {
            EndingTextStyle.ResultSmall => new(ResultLetterBase + letter, 0, highPriority: false),
            EndingTextStyle.PercentageSmall => new(letter, PercentageLetterColourVariant, highPriority: true),
            EndingTextStyle.CopyrightLarge => new(LargeTile(letter, bottom), 0, highPriority: false),
            EndingTextStyle.FinalLarge => new(LargeTile(letter, bottom), 0, highPriority: true),
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
        };
    }

    /// <summary>
    /// Decodes a supported ending-text tile into its editable character: the one character of <paramref name="style"/>
    /// whose <see cref="CompileGlyph"/> tile is exactly <paramref name="cell"/>.
    /// </summary>
    /// <param name="cell">The tile shown.</param>
    /// <param name="style">The text style that owns the tile.</param>
    /// <param name="bottom">Whether the supplied tile is a large glyph's bottom half.</param>
    /// <returns>The decoded space, letter, or copyright digit.</returns>
    public static char DecodeGlyph(BackgroundTile cell, EndingTextStyle style, bool bottom = false)
    {
        if (cell == CompileGlyph(' ', style, bottom)) return ' ';
        if (style == EndingTextStyle.CopyrightLarge)
        {
            for (char digit = '0'; digit <= '9'; digit++)
                if (cell == CompileGlyph(digit, style, bottom)) return digit;
        }
        for (char letter = 'A'; letter <= 'Z'; letter++)
            if (cell == CompileGlyph(letter, style, bottom)) return letter;
        throw new InvalidDataException(
            $"Ending {style} tile {cell} has no safe UTF-8 mapping.");
    }

    /// <summary>Maps A-Z into the split large-font tile ranges and optionally selects the bottom half.</summary>
    private static int LargeTile(int letter, bool bottom)
    {
        int tile = letter < LargeSecondGroupFirstLetter
            ? LargeFirstGroupTopBase + letter
            : LargeSecondGroupTopBase + letter - LargeSecondGroupFirstLetter;
        return bottom ? tile + LargeBottomOffset : tile;
    }
}
