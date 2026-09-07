using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static class Mode7SmokeTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var random = new Random(32113);
        var vram = new byte[SnesPpuLayout.VramByteCount];
        var oam = new byte[SnesPpuLayout.OamUploadByteCount];
        random.NextBytes(vram); random.NextBytes(oam);
        ushort[] palette = Enumerable.Range(0, 256).Select(_ => (ushort)random.Next(32768)).ToArray();
        var memory = new PpuMemorySnapshot(vram, palette, oam, 128);
        int count = 0;
        foreach (short scale in new short[] { 0, 1, 255, 256, 512, -256, short.MinValue, short.MaxValue })
        foreach (short offset in new short[] { -1024, -1, 0, 1023, short.MinValue, short.MaxValue })
        foreach (bool fill in new[] { false, true })
        {
            var registers = new Mode7RenderRegisters(scale, 113, -113, scale, 128, 112, offset, offset, fill);
            var frame = new RenderFrameSnapshot(new(++count, 1, 0),
                new Mode7ObjRenderSnapshot(memory, registers, 3, 13), new byte[] { 11 });
            Check(frame);
            // Exercise Mode 7 as an ordered layer too, not just the special packet shape.
            Check(new RenderFrameSnapshot(new(++count, 1, 0), new LayeredRenderSnapshot(memory,
                new RenderLayer[] { new ObjRenderLayer(), new Mode7RenderLayer(registers),
                    new FixedColorAddRenderLayer(3, 7, 11) }, 3, 15)));
        }
        Check(new RenderFrameSnapshot(new(++count, 1, 0), new Mode7ObjRenderSnapshot(memory, null, 3, 15)));
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {count} exact Mode 7 comparisons passed.");

        void Check(RenderFrameSnapshot frame) => PixelComparison.Verify(frame,
            SoftwareFrameSnapshotRenderer.Render(frame), renderer.RenderForReadback(frame),
            $"{device.Kind}: Mode 7 sample {count}");
    }
}
