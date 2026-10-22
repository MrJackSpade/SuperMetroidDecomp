using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.PpuFeatures;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Checks D3D11 Mode 7 rendering against software output over native PPU geometry.</summary>
internal static class Mode7SmokeTests
{
    /// <summary>Compares scale, offset, overflow, ordered-layer, HUD, and floor-band cases with exact software pixels.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables, native PPU geometry, native VRAM words")]
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var random = new Random(32113);
        var vram = new byte[SnesPpuLayout.VramByteCount];
        var oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
        random.NextBytes(vram); random.NextBytes(oam);
        Bgr555[] palette = Enumerable.Range(0, 256).Select(_ => Bgr555.FromWord((ushort)random.Next(32768))).ToArray();
        var memory = PpuMemorySnapshot.FromOam(vram, palette, oam, 128, 3);
        Layer3Plane bg3 = Layer3PlaneFixtures.FromVram(vram);
        Mode7MapSnapshot map = Mode7MapFixtures.FromVram(vram);
        int count = 0;
        foreach (short scale in new short[] { 0, 1, 255, 256, 512, -256, short.MinValue, short.MaxValue })
        foreach (short offset in new short[] { -1024, -1, 0, 1023, short.MinValue, short.MaxValue })
        foreach (Mode7OverflowMode overflow in Enum.GetValues<Mode7OverflowMode>())
        {
            var registers = new Mode7RenderRegisters(scale, 113, -113, scale, new Offset16<Mode7Pixels>(128, 112), new Offset16<Mode7Pixels>(offset, offset),
                overflow == Mode7OverflowMode.CharacterZero, overflow == Mode7OverflowMode.Wrap);
            var frame = new RenderFrameSnapshot(new(++count, 1, 0),
                new Mode7ObjRenderSnapshot(memory, registers, 13), [new FadeLevel(11)]);
            Check(frame);
            Check(new RenderFrameSnapshot(new(++count, 1, 0), new LayeredRenderSnapshot(memory,
                new RenderLayer[] { new ObjRenderLayer(), new Mode7RenderLayer(registers),
                    new FixedColorAddRenderLayer(3, 7, 11) }, 15)));
            Check(new RenderFrameSnapshot(new(++count, 1, 0), new LayeredRenderSnapshot(memory,
                new RenderLayer[] { new Mode7RenderLayer(registers, AddBg1Subscreen: true) }, 15)));
        }
        Check(new RenderFrameSnapshot(new(++count, 1, 0), new Mode7ObjRenderSnapshot(memory, null, 15)));
        foreach (int hud in new[] { 0, 8, 32, 208 })
        foreach (int? floorStart in new int?[] { null, 208, 223 })
        foreach (short scale in new short[] { 256, -113, 511, 0 })
        {
            var band = floorStart is { } start
                ? new Mode1FloorBand(start, 0x4000, 0x6000, new Position16<Bg2Pixels>(17, 65535), new Extent16<TilemapCells>(64, 32))
                : (Mode1FloorBand?)null;
            var layer = new Mode7GameplayRenderLayer(
                new(scale, 71, -71, scale, new(128, 112), new(-17, 211), true),
                bg3, map, 0x4000, hud, band);
            Check(new RenderFrameSnapshot(new(++count, 1, 0),
                new LayeredRenderSnapshot(memory, new RenderLayer[] { layer }, 13)));
        }
        // A Ceres room's map drawn from its tiles' pictures (#1392), some rows recoloured by the scene, and from VRAM, with
        // outside pixels transparent, as Ceres rooms show them, or character zero.
        Mode7MapSnapshot pictured = Mode7MapFixtures.WithSeededTiles(map, random, palette);
        foreach (Mode7MapSnapshot shown in new[] { map, pictured })
        foreach (int hud in new[] { 0, 32 })
        foreach (short scale in new short[] { 256, -113, 511 })
        foreach (bool fill in new[] { false, true })
        {
            var layer = new Mode7GameplayRenderLayer(
                new(scale, 71, -71, scale, new(128, 112), new(-17, 211), fill),
                bg3, shown, 0x4000, hud, null);
            Check(new RenderFrameSnapshot(new(++count, 1, 0),
                new LayeredRenderSnapshot(memory, new RenderLayer[] { layer }, 13)));
        }
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {count} exact Mode 7 comparisons passed.");

        void Check(RenderFrameSnapshot frame) => PixelComparison.Verify(frame,
            SoftwareFrameSnapshotRenderer.Render(frame), renderer.RenderForReadback(frame),
            $"{device.Kind}: Mode 7 sample {count}");
    }
}
