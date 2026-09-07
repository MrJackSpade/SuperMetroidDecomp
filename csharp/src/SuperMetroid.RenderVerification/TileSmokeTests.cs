using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static class TileSmokeTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        VerifyFourthPlaneByteSelection(device, renderer);
        var random = new Random(32111);
        byte[] bytes = new byte[SnesPpuLayout.VramByteCount]; random.NextBytes(bytes);
        ushort[] palette = Enumerable.Range(0, SnesPpuLayout.CgramColorCount).Select(_ => (ushort)random.Next(32768)).ToArray();
        var memory = new PpuMemorySnapshot(bytes, palette, new byte[SnesPpuLayout.OamUploadByteCount], 0);
        int count = 0;
        foreach (int width in new[] { 32, 64 })
        foreach (int height in new[] { 32, 64 })
        foreach (bool? priority in new bool?[] { null, false, true })
        foreach (ushort scroll in new ushort[] { 0, 7, 255, 511, ushort.MaxValue })
            Check(new RenderLayer[] { new Bg4BppRenderLayer(0x7ffe, 0x6000, scroll, scroll, width, height, priority) }, "4bpp geometry/priority/scroll");
        foreach (bool transparent in new[] { false, true })
        foreach (bool? priority in new bool?[] { null, false, true })
        foreach (ushort scroll in new ushort[] { 0, 1, 223, 255, ushort.MaxValue })
            Check(new RenderLayer[] { new Bg2BppViewportRenderLayer(0x7fff, 0x7ff8, scroll, transparent, priority) }, "2bpp viewport/priority/key");
        foreach (bool priority in new[] { false, true })
            Check(new RenderLayer[] { new Bg2BppRenderLayer(0x5800, 0x4000, 28, priority) }, "2bpp visible plane");
        for (byte brightness = 0; brightness <= 15; brightness++)
            Check(new RenderLayer[] {
                new Bg4BppRenderLayer(0x4000, 0, 3, 11, 64, 32, false),
                new Bg4BppRenderLayer(0x4800, 0x6000, 65535, 255, 32, 64, true),
                new Bg2BppViewportRenderLayer(0x5800, 0x4000, 13, true, null),
                new FixedColorAddRenderLayer(3, 17, 31) }, "ordered planes/fixed-color/fades", brightness);
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {count} tile/color-math GPU comparisons passed.");

        void Check(RenderLayer[] layers, string name, byte brightness = 15)
        {
            var frame = new RenderFrameSnapshot(new(++count, 1, 0), new LayeredRenderSnapshot(memory, layers, 3, brightness), new byte[] { 13, 11 });
            PixelComparison.Verify(frame, SoftwareFrameSnapshotRenderer.Render(frame), renderer.RenderForReadback(frame), $"{device.Kind}: {name} #{count}");
        }
    }

    private static void VerifyFourthPlaneByteSelection(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var vram = new byte[SnesPpuLayout.VramByteCount];
        // A flipped tile's bottom row puts plane three in the high byte of a packed
        // uint. The adjacent second byte is deliberately different: the original
        // optimized shader selected it and produced palette index ten instead of two.
        for (int row = 0; row < 8; row++)
        {
            int address = (0x6000 + 4 * 16) * 2 + row * 2;
            vram[address] = 182; vram[address + 1] = 95;
            vram[address + 16] = 54; vram[address + 17] = 144;
        }
        vram[(0x6000 + 4 * 16) * 2 + 29] = 233;
        for (int i = 0; i < 1024; i++)
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(vram.AsSpan(0x4000 * 2 + i * 2), 0xec04);
        var palette = new ushort[SnesPpuLayout.CgramColorCount];
        palette[3 * 16 + 2] = 31; palette[3 * 16 + 10] = 31 << 10;
        var memory = new PpuMemorySnapshot(vram, palette, new byte[SnesPpuLayout.OamUploadByteCount], 0);
        var frame = new RenderFrameSnapshot(new(1, 1, 0), new LayeredRenderSnapshot(memory,
            new RenderLayer[] { new Bg4BppRenderLayer(0x4000, 0x6000, 0, 0, 32, 32, null) }, 3, 15));
        var expected = SoftwareFrameSnapshotRenderer.Render(frame);
        if (expected[0] != new SuperMetroid.Core.Assets.Rgba32(255, 0, 0))
            throw new InvalidOperationException("Fourth-plane fixture no longer selects color two.");
        PixelComparison.Verify(frame, expected, renderer.RenderForReadback(frame), $"{device.Kind}: fourth-plane byte selection");
    }
}
