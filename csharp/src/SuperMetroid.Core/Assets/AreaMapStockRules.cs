using SuperMetroid.Core.Game;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Assets;

/// <summary>Application exploration semantics over immutable stock map content, never modded artwork.</summary>
internal sealed class AreaMapStockRules(IAreaMapView stock, IReadOnlySet<int> stationCells) : IAreaMapView
{
    public AreaId Area => stock.Area;
    public MapTileWord GetTile(Point32<MapTiles> cell) => stock.GetTile(cell);
    public bool IsDiscoverable(Point32<MapTiles> cell) => AreaMapExplorationRules.IsDiscoverable(GetTile(cell));
    public bool RevealsCellAbove(Point32<MapTiles> cell) => AreaMapExplorationRules.RevealsCellAbove(GetTile(cell));
    public bool IsRevealedByMapStation(Point32<MapTiles> cell)
    {
        _ = AreaMapLayout.GetTilemapWordIndex(cell);
        return stationCells.Contains(AreaMapLayout.Size.RowMajorIndex(cell));
    }
}

/// <summary>Decode-only rule source: stock artwork is read before its authored reveal masks are attached.</summary>
internal sealed class AreaMapDecodeRules(AreaId area) : IAreaMapView
{
    public AreaId Area => area;
    public MapTileWord GetTile(Point32<MapTiles> cell) => throw new InvalidOperationException("Decode rules contain no artwork.");
    public bool IsDiscoverable(Point32<MapTiles> cell) => false;
    public bool IsRevealedByMapStation(Point32<MapTiles> cell) => false;
    public bool RevealsCellAbove(Point32<MapTiles> cell) => false;
}
