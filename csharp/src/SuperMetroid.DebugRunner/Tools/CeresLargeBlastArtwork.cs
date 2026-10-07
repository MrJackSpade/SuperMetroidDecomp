using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rom;

internal static partial class AssetTools
{
    private static void ExportCeresLargeBlastArtwork(CartridgeImportAddressSpace rom)
    {
        string directory = Path.GetFullPath("csharp/test-temp/1165-ceres-large-blast-art");
        Directory.CreateDirectory(directory);
        byte[] graphics = RomDataReader.ReadFixedBank(rom, IntroCinematicRomData.Assets.IntroObjectCharacters, 0x1a00);
        byte[] tiles = SnesGraphics.DecodePlanarTiles(graphics, 4, 16, out int width, out _);
        var palette = Enumerable.Range(0, 16).Select(index => new Rgba32((byte)(index * 17), (byte)(index * 17), (byte)(index * 17))).ToArray();
        PngWriter.WriteIndexedAsRgba(Path.Combine(directory, "atlas.png"), width, tiles.Length / width, tiles, palette, 4);
        var pixels = new byte[128 * 32];
        for (int frame = 0; frame < 4; frame++)
        {
            var part = IntroCinematicSpriteFrameExtractor.Extract(rom, (ushort)(0x98d2 + 7 * frame), 1, "large-blast")[0];
            for (int y = 0; y < part.Size; y++)
            for (int x = 0; x < part.Size; x++)
            {
                int tile = part.TileRow * 16 + part.TileColumn + y / 8 * 16 + x / 8;
                byte color = tiles[(tile / 16 * 8 + y % 8) * width + tile % 16 * 8 + x % 8];
                pixels[(24 + part.OffsetY + y) * 128 + 32 * frame + 16 + part.OffsetX + x] = color;
            }
            Console.WriteLine($"frame {frame}: tile {part.TileRow * 16 + part.TileColumn:X}, offset ({part.OffsetX},{part.OffsetY})");
        }
        PngWriter.WriteIndexedAsRgba(Path.Combine(directory, "four-frames.png"), 128, 32, pixels, palette, 6);
        Console.WriteLine(directory);
    }
}