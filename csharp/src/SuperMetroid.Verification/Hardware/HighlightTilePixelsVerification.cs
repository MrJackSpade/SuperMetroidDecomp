using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.Verification.Hardware;

/// <summary>Checks editable map highlight tiles against native OBJ bytes and exact VRAM edits.</summary>
internal static class HighlightTilePixelsVerification
{
    /// <summary>Checks stock pixels, independently edited highlight colors, transfer bounds, and resulting planar bytes.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyHighlightTilePixels(ISnesAddressSpace rom)
    {
        var files = MapSpriteExtractor.Extract(rom);
        byte[] json = files[MapSpriteFormat.JsonFile];
        var image = IndexedPng.Read(new MemoryStream(files[MapSpriteFormat.PngFile]), new(128, 128));
        var artwork = new MapObjectTileArtwork(image);
        var stock = MapSpriteCatalog.Load(new MemoryStream(json), new MemoryStream(files[MapSpriteFormat.PngFile]));
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
            byte[] pixels = (byte[])image.Pixels.Clone();
            int position = (tile / 16 * 8 + y) * 128 + tile % 16 * 8 + x;
            pixels[position] = (byte)((expected + 1) % 16);
            CheckEdited(pixels, 1);
        }
        // All legal color indexes at a fixed pixel, plus a fully independent custom region.
        int sample = 5 * 8 * 128 + 11 * 8;
        for (byte value = 0; value < 16; value++)
        {
            byte[] pixels = (byte[])image.Pixels.Clone(); pixels[sample] = value;
            CheckEdited(pixels, value == image.Pixels[sample] ? 0 : 1);
        }
        byte[] all = (byte[])image.Pixels.Clone();
        for (int tile = 0x5b; tile <= 0x5e; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            int position = (tile / 16 * 8 + y) * 128 + tile % 16 * 8 + x;
            all[position] = (byte)((all[position] + 1) % 16);
        }
        all[0] = (byte)((all[0] + 1) % 16); all[^1] = (byte)((all[^1] + 1) % 16);
        CheckEdited(all, 256);
        int cursorSample = (4 * 8 + 2) * 128 + 6 * 8 + 3;
        for (byte value = 0; value < 16; value++)
        {
            byte[] pixels = (byte[])image.Pixels.Clone(); pixels[cursorSample] = value;
            CheckEdited(pixels, value == image.Pixels[cursorSample] ? 0 : 1);
        }
        byte[] cursorEdit = (byte[])image.Pixels.Clone();
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            int position = (4 * 8 + y) * 128 + 6 * 8 + x;
            cursorEdit[position] = (byte)((cursorEdit[position] + 1) % 16);
        }
        CheckEdited(cursorEdit, 49);
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

        void CheckEdited(byte[] pixels, int expectedStored)
        {
            using var png = new MemoryStream(); IndexedPng.Write(png, new(128, 128), pixels, image.Colours); png.Position = 0;
            var edited = MapSpriteCatalog.Load(new MemoryStream(json), png);
            AssertEqual(expectedStored, edited.StoredHighlightPixelCount, "exact independent pixel capture count");
            var vram = new SnesVram(); edited.LoadArtworkTo(new VramRegion(vram, PpuMemoryLayout.Vram).Slice(0x4000 / sizeof(ushort), MapSpriteFormat.ByteCount / sizeof(ushort)));
            byte[] expected = SnesPlanarTileEncoder.Encode(pixels, new(128, 128), 4);
            AssertTrue(vram.Bytes.Slice(0x4000, expected.Length).SequenceEqual(expected), "every authored and unchanged atlas pixel reaches VRAM");
        }
    }

    /// <summary>Reads a native map OBJ character byte for the highlight-pixel oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for highlight tile pixels")]
    private static byte HighlightTilePixelsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
