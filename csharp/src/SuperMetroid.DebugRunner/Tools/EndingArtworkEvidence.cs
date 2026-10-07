using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using System.Text.Json;

internal static partial class AssetTools
{
    private static void ExportEndingExplosionArtworkEvidence(CartridgeImportAddressSpace rom)
    {
        string directory = Path.GetFullPath("csharp/test-temp/1165-ending-explosion-art");
        Directory.CreateDirectory(directory);
        byte[] native = RomDataReader.Decompress(rom, 0x988304, EndingCreditsRomData.Rendering.DecompressionLimit);
        byte[] tiles = SnesGraphics.DecodePlanarTiles(native.AsSpan(0, 0x4000), 4, 16, out int width, out _);
        Rgba32[] palette = Enumerable.Range(0, 16).Select(index => new Rgba32((byte)(index * 17), (byte)(index * 17), (byte)(index * 17))).ToArray();
        PngWriter.WriteIndexedAsRgba(Path.Combine(directory, "original-tile-indexes.png"), width, tiles.Length / width, tiles, palette, 3);
        foreach (var source in new[] { (0xa472, "glow"), (0xa4b0, "supernova-one"), (0xa516, "supernova-two"), (0xa28b, "starfield"), (0xa57c, "silhouette"), (0xa5e2, "afterglow") })
        {
            int address = 0x8c0000 | source.Item1;
            int count = rom.ReadByte(address) | rom.ReadByte(address + 1) << 8;
            var pixels = new byte[256 * 256];
            foreach (SpriteVisualPart part in IntroCinematicSpriteFrameExtractor.Extract(rom, (ushort)source.Item1, count, source.Item2))
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
            PngWriter.WriteIndexedAsRgba(Path.Combine(directory, source.Item2 + ".png"), 256, 256, pixels, palette, 2);
            Console.WriteLine($"{source.Item2}: {count} original parts, {pixels.Count(pixel => pixel != 0)} visible pixels");
            if (source.Item1 is 0xa472 or 0xa4b0 or 0xa516)
            {
                InspectEndingGlowPixelBands(pixels, source.Item2);
                InspectEndingGlowCircleBands(pixels, source.Item2);
                Console.WriteLine("Original upper-left24x24 color indexes (0 is transparent):");
                for (int y = 104; y < 128; y++)
                    Console.WriteLine(Convert.ToHexString(pixels.AsSpan(y * 256 + 104, 24)));
            }
        }
        Console.WriteLine(directory);
    }

