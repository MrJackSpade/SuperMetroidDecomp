using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// One retail area map and station reveal plane as the import decoded them (#1416): the cartridge bytes, where they were
/// read from, and the decoded cells the map presentation is extracted from.
/// </summary>
public sealed class ImportedAreaMap : IAreaMapView
{
    /// <summary>Decoded map words addressed in logical 64-by-32 cell order.</summary>
    private readonly MapTileWord[] tilemap;

    /// <summary>Pairs the cartridge bytes and their sources with the decoded map and reveal plane.</summary>
    internal ImportedAreaMap(
        AreaId area,
        int tilemapSource,
        int stationRevealMaskSource,
        byte[] rawTilemapBytes,
        byte[] stationRevealMaskBytes,
        MapTileWord[] tilemap)
    {
        Area = area;
        TilemapSource = tilemapSource;
        StationRevealMaskSource = stationRevealMaskSource;
        RawTilemapBytes = rawTilemapBytes;
        StationRevealMaskBytes = stationRevealMaskBytes;
        this.tilemap = tilemap;
    }

    /// <summary>Gets the area whose tilemap and map-station reveal plane were decoded.</summary>
    public AreaId Area { get; }

    /// <summary>Gets where on the cartridge bus the area's packed tilemap was read from.</summary>
    public int TilemapSource { get; }

    /// <summary>Gets where on the cartridge bus the area's map-station reveal mask was read from.</summary>
    public int StationRevealMaskSource { get; }

    /// <summary>Gets the packed tilemap bytes in cartridge storage order.</summary>
    public byte[] RawTilemapBytes { get; }

    /// <summary>Gets the map-station reveal-mask bytes.</summary>
    public byte[] StationRevealMaskBytes { get; }

    /// <summary>Reads one decoded tilemap word in logical 64-by-32 coordinates.</summary>
    public MapTileWord GetTile(Position16<MapTiles> cell)
    {
        _ = AreaMapLayout.GetTilemapWordIndex(cell);
        return tilemap[AreaMapLayout.Size.RowMajorIndex(cell)];
    }

    /// <summary>Whether the area's ordinary map station reveals this coordinate.</summary>
    public bool IsRevealedByMapStation(Position16<MapTiles> cell)
    {
        int byteIndex = AreaMapLayout.GetBitByteIndex(cell);
        return (StationRevealMaskBytes[byteIndex] & AreaMapLayout.GetBitMask(cell.X)) != 0;
    }

    /// <summary>
    /// Whether the tilemap defines a discoverable cell, including secret cells
    /// deliberately absent from the map-station reveal plane.
    /// </summary>
    public bool IsDiscoverable(Position16<MapTiles> cell) => AreaMapExplorationRules.IsDiscoverable(GetTile(cell));

    /// <summary>Returns whether discovering one map tile also reveals the logical cell immediately above it.</summary>
    /// <param name="cell">Logical map cell, X 0 through 63 and Y 0 through 31.</param>
    /// <returns>Whether the tile's shape has the upward reveal behavior.</returns>
    public bool RevealsCellAbove(Position16<MapTiles> cell) =>
        AreaMapExplorationRules.RevealsCellAbove(GetTile(cell));
}
