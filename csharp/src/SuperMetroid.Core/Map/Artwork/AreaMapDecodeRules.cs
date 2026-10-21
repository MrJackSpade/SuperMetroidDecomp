using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Map.Artwork;

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
