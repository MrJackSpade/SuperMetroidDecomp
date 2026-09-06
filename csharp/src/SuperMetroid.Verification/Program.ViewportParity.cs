using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyViewportTileRowParity()
    {
        var random = new Random(54);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        var words = new ushort[32768];
        for (int i = 0; i < words.Length; i++) words[i] = (ushort)random.Next(65536);
        vram.ExecuteWordTransfer(words, 0, 1);
        for (int i = 0; i < 256; i++) cgram.SetColor(i, (ushort)random.Next(32768));
        for (int test = 0; test < 120; test++)
        {
            int width = test % 2 == 0 ? 256 : 253, height = 31;
            var expected = new Rgba32[width * height];
            Array.Fill(expected, new Rgba32(17, 29, 43, 255));
            var actual = (Rgba32[])expected.Clone();
            ushort map = (ushort)random.Next(32768), chars = (ushort)(random.Next(16) * 2048);
            ushort x = (ushort)random.Next(65536), y = (ushort)random.Next(65536);
            int columns = test % 4 < 2 ? 32 : 64, rows = test % 2 == 0 ? 32 : 64;
            bool? priority = test % 3 == 0 ? null : test % 3 == 1;
            ushort[]? lineScroll = test % 5 == 0 ? Enumerable.Range(0, height).Select(_ => (ushort)random.Next(65536)).ToArray() : null;
            ViewportReference.Composite4BppViewport(expected, vram, cgram, map, chars, x, y, width, height, columns, rows, lineScroll, priority);
            SnesBgTilemapRenderer.Composite4BppViewport(actual, vram, cgram, map, chars, x, y, width, height, columns, rows, lineScroll, priority);
            AssertTrue(actual.AsSpan().SequenceEqual(expected), $"tile-row viewport exact pixels case {test}");
        }
        Console.WriteLine("  Viewport: 120 scalar-reference cases match flips, priority, scroll, wrapping and clipped tile rows.");
        var benchmarkPixels = new Rgba32[256 * 224];
        var clock = System.Diagnostics.Stopwatch.StartNew();
        for (int frame = 0; frame < 50; frame++)
            ViewportReference.Composite4BppViewport(benchmarkPixels, vram, cgram, 0x3000, 0, 3, 5, 256, 224);
        double scalarMilliseconds = clock.Elapsed.TotalMilliseconds / 50;
        clock.Restart();
        for (int frame = 0; frame < 50; frame++)
            SnesBgTilemapRenderer.Composite4BppViewport(benchmarkPixels, vram, cgram, 0x3000, 0, 3, 5, 256, 224);
        Console.WriteLine($"  Viewport benchmark: scalar={scalarMilliseconds:F3}ms tile-row={clock.Elapsed.TotalMilliseconds / 50:F3}ms per 256x224 plane (informational, not a flaky timing assertion).");
    }
}
