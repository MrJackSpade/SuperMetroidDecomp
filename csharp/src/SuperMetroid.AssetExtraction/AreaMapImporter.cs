using System.Buffers.Binary;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts native area map tilemaps and reveal masks before gameplay starts.</summary>
public static class AreaMapImporter
{
    public static AreaMapCartridgeData Load(ISnesAddressSpace bus, AreaId area)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int areaIndex = AreaIds.ToIndex(area);
        int tilemapAddress = RomDataReader.ReadLongFixedBank(
            CartridgeImportSource.Require(bus),
            AreaMapRomData.TilemapPointerTable + areaIndex * 3);
        ushort revealPointer = RomDataReader.ReadWordFixedBank(
            CartridgeImportSource.Require(bus),
            AreaMapRomData.StationRevealMaskPointerTable + areaIndex * sizeof(ushort));
        int revealAddress = AreaMapRomData.StationRevealMaskBank | revealPointer;
        byte[] tilemapBytes = RomDataReader.ReadFixedBank(
            CartridgeImportSource.Require(bus), tilemapAddress, AreaMapRomData.TilemapByteCount);
        byte[] revealBytes = RomDataReader.ReadFixedBank(
            CartridgeImportSource.Require(bus), revealAddress, AreaMapRomData.StationRevealMaskByteCount);
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
            area, tilemapAddress, revealAddress, tilemapBytes, revealBytes, tilemap);
    }
}
