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

/// <summary>Checks calculated menu panel pixels against native tiles.</summary>
internal static class MenuPanelPixelsVerification
{
    /// <summary>Compares stock atlas uploads and every panel pixel, then checks invalid coordinates.</summary>
    internal static void VerifyMenuPanelPixels(ISnesAddressSpace rom)
    {
        int[] tiles = [0x57, 0x58, 0x59, 0x5a];
        var files = MapSpriteExtractor.Extract(rom);
        byte[] json = files[MapSpriteFormat.JsonFile];
        var image = IndexedPng.Read(new MemoryStream(files[MapSpriteFormat.PngFile]), new(128, 128));
        var panel = new MenuPanelTileArtwork(image);
        AssertEqual(0, panel.StoredEditCount, "all stock panel bands are calculated without retained pixel exceptions");
        var stock = MapSpriteArtwork.Read(new MemoryStream(json), new MemoryStream(files[MapSpriteFormat.PngFile]));
        AssertEqual(3360, stock.StoredArtworkByteCount, "panel planar tiles absent from retained atlas");
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
            AssertEqual((byte)expected, panel.Pixel(tile, new(checked((ushort)x), checked((ushort)y))), "original panel border, highlight and transparency");
        }
        foreach (int invalid in new[] { -1, 0x56, 0x5b, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => panel.Pixel(invalid, new(0, 0)), "invalid panel tile");
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => panel.Pixel(0x57, new(unchecked((ushort)invalid), 0)), "invalid panel X");
            AssertThrows<ArgumentOutOfRangeException>(() => panel.Pixel(0x57, new(0, unchecked((ushort)invalid))), "invalid panel Y");
        }
    }
}
