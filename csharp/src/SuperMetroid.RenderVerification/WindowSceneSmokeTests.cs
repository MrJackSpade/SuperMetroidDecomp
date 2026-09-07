using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static class WindowSceneSmokeTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var random = new Random(32116);
        PpuMemorySnapshot Memory()
        {
            var vram = new byte[SnesPpuLayout.VramByteCount]; random.NextBytes(vram);
            var oam = new byte[SnesPpuLayout.OamUploadByteCount]; random.NextBytes(oam);
            ushort[] palette = Enumerable.Range(0, 256).Select(_ => (ushort)random.Next(32768)).ToArray();
            return new(vram, palette, oam, 128);
        }
        var parentMemory = Memory(); var childMemory = Memory();
        int count = 0;
        foreach (var edges in new[] { (0,0,256,224), (0,0,0,224), (1,1,255,223),
            (17,32,219,200), (255,223,256,224), (0,0,1,1), (0,224,256,224) })
        for (byte brightness = 0; brightness <= 15; brightness++)
        {
            var child = new LayeredRenderSnapshot(childMemory, new RenderLayer[] {
                new Mode7RenderLayer(new(255,71,-71,255,128,112,13,-11,true)),
                new ObjRenderLayer() }, 7, brightness);
            var window = new WindowedSceneRenderLayer(child, edges.Item1, edges.Item2, edges.Item3, edges.Item4);
            var frame = new RenderFrameSnapshot(new(++count, 1, 0), new LayeredRenderSnapshot(parentMemory,
                new RenderLayer[] { new ObjPriorityRenderLayer(0), window,
                    new Bg4BppRenderLayer(0x4000, 0x6000, 3, 11, 32, 32, true),
                    new ObjPriorityRenderLayer(3), window, new FixedColorAddRenderLayer(1,2,3) }, 3, 13), new byte[] { 11 });
            PixelComparison.Verify(frame, SoftwareFrameSnapshotRenderer.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: scene window {count}, edges {edges}, brightness {brightness}");
        }
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {count} exact windowed-scene comparisons passed.");
    }
}
