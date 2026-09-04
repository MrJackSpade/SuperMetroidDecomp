namespace SuperMetroid.Core.Game;

/// <summary>Lossless decoded view of one retail area map and station reveal plane.</summary>
public sealed class AreaMapCartridgeData
{
    private readonly MapTileWord[] tilemap;

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

    public AreaId Area { get; }
    public int TilemapAddress { get; }
    public int StationRevealMaskAddress { get; }
    public byte[] RawTilemapBytes { get; }
    public byte[] StationRevealMaskBytes { get; }
    public IReadOnlyList<MapTileWord> Tilemap => tilemap;

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
    public bool IsDiscoverable(int mapX, int mapY) => !GetTile(mapX, mapY).IsBlank;
}
