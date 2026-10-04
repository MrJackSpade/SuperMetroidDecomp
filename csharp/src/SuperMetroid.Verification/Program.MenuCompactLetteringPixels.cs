using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMenuCompactLetteringPixels(ISnesAddressSpace rom)
    {
        int[] tiles = [0,1,2,3,4,5,6,7,8,9,0x10,0x12,0x13,0x14,0x15,0x16,0x18,0x19,0x20,0x32,0x44,0x45,0x53,0x54,0x55,0x56,0x5f,0xa5,0xa6,0xa7,0xa8,0xa9,0xaa,0xb7];
        var files = MapSpriteExtractor.Extract(rom);
        byte[] json = files[MapSpriteFormat.JsonFile];
        var image = IndexedPng.Read(new MemoryStream(files[MapSpriteFormat.PngFile]), 128, 128);
        var font = new MenuCompactLetteringArtwork(image);
        AssertEqual(80, font.StoredInkByteCount, "twenty shared six-row glyph silhouettes only");
        AssertEqual(1, font.StoredEditCount, "only the unresolved Tourian T-cap pixel remains pending; EXIT/SELECT/START need no deviations");
        AssertTrue(font.HasPixelOverride(8, 2, 2), "the pending source pixel is Tourian's left T-cap corner");
        AssertTrue(!font.HasPixelOverride(0xb7, 6, 2), "EXIT's mirrored T-cap corner is calculated by the ordinary bevel");
        AssertTrue(!font.HasPixelOverride(0xaa, 0, 4), "START R's enclosed shadow gap is calculated without an override");
        var stock = MapSpriteCatalog.Load(new MemoryStream(json), new MemoryStream(files[MapSpriteFormat.PngFile]));
        AssertEqual(3360, stock.StoredArtworkByteCount, "lettering planar tiles absent from retained atlas");
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
            AssertEqual((byte)expected, font.Pixel(tile, x, y), "original letter ink, outline, trim and transparency");
            byte[] pixels = (byte[])image.Pixels.Clone();
            int position = (tile / 16 * 8 + y) * 128 + tile % 16 * 8 + x;
            pixels[position] = (byte)((expected + 1) % 16);
            CheckEdited(pixels);
        }
        // Every color at an strip-edge ink pixel, neighboring outline and trimmed corner, including introducing/removing foreground.
        foreach (int position in new[] { 128 + 7, 8, 0, 81 * 128 + 40, 82 * 128 + 72 })
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
        foreach (int invalid in new[] { -1, 0xa, 0x11, 0x17, 0x52, 0x57, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => font.Pixel(invalid, 0, 0), "invalid elevator lettering tile");
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => font.Pixel(0, invalid, 0), "invalid glyph X");
            AssertThrows<ArgumentOutOfRangeException>(() => font.Pixel(0, 0, invalid), "invalid glyph Y");
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
