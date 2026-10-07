using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rom;
using SuperMetroid.AssetExtraction;

internal static partial class AssetTools
{
    private static void ExportEndingGunshipPaletteEvidence()
    {
        var rom = LoadRepositoryRom();
        byte[] tiles = RomDataReader.Decompress(rom, 0x95a82f, maximumOutputBytes: 0x4000);
        byte[] maps = RomDataReader.Decompress(rom, 0x96fe69, maximumOutputBytes: 0x1000);
        const int width = 1024, height = 256;
        var pixels = new byte[width * height];
        int left = width, right = 0, top = height, bottom = 0;
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            // Native ending staging preserves only map bytes0..2FF and fills the
            // rest with tile8C; later Ceres artwork in the source is not displayed.
            int cell = y / 8 * 128 + x / 8;
            int tile = cell < 0x300 ? maps[cell] : 0x8c;
            byte pixel = tiles[tile * 64 + (y & 7) * 8 + (x & 7)];
            if (pixel is >= 0x50 and <= 0x5f)
            {
                pixels[y * width + x] = pixel;
                left = Math.Min(left, x); right = Math.Max(right, x);
                top = Math.Min(top, y); bottom = Math.Max(bottom, y);
            }
        }
        if (left > right) throw new InvalidDataException("Original map contains no gunship palette pixels.");
        int croppedWidth = right - left + 1, croppedHeight = bottom - top + 1;
        var cropped = new byte[croppedWidth * croppedHeight];
        for (int y = 0; y < croppedHeight; y++)
            pixels.AsSpan((top + y) * width + left, croppedWidth).CopyTo(cropped.AsSpan(y * croppedWidth));
        var palette = new Rgba32[256];
        Array.Fill(palette, new Rgba32(0, 0, 0));
        for (int color = 0; color < 16; color++)
            palette[0x50 + color] = SnesGraphics.DecodeBgr555Color(
                RomDataReader.ReadWordFixedBank(rom, 0x8dd8dc + color * 2));
        string output = Path.GetFullPath("csharp/test-temp/1165-ending-gunship-original.png");
        PngWriter.WriteIndexedAsRgba(output, croppedWidth, croppedHeight, cropped, palette, 4);
        Console.WriteLine($"Original palette5 map bounds ({left},{top})..({right},{bottom}): {output}");
        for (int color = 0; color < 16; color++)
            Console.WriteLine($"slot{color}: loaded character pixels={tiles.Count(pixel => pixel == 0x50 + color)}, mapped pixels={pixels.Count(pixel => pixel == 0x50 + color)}");
    }

    // Static source inspection for #1165's unresolved explosion offsets; no scene execution.
    private static void ExportCeresPlacementArtwork()
    {
        var rom = LoadRepositoryRom();
        byte[] tiles = RomDataReader.Decompress(rom, 0x95a82f, maximumOutputBytes: 0x4000);
        byte[] maps = RomDataReader.Decompress(rom, 0x96fe69, maximumOutputBytes: 0x1000);
        var palette = SnesGraphics.DecodeBgr555Palette(RomDataReader.ReadFixedBank(rom, 0x8ce5e9, 512));
        const int width = 128, height = 96, magnification = 4;
        byte[] pixels = new byte[width * height * magnification * magnification];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            // Native Mode7 tilemap has 128 cells per row. C11B transfers source600..BFF
            // to its first1536 cells; each character is64 direct eight-bit pixel indices.
            int tile = maps[0x600 + (y / 8) * 128 + x / 8];
            byte pixel = tiles[tile * 64 + (y & 7) * 8 + (x & 7)];
            for (int dy = 0; dy < magnification; dy++)
            for (int dx = 0; dx < magnification; dx++)
                pixels[(y * magnification + dy) * width * magnification + x * magnification + dx] = pixel;
        }
        string path = Path.GetFullPath("csharp/test-temp/1165-ceres-station-source.png");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var output = File.Create(path);
        IndexedPng.Write(output, width * magnification, height * magnification, pixels, palette);
        Console.WriteLine($"Static original station map, source coordinates x0..127/y0..95 at4x: {path}");
    }
}
