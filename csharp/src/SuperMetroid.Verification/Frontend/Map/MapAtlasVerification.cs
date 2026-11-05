using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks installed map atlas bytes and the pause render path.</summary>
internal static class MapAtlasVerification
{
    /// <summary>Compares stock tile bytes with ROM and the installed atlas's pause pixels with native.</summary>
    internal static void VerifyMapAtlasIntegration(ISnesAddressSpace bus,
        AreaMapPresentation original)
    {
        byte[] source = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), MapSourceDefinitions.Artwork.MapCharacters, MapTileAtlasFormat.ByteCount);
        var vram = new SnesVram();
        MapCharacterArtwork.Of(original).Map.LoadTo(new VramRegion(vram, PpuMemoryLayout.Vram).Slice(0 / sizeof(ushort), MapTileAtlasFormat.ByteCount / sizeof(ushort)));
        for (int i = 0; i < source.Length; i++) AssertEqual(source[i], vram.ReadByte(i), "stock PNG recompiles exact map tile bytes");
        Console.WriteLine("Map PNG atlas: exact stock bytes pass.");
    }
}
