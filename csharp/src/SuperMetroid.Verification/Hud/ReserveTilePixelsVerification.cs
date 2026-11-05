using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.Verification.Hud;

/// <summary>Checks calculated reserve-indicator pixels and the stock map atlas transfer.</summary>
internal static class ReserveTilePixelsVerification
{
    /// <summary>Compares reserve tiles $47-$4F with retail planar art and checks input and transfer bounds.</summary>
    internal static void VerifyReserveTilePixels(ISnesAddressSpace rom)
    {
        var files = MapSpriteExtractor.Extract(rom);
        byte[] json = files[MapSpriteFormat.JsonFile];
        var stock = MapSpriteArtwork.Read(new MemoryStream(json), new MemoryStream(files[MapSpriteFormat.PngFile]));
        AssertEqual(0, stock.StoredReservePixelCount, "stock reserve pixel table eliminated");
        AssertEqual(8192 - 151 * 32, stock.StoredArtworkByteCount, "reserve planar bytes absent from stored atlas");
        var native = new byte[8192];
        for (int i = 0; i < native.Length; i++) native[i] = rom.ReadCartridgeByte(0xb6c000 + i);
        foreach (int destination in new[] { 0x4000, 0xc000 })
        {
            var vram = new SnesVram(); stock.LoadArtworkTo(new VramRegion(vram, PpuMemoryLayout.Vram).Slice(destination / sizeof(ushort), MapSpriteFormat.ByteCount / sizeof(ushort)));
            AssertTrue(vram.Bytes.Slice(destination, native.Length).SequenceEqual(native), "full original map OBJ upload bytes");
        }
        for (int tile = 0x47; tile <= 0x4f; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            int expected = 0;
            for (int plane = 0; plane < 4; plane++)
                expected |= ((native[tile * 32 + plane / 2 * 16 + y * 2 + plane % 2] >> (7 - x)) & 1) << plane;
            AssertEqual((byte)expected, MapObjectTileArtwork.ReservePixel(tile, new(checked((ushort)x), checked((ushort)y))), "original reserve pixel geometry");
        }
        foreach (int invalid in new[] { -1, 0x46, 0x50, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => MapObjectTileArtwork.ReservePixel(invalid, new(0, 0)), "invalid reserve tile");
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => MapObjectTileArtwork.ReservePixel(0x47, new(unchecked((ushort)invalid), 0)), "invalid reserve pixel X");
            AssertThrows<ArgumentOutOfRangeException>(() => MapObjectTileArtwork.ReservePixel(0x47, new(0, unchecked((ushort)invalid))), "invalid reserve pixel Y");
        }
        var unchanged = new SnesVram();
        AssertThrows<ArgumentOutOfRangeException>(() => stock.LoadArtworkTo(new VramRegion(unchanged, PpuMemoryLayout.Vram).Slice(0, 1)), "invalid full transfer destination");
        AssertTrue(unchanged.Bytes.All(value => value == 0), "invalid transfer makes no partial writes");
    }
}
