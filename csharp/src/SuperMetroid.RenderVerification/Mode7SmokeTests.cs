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
        foreach (Mode7OverflowMode overflow in Enum.GetValues<Mode7OverflowMode>())
        {
            var registers = new Mode7RenderRegisters(scale, 113, -113, scale, 128, 112, offset, offset,
                overflow == Mode7OverflowMode.CharacterZero, overflow == Mode7OverflowMode.Wrap);
            var frame = new RenderFrameSnapshot(new(++count, 1, 0),
                new Mode7ObjRenderSnapshot(memory, registers, 3, 13), new byte[] { 11 });
            Check(frame);
            // Exercise Mode 7 as an ordered layer too, not just the special packet shape.
            Check(new RenderFrameSnapshot(new(++count, 1, 0), new LayeredRenderSnapshot(memory,
                new RenderLayer[] { new ObjRenderLayer(), new Mode7RenderLayer(registers),
                    new FixedColorAddRenderLayer(3, 7, 11) }, 3, 15)));
            Check(new RenderFrameSnapshot(new(++count, 1, 0), new LayeredRenderSnapshot(memory,
                new RenderLayer[] { new Mode7RenderLayer(registers, AddBg1Subscreen: true) }, 3, 15)));
        }
        Check(new RenderFrameSnapshot(new(++count, 1, 0), new Mode7ObjRenderSnapshot(memory, null, 3, 15)));
        foreach (int hud in new[] { 0, 8, 32, 208 })
        foreach (int? floorStart in new int?[] { null, 208, 223 })
        foreach (short scale in new short[] { 256, -113, 511, 0 })
        {
            var band = floorStart is { } start
                ? new Mode1FloorBand(start, 0x4000, 0x6000, 17, 65535, 64, 32)
                : (Mode1FloorBand?)null;
            var layer = new Mode7GameplayRenderLayer(
                new(scale, 71, -71, scale, 128, 112, -17, 211, true),
                0x5800, 0x4000, hud, band);
            Check(new RenderFrameSnapshot(new(++count, 1, 0),
                new LayeredRenderSnapshot(memory, new RenderLayer[] { layer }, 3, 13)));
        }
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {count} exact Mode 7 comparisons passed.");

        void Check(RenderFrameSnapshot frame) => PixelComparison.Verify(frame,
            SoftwareFrameSnapshotRenderer.Render(frame), renderer.RenderForReadback(frame),
            $"{device.Kind}: Mode 7 sample {count}");
    }
}
