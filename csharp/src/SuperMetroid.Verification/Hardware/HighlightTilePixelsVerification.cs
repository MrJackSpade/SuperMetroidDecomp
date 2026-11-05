using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.Verification.Hardware;

/// <summary>Checks calculated map highlight tiles against native OBJ bytes and the stock VRAM upload.</summary>
internal static class HighlightTilePixelsVerification
{
    /// <summary>Checks stock pixels, transfer bounds, and resulting planar bytes.</summary>
    internal static void VerifyHighlightTilePixels(ISnesAddressSpace rom)
    {
        var files = MapSpriteExtractor.Extract(rom);
        byte[] json = files[MapSpriteFormat.JsonFile];
        var image = IndexedPng.Read(new MemoryStream(files[MapSpriteFormat.PngFile]), new(128, 128));
        var artwork = new MapObjectTileArtwork(image);
        var stock = MapSpriteArtwork.Read(new MemoryStream(json), new MemoryStream(files[MapSpriteFormat.PngFile]));
        AssertEqual(0, stock.StoredHighlightPixelCount, "stock highlight pixel table eliminated");
        AssertEqual(8192 - 151 * 32, stock.StoredArtworkByteCount, "highlight planar bytes absent from stored atlas");
        var native = new byte[8192];
        for (int i = 0; i < native.Length; i++) native[i] = HighlightTilePixelsRomByte(rom, 0xb6c000 + i);
        foreach (int destination in new[] { 0x4000, 0xc000 })
        {
            var vram = new SnesVram(); stock.LoadArtworkTo(new VramRegion(vram, PpuMemoryLayout.Vram).Slice(destination / sizeof(ushort), MapSpriteFormat.ByteCount / sizeof(ushort)));
            AssertTrue(vram.Bytes.Slice(destination, native.Length).SequenceEqual(native), "full original map OBJ upload bytes");
        }
        for (int tile = 0x5b; tile <= 0x5e; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            int expected = 0;
            for (int plane = 0; plane < 4; plane++)
                expected |= ((native[tile * 32 + plane / 2 * 16 + y * 2 + plane % 2] >> (7 - x)) & 1) << plane;
            AssertEqual((byte)expected, artwork.HighlightPixel(tile, new(checked((ushort)x), checked((ushort)y))), "original highlight pixel geometry");
        }
        foreach (int invalid in new[] { -1, 0x5a, 0x5f, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => artwork.HighlightPixel(invalid, new(0, 0)), "invalid highlight tile");
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => artwork.HighlightPixel(0x5b, new(unchecked((ushort)invalid), 0)), "invalid highlight pixel X");
            AssertThrows<ArgumentOutOfRangeException>(() => artwork.HighlightPixel(0x5b, new(0, unchecked((ushort)invalid))), "invalid highlight pixel Y");
        }
        var unchanged = new SnesVram();
        AssertThrows<ArgumentOutOfRangeException>(() => stock.LoadArtworkTo(new VramRegion(unchanged, PpuMemoryLayout.Vram).Slice(0, 1)), "invalid full transfer destination");
        AssertTrue(unchanged.Bytes.All(value => value == 0), "invalid transfer makes no partial writes");
    }

    /// <summary>Reads a native map OBJ character byte for the highlight-pixel oracle.</summary>
    private static byte HighlightTilePixelsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
