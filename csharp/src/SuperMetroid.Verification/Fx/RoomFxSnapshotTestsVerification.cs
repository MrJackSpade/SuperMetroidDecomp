using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Fx;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks captured room FX color math keeps the HUD band and owns its register rows.</summary>
internal static class RoomFxSnapshotTestsVerification
{
    /// <summary>Draws liquid and weather effects while checking HUD exclusion and scroll ownership.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native PPU geometry, native VRAM words")]
    internal static void VerifyRoomFxSnapshots()
    {
        var random = new Random(32108);
        var vram = new SnesVram();
        byte[] bytes = new byte[SnesPpuLayout.VramByteCount]; random.NextBytes(bytes); vram.LoadBytes(0, bytes);
        var cgram = new SnesCgram();
        for (int i = 0; i < SnesCgram.ColorCount; i++) cgram.SetColor(i, Bgr555.FromWord(checked((ushort)random.Next(32768))));
        var oam = new OamBuffer(); oam.BeginFrame(); oam.FinalizeFrame();
        var memory = FramePaintConstants.CaptureFrontend(vram, cgram, oam, 3);
        Rgba32[] baseline = DisplayRendering.Render(new LayeredScene(memory, [], 15));
        int samples = 0;
        var configurations = new[]
        {
            new Configuration(RoomFxType.Water, LayerBlendingConfiguration.WaterSubtractive),
            new Configuration(RoomFxType.Water, LayerBlendingConfiguration.WaterfallSubtractive),
            new Configuration(RoomFxType.Water, LayerBlendingConfiguration.LiquidOrFogAdditive),
            new Configuration(RoomFxType.Lava, LayerBlendingConfiguration.LavaAcidAdditive),
            new Configuration(RoomFxType.Acid, LayerBlendingConfiguration.LavaAcidAdditive),
            new Configuration(RoomFxType.Rain, LayerBlendingConfiguration.Rain),
            new Configuration(RoomFxType.Fog, LayerBlendingConfiguration.FogAdditive),
        };
        // The BG3 plane shows the fixture's random VRAM (#1348).
        Layer3Cells plane = Layer3CellsFixtures.FromVram(vram.Bytes, cgram.Colors);
        foreach ((RoomFxType type, LayerBlendingConfiguration blend) in configurations)
        foreach (short surface in new short[] { -20, 0, 32, 100, 223, 300 })
        foreach (int phase in new[] { 0, 7, 15 })
        foreach (ushort currentY in new ushort[] { 400, ushort.MaxValue })
        {
            var fx = new RoomLayer3FxRenderSnapshot(type, blend, new Position16<Bg3Pixels>(65530, 511), new Position16<RoomPixels>(0, currentY),
                WaterBg3WavePhase: phase, WaterSurfaceScreenY: surface);
            FxPlaneBlendRenderLayer? layer = GameplayEffectCapture.CaptureRoomLayer3Fx(fx, plane);
            var packet = new DisplayPacket(new(++samples, 1, 0),
                new LayeredScene(memory, layer is null ? [] : new RenderLayer[] { layer }, 15));
            Rgba32[] actual = DisplayRendering.Render(packet);
            AssertTrue(actual.AsSpan(0, 256 * 32).SequenceEqual(baseline.AsSpan(0, 256 * 32)),
                $"room FX {type}/{blend} at {surface}, phase {phase}, Y {currentY} preserves HUD band");
        }
        var lines = new Position16<BgPlanePixels>[224];
        var owned = new FxPlaneBlendRenderLayer(plane, 0, 64, 32, ExpandedColorMathOperation.Add, lines);
        Array.Fill(lines, new Position16<BgPlanePixels>(1234, 5678));
        AssertTrue(owned.Scrolls.All(line => line == default), "BG math owns register rows");
        AssertThrows<ArgumentException>(() => new FxPlaneBlendRenderLayer(plane, 0, 64, 32,
            ExpandedColorMathOperation.Add, new Position16<BgPlanePixels>[223]), "BG math rejects truncated register table");
        AssertThrows<ArgumentOutOfRangeException>(() => new FxPlaneBlendRenderLayer(plane, 0, 64, 32,
            (ExpandedColorMathOperation)255, lines), "BG math rejects unknown equation");
        var encoded = new DisplayPacket(new(1, 1, 0), new LayeredScene(memory, [owned], 15));
        AssertThrows<InvalidDataException>(() => GameplayEffectCapture.CaptureRoomLayer3Fx(
            new(RoomFxType.Water, LayerBlendingConfiguration.Rain, Position16<Bg3Pixels>.Zero, new Position16<RoomPixels>(0, ushort.MaxValue)), plane), "capture preserves invalid water configuration failure");
        Console.WriteLine($"  Room FX snapshots: {samples} water/lava/acid/rain/fog cases, HUD exclusion and ownership agree.");
    }
}
