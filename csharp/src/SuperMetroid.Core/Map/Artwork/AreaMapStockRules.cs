using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Map.Artwork;

/// <summary>Application exploration semantics over immutable stock map content, never modded artwork.</summary>
internal sealed class AreaMapStockRules(IAreaMapView stock, IReadOnlySet<int> stationCells) : IAreaMapView
{
    /// <summary>Uses the stock map's area identity for exploration decisions.</summary>
    public AreaId Area => stock.Area;
    /// <summary>Reads the stock tile word so customized artwork cannot change exploration rules.</summary>
    public MapTileWord GetTile(Position16<MapTiles> cell) => stock.GetTile(cell);
    /// <summary>Applies the exploration bit rule to the stock tile word at this cell.</summary>
    public bool IsDiscoverable(Position16<MapTiles> cell) => AreaMapExplorationRules.IsDiscoverable(GetTile(cell));
    /// <summary>Checks the stock tile's authored above-cell reveal behavior.</summary>
    public bool RevealsCellAbove(Position16<MapTiles> cell) => AreaMapExplorationRules.RevealsCellAbove(GetTile(cell));
    /// <summary>Validates the map cell before testing its row-major station reveal mask.</summary>
    public bool IsRevealedByMapStation(Position16<MapTiles> cell)
    {
        _ = AreaMapLayout.GetTilemapWordIndex(cell);
        return stationCells.Contains(AreaMapLayout.Size.RowMajorIndex(cell));
    }
}
