using SuperMetroid.Core;
using System.Buffers.Binary;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Extracts native area map tilemaps and reveal masks before gameplay starts.</summary>
[CpuBusBoundary("codec: ROM area map tilemaps and reveal masks → area map definitions")]
public static class AreaMapImporter
{
    /// <summary>Resolves one area's native tilemap and map-station reveal-mask pointers, copies their bytes, and decodes the split native tilemap layout into row-major cells.</summary>
    /// <param name="bus">Supported-cartridge address space containing the area pointer tables and map data.</param>
    /// <param name="area">Defined area whose complete map and reveal mask are imported.</param>
    /// <returns>Owned native bytes, source addresses, and decoded tile words for the selected area.</returns>
    public static ImportedAreaMap Load(ISnesAddressSpace bus, AreaId area)
    {
        Ensure.NotNull(bus);
        int areaIndex = AreaIds.ToIndex(area);
        int tilemapAddress = RomDataReader.ReadLongFixedBank(
            CartridgeImportSource.Require(bus),
            MapSourceDefinitions.AreaMaps.TilemapPointers + areaIndex * MapSourceDefinitions.AreaMaps.TilemapPointerBytes);
        ushort revealPointer = RomDataReader.ReadWordFixedBank(
            CartridgeImportSource.Require(bus),
            MapSourceDefinitions.AreaMaps.StationRevealMaskPointers + areaIndex * sizeof(ushort));
        int revealAddress = MapSourceDefinitions.AreaMaps.StationRevealMaskBank | revealPointer;
        byte[] tilemapBytes = RomDataReader.ReadFixedBank(
            CartridgeImportSource.Require(bus), tilemapAddress, MapSourceDefinitions.AreaMaps.TilemapBytes);
        byte[] revealBytes = RomDataReader.ReadFixedBank(
            CartridgeImportSource.Require(bus), revealAddress, MapSourceDefinitions.AreaMaps.StationRevealMaskBytes);
        var tilemap = new MapTileWord[AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles];
        for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
        {
            for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
            {
                int nativeWordIndex = AreaMapLayout.GetTilemapWordIndex(new(checked((ushort)x), checked((ushort)y)));
                tilemap[y * AreaMapLayout.WidthInTiles + x] = new MapTileWord(
                    BinaryPrimitives.ReadUInt16LittleEndian(
                        tilemapBytes.AsSpan(nativeWordIndex * sizeof(ushort), sizeof(ushort))));
            }
        }

        return new ImportedAreaMap(
            area, tilemapAddress, revealAddress, tilemapBytes, revealBytes, tilemap);
    }
}
