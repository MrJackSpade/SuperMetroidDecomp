using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Menus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Menus;

namespace SuperMetroid.Verification.Frontend.Menus;

/// <summary>Checks calculated menu shoulder-highlight pixels and the stock atlas upload.</summary>
internal static class MenuShoulderHighlightPixelsVerification
{
    /// <summary>Compares stock planar tiles with retail VRAM data and checks input bounds.</summary>
    internal static void VerifyMenuShoulderHighlightPixels(ISnesAddressSpace rom)
    {
        int[] tiles = [0x3c, 0x3d, 0x43, 0x51];
        var files = MapSpriteExtractor.Extract(rom);
        byte[] json = files[MapSpriteFormat.JsonFile];
        var image = IndexedPng.Read(new MemoryStream(files[MapSpriteFormat.PngFile]), new(128, 128));
        var highlight = new MenuShoulderHighlightArtwork(image);
        AssertEqual(0, highlight.StoredEditCount, "all stock highlight frame and quadratic cutout are calculated without retained pixel exceptions");
        var stock = MapSpriteArtwork.Read(new MemoryStream(json), new MemoryStream(files[MapSpriteFormat.PngFile]));
        AssertEqual(3360, stock.StoredArtworkByteCount, "highlight planar tiles absent from retained atlas");
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
            AssertEqual((byte)expected, highlight.Pixel(tile, new(checked((ushort)x), checked((ushort)y))), "original highlight frame, quadratic contour and transparency");
        }
        foreach (int invalid in new[] { -1, 0x3b, 0x3e, 0x42, 0x44, 0x50, 0x52, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => highlight.Pixel(invalid, new(0, 0)), "invalid highlight tile");
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => highlight.Pixel(0x3c, new(unchecked((ushort)invalid), 0)), "invalid highlight X");
            AssertThrows<ArgumentOutOfRangeException>(() => highlight.Pixel(0x3c, new(0, unchecked((ushort)invalid))), "invalid highlight Y");
        }
    }
}
