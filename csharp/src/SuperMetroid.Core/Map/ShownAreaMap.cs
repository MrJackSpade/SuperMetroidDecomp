using System.Collections.Immutable;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Map;

/// <summary>
/// How a map screen shows a whole area map (#1466): each of its <see cref="AreaMapLayout.WidthInTiles"/> by
/// <see cref="AreaMapLayout.HeightInTiles"/> cells as <see cref="AreaMapProjection"/> projects it.
/// </summary>
public sealed class ShownAreaMap
{
    /// <summary>Keeps each cell, row by row.</summary>
    /// <exception cref="ArgumentException">There is not one shown cell for each of the area map's cells.</exception>
    public ShownAreaMap(ImmutableArray<ShownMapCell> cells)
    {
        if (cells.IsDefault || cells.Length != AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles)
            throw new ArgumentException(
                $"A shown area map needs each of its {AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles} cells.", nameof(cells));
        Cells = cells;
    }

    /// <summary>Each cell, row by row.</summary>
    public ImmutableArray<ShownMapCell> Cells { get; }

    /// <summary>How the screen shows <paramref name="cell"/>.</summary>
    public ShownMapCell this[Position16<MapTiles> cell]
    {
        get
        {
            if (cell.X >= AreaMapLayout.WidthInTiles || cell.Y >= AreaMapLayout.HeightInTiles)
                throw new ArgumentOutOfRangeException(nameof(cell), "The cell lies outside the area map.");
            return Cells[cell.Y * AreaMapLayout.WidthInTiles + cell.X];
        }
    }
}
