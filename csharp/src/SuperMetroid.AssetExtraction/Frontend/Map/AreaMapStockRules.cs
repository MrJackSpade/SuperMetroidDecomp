using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Application exploration semantics over immutable stock map content, never modded artwork.</summary>
internal sealed class AreaMapStockRules(IAreaMapView stock, IReadOnlySet<int> stationCells) : IAreaMapView
{
    /// <summary>Uses the stock map's area identity for exploration decisions.</summary>
    public AreaId Area => stock.Area;
    /// <summary>Reads the stock cell so customized artwork cannot change exploration rules.</summary>
    public MapCell Cell(Position16<MapTiles> cell) => stock.Cell(cell);
    /// <summary>Applies the exploration rule to the stock cell at this coordinate.</summary>
    public bool IsDiscoverable(Position16<MapTiles> cell) => AreaMapExplorationRules.IsDiscoverable(Cell(cell));
    /// <summary>Checks the stock cell's authored above-cell reveal behavior.</summary>
    public bool RevealsCellAbove(Position16<MapTiles> cell) => AreaMapExplorationRules.RevealsCellAbove(Cell(cell));
    /// <summary>Validates the map cell before testing its row-major station reveal mask.</summary>
    public bool IsRevealedByMapStation(Position16<MapTiles> cell)
    {
        _ = AreaMapPages.GetTilemapWordIndex(cell);
        return stationCells.Contains(AreaMapLayout.Size.RowMajorIndex(cell));
    }
}
