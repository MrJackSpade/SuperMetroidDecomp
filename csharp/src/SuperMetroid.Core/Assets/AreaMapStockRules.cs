using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Application exploration semantics over immutable stock map content, never modded artwork.</summary>
/// <param name="stock">View supplying the unmodified stock tile and area data used for exploration decisions.</param>
/// <param name="stationCells">Tile indices marked as revealed by the area's map-station data.</param>
internal sealed class AreaMapStockRules(IAreaMapView stock, IReadOnlySet<int> stationCells) : IAreaMapView
{
    /// <summary>Area identity forwarded from the stock map view.</summary>
    public AreaId Area => stock.Area;

    /// <summary>Returns the stock tile at the requested map coordinates.</summary>
    /// <param name="x">Zero-based tile column.</param>
    /// <param name="y">Zero-based tile row.</param>
    /// <returns>The stock map tile, including its authored exploration bits.</returns>
    public MapTileWord GetTile(int x, int y) => stock.GetTile(x, y);

    /// <summary>Reports whether the stock tile at these coordinates becomes visible through ordinary exploration.</summary>
    /// <param name="x">Zero-based tile column.</param>
    /// <param name="y">Zero-based tile row.</param>
    /// <returns>The exploration rule result for the stock tile.</returns>
    public bool IsDiscoverable(int x, int y) => AreaMapExplorationRules.IsDiscoverable(GetTile(x, y));

    /// <summary>Reports whether discovering the stock tile at these coordinates also reveals the tile above it.</summary>
    /// <param name="x">Zero-based tile column.</param>
    /// <param name="y">Zero-based tile row.</param>
    /// <returns>The stock tile's reveal-above rule result.</returns>
    public bool RevealsCellAbove(int x, int y) => AreaMapExplorationRules.RevealsCellAbove(GetTile(x, y));

    /// <summary>Checks whether authored map-station data reveals the tile at these coordinates.</summary>
    /// <param name="x">Zero-based tile column.</param>
    /// <param name="y">Zero-based tile row.</param>
    /// <returns>True when the corresponding row-major tile index is in the station reveal set.</returns>
    public bool IsRevealedByMapStation(int x, int y)
    {
        _ = AreaMapLayout.GetTilemapWordIndex(x, y);
        return stationCells.Contains(y * AreaMapLayout.WidthInTiles + x);
    }
}

/// <summary>Decode-only rule source: stock artwork is read before its authored reveal masks are attached.</summary>
/// <param name="area">Area identity associated with the map data being decoded.</param>
internal sealed class AreaMapDecodeRules(AreaId area) : IAreaMapView
{
    /// <summary>Area identity supplied when the decode-only view is created.</summary>
    public AreaId Area => area;

    /// <summary>Rejects tile access because decoding rules intentionally have no artwork source.</summary>
    /// <param name="x">Unused tile column; decode-only views cannot resolve tiles.</param>
    /// <param name="y">Unused tile row; decode-only views cannot resolve tiles.</param>
    /// <returns>This method does not return.</returns>
    /// <exception cref="InvalidOperationException">Tile artwork is unavailable during the decode-only pass.</exception>
    public MapTileWord GetTile(int x, int y) => throw new InvalidOperationException("Decode rules contain no artwork.");

    /// <summary>Returns false because decode-only rules cannot inspect a tile's ordinary exploration bits.</summary>
    public bool IsDiscoverable(int x, int y) => false;

    /// <summary>Returns false because map-station reveal cells are unavailable during decoding.</summary>
    public bool IsRevealedByMapStation(int x, int y) => false;

    /// <summary>Returns false because reveal-above bits are unavailable before stock artwork is attached.</summary>
    public bool RevealsCellAbove(int x, int y) => false;
}
