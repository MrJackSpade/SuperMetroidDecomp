using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMenuLargeFontPixels(ISnesAddressSpace rom)
    {
        int[] tiles = [0x0a,0x0b,0x0c,0x0d,0x0e,0x0f,0x11,0x17,0x1a,0x1b,0x1c,0x1d,0x1e,0x1f,0x21,0x22,0x23,0x24,0x25,0x26,0x27,0x2b,0x2c,0x2d,0x2f,0x30,0x31,0x33,0x34,0x35,0x36,0x37,0x38,0x39,0x3a,0x3b,0x3e,0x3f,0x40,0x41,0x42,0x50,0x52];
        var files = MapSpriteExtractor.Extract(rom);
        byte[] json = files[MapSpriteFormat.JsonFile];
        var image = IndexedPng.Read(new MemoryStream(files[MapSpriteFormat.PngFile]), 128, 128);
        var font = new MenuLargeFontArtwork(image);
        AssertEqual(336, font.StoredFaceByteCount, "one-bit authored silhouettes only");
        AssertEqual(3, font.StoredEditCount, "only three unresolved T/W/N shadow differences remain");
        AssertTrue(font.HasPixelOverride(0x11, 3, 6), "T terminal addition remains pending");
        AssertTrue(font.HasPixelOverride(0x2f, 4, 7), "W boundary removal remains pending");
        AssertTrue(font.HasPixelOverride(0x37, 1, 6), "N edge removal remains pending");
        var stock = MapSpriteCatalog.Load(new MemoryStream(json), new MemoryStream(files[MapSpriteFormat.PngFile]));
        AssertEqual(3360, stock.StoredArtworkByteCount, "font planar tiles absent from retained atlas");
        var native = new byte[8192];
        for (int index = 0; index < native.Length; index++) native[index] = rom.ReadByte(0xb6c000 + index);
        foreach (int destination in new[] { 0x4000, 0xc000 })
        {
            var vram = new SnesVram(); stock.LoadArtworkTo(vram, destination);
            AssertTrue(vram.Bytes.Slice(destination, native.Length).SequenceEqual(native), "whole original menu atlas upload");
        }
        foreach (int tile in tiles)
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
        // Every color at an upper-half face and a lower-half boundary shadow, including introducing/removing foreground.
        foreach (int position in new[] { 7 * 128 + 10 * 8 + 1, 8 * 128 + 10 * 8 + 3, 15 * 8 })
        for (byte value = 0; value < 16; value++)
        {
            byte[] pixels = (byte[])image.Pixels.Clone(); pixels[position] = value; CheckEdited(pixels);
        }
        byte[] all = (byte[])image.Pixels.Clone();
        foreach (int tile in tiles)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            int position = (tile / 16 * 8 + y) * 128 + tile % 16 * 8 + x;
            all[position] = (byte)((all[position] + 1) % 16);
        }
        CheckEdited(all);
        foreach (int invalid in new[] { -1, 0x10, 0x18, 0x3c, 0x43, 0x53, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => font.Pixel(invalid, 0, 0), "invalid title font tile");
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => font.Pixel(0x0a, invalid, 0), "invalid glyph X");
            AssertThrows<ArgumentOutOfRangeException>(() => font.Pixel(0x0a, 0, invalid), "invalid glyph Y");
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
