namespace SuperMetroid.Core.Progression;

/// <summary>
/// One area map's explored cells (#1777): each map tile of the 64-by-32 area map, explored or not. A cell outside the
/// area map is rejected rather than reaching a neighbouring cell.
/// </summary>
public sealed class ExploredMapCells
{
    /// <summary>Whether each map tile is explored, by column and row.</summary>
    private readonly bool[,] _explored = new bool[AreaMapSize.WidthInMapTiles, AreaMapSize.HeightInMapTiles];

    /// <summary>Whether <paramref name="cell"/> is explored.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="cell"/> is outside the area map.</exception>
    public bool Contains(Position16<MapTiles> cell) => _explored[Require(cell).X, cell.Y];

    /// <summary>Records whether <paramref name="cell"/> is explored.</summary>
    /// <param name="cell">The map tile.</param>
    /// <param name="explored">Whether the tile is explored afterwards.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="cell"/> is outside the area map.</exception>
    public void Set(Position16<MapTiles> cell, bool explored) => _explored[Require(cell).X, cell.Y] = explored;

    /// <summary>The explored cells, row by row from the top, each row from the left.</summary>
    public IEnumerable<Position16<MapTiles>> Cells
    {
        get
        {
            for (int y = 0; y < AreaMapSize.HeightInMapTiles; y++)
            {
                for (int x = 0; x < AreaMapSize.WidthInMapTiles; x++)
                {
                    if (_explored[x, y])
                        yield return new Position16<MapTiles>(x, y);
                }
            }
        }
    }

    /// <summary>Makes every cell match <paramref name="source"/>'s.</summary>
    public void CopyFrom(ExploredMapCells source) =>
        Array.Copy(Ensure.NotNull(source)._explored, _explored, _explored.Length);

    /// <summary>Rejects a cell outside the area map.</summary>
    private static Position16<MapTiles> Require(Position16<MapTiles> cell) => AreaMapSize.Extent.Contains(cell)
        ? cell
        : throw new ArgumentOutOfRangeException(nameof(cell), cell, "The cell lies outside the 64-by-32 area map.");
}