    private static void ExportEndingLogoPaletteEvidence(CartridgeImportAddressSpace rom)
    {
        string directory = Path.GetFullPath("csharp/test-temp/1165-ending-logo-colors");
        Directory.CreateDirectory(directory);
        byte[] nativeTiles = RomDataReader.Decompress(rom, 0x99e089, EndingCreditsRomData.Rendering.DecompressionLimit);
        byte[] tiles = SnesGraphics.DecodePlanarTiles(nativeTiles.AsSpan(0, 0x2000), 4, 16, out int width, out _);
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        Rgba32[] Palette(int address) => Enumerable.Range(0, 16)
            .Select(color => SnesGraphics.DecodeBgr555Color(Word(address + color * 2))).ToArray();
        Rgba32[] spritePalette = Palette(0x8cefe9), backgroundPalette = Palette(0x8cf1e9);
        var spritePixels = new byte[256 * 256];
        // Final OAM compositions and native landing/origin positions; no cinematic replay.
        foreach (var frame in new[] { (0, 138, 111), (1, 126, 127), (4, 129, 110), (7, 135, 128) })
        {
            EndingLogoSpriteFrameDefinition definition = EndingLogoSpriteDefinitions.Frames[frame.Item1];
            foreach (SpriteVisualPart part in IntroCinematicSpriteFrameExtractor.Extract(rom,
                definition.Pointer, definition.StockPartCount, definition.Name))
            for (int y = 0; y < part.Size; y++)
            for (int x = 0; x < part.Size; x++)
            {
                int sx = part.FlipX ? part.Size - 1 - x : x, sy = part.FlipY ? part.Size - 1 - y : y;
                int tile = part.TileRow * 16 + part.TileColumn + sy / 8 * 16 + sx / 8;
                byte color = tiles[(tile / 16 * 8 + sy % 8) * width + tile % 16 * 8 + sx % 8];
                int dx = frame.Item2 + part.OffsetX + x, dy = frame.Item3 + part.OffsetY + y;
                if (color != 0 && (uint)dx < 256 && (uint)dy < 256) spritePixels[dy * 256 + dx] = color;
            }
        }
        byte[] nativeMap = RomDataReader.Decompress(rom, 0x99ecc4, EndingCreditsRomData.Rendering.DecompressionLimit);
        var backgroundPixels = new byte[256 * 256];
        for (int cell = 0; cell < 1024; cell++)
        {
            int word = nativeMap[cell * 2] | nativeMap[cell * 2 + 1] << 8;
            int tile = word & 0x3ff;
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                int sx = (word & 0x4000) != 0 ? 7 - x : x, sy = (word & 0x8000) != 0 ? 7 - y : y;
                backgroundPixels[(cell / 32 * 8 + y) * 256 + cell % 32 * 8 + x] =
                    tiles[(tile / 16 * 8 + sy) * width + tile % 16 * 8 + sx];
            }
        }
        PngWriter.WriteIndexedAsRgba(Path.Combine(directory, "original-sprite-logo.png"), 256, 256, spritePixels, spritePalette, 2);
        PngWriter.WriteIndexedAsRgba(Path.Combine(directory, "original-background-logo.png"), 256, 256, backgroundPixels, backgroundPalette, 2);
        Console.WriteLine(directory);
        for (int color = 0; color < 16; color++)
            Console.WriteLine($"slot{color}: OBJ=${Word(0x8cefe9 + color * 2):X4}, pixels={spritePixels.Count(p => p == color)}; BG=${Word(0x8cf1e9 + color * 2):X4}, pixels={backgroundPixels.Count(p => p == color)}");
    }

    private static void InspectEndingGlowPixelBands(byte[] pixels, string name)
    {
        // Source inspection only: measure lattice depth from the supplied original mask.
        var depths = new int[pixels.Length];
        var pending = new Queue<int>();
        for (int index = 0; index < pixels.Length; index++)
        {
            depths[index] = pixels[index] == 0 ? 0 : int.MaxValue;
            if (pixels[index] == 0) pending.Enqueue(index);
        }
        while (pending.TryDequeue(out int index))
        {
            int x = index % 256, y = index / 256;
            void Visit(int next)
            {
                if (depths[next] <= depths[index] + 1) return;
                depths[next] = depths[index] + 1;
                pending.Enqueue(next);
            }
            if (x > 0) Visit(index - 1);
            if (x < 255) Visit(index + 1);
            if (y > 0) Visit(index - 256);
            if (y < 255) Visit(index + 256);
        }
        int rimDifferences = 0;
        var baselineDifferences = new List<string>();
        for (int y = 104; y < 128; y++)
        for (int x = 104; x < 128; x++)
        {
            int index = y * 256 + x;
            if ((pixels[index] == 14) != (depths[index] == 1)) rimDifferences++;
            if (name != "glow") continue;
            int band = depths[index] switch { 0 => 0, 1 => 14, 2 => 13, 3 => 12, 4 => 10, _ => 9 };
            if (band != pixels[index]) baselineDifferences.Add($"({x - 104},{y - 104}) depth{depths[index]} original{pixels[index]:X} baseline{band:X}");
        }
        Console.WriteLine($"{name}: outer-rim versus four-neighbor depth1 differences={rimDifferences}");
        if (name == "glow") Console.WriteLine("glow base-band differences: " + string.Join(", ", baselineDifferences));
        foreach (byte color in pixels.Where(pixel => pixel != 0).Distinct().Order())
            Console.WriteLine($"color{color:X}: lattice depths " + string.Join(",", Enumerable.Range(0, pixels.Length)
                .Where(index => pixels[index] == color).Select(index => depths[index]).Distinct().Order()));
    }

    private static void InspectEndingGlowCircleBands(byte[] pixels, string name)
    {
        // A symmetric circle centered at(c,c) requires every inside pixel to have
        // smaller squared distance than every outside pixel. Subtracting the two
        // distances cancels c*c and gives an exact linear bound on c. This reports
        // feasibility, not a historical generator or a production conversion.
        foreach (int innerLimit in new[] { 9, 10, 12, 13, 14 })
        {
            var inside = new List<(int X, int Y, int Sum, int Squared)>();
            var outside = new List<(int X, int Y, int Sum, int Squared)>();
            for (int y = 0; y < 24; y++)
            for (int x = 0; x < 24; x++)
            {
                byte color = pixels[(104 + y) * 256 + 104 + x];
                var point = (x, y, x + y, x * x + y * y);
                (color > 0 && color <= innerLimit ? inside : outside).Add(point);
            }
            double lower = double.NegativeInfinity, upper = double.PositiveInfinity;
            int contradictions = 0;
            string? witness = null;
            foreach (var included in inside)
            foreach (var excluded in outside)
            {
                int constant = included.Squared - excluded.Squared;
                int coefficient = 2 * (included.Sum - excluded.Sum);
                if (coefficient > 0) lower = Math.Max(lower, (double)constant / coefficient);
                else if (coefficient < 0) upper = Math.Min(upper, (double)constant / coefficient);
                else if (constant >= 0)
                {
                    contradictions++;
                    witness ??= $"inside({included.X},{included.Y}) versus outside({excluded.X},{excluded.Y})";
                }
            }
            Console.WriteLine($"{name} inner<={innerLimit:X}: circle center interval({lower:R},{upper:R}), equal-sum contradictions={contradictions}; {witness}");
        }
    }
}
