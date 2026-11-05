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

/// <summary>Checks calculated thin menu border pixels against native tiles.</summary>
internal static class MenuThinBorderPixelsVerification
{
    /// <summary>Compares stock atlas uploads and every border pixel, then checks invalid coordinates.</summary>
    internal static void VerifyMenuThinBorderPixels(ISnesAddressSpace rom)
    {
        int[] tiles = [0xad, 0xae];
        var files = MapSpriteExtractor.Extract(rom);
        byte[] json = files[MapSpriteFormat.JsonFile];
        var image = IndexedPng.Read(new MemoryStream(files[MapSpriteFormat.PngFile]), new(128, 128));
        var border = new MenuThinBorderArtwork(image);
        AssertEqual(0, border.StoredEditCount, "all stock border lines are calculated without retained pixel exceptions");
        var stock = MapSpriteArtwork.Read(new MemoryStream(json), new MemoryStream(files[MapSpriteFormat.PngFile]));
        AssertEqual(3360, stock.StoredArtworkByteCount, "border planar tiles absent from retained atlas");
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
            AssertEqual((byte)expected, border.Pixel(tile, new(checked((ushort)x), checked((ushort)y))), "original border line and transparency");
        }
        foreach (int invalid in new[] { -1, 0xac, 0xaf, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => border.Pixel(invalid, new(0, 0)), "invalid border tile");
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => border.Pixel(0xad, new(unchecked((ushort)invalid), 0)), "invalid border X");
            AssertThrows<ArgumentOutOfRangeException>(() => border.Pixel(0xad, new(0, unchecked((ushort)invalid))), "invalid border Y");
        }
    }
}
