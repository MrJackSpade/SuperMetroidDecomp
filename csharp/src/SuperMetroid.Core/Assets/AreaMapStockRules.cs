using SuperMetroid.Core.Game;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Assets;

/// <summary>Application exploration semantics over immutable stock map content, never modded artwork.</summary>
internal sealed class AreaMapStockRules(IAreaMapView stock, IReadOnlySet<int> stationCells) : IAreaMapView
{
    /// <summary>Gets area.</summary>
    public AreaId Area => stock.Area;
    /// <summary>Rejects tile lookup because these decode rules contain no artwork.</summary>
    public MapTileWord GetTile(Position16<MapTiles> cell) => stock.GetTile(cell);
    /// <summary>Checks whether this map cell can be discovered.</summary>
    public bool IsDiscoverable(Position16<MapTiles> cell) => AreaMapExplorationRules.IsDiscoverable(GetTile(cell));
    /// <summary>Reveals cell above.</summary>
    public bool RevealsCellAbove(Position16<MapTiles> cell) => AreaMapExplorationRules.RevealsCellAbove(GetTile(cell));
    /// <summary>Checks whether a map station reveals this cell.</summary>
    public bool IsRevealedByMapStation(Position16<MapTiles> cell)
    {
        _ = AreaMapLayout.GetTilemapWordIndex(cell);
        return stationCells.Contains(AreaMapLayout.Size.RowMajorIndex(cell));
    }
}

/// <summary>Decode-only rule source: stock artwork is read before its authored reveal masks are attached.</summary>
internal sealed class AreaMapDecodeRules(AreaId area) : IAreaMapView
{
    /// <summary>Gets area.</summary>
    public AreaId Area => area;
    /// <summary>Gets tile.</summary>
    public MapTileWord GetTile(Position16<MapTiles> cell) => throw new InvalidOperationException("Decode rules contain no artwork.");
    /// <summary>Checks whether this map cell can be discovered.</summary>
    public bool IsDiscoverable(Position16<MapTiles> cell) => false;
    /// <summary>Checks whether a map station reveals this cell.</summary>
    public bool IsRevealedByMapStation(Position16<MapTiles> cell) => false;
    /// <summary>Reveals cell above.</summary>
    public bool RevealsCellAbove(Position16<MapTiles> cell) => false;
}
