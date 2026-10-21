using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Map;

/// <summary>Lossless decoded view of one retail area map and station reveal plane.</summary>
public sealed class AreaMapCartridgeData : IAreaMapView
{
    /// <summary>Decoded map words addressed in logical 64-by-32 cell order.</summary>
    private readonly MapTileWord[] tilemap;

    /// <summary>Pairs native source bytes and addresses with the decoded map and reveal plane.</summary>
    internal AreaMapCartridgeData(
        AreaId area,
        int tilemapAddress,
        int stationRevealMaskAddress,
        byte[] rawTilemapBytes,
        byte[] stationRevealMaskBytes,
        MapTileWord[] tilemap)
    {
        Area = area;
        TilemapAddress = tilemapAddress;
        StationRevealMaskAddress = stationRevealMaskAddress;
        RawTilemapBytes = rawTilemapBytes;
        StationRevealMaskBytes = stationRevealMaskBytes;
        this.tilemap = tilemap;
    }

    /// <summary>Gets the area whose native tilemap and map-station reveal plane were decoded.</summary>
    public AreaId Area { get; }

    /// <summary>Gets the full 24-bit cartridge source address of the area's packed tilemap.</summary>
    public int TilemapAddress { get; }

    /// <summary>Gets the full 24-bit cartridge source address of the area's map-station reveal mask.</summary>
    public int StationRevealMaskAddress { get; }

    /// <summary>Gets the lossless native packed tilemap bytes in cartridge storage order.</summary>
    public byte[] RawTilemapBytes { get; }

    /// <summary>Gets the lossless native map-station reveal-mask bytes.</summary>
    public byte[] StationRevealMaskBytes { get; }

    /// <summary>Reads one decoded cartridge tilemap word in logical 64-by-32 coordinates.</summary>
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
    /// Whether the cartridge tilemap defines a discoverable cell, including secret cells
    /// deliberately absent from the map-station reveal plane.
    /// </summary>
    public bool IsDiscoverable(Position16<MapTiles> cell) => AreaMapExplorationRules.IsDiscoverable(GetTile(cell));

    /// <summary>Returns whether discovering one map tile also reveals the logical cell immediately above it.</summary>
    /// <param name="cell">Logical map cell, X 0 through 63 and Y 0 through 31.</param>
    /// <returns>Whether the tile's native shape has the upward reveal behavior.</returns>
    public bool RevealsCellAbove(Position16<MapTiles> cell) =>
        AreaMapExplorationRules.RevealsCellAbove(GetTile(cell));
}
