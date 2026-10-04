using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void ExportIntroCollisionArtwork(CartridgeImportAddressSpace rom)
    {
        string directory = Path.GetFullPath("csharp/test-temp/1165-intro-collision-art");
        Directory.CreateDirectory(directory);
        byte[] graphics = RomDataReader.Decompress(rom, 0x95f90e, 0x8000);
        byte[] tiles = SnesGraphics.DecodePlanarTiles(graphics, 4, 16, out int width, out _);
        byte[] maps = RomDataReader.Decompress(rom, 0x96ff14, 0x2000);
        var palette = new Rgba32[129];
        for (int index = 0; index < 128; index++)
        {
            int word = rom.ReadByte(0x8ce3e9 + 2 * index) | rom.ReadByte(0x8ce3ea + 2 * index) << 8;
            static byte Channel(int value) => (byte)(((value & 31) << 3) | ((value & 31) >> 2));
            palette[index] = new(Channel(word), Channel(word >> 5), Channel(word >> 10));
        }
        palette[128] = new Rgba32(255, 64, 64);
        for (int page = 0; page < 1; page++)
        {
            var pixels = new byte[256 * 256];
            for (int ty = 0; ty < 32; ty++)
            for (int tx = 0; tx < 32; tx++)
            {
                int offset = page * 0x800 + (ty * 32 + tx) * 2;
                int word = maps[offset] | maps[offset + 1] << 8;
                int tile = word & 1023, colors = (word >> 10 & 7) * 16;
                for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                {
                    int sx = (word & 0x4000) == 0 ? x : 7 - x;
                    int sy = (word & 0x8000) == 0 ? y : 7 - y;
                    pixels[(ty * 8 + y) * 256 + tx * 8 + x] =
                        (byte)(colors + tiles[(tile / 16 * 8 + sy) * width + tile % 16 * 8 + sx]);
                }
            }
            PngWriter.WriteIndexedAsRgba(Path.Combine(directory, $"page-{page}.png"), 256, 256, pixels, palette, 2);
            // Overlay the original left solid prefix, without consulting the replacement definitions.
            for (int row = 2; row <= 12; row++)
            {
                int solidWidth = 0;
                while (solidWidth < 16)
                {
                    int address = 0x8cbec3 + (row * 16 + solidWidth) * 2;
                    if ((rom.ReadByte(address) | rom.ReadByte(address + 1) << 8) != 0x8000) break;
                    solidWidth++;
                }
                int boundary = solidWidth * 16 - 1;
                for (int y = row * 16; y < (row + 1) * 16; y++) pixels[y * 256 + boundary] = 128;
                for (int x = Math.Max(0, boundary - 3); x <= boundary; x++) pixels[row * 16 * 256 + x] = 128;
            }
            PngWriter.WriteIndexedAsRgba(Path.Combine(directory, "left-collision-contour.png"), 256, 256, pixels, palette, 2);
        }
        Console.WriteLine(directory);
    }
}
