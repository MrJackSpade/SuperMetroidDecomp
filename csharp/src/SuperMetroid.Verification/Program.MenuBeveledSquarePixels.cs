using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMenuBeveledSquarePixels(ISnesAddressSpace rom)
    {
        int[] tiles = [0x98, 0x99];
        var files = MapSpriteExtractor.Extract(rom);
        byte[] json = files[MapSpriteFormat.JsonFile];
        var image = IndexedPng.Read(new MemoryStream(files[MapSpriteFormat.PngFile]), 128, 128);
        var square = new MenuBeveledSquareArtwork(image);
        AssertEqual(0, square.StoredEditCount, "all stock square faces and bevels are calculated without retained pixel exceptions");
        var stock = MapSpriteCatalog.Load(new MemoryStream(json), new MemoryStream(files[MapSpriteFormat.PngFile]));
        AssertEqual(3680, stock.StoredArtworkByteCount, "square planar tiles absent from retained atlas");
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
            AssertEqual((byte)expected, square.Pixel(tile, x, y), "original square face, bevel and transparency");
            byte[] pixels = (byte[])image.Pixels.Clone();
            int position = (tile / 16 * 8 + y) * 128 + tile % 16 * 8 + x;
            pixels[position] = (byte)((expected + 1) % 16);
            CheckEdited(pixels);
        }
        // Every color at highlight, face and shadow locations in both colorways.
        foreach (int tile in tiles)
        foreach (int offset in new[] { 1, 3, 6 })
        for (byte value = 0; value < 16; value++)
        {
            int position = (tile / 16 * 8 + offset) * 128 + tile % 16 * 8 + offset;
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
        foreach (int invalid in new[] { -1, 0x97, 0x9a, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => square.Pixel(invalid, 0, 0), "invalid square tile");
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => square.Pixel(0x98, invalid, 0), "invalid square X");
            AssertThrows<ArgumentOutOfRangeException>(() => square.Pixel(0x98, 0, invalid), "invalid square Y");
        }
        void CheckEdited(byte[] pixels)
        {
            using var png = new MemoryStream(); IndexedPng.Write(png, 128, 128, pixels, image.Palette); png.Position = 0;
            var edited = MapSpriteCatalog.Load(new MemoryStream(json), png);
            var vram = new SnesVram(); edited.LoadArtworkTo(vram, 0x4000);
            byte[] expected = SnesPlanarTileEncoder.Encode(pixels, 128, 128, 4);
            AssertTrue(vram.Bytes.Slice(0x4000, expected.Length).SequenceEqual(expected), "independent edits do not change neighboring borders or other artwork");
        }
    }
}
