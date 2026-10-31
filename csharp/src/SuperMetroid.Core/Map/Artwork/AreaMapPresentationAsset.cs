using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Map.Artwork;

/// <summary>
/// One area's installed map (#1466): how each cell looks, which the import decodes from the area's editable presentation
/// document, paired with exploration rules the import decided from the stock map, which edited artwork cannot change.
/// </summary>
public sealed class AreaMapPresentationAsset : IAreaMapView
{
    /// <summary>Validated cells in logical 64-column row order, independent of native page layout.</summary>
    private readonly MapCell[] cells;
    /// <summary>Snapshot of stock discoverability rules, unaffected by visual tile edits.</summary>
    private readonly bool[] discoverable;
    /// <summary>Snapshot of map-station reveal rules, unaffected by visual tile edits.</summary>
    private readonly bool[] station;
    /// <summary>Snapshot of sloped-cell reveal-above rules, unaffected by visual tile edits.</summary>
    private readonly bool[] revealsAbove;
    /// <summary>Area shared by the decoded cells and their separately supplied exploration rules.</summary>
    public AreaId Area { get; }

    /// <summary>Pairs copied cells with a complete independent snapshot of gameplay map rules.</summary>
    /// <param name="cells">Every cell of the area in logical 64-column row order.</param>
    /// <param name="rules">The exploration rules the cells are shown under.</param>
    /// <exception cref="ArgumentException">There is not one cell for each of the area map's cells.</exception>
    internal AreaMapPresentationAsset(MapCell[] cells, IAreaMapView rules)
    {
        Ensure.NotNull(cells);
        Ensure.NotNull(rules);
        Ensure.LengthEqual<MapCell>(cells, AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles);
        this.cells = cells;
        Area = rules.Area;
        discoverable = new bool[cells.Length];
        station = new bool[cells.Length];
        revealsAbove = new bool[cells.Length];
        for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
        for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
        {
            var cell = new Position16<MapTiles>(x, y);
            int i = Index(cell);
            discoverable[i] = rules.IsDiscoverable(cell);
            station[i] = rules.IsRevealedByMapStation(cell);
            revealsAbove[i] = rules.RevealsCellAbove(cell);
        }
    }

    /// <summary>Applies other exploration rules to these already-validated, never-mutated cells.</summary>
    internal AreaMapPresentationAsset WithRules(IAreaMapView rules)
    {
        Ensure.NotNull(rules);
        if (rules.Area != Area) throw new ArgumentException($"Rules for {rules.Area} cannot apply to the {Area} map.", nameof(rules));
        return new(cells, rules);
    }

    /// <summary>How the cell at logical map coordinates X 0–63, Y 0–31 looks.</summary>
    public MapCell Cell(Position16<MapTiles> cell) => cells[Index(cell)];
    /// <summary>Gets the supplied rule snapshot for whether logical cell (X 0–63, Y 0–31) can be discovered, independent of the edited character or blank artwork.</summary>
    public bool IsDiscoverable(Position16<MapTiles> cell) => discoverable[Index(cell)];
    /// <summary>Gets the supplied rule snapshot for whether downloading this area's map reveals logical cell (X 0–63, Y 0–31); no exploration or save bits are changed.</summary>
    public bool IsRevealedByMapStation(Position16<MapTiles> cell) => station[Index(cell)];
    /// <summary>Gets the supplied slope-rule snapshot for whether visiting logical cell (X 0–63, Y 0–31) also explores the cell above; replacing its artwork does not change this rule.</summary>
    public bool RevealsCellAbove(Position16<MapTiles> cell) => revealsAbove[Index(cell)];

    /// <summary>Validates the logical coordinate, then indexes the row-major arrays.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The cell lies outside the area map.</exception>
    private static int Index(Position16<MapTiles> cell)
    {
        if (cell.X >= AreaMapLayout.WidthInTiles || cell.Y >= AreaMapLayout.HeightInTiles)
            throw new ArgumentOutOfRangeException(nameof(cell), "The cell lies outside the area map.");
        return AreaMapLayout.Size.RowMajorIndex(cell);
    }
}
