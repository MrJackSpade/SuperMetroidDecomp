using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>English caption in native8C:D629..D66A: emit the non-space characters
/// of SPACE COLONY left-to-right, one eight-pixel cell each, starting at column10.
/// Glyph identities come from the one-cell drawing operations8C:D7C1..D7F6.
/// Tiles select the high-priority plane; no per-letter layout table.</summary>
internal static class SpaceColonyCaptionDefinitions
{
    /// <summary>Native English caption; its one space occupies a cell but has no drawing operation.</summary>
    private const string Text = "SPACE COLONY";
    /// <summary>Counts the visible glyph operations after omitting the caption's space.</summary>
    internal static int LetterCount => Text.Length - 1;

    /// <summary>Places a visible glyph from column ten, skipping the undrawn space cell.</summary>
    internal static SpaceColonyCaptionLetter Letter(int index)
    {
        if ((uint)index >= LetterCount) throw new ArgumentOutOfRangeException(nameof(index));
        int column = index + (index >= Text.IndexOf(' ') ? 1 : 0);
        return new SpaceColonyCaptionLetter(10 + column, Glyph(Text[column]));
    }

    /// <summary>
    /// $8C:D66B-D671: with Japanese text the list draws <c>$8C:D7F7</c>, the katakana caption
    /// below SPACE COLONY, as an eight-by-two block at column $0C, row $1A.
    /// </summary>
    internal const int JapaneseColumn = 0x0c, JapaneseRow = 0x1a, JapaneseWidth = 8;

    /// <summary>$8C:D7FB: the Japanese caption's two rows of tiles, each a high-priority character in colour variant 0.</summary>
    internal static ReadOnlySpan<BackgroundTile> JapaneseTiles => japaneseTiles;

    /// <summary>The tiles behind <see cref="JapaneseTiles"/>.</summary>
    private static readonly BackgroundTile[] japaneseTiles =
    [
        Caption(27, 10), Caption(27, 11), Caption(27, 3), Caption(27, 10), Caption(27, 12), Caption(14, 0), Caption(14, 1), Caption(27, 3),
        Caption(14, 2), Caption(14, 3), Caption(27, 4), Caption(14, 2), Caption(14, 4), Caption(26, 3), Caption(26, 4), Caption(27, 4),
    ];

    /// <summary>The high-priority tile, in colour variant 0, of the caption character at <paramref name="row"/> and <paramref name="column"/>.</summary>
    private static BackgroundTile Caption(int row, int column) => new(CharacterSheetGrid.Cell(row, column), 0, highPriority: true);

    /// <summary>Native SPACE COLONY glyphs, each a high-priority tile in colour variant 0.
    /// Repeated C and O characters select the same drawing operation.</summary>
    private static BackgroundTile Glyph(char character) => new(character switch
    {
        'S' => 0x1ed,
        'P' => 0x1ee,
        'A' => 0x1ef,
        'C' => 0x1f7,
        'E' => 0x1f8,
        'O' => 0x1f9,
        'L' => 0x1fa,
        'N' => 0x1fb,
        'Y' => 0x1b9,
        _ => throw new ArgumentOutOfRangeException(nameof(character))
    }, 0, highPriority: true);
}
