using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static class ObjectSmokeTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var random = new Random(32112);
        var vram = new byte[SnesPpuLayout.VramByteCount];
        var oam = new byte[SnesPpuLayout.OamUploadByteCount];
        random.NextBytes(vram);
        random.NextBytes(oam);
        ushort[] palette = Enumerable.Range(0, SnesPpuLayout.CgramColorCount)
            .Select(_ => (ushort)random.Next(32768)).ToArray();
        int cases = 0;
        for (int size = 0; size < 8; size++)
        foreach (int count in new[] { 0, 1, 37, 128 })
        foreach (byte selection in new byte[] { 0, 11, 31 })
        {
            var memory = new PpuMemorySnapshot(vram, palette, oam, count);
            byte obsel = (byte)((size << 5) | selection);
            Check(memory, obsel, new RenderLayer[] { new ObjRenderLayer() });
            Check(memory, obsel, new RenderLayer[] {
                new ObjPriorityRenderLayer(0),
                new Bg4BppRenderLayer(0x4000, 0x6000, 3, 11, 32, 32, false),
                new ObjPriorityRenderLayer(1), new ObjPriorityRenderLayer(2),
                new Bg4BppRenderLayer(0x4000, 0x6000, 3, 11, 32, 32, true),
                new ObjPriorityRenderLayer(3) });
        }
        // Earlier OAM wins even when the later entry has greater display priority.
        // A transparent hole in the winner must still expose that later entry.
        Array.Clear(vram); Array.Clear(oam); Array.Clear(palette);
        palette[129] = 31;
        palette[145] = 31 << 10;
        for (int row = 0; row < 8; row++) vram[row * 2] = 0x7f;
        for (int row = 0; row < 8; row++) vram[32 + row * 2] = 0xff;
        oam[6] = 1; oam[7] = 0x32;
        var overlap = new PpuMemorySnapshot(vram, palette, oam, 2);
        Check(overlap, 0, new RenderLayer[] { new ObjRenderLayer() });
        for (byte priority = 0; priority < 4; priority++)
            Check(overlap, 0, new RenderLayer[] { new ObjPriorityRenderLayer(priority) });
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {cases} exact OBJ comparisons passed.");

        void Check(PpuMemorySnapshot memory, byte obsel, RenderLayer[] layers)
        {
            var frame = new RenderFrameSnapshot(new(++cases, 1, 0),
                new LayeredRenderSnapshot(memory, layers, obsel, 15));
            PixelComparison.Verify(frame, SoftwareFrameSnapshotRenderer.Render(frame),
                renderer.RenderForReadback(frame), $"{device.Kind}: OBJ sample {cases}, OBSEL {obsel}, count {memory.ModeledSpriteCount}");
        }
    }
}
