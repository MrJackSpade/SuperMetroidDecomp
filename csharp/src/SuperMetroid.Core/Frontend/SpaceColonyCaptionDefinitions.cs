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
