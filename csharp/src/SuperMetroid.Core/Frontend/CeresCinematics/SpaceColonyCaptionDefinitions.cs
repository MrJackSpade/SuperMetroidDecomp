namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>English caption in native8C:D629..D66A: emit the non-space characters
/// of SPACE COLONY left-to-right, one eight-pixel cell each, starting at column10 of row 24,
/// and, with Japanese text, the katakana caption below it. The import pictures each letter and the katakana caption.</summary>
public static class SpaceColonyCaptionDefinitions
{
    /// <summary>The English caption; its one space occupies a cell but has no drawing operation.</summary>
    public const string Text = "SPACE COLONY";
    /// <summary>Counts the visible glyph operations after omitting the caption's space.</summary>
    public static int LetterCount => Text.Length - 1;

    /// <summary>The column the caption's first cell is drawn at.</summary>
    internal const int FirstColumn = 10;

    /// <summary>The row the caption is drawn on.</summary>
    internal const int Row = 24;

    /// <summary>
    /// With Japanese text the caption list draws the katakana caption
    /// below SPACE COLONY, as an eight-by-two block at column $0C, row $1A.
    /// </summary>
    public const int JapaneseColumn = 12, JapaneseRow = 26, JapaneseWidth = 8;
}
