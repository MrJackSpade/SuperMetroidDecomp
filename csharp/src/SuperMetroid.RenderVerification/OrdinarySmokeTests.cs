using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static class OrdinarySmokeTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var random = new Random(32115);
        var vram = new byte[SnesPpuLayout.VramByteCount]; random.NextBytes(vram);
        var oam = new byte[SnesPpuLayout.OamUploadByteCount]; random.NextBytes(oam);
        ushort[] colors = Enumerable.Range(0, 256).Select(_ => (ushort)random.Next(32768)).ToArray();
        var memory = new PpuMemorySnapshot(vram, colors, oam, 128);
        ushort[] x = Enumerable.Range(0, 192).Select(i => unchecked((ushort)(65535 - i * 17))).ToArray();
        ushort[] y = Enumerable.Range(0, 192).Select(i => unchecked((ushort)(511 + i * 3))).ToArray();
        int count = 0;
        foreach (var geometry in new[] { (64,32), (32,64), (64,64) })
        for (int mask = 0; mask < 8; mask++)
        for (int hdma = 0; hdma < 4; hdma++)
        {
            var flags = (mask & 1) != 0 ? SnesMainScreenLayers.Bg1 : 0;
            if ((mask & 2) != 0) flags |= SnesMainScreenLayers.Bg2;
            if ((mask & 4) != 0) flags |= SnesMainScreenLayers.Obj;
            var registers = new OrdinaryGameplayRegisters(511, 255, 65535, 511,
                geometry.Item1, geometry.Item2, 0x4800, 0, 0x6000, 0x4000, flags);
            var layer = new OrdinaryGameplayRenderLayer(registers,
                (hdma & 1) != 0 ? x : default, (hdma & 2) != 0 ? y : default);
            var frame = new RenderFrameSnapshot(new(++count, 1, 0), new LayeredRenderSnapshot(memory,
                new RenderLayer[] { layer }, 3, 15));
            PixelComparison.Verify(frame, SoftwareFrameSnapshotRenderer.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: ordinary {count}, geometry {geometry}, layers {flags}, HDMA {hdma}");
            if (count == 96)
            {
                for (int warmup = 0; warmup < 8; warmup++) renderer.Render(frame);
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int sample = 0; sample < 32; sample++) renderer.Render(frame);
                long allocated = (GC.GetAllocatedBytesForCurrentThread() - before) / 32;
                Console.WriteLine($"{device.Kind}: ordinary CPU submission allocates {allocated} bytes/frame (capture/readback excluded).");
                if (allocated > 4096) throw new InvalidOperationException($"Ordinary submission allocates {allocated} bytes/frame; upload scratch must be reused.");
                GpuTimingTests.Run(device, renderer, frame);
            }
        }
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {count} exact ordinary gameplay comparisons passed.");
    }
}
