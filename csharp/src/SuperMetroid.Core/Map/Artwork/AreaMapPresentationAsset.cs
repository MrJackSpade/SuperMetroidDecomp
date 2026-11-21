using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Map.Artwork;

/// <summary>
/// One area's installed map (#1466): how each cell looks, which the import decodes from the area's editable presentation
/// document, paired with exploration rules the import decided from the stock map, which edited artwork cannot change.
/// </summary>
public sealed class AreaMapPresentationAsset : IAreaMapView
{
    /// <summary>
    /// Each validated cell, row by row from the map's top left, <see cref="AreaMapSize.WidthInMapTiles"/> to a row: how it
    /// looks with a snapshot of the stock exploration rules it is shown under, which visual tile edits do not affect.
    /// </summary>
    private readonly ExploredMapCell[] cells;
    /// <summary>Area shared by the decoded cells and their separately supplied exploration rules.</summary>
    public AreaId Area { get; }

    /// <summary>Pairs copied cells with a complete independent snapshot of gameplay map rules.</summary>
    /// <param name="cells">Every cell of the area in logical row order.</param>
    /// <param name="rules">The exploration rules the cells are shown under.</param>
    /// <exception cref="ArgumentException">There is not one cell for each of the area map's cells.</exception>
    internal AreaMapPresentationAsset(MapCell[] cells, IAreaMapView rules)
    {
        Ensure.NotNull(cells);
        Ensure.NotNull(rules);
        Ensure.LengthEqual<MapCell>(cells, AreaMapSize.WidthInMapTiles * AreaMapSize.HeightInMapTiles);
        Area = rules.Area;
        this.cells = new ExploredMapCell[cells.Length];
        for (int y = 0; y < AreaMapSize.HeightInMapTiles; y++)
        for (int x = 0; x < AreaMapSize.WidthInMapTiles; x++)
        {
            var cell = new Position16<MapTiles>(x, y);
            int i = Index(cell);
            this.cells[i] = new ExploredMapCell(cells[i], rules.IsDiscoverable(cell), rules.IsRevealedByMapStation(cell), rules.RevealsCellAbove(cell));
        }
    }

    /// <summary>How the cell at logical map coordinates within the area map looks.</summary>
    public MapCell Cell(Position16<MapTiles> cell) => cells[Index(cell)].Look;
    /// <summary>Gets the supplied rule snapshot for whether a logical cell within the area map can be discovered, independent of the cell's edited or blank artwork.</summary>
    public bool IsDiscoverable(Position16<MapTiles> cell) => cells[Index(cell)].Discoverable;
    /// <summary>Gets the supplied rule snapshot for whether downloading this area's map reveals a logical cell within the area map; asking marks nothing explored.</summary>
    public bool IsRevealedByMapStation(Position16<MapTiles> cell) => cells[Index(cell)].RevealedByMapStation;
    /// <summary>Gets the supplied slope-rule snapshot for whether visiting a logical cell within the area map also explores the cell above; replacing its artwork does not change this rule.</summary>
    public bool RevealsCellAbove(Position16<MapTiles> cell) => cells[Index(cell)].RevealsCellAbove;

    /// <summary>Validates the logical coordinate, then indexes the row-major arrays.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The cell lies outside the area map.</exception>
    private static int Index(Position16<MapTiles> cell)
    {
        if (cell.X >= AreaMapSize.WidthInMapTiles || cell.Y >= AreaMapSize.HeightInMapTiles)
            throw new ArgumentOutOfRangeException(nameof(cell), "The cell lies outside the area map.");
        return AreaMapSize.Extent.RowMajorIndex(cell);
    }

    /// <summary>One map cell: how it looks and the exploration rules it is shown under.</summary>
    /// <param name="Look">How the cell looks.</param>
    /// <param name="Discoverable">Whether the cell can be discovered.</param>
    /// <param name="RevealedByMapStation">Whether downloading the area's map reveals the cell.</param>
    /// <param name="RevealsCellAbove">Whether visiting the cell also explores the cell above.</param>
    private readonly record struct ExploredMapCell(MapCell Look, bool Discoverable, bool RevealedByMapStation, bool RevealsCellAbove);
}
