namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>One compact-lettering label laid across menu atlas cells.</summary>
/// <param name="Text">The label's letters, placed left to right with one-pixel gaps.</param>
/// <param name="Cells">The label's physical atlas cells, left to right; a strip may skip cells another artwork owns.</param>
/// <param name="Origin">The first letter's top-left pixel inside the first cell; the letters share its row in every cell.</param>
/// <param name="Shadow">Whether the label is drawn as ink with a one-pixel shadow (SELECT/START) rather than outlined.</param>
[IndependentFlag("whether the authored strip draws a shadow copy, a fact of the lettering data")]
internal sealed record MenuLetteringStrip(string Text, MenuAtlasCellRange[] Cells, Position16<CharacterPixels> Origin, bool Shadow)
{
    /// <summary>The strip's atlas cells in left-to-right order.</summary>
    internal IReadOnlyList<int> OrderedCells { get; } =
        Cells.SelectMany(range => Enumerable.Range(range.First, range.Last - range.First + 1)).ToArray();

    /// <summary>The strip column (cell position, left to right) of <paramref name="cell"/>.</summary>
    internal int Column(int cell)
    {
        for (int column = 0; column < OrderedCells.Count; column++)
        {
            if (OrderedCells[column] == cell)
                return column;
        }
        throw new ArgumentOutOfRangeException(nameof(cell), cell, $"{Text} does not occupy this cell.");
    }
}
