using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks calculated map-marker tiles against native pixels.</summary>
internal static class MapMarkerPixelsVerification
{
    /// <summary>Checks the two file-map arrow glyphs.</summary>
    internal static void VerifyMapArrowPixels(ISnesAddressSpace rom) => VerifyMapMarkerPixelFamily(rom, [0x9d, 0x9e]);
    /// <summary>Checks the map position pulse glyph.</summary>
    internal static void VerifyMapPulsePixels(ISnesAddressSpace rom) => VerifyMapMarkerPixelFamily(rom, [0xaf]);
    /// <summary>Checks the defeated-boss marker glyph.</summary>
    internal static void VerifyDefeatedBossPixels(ISnesAddressSpace rom) => VerifyMapMarkerPixelFamily(rom, [0x9f]);

    /// <summary>Compares stock pixels and VRAM uploads, then checks coordinate bounds.</summary>
    private static void VerifyMapMarkerPixelFamily(ISnesAddressSpace rom, int[] tiles)
    {
        var files = MapSpriteExtractor.Extract(rom);
        byte[] json = files[MapSpriteFormat.JsonFile];
        var image = IndexedPng.Read(new MemoryStream(files[MapSpriteFormat.PngFile]), new(128, 128));
        var marker = new MapMarkerTileArtwork(image);
        AssertEqual(0, marker.StoredEditCount, "all stock marker pixels are calculated without retained exceptions");
        var stock = MapSpriteArtwork.Read(new MemoryStream(json), new MemoryStream(files[MapSpriteFormat.PngFile]));
        AssertEqual(3360, stock.StoredArtworkByteCount, "marker planar tiles absent from retained atlas");
        var native = new byte[8192];
        for (int index = 0; index < native.Length; index++) native[index] = rom.ReadCartridgeByte(0xb6c000 + index);
        foreach (int destination in new[] { 0x4000, 0xc000 })
        {
            var vram = new SnesVram(); stock.LoadArtworkTo(new VramRegion(vram, PpuMemoryLayout.Vram).Slice(destination / sizeof(ushort), MapSpriteFormat.ByteCount / sizeof(ushort)));
            AssertTrue(vram.Bytes.Slice(destination, native.Length).SequenceEqual(native), "whole original menu atlas upload");
        }
        foreach (int tile in tiles)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            int expected = 0;
            for (int plane = 0; plane < 4; plane++)
                expected |= ((native[tile * 32 + plane / 2 * 16 + y * 2 + plane % 2] >> (7 - x)) & 1) << plane;
            AssertEqual((byte)expected, marker.Pixel(tile, new(checked((ushort)x), checked((ushort)y))), "original marker geometry and transparency");
        }
        foreach (int invalid in new[] { -1, 0x9c, 0xa0, 0xae, 0xb0, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => marker.Pixel(invalid, new(0, 0)), "invalid marker tile");
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => marker.Pixel(tiles[0], new(unchecked((ushort)invalid), 0)), "invalid marker X");
            AssertThrows<ArgumentOutOfRangeException>(() => marker.Pixel(tiles[0], new(0, unchecked((ushort)invalid))), "invalid marker Y");
        }
    }
}
