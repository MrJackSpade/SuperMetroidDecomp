using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks calculated pixels and independent edits for the map-arrow tiles.</summary>
    /// <param name="rom">Cartridge address space supplying the original menu map atlas.</param>
    private static void VerifyMapArrowPixels(ISnesAddressSpace rom) => VerifyMapMarkerPixelFamily(rom, [0x9d, 0x9e]);

    /// <summary>Checks calculated pixels and independent edits for the pulsing map-marker tile.</summary>
    /// <param name="rom">Cartridge address space supplying the original menu map atlas.</param>
    private static void VerifyMapPulsePixels(ISnesAddressSpace rom) => VerifyMapMarkerPixelFamily(rom, [0xaf]);

    /// <summary>Checks calculated pixels and independent edits for the defeated-boss map marker.</summary>
    /// <param name="rom">Cartridge address space supplying the original menu map atlas.</param>
    private static void VerifyDefeatedBossPixels(ISnesAddressSpace rom) => VerifyMapMarkerPixelFamily(rom, [0x9f]);

    /// <summary>Verifies one map-marker tile family against native planar data and confirms edited pixels encode independently.</summary>
    /// <param name="rom">Cartridge address space containing the original 4bpp menu map atlas.</param>
    /// <param name="tiles">Tile indices whose calculated pixels and editable image regions are checked.</param>
    private static void VerifyMapMarkerPixelFamily(ISnesAddressSpace rom, int[] tiles)
    {
        var files = MapSpriteExtractor.Extract(rom);
        byte[] json = files[MapSpriteFormat.JsonFile];
        var image = IndexedPng.Read(new MemoryStream(files[MapSpriteFormat.PngFile]), 128, 128);
        var marker = new MapMarkerTileArtwork(image);
        AssertEqual(0, marker.StoredEditCount, "all stock marker pixels are calculated without retained exceptions");
        var stock = MapSpriteCatalog.Load(new MemoryStream(json), new MemoryStream(files[MapSpriteFormat.PngFile]));
        AssertEqual(3360, stock.StoredArtworkByteCount, "marker planar tiles absent from retained atlas");
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
            AssertEqual((byte)expected, marker.Pixel(tile, x, y), "original marker geometry and transparency");
            byte[] pixels = (byte[])image.Pixels.Clone();
            int position = (tile / 16 * 8 + y) * 128 + tile % 16 * 8 + x;
            pixels[position] = (byte)((expected + 1) % 16);
            CheckEdited(pixels);
        }
        // All colors at corner, interior and far-edge locations in every independently editable tile.
        foreach (int tile in tiles)
        foreach (int offset in new[] { 0, 3, 7 })
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
        foreach (int invalid in new[] { -1, 0x9c, 0xa0, 0xae, 0xb0, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => marker.Pixel(invalid, 0, 0), "invalid marker tile");
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => marker.Pixel(tiles[0], invalid, 0), "invalid marker X");
            AssertThrows<ArgumentOutOfRangeException>(() => marker.Pixel(tiles[0], 0, invalid), "invalid marker Y");
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
