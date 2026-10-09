namespace SuperMetroid.Core.Game;

/// <summary>Map presentation together with separately owned exploration rules.</summary>
public interface IAreaMapView
{
    /// <summary>Gets the retail area whose 64-by-32 map and exploration rules are represented.</summary>
    AreaId Area { get; }
    /// <summary>Gets the packed SNES background word at logical map coordinates X 0..63 and Y 0..31.</summary>
    MapTileWord GetTile(int x, int y);
    /// <summary>Returns whether the logical cell can acquire an explored save bit, independently of its current artwork.</summary>
    bool IsDiscoverable(int x, int y);
    /// <summary>Returns whether downloading the area's map reveals the logical cell, without changing exploration state.</summary>
    bool IsRevealedByMapStation(int x, int y);
    /// <summary>Native slope rule; replacing its artwork must not change exploration.</summary>
    bool RevealsCellAbove(int x, int y);
}
