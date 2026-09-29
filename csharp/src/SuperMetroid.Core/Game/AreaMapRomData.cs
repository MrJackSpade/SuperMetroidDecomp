namespace SuperMetroid.Core.Game;

/// <summary>Cartridge addresses and fixed geometry for the seven retail area maps.</summary>
public static class AreaMapRomData
{
    /// <summary>Long-pointer table <c>kPauseMenuMapTilemaps</c> at <c>$82:964A</c>.</summary>
    public const int TilemapPointerTable = 0x82964a;

    /// <summary>Bank-$82 word-pointer table <c>kPauseMenuMapData</c> at <c>$82:9717</c>.</summary>
    public const int StationRevealMaskPointerTable = 0x829717;

    /// <summary>SNES bank containing the map-station reveal masks addressed by the pointer table.</summary>
    public const int StationRevealMaskBank = 0x820000;

    /// <summary>Bytes in one 64-by-32 map tilemap: 2,048 little-endian words.</summary>
    public const int TilemapByteCount = 0x1000;

    /// <summary>Bytes in one 64-by-32 one-bit map-station reveal plane.</summary>
    public const int StationRevealMaskByteCount = 0x0100;

}
