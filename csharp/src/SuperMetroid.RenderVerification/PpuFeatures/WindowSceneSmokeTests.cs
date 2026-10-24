using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.PpuFeatures;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Compares D3D11 on the graphics device and the WARP reference's windowed-scene rendering across native PPU geometry and brightness cases.</summary>
internal static class WindowSceneSmokeTests
{
    /// <summary>Checks parent/child window pixels and exact upload counts, including clipped empty windows and a child reused twice.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables, native PPU geometry, native VRAM words")]
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var random = new Random(32116);
        (byte[] Vram, FramePaint Paint) Memory(byte obsel)
        {
            var vram = new byte[SnesPpuLayout.VramByteCount]; random.NextBytes(vram);
            var oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount]; random.NextBytes(oam);
            Bgr555[] palette = Enumerable.Range(0, 256).Select(_ => Bgr555.FromWord((ushort)random.Next(32768))).ToArray();
            return (vram, FramePaint.FromOam(vram, palette, oam, 128, obsel));
        }
        var parentMemory = Memory(3); var childMemory = Memory(7);
        var background = new TileRenderLayer(VramTileFixtures.Cells(parentMemory.Vram, parentMemory.Paint.Colors, 0x4000, 0x6000, BgBitDepth.FourBpp,
            new Extent16<TilemapCells>(32, 32)), new Position16<BgPlanePixels>(3, 11), true);
        AffineFloor childFloor = AffineFloorFixtures.Floor(childMemory.Vram, childMemory.Paint.Colors);
        LayeredScene Child(byte brightness) => new(childMemory.Paint, new RenderLayer[] {
            new Mode7RenderLayer(new(255, 71, -71, 255, new(128, 112), new(13, -11), Mode7OverflowMode.CharacterZero), childFloor),
            new ObjRenderLayer() }, brightness);
        // The parent's tile layer and the child's floor pack once; drawing them before the counted frames leaves each of
        // those only its memory and sprite-draw uploads.
        int count = 0;
        renderer.RenderForReadback(new DisplayPacket(new(++count, 1, 0), new LayeredScene(parentMemory.Paint,
            [background, new WindowedSceneRenderLayer(Child(15), 0, 0, 256, 224)], 15)));
        // Each scene render uploads its memory and sprite-draw buffers once. The draw buffer uploads its grown capacity,
        // so a frame's bytes are fixed per window shape rather than computable from the scene alone.
        var bytesByChildren = new Dictionary<int, long>();
        foreach (var edges in new[] { new WindowEdges(0,0,256,224), new WindowEdges(0,0,0,224), new WindowEdges(1,1,255,223),
            new WindowEdges(17,32,219,200), new WindowEdges(255,223,256,224), new WindowEdges(0,0,1,1), new WindowEdges(0,224,256,224) })
        for (byte brightness = 0; brightness <= 15; brightness++)
        {
            LayeredScene child = Child(brightness);
            var window = new WindowedSceneRenderLayer(child, edges.Left, edges.Top, edges.Right, edges.Bottom);
            var frame = new DisplayPacket(new(++count, 1, 0), new LayeredScene(parentMemory.Paint,
                new RenderLayer[] { new ObjPriorityRenderLayer(0), window,
                    background,
                    new ObjPriorityRenderLayer(3), window, new FixedColorAddRenderLayer(1,2,3) }, 13), [new FadeLevel(11)]);
            var before = renderer.UploadStatistics;
            PixelComparison.Verify(frame, DisplayRendering.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: scene window {count}, edges {edges}, brightness {brightness}");
            var after = renderer.UploadStatistics;
            int children = edges.Left == edges.Right || edges.Top == edges.Bottom ? 0 : 2;
            // Each scene uploads at least its backdrop colour (#1408).
            long memoryAndConstantBytes = (1 + children) * sizeof(uint)
                + (7 + children * 4) * D3D11ShaderLayout.SolidConstantWords * sizeof(uint);
            // Each scene uploads its colours and sprite draws, then one constant buffer per composition pass; the sprite
            // layer's quads draw without one (#1408).
            long expectedCalls = (1 + children) * 2 + 7 + children * 4;
            long bytes = after.Bytes - before.Bytes;
            long firstBytes = bytesByChildren.TryGetValue(children, out long seen) ? seen : bytesByChildren[children] = bytes;
            if (after.Calls - before.Calls != expectedCalls || bytes <= memoryAndConstantBytes || bytes != firstBytes)
                throw new InvalidOperationException(
                    $"Upload counters omitted or duplicated parent/child transfers: {bytes} bytes in "
                    + $"{after.Calls - before.Calls} calls, expected {expectedCalls} calls and the {firstBytes} bytes of this "
                    + $"window shape's first frame, above {memoryAndConstantBytes} (scene window {count}, edges {edges}).");
            if (after.CpuMilliseconds < before.CpuMilliseconds)
                throw new InvalidOperationException("Cumulative upload CPU time regressed.");
        }
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {count} exact windowed-scene comparisons passed.");
    }
}
