using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void ExportEndingRewardGestureArtwork(CartridgeImportAddressSpace rom, bool suitless)
    {
        string directory = Path.GetFullPath(suitless ? "csharp/test-temp/1165-ending-reward-hair-art" : "csharp/test-temp/1165-ending-reward-arm-art");
        Directory.CreateDirectory(directory);
        byte[] native = RomDataReader.Decompress(rom, suitless ? 0x97b957 : 0x979803, EndingCreditsRomData.Rendering.DecompressionLimit);
        byte[] tiles = SnesGraphics.DecodePlanarTiles(native.AsSpan(0, 0x4000), 4, 16, out int width, out _);
        Rgba32[] palette = Enumerable.Range(0, 16).Select(index => new Rgba32((byte)(index * 17), (byte)(index * 17), (byte)(index * 17))).ToArray();
        PngWriter.WriteIndexedAsRgba(Path.Combine(directory, "original-atlas.png"), width, tiles.Length / width, tiles, palette, 3);
        var sheet = new byte[512 * 320];
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int pose = 0; pose < 8; pose++)
        {
            ushort pointer = Word((suitless ? 0x8bed33 : 0x8bedd9) + pose * 4);
            var pixels = new byte[128 * 160];
            void Draw(ushort map, int originX, int originY)
            {
                foreach (var part in IntroCinematicSpriteFrameExtractor.Extract(rom, map, Word(0x8c0000 | map), "original reward art"))
                for (int y = 0; y < part.Size; y++)
                for (int x = 0; x < part.Size; x++)
                {
                    int sx = part.FlipX ? part.Size - 1 - x : x, sy = part.FlipY ? part.Size - 1 - y : y;
                    int tile = part.TileRow * 16 + part.TileColumn + sy / 8 * 16 + sx / 8;
                    byte color = tiles[(tile / 16 * 8 + sy % 8) * width + tile % 16 * 8 + sx % 8];
                    int dx = originX + part.OffsetX + x, dy = originY + part.OffsetY + y;
                    if (color != 0 && (uint)dx < 128 && (uint)dy < 160 && pixels[dy * 128 + dx] == 0)
                        pixels[dy * 128 + dx] = color;
                }
            }
            if (suitless)
            {
                // Both suitless actors use F143's identical origin; upper actor draws first.
                Draw(pointer, 64, 72);
                Draw(0xa243, 64, 72);
            }
            else
            {
                // Native F156/F169 place the helmet four pixels right and44 up from body/arm.
                Draw(0x9cac, 68, 28);
                Draw(0x9cc2, 64, 72);
                Draw(pointer, 64, 72);
            }
            for (int y = 0; y < 160; y++)
                pixels.AsSpan(y * 128, 128).CopyTo(sheet.AsSpan((pose / 4 * 160 + y) * 512 + pose % 4 * 128, 128));
            var parts = IntroCinematicSpriteFrameExtractor.Extract(rom, pointer, Word(0x8c0000 | pointer), "original gesture positions");
            var selected = suitless ? parts.Where(part => part.TileRow * 16 + part.TileColumn is 0x1b6 or 0x1b8) : parts.Take(parts.Length - 2);
            Console.WriteLine($"pose{pose + 1} map{pointer:X4}: " + string.Join("; ", selected
                .Select(part => $"tile{part.TileRow * 16 + part.TileColumn:X3} at({part.OffsetX},{part.OffsetY})")));
        }
        PngWriter.WriteIndexedAsRgba(Path.Combine(directory, "original-poses.png"), 512, 320, sheet, palette, 2);
        Console.WriteLine("Original poses1..4 top row,5..8 bottom row; grayscale color indexes, not scene palette.");
        Console.WriteLine(directory);
    }
}
