using System.Buffers.Binary;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

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

    /// <summary>Loads one lossless area tilemap and its native map-station reveal mask.</summary>
    public static AreaMapCartridgeData Load(ISnesAddressSpace bus, AreaId area)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int areaIndex = AreaIds.ToIndex(area);
        int tilemapAddress = RomDataReader.ReadLongFixedBank(
            bus,
            TilemapPointerTable + areaIndex * 3);
        ushort revealPointer = RomDataReader.ReadWordFixedBank(
            bus,
            StationRevealMaskPointerTable + areaIndex * sizeof(ushort));
        int revealAddress = StationRevealMaskBank | revealPointer;
        byte[] tilemapBytes = RomDataReader.ReadFixedBank(bus, tilemapAddress, TilemapByteCount);
        byte[] revealBytes = RomDataReader.ReadFixedBank(
            bus,
            revealAddress,
            StationRevealMaskByteCount);
        var tilemap = new MapTileWord[AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles];
        for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
        {
            for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
            {
                int nativeWordIndex = AreaMapLayout.GetTilemapWordIndex(x, y);
                tilemap[y * AreaMapLayout.WidthInTiles + x] = new MapTileWord(
                    BinaryPrimitives.ReadUInt16LittleEndian(
                        tilemapBytes.AsSpan(nativeWordIndex * sizeof(ushort), sizeof(ushort))));
            }
        }

        return new AreaMapCartridgeData(
            area,
            tilemapAddress,
            revealAddress,
            tilemapBytes,
            revealBytes,
            tilemap);
    }
}
