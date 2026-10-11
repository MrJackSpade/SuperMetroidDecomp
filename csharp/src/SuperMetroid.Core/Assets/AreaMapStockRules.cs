using SuperMetroid.Core.Game;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Assets;

/// <summary>Application exploration semantics over immutable stock map content, never modded artwork.</summary>
internal sealed class AreaMapStockRules(IAreaMapView stock, IReadOnlySet<int> stationCells) : IAreaMapView
{
    public AreaId Area => stock.Area;
    public MapTileWord GetTile(Position16<MapTiles> cell) => stock.GetTile(cell);
    public bool IsDiscoverable(Position16<MapTiles> cell) => AreaMapExplorationRules.IsDiscoverable(GetTile(cell));
    public bool RevealsCellAbove(Position16<MapTiles> cell) => AreaMapExplorationRules.RevealsCellAbove(GetTile(cell));
    public bool IsRevealedByMapStation(Position16<MapTiles> cell)
    {
        _ = AreaMapLayout.GetTilemapWordIndex(cell);
        return stationCells.Contains(AreaMapLayout.Size.RowMajorIndex(cell));
    }
}

/// <summary>Decode-only rule source: stock artwork is read before its authored reveal masks are attached.</summary>
internal sealed class AreaMapDecodeRules(AreaId area) : IAreaMapView
{
    public AreaId Area => area;
    public MapTileWord GetTile(Position16<MapTiles> cell) => throw new InvalidOperationException("Decode rules contain no artwork.");
    public bool IsDiscoverable(Position16<MapTiles> cell) => false;
    public bool IsRevealedByMapStation(Position16<MapTiles> cell) => false;
    public bool RevealsCellAbove(Position16<MapTiles> cell) => false;
}
