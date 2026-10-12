namespace SuperMetroid.Core.Frontend;

/// <summary>English caption in native8C:D629..D66A: emit the non-space characters
/// of SPACE COLONY left-to-right, one eight-pixel cell each, starting at column10.
/// Glyph identities come from the one-cell drawing operations8C:D7C1..D7F6.
/// Tile words select the high-priority BG plane; no per-letter layout table.</summary>
internal static class SpaceColonyCaptionDefinitions
{
    private const string Text = "SPACE COLONY";
    internal static int LetterCount => Text.Length - 1;

    internal static (int Column, ushort Tile) Letter(int index)
    {
        if ((uint)index >= LetterCount) throw new ArgumentOutOfRangeException(nameof(index));
        int column = index + (index >= Text.IndexOf(' ') ? 1 : 0);
        return (10 + column, Glyph(Text[column]));
    }

    /// <summary>
    /// $8C:D66B-D671: with Japanese text the list draws <c>$8C:D7F7</c>, the katakana caption
    /// below SPACE COLONY, as an eight-by-two block at column $0C, row $1A.
    /// </summary>
    internal const int JapaneseColumn = 0x0c, JapaneseRow = 0x1a, JapaneseWidth = 8;

    /// <summary>$8C:D7FB: the Japanese caption's two rows of tilemap words.</summary>
    internal static ReadOnlySpan<ushort> JapaneseTiles =>
    [
        0x21ba, 0x21bb, 0x21b3, 0x21ba, 0x21bc, 0x20e0, 0x20e1, 0x21b3,
        0x20e2, 0x20e3, 0x21b4, 0x20e2, 0x20e4, 0x21a3, 0x21a4, 0x21b4,
    ];

    /// <summary>Native SPACE COLONY glyph identities, including high-priority bit2000.
    /// Repeated C and O characters select the same drawing operation.</summary>
    private static ushort Glyph(char character) => character switch
    {
        'S' => 0x21ed,
        'P' => 0x21ee,
        'A' => 0x21ef,
        'C' => 0x21f7,
        'E' => 0x21f8,
        'O' => 0x21f9,
        'L' => 0x21fa,
        'N' => 0x21fb,
        'Y' => 0x21b9,
        _ => throw new ArgumentOutOfRangeException(nameof(character))
    };
}
