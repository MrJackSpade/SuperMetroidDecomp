using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rom;
using SuperMetroid.AssetExtraction;

internal static partial class Program
{
    // Static source inspection for #1165's unresolved explosion offsets; no scene execution.
    private static void ExportCeresPlacementArtwork()
    {
        var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Ceres art source revision");
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
