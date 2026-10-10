using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks that extracted menu small-font artwork preserves the native glyph pixels and that edited indexed pixels encode back to the expected VRAM tile data without disturbing neighboring artwork.</summary>
    /// <param name="rom">SNES address space containing the native menu sprite atlas used as the pixel and upload reference.</param>
    private static void VerifyMenuSmallFontPixels(ISnesAddressSpace rom)
    {
        var files = MapSpriteExtractor.Extract(rom);
        byte[] json = files[MapSpriteFormat.JsonFile];
        var image = IndexedPng.Read(new MemoryStream(files[MapSpriteFormat.PngFile]), 128, 128);
        var font = new MenuSmallFontArtwork(image);
        AssertEqual(328, font.StoredFaceByteCount, "one-bit authored silhouettes only");
        AssertEqual(3, font.StoredEditCount, "three independently reviewed B/K drawing choices");
        AssertTrue(font.HasPixelOverride(0x6b, 1, 7), "B's selected lower-left bevel");
        AssertTrue(font.HasPixelOverride(0x74, 6, 2), "first K arm trim");
        AssertTrue(font.HasPixelOverride(0x74, 5, 3), "second K arm trim");
        AssertMenuInkRegionEqual(image, 0x6b, 0, 4, 0x6d, 0, 4, 8, 4, "B/D lower contour");
        AssertMenuInkRegionEqual(image, 0x74, 5, 1, 0x83, 5, 2, 3, 3, "K/Z first diagonal neighborhood");
        AssertMenuInkRegionEqual(image, 0x74, 4, 2, 0x83, 4, 3, 3, 2, "K/Z second diagonal source/current rows");
        AssertEqual((byte)0, MenuEvidencePixel(image, 0x6b, 1, 7), "B omits lower-left shadow");
        AssertEqual((byte)13, MenuEvidencePixel(image, 0x6d, 1, 7), "D keeps shadow below identical lower ink");
        AssertEqual((byte)13, MenuEvidencePixel(image, 0x83, 6, 3), "Z keeps first matching diagonal shadow");
        AssertEqual((byte)13, MenuEvidencePixel(image, 0x83, 5, 4), "Z keeps second matching diagonal shadow");
        var stock = MapSpriteCatalog.Load(new MemoryStream(json), new MemoryStream(files[MapSpriteFormat.PngFile]));
        AssertEqual(3360, stock.StoredArtworkByteCount, "font planar tiles absent from retained atlas");
        var native = new byte[8192];
        for (int index = 0; index < native.Length; index++) native[index] = rom.ReadByte(0xb6c000 + index);
        foreach (int destination in new[] { 0x4000, 0xc000 })
        {
            var vram = new SnesVram(); stock.LoadArtworkTo(vram, destination);
            AssertTrue(vram.Bytes.Slice(destination, native.Length).SequenceEqual(native), "whole original menu atlas upload");
        }
        for (int tile = 0x60; tile <= 0x88; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            int expected = 0;
            for (int plane = 0; plane < 4; plane++)
                expected |= ((native[tile * 32 + plane / 2 * 16 + y * 2 + plane % 2] >> (7 - x)) & 1) << plane;
            AssertEqual((byte)expected, font.Pixel(tile, x, y), "original glyph foreground, shadow, retouch and transparency");
            byte[] pixels = (byte[])image.Pixels.Clone();
            int position = (tile / 16 * 8 + y) * 128 + tile % 16 * 8 + x;
            pixels[position] = (byte)((expected + 1) % 16);
            CheckEdited(pixels);
        }
        // Every color at one face and one shadow location, including introducing/removing foreground.
        foreach (int position in new[] { 48 * 128 + 2, 49 * 128 + 2 })
        for (byte value = 0; value < 16; value++)
        {
            byte[] pixels = (byte[])image.Pixels.Clone(); pixels[position] = value; CheckEdited(pixels);
        }
        byte[] all = (byte[])image.Pixels.Clone();
        for (int tile = 0x60; tile <= 0x88; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            int position = (tile / 16 * 8 + y) * 128 + tile % 16 * 8 + x;
            all[position] = (byte)((all[position] + 1) % 16);
        }
        CheckEdited(all);
        foreach (int invalid in new[] { -1, 0x5f, 0x89, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => font.Pixel(invalid, 0, 0), "invalid small font tile");
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => font.Pixel(0x60, invalid, 0), "invalid glyph X");
            AssertThrows<ArgumentOutOfRangeException>(() => font.Pixel(0x60, 0, invalid), "invalid glyph Y");
        }
        void CheckEdited(byte[] pixels)
        {
            using var png = new MemoryStream(); IndexedPng.Write(png, 128, 128, pixels, image.Palette); png.Position = 0;
            var edited = MapSpriteCatalog.Load(new MemoryStream(json), png);
            var vram = new SnesVram(); edited.LoadArtworkTo(vram, 0x4000);
            byte[] expected = SnesPlanarTileEncoder.Encode(pixels, 128, 128, 4);
            AssertTrue(vram.Bytes.Slice(0x4000, expected.Length).SequenceEqual(expected), "independent edits do not change neighboring shadows or glyphs");
        }
    }
}
