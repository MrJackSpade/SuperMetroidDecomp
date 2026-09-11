namespace SuperMetroid.Core.Game;

/// <summary>Map presentation together with separately owned exploration rules.</summary>
public interface IAreaMapView
{
    AreaId Area { get; }
    MapTileWord GetTile(int x, int y);
    bool IsDiscoverable(int x, int y);
    bool IsRevealedByMapStation(int x, int y);
    /// <summary>Native slope rule; replacing its artwork must not change exploration.</summary>
    bool RevealsCellAbove(int x, int y);
}
