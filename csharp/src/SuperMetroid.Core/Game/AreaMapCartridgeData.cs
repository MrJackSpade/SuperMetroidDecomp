namespace SuperMetroid.Core.Game;

/// <summary>Lossless decoded view of one retail area map and station reveal plane.</summary>
public sealed class AreaMapCartridgeData : IAreaMapView
{
    /// <summary>Decoded map words in row-major order over the logical 64-by-32 area grid.</summary>
    private readonly MapTileWord[] tilemap;

    /// <summary>Creates the area's map view while retaining its raw cartridge data and source addresses.</summary>
    /// <param name="area">Area identity associated with the imported map and reveal plane.</param>
    /// <param name="tilemapAddress">Full cartridge address from which the packed tilemap was read.</param>
    /// <param name="stationRevealMaskAddress">Full cartridge address from which the station reveal mask was read.</param>
    /// <param name="rawTilemapBytes">Packed tilemap bytes in their original cartridge storage order.</param>
    /// <param name="stationRevealMaskBytes">Native station-reveal bits indexed by logical map coordinate.</param>
    /// <param name="tilemap">Decoded tile words ordered by row, then column.</param>
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
    public MapTileWord GetTile(int mapX, int mapY)
    {
        _ = AreaMapLayout.GetTilemapWordIndex(mapX, mapY);
        return tilemap[mapY * AreaMapLayout.WidthInTiles + mapX];
    }

    /// <summary>Whether the area's ordinary map station reveals this coordinate.</summary>
    public bool IsRevealedByMapStation(int mapX, int mapY)
    {
        int byteIndex = AreaMapLayout.GetBitByteIndex(mapX, mapY);
        return (StationRevealMaskBytes[byteIndex] & AreaMapLayout.GetBitMask(mapX)) != 0;
    }

    /// <summary>
    /// Whether the cartridge tilemap defines a discoverable cell, including secret cells
    /// deliberately absent from the map-station reveal plane.
    /// </summary>
    public bool IsDiscoverable(int mapX, int mapY) => AreaMapExplorationRules.IsDiscoverable(GetTile(mapX, mapY));

    /// <summary>Returns whether discovering one map tile also reveals the logical cell immediately above it.</summary>
    /// <param name="x">Logical map X coordinate from zero through 63.</param>
    /// <param name="y">Logical map Y coordinate from zero through 31.</param>
    /// <returns>Whether the tile's native shape has the upward reveal behavior.</returns>
    public bool RevealsCellAbove(int x, int y) =>
        AreaMapExplorationRules.RevealsCellAbove(GetTile(x, y));
}
