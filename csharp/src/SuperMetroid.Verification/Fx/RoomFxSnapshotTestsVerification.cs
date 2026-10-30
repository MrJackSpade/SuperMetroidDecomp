using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Fx;
using SuperMetroid.TestSupport.FrameRendering;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Fx;

/// <summary>Checks captured room FX color math keeps the HUD band and owns its register rows.</summary>
internal static class RoomFxSnapshotTestsVerification
{
    /// <summary>Draws liquid and weather effects while checking HUD exclusion and scroll ownership.</summary>
    internal static void VerifyRoomFxSnapshots()
    {
        var random = new Random(32108);
        var vram = new SnesVram();
        byte[] bytes = new byte[SnesVram.ByteCount]; random.NextBytes(bytes); vram.LoadBytes(0, bytes);
        var cgram = new SnesCgram();
        for (int i = 0; i < SnesCgram.ColorCount; i++) cgram.SetColor(i, RgbColor.FromWord(checked((ushort)random.Next(32768))));
        var oam = new OamBuffer(); oam.BeginFrame(); oam.FinalizeFrame();
        var memory = FramePaintConstants.CaptureFrontend(vram, cgram, oam, 3);
        RgbaPixel[] baseline = DisplayRendering.Render(new LayeredScene(memory, [], 15));
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
            var fx = new RoomLayer3FxRenderSnapshot(type, blend, new Position16<OverlayPixels>(65530, 511), new Position16<RoomPixels>(0, currentY),
                WaterBg3WavePhase: phase, WaterSurfaceScreenY: surface);
            OverlayBlendLayer? layer = GameplayEffectCapture.CaptureRoomLayer3Fx(fx, plane);
            var packet = new DisplayPacket(new(++samples, 1, 0),
                new LayeredScene(memory, layer is null ? [] : new DisplayLayer[] { layer }, 15));
            RgbaPixel[] actual = DisplayRendering.Render(packet);
            AssertTrue(actual.AsSpan(0, 256 * 32).SequenceEqual(baseline.AsSpan(0, 256 * 32)),
                $"room FX {type}/{blend} at {surface}, phase {phase}, Y {currentY} preserves HUD band");
        }
        var owned = new OverlayBlendLayer(plane, 0, 64, 32, BlendOperation.Add, ScrollBands.Uniform(0), ScrollBands.Uniform(0));
        AssertThrows<ArgumentNullException>(() => new OverlayBlendLayer(plane, 0, 64, 32,
            BlendOperation.Add, null!, ScrollBands.Uniform(0)), "BG math requires its described line scrolls");
        AssertThrows<ArgumentOutOfRangeException>(() => new OverlayBlendLayer(plane, 0, 64, 32,
            (BlendOperation)255, ScrollBands.Uniform(0), ScrollBands.Uniform(0)), "BG math rejects unknown equation");
        var encoded = new DisplayPacket(new(1, 1, 0), new LayeredScene(memory, [owned], 15));
        AssertThrows<InvalidDataException>(() => GameplayEffectCapture.CaptureRoomLayer3Fx(
            new(RoomFxType.Water, LayerBlendingConfiguration.Rain, Position16<OverlayPixels>.Zero, new Position16<RoomPixels>(0, ushort.MaxValue)), plane), "capture preserves invalid water configuration failure");
        Console.WriteLine($"  Room FX snapshots: {samples} water/lava/acid/rain/fog cases, HUD exclusion and ownership agree.");
    }
}
