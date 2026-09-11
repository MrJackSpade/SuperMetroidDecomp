using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Application exploration semantics over immutable stock map content, never modded artwork.</summary>
internal sealed class AreaMapStockRules(IAreaMapView stock, IReadOnlySet<int> stationCells) : IAreaMapView
{
    public AreaId Area => stock.Area;
    public MapTileWord GetTile(int x, int y) => stock.GetTile(x, y);
    public bool IsDiscoverable(int x, int y) => AreaMapExplorationRules.IsDiscoverable(GetTile(x, y));
    public bool RevealsCellAbove(int x, int y) => AreaMapExplorationRules.RevealsCellAbove(GetTile(x, y));
    public bool IsRevealedByMapStation(int x, int y)
    {
        _ = AreaMapLayout.GetTilemapWordIndex(x, y);
        return stationCells.Contains(y * AreaMapLayout.WidthInTiles + x);
    }
}

/// <summary>Decode-only rule source: stock artwork is read before its authored reveal masks are attached.</summary>
internal sealed class AreaMapDecodeRules(AreaId area) : IAreaMapView
{
    public AreaId Area => area;
    public MapTileWord GetTile(int x, int y) => throw new InvalidOperationException("Decode rules contain no artwork.");
    public bool IsDiscoverable(int x, int y) => false;
    public bool IsRevealedByMapStation(int x, int y) => false;
    public bool RevealsCellAbove(int x, int y) => false;
}
