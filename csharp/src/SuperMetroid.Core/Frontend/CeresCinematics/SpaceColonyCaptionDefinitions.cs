namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>English caption in native8C:D629..D66A: emit the non-space characters
/// of SPACE COLONY left-to-right, one eight-pixel cell each, starting at column10 of row 24,
/// and, with Japanese text, the katakana caption below it. The import pictures each letter and the katakana caption.</summary>
public static class SpaceColonyCaptionDefinitions
{
    /// <summary>Native English caption; its one space occupies a cell but has no drawing operation.</summary>
    public const string Text = "SPACE COLONY";
    /// <summary>Counts the visible glyph operations after omitting the caption's space.</summary>
    public static int LetterCount => Text.Length - 1;

    /// <summary>The column the caption's first cell is drawn at.</summary>
    internal const int FirstColumn = 10;

    /// <summary>The row the caption is drawn on.</summary>
    internal const int Row = 24;

    /// <summary>
    /// $8C:D66B-D671: with Japanese text the list draws <c>$8C:D7F7</c>, the katakana caption
    /// below SPACE COLONY, as an eight-by-two block at column $0C, row $1A.
    /// </summary>
    public const int JapaneseColumn = 0x0c, JapaneseRow = 0x1a, JapaneseWidth = 8;
}
