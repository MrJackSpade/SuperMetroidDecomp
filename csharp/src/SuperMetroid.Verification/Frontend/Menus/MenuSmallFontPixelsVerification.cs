using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using static SuperMetroid.Verification.Frontend.Menus.MenuFontEvidenceVerification;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Menus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Menus;

namespace SuperMetroid.Verification.Frontend.Menus;

/// <summary>Checks small menu glyph silhouettes, reviewed B/K shadow edits,
/// and faithful native VRAM reconstruction from the stock pixels.</summary>
internal static class MenuSmallFontPixelsVerification
{
    /// <summary>Compares every small-font pixel with bank-$B6 tiles and checks input bounds.</summary>
    internal static void VerifyMenuSmallFontPixels(ISnesAddressSpace rom)
    {
        var files = MapSpriteExtractor.Extract(rom);
        byte[] json = files[MapSpriteFormat.JsonFile];
        var image = IndexedPng.Read(new MemoryStream(files[MapSpriteFormat.PngFile]), new(128, 128));
        var font = new MenuSmallFontArtwork(image);
        AssertEqual(328, font.StoredFaceByteCount, "one-bit authored silhouettes only");
        AssertEqual(3, font.StoredEditCount, "three independently reviewed B/K drawing choices");
        AssertTrue(font.HasPixelOverride(0x6b, new(1, 7)), "B's selected lower-left bevel");
        AssertTrue(font.HasPixelOverride(0x74, new(6, 2)), "first K arm trim");
        AssertTrue(font.HasPixelOverride(0x74, new(5, 3)), "second K arm trim");
        AssertMenuInkRegionEqual(image, 0x6b, new(0, 4), 0x6d, new(0, 4), new(8, 4), "B/D lower contour");
        AssertMenuInkRegionEqual(image, 0x74, new(5, 1), 0x83, new(5, 2), new(3, 3), "K/Z first diagonal neighborhood");
        AssertMenuInkRegionEqual(image, 0x74, new(4, 2), 0x83, new(4, 3), new(3, 2), "K/Z second diagonal source/current rows");
        AssertEqual((byte)0, MenuEvidencePixel(image, 0x6b, new(1, 7)), "B omits lower-left shadow");
        AssertEqual((byte)13, MenuEvidencePixel(image, 0x6d, new(1, 7)), "D keeps shadow below identical lower ink");
        AssertEqual((byte)13, MenuEvidencePixel(image, 0x83, new(6, 3)), "Z keeps first matching diagonal shadow");
        AssertEqual((byte)13, MenuEvidencePixel(image, 0x83, new(5, 4)), "Z keeps second matching diagonal shadow");
        var stock = MapSpriteArtwork.Read(new MemoryStream(json), new MemoryStream(files[MapSpriteFormat.PngFile]));
        AssertEqual(3360, stock.StoredArtworkByteCount, "font planar tiles absent from retained atlas");
        var native = new byte[8192];
        for (int index = 0; index < native.Length; index++) native[index] = rom.ReadCartridgeByte(0xb6c000 + index);
        foreach (int destination in new[] { 0x4000, 0xc000 })
        {
            var vram = new SnesVram(); stock.LoadArtworkTo(new VramRegion(vram, PpuMemoryLayout.Vram).Slice(destination / sizeof(ushort), MapSpriteFormat.ByteCount / sizeof(ushort)));
            AssertTrue(vram.Bytes.Slice(destination, native.Length).SequenceEqual(native), "whole original menu atlas upload");
        }
        for (int tile = 0x60; tile <= 0x88; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            int expected = 0;
            for (int plane = 0; plane < 4; plane++)
                expected |= ((native[tile * 32 + plane / 2 * 16 + y * 2 + plane % 2] >> (7 - x)) & 1) << plane;
            AssertEqual((byte)expected, font.Pixel(tile, new(checked((ushort)x), checked((ushort)y))), "original glyph foreground, shadow, retouch and transparency");
        }
        foreach (int invalid in new[] { -1, 0x5f, 0x89, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => font.Pixel(invalid, new(0, 0)), "invalid small font tile");
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => font.Pixel(0x60, new(unchecked((ushort)invalid), 0)), "invalid glyph X");
            AssertThrows<ArgumentOutOfRangeException>(() => font.Pixel(0x60, new(0, unchecked((ushort)invalid))), "invalid glyph Y");
        }
    }
}
