using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void ExportCeresAsteroidArtwork(CartridgeImportAddressSpace rom)
    {
        string directory = Path.GetFullPath("csharp/test-temp/1165-ceres-asteroid-art");
        Directory.CreateDirectory(directory);
        byte[] graphics = RomDataReader.Decompress(rom, CeresFlightRomData.Assets.ObjectCharacters, 0x4000);
        byte[] tiles = SnesGraphics.DecodePlanarTiles(graphics, 4, 16, out int width, out _);
        var palette = new Rgba32[16];
        for (int index = 0; index < palette.Length; index++)
        {
            int address = CeresFlightRomData.Assets.Palette + (192 + index) * 2;
            int word = rom.ReadByte(address) | rom.ReadByte(address + 1) << 8;
            static byte Channel(int value) => (byte)(((value & 31) << 3) | ((value & 31) >> 2));
            palette[index] = new(Channel(word), Channel(word >> 5), Channel(word >> 10));
        }
        PngWriter.WriteIndexedAsRgba(Path.Combine(directory, "atlas.png"), width, tiles.Length / width, tiles, palette, 3);
        foreach (var source in new[]
        {
            (Pointer: (ushort)0x94f7, Count: 19, Name: "large-asteroids"),
            (Pointer: (ushort)0x90fe, Count: 16, Name: "small-asteroids"),
        })
        {
            var pixels = new byte[256 * 256];
            var parts = IntroCinematicSpriteFrameExtractor.Extract(rom, source.Pointer, source.Count, source.Name);
            foreach (SpriteVisualPart part in parts)
            for (int y = 0; y < part.Size; y++)
            for (int x = 0; x < part.Size; x++)
            {
                int sx = part.FlipX ? part.Size - 1 - x : x, sy = part.FlipY ? part.Size - 1 - y : y;
                int tile = part.TileRow * 16 + part.TileColumn + sy / 8 * 16 + sx / 8;
                byte color = tiles[(tile / 16 * 8 + sy % 8) * width + tile % 16 * 8 + sx % 8];
                int dx = 128 + part.OffsetX + x, dy = 128 + part.OffsetY + y;
                if (color != 0 && (uint)dx < 256 && (uint)dy < 256 && pixels[dy * 256 + dx] == 0)
                    pixels[dy * 256 + dx] = color;
            }
            PngWriter.WriteIndexedAsRgba(Path.Combine(directory, source.Name + ".png"), 256, 256, pixels, palette, 2);

        }
        Console.WriteLine(directory);
    }
}
