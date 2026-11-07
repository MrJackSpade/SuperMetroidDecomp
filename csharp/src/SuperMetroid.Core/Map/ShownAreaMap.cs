using System.Collections.Immutable;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Map;

/// <summary>
/// How a map screen shows a whole area map (#1466): each of its <see cref="AreaMapSize.WidthInTiles"/> by
/// <see cref="AreaMapSize.HeightInTiles"/> cells as <see cref="AreaMapProjection"/> projects it.
/// </summary>
public sealed class ShownAreaMap
{
    /// <summary>Keeps each cell, row by row.</summary>
    /// <exception cref="ArgumentException">There is not one shown cell for each of the area map's cells.</exception>
    public ShownAreaMap(ImmutableArray<ShownMapCell> cells)
    {
        if (cells.IsDefault || cells.Length != AreaMapSize.WidthInTiles * AreaMapSize.HeightInTiles)
            throw new ArgumentException(
                $"A shown area map needs each of its {AreaMapSize.WidthInTiles * AreaMapSize.HeightInTiles} cells.", nameof(cells));
        Cells = cells;
    }

    /// <summary>Each cell, row by row.</summary>
    public ImmutableArray<ShownMapCell> Cells { get; }
}
