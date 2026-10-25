namespace SuperMetroid.Core.Assets;

/// <summary>
/// The layout of an artwork's character sheet: characters run sixteen to a row, so a character's number is its row times
/// sixteen plus its column. Artwork that picks characters out of a sheet names them by row and column.
/// </summary>
public static class CharacterSheetGrid
{
    /// <summary>The characters in one sheet row.</summary>
    public const int CharactersPerRow = 16;

    /// <summary>The number of the character at <paramref name="row"/> and <paramref name="column"/>.</summary>
    public static int Cell(int row, int column) => (uint)column < CharactersPerRow && row >= 0
        ? row * CharactersPerRow + column
        : throw new ArgumentOutOfRangeException(nameof(column), column, "A sheet row holds columns 0 through 15.");
}
