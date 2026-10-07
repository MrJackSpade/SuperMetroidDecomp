using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rom;

internal static partial class AssetTools
{
    private static void ExportZebesPlanetArtwork(CartridgeImportAddressSpace rom)
    {
        string directory = Path.GetFullPath("csharp/test-temp/1165-zebes-planet-art");
        Directory.CreateDirectory(directory);
        byte[] graphics = RomDataReader.Decompress(rom, CeresDestructionRomData.Assets.ZebesCharacters, 0x4000);
        byte[] tiles = SnesGraphics.DecodePlanarTiles(graphics, 4, 16, out int width, out _);
        var palette = Enumerable.Range(0, 16).Select(index => new Rgba32((byte)(index * 17), (byte)(index * 17), (byte)(index * 17))).ToArray();
        PngWriter.WriteIndexedAsRgba(Path.Combine(directory, "atlas.png"), width, tiles.Length / width, tiles, palette, 3);
        var pixels = new byte[256 * 256];
        var parts = IntroCinematicSpriteFrameExtractor.Extract(rom, 0x9558, 50, "zebes-planet");
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
        PngWriter.WriteIndexedAsRgba(Path.Combine(directory, "zebes-planet.png"), 256, 256, pixels, palette, 2);
        Console.WriteLine(directory);
    }
}
