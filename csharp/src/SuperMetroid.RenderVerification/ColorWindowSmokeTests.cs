using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static class ColorWindowSmokeTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var random = new Random(32114);
        var vram = new byte[SnesPpuLayout.VramByteCount]; random.NextBytes(vram);
        ushort[] palette = Enumerable.Range(0, 256).Select(_ => (ushort)random.Next(32768)).ToArray();
        var memory = new PpuMemorySnapshot(vram, palette, new byte[SnesPpuLayout.OamUploadByteCount], 0);
        int count = 0;
        for (int sample = 0; sample < 64; sample++)
        {
            var lines = new ColorAddWindow[224];
            for (int y = 0; y < lines.Length; y++)
                lines[y] = (y % 5) switch {
                    0 => ColorAddWindow.Empty,
                    1 => new(0, 255, 255, 0, 1),
                    2 => new(255, 255, 0, 255, 127),
                    3 => new(0, 0, 127, 1, 255),
                    _ => new((byte)random.Next(256), (byte)random.Next(256),
                        (byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)) };
            Check(new ScanlineColorAddRenderLayer(lines));
        }
        Check(SnesGameplayFrameRenderer.CaptureCeresHaze(false));
        Check(SnesGameplayFrameRenderer.CaptureCeresHaze(true));
        foreach (int height in new[] { 32, 64 })
        foreach (int first in new[] { 0, 32, 223, 224 })
        foreach (ExpandedColorMathOperation operation in Enum.GetValues<ExpandedColorMathOperation>())
        foreach (ushort offset in new ushort[] { 0, 255, 511, ushort.MaxValue })
        {
            var scrolls = new BackgroundLineScroll[224];
            for (int y = 0; y < scrolls.Length; y++)
                scrolls[y] = new(unchecked((ushort)(offset + y * 7)), unchecked((ushort)(offset - y * 3)));
            var math = new Bg2BppColorMathRenderLayer(0x7ffe, 0x7ff8, height, first, operation, scrolls);
            var frame = new RenderFrameSnapshot(new(++count, 1, 0), new LayeredRenderSnapshot(memory,
                new RenderLayer[] { new Bg4BppRenderLayer(0x4000, 0x6000, 3, 11, 32, 32, null), math }, 3, 15));
            PixelComparison.Verify(frame, SoftwareFrameSnapshotRenderer.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: BG color math {count}, {operation}, height {height}, first {first}, offset {offset}");
        }
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {count} exact color-window comparisons passed.");

        void Check(ScanlineColorAddRenderLayer windows)
        {
            var frame = new RenderFrameSnapshot(new(++count, 1, 0), new LayeredRenderSnapshot(memory,
                new RenderLayer[] { new Bg4BppRenderLayer(0x4000, 0x6000, 3, 11, 32, 32, null),
                    windows, new FixedColorAddRenderLayer(1, 2, 3), windows }, 3, 13), new byte[] { 11 });
            PixelComparison.Verify(frame, SoftwareFrameSnapshotRenderer.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: color-window sample {count}");
        }
    }
}
