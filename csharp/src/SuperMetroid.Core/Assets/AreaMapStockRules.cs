using SuperMetroid.Core.Game;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Assets;

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

/// <summary>Decode-only rule source: stock artwork is read before its authored reveal masks are attached.</summary>
internal sealed class AreaMapDecodeRules(AreaId area) : IAreaMapView
{
    /// <summary>Preserves the area identity while stock tile artwork is still being decoded.</summary>
    public AreaId Area => area;
    /// <summary>Rejects tile reads because decode rules do not yet own artwork.</summary>
    public MapTileWord GetTile(Position16<MapTiles> cell) => throw new InvalidOperationException("Decode rules contain no artwork.");
    /// <summary>Leaves discovery disabled until the stock map has been loaded.</summary>
    public bool IsDiscoverable(Position16<MapTiles> cell) => false;
    /// <summary>Leaves station reveals disabled until their authored mask is attached.</summary>
    public bool IsRevealedByMapStation(Position16<MapTiles> cell) => false;
    /// <summary>Leaves adjacent-cell reveals disabled until tile artwork is available.</summary>
    public bool RevealsCellAbove(Position16<MapTiles> cell) => false;
}
