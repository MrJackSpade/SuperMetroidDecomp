using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Rendering.RenderPacketCodecTestsVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks room FX color math capture and packet round-tripping against direct rendering.</summary>
internal static class RoomFxSnapshotTestsVerification
{
    /// <summary>Compares liquid and weather effects while checking HUD exclusion, scroll ownership, and codec failures.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native PPU geometry, native VRAM words")]
    internal static void VerifyRoomFxSnapshots()
    {
        var random = new Random(32108);
        var vram = new SnesVram();
        byte[] bytes = new byte[SnesPpuLayout.VramByteCount]; random.NextBytes(bytes); vram.LoadBytes(0, bytes);
        var cgram = new SnesCgram();
        for (int i = 0; i < SnesCgram.ColorCount; i++) cgram.SetColor(i, Bgr555.FromWord(checked((ushort)((ushort)random.Next(32768)))));
        var oam = new OamBuffer(); oam.BeginFrame(); oam.FinalizeFrame();
        var memory = PpuMemorySnapshot.Capture(vram, cgram, oam);
        Rgba32[] baseline = SnesLayerCompositor.CreateBackdrop(cgram, 256 * 224);
        int samples = 0;
        var configurations = new[]
        {
            (RoomFxType.Water, LayerBlendingConfiguration.WaterSubtractive),
            (RoomFxType.Water, LayerBlendingConfiguration.WaterfallSubtractive),
            (RoomFxType.Water, LayerBlendingConfiguration.LiquidOrFogAdditive),
            (RoomFxType.Lava, LayerBlendingConfiguration.LavaAcidAdditive),
            (RoomFxType.Acid, LayerBlendingConfiguration.LavaAcidAdditive),
            (RoomFxType.Rain, LayerBlendingConfiguration.Rain),
            (RoomFxType.Fog, LayerBlendingConfiguration.FogAdditive),
        };
        foreach ((RoomFxType type, LayerBlendingConfiguration blend) in configurations)
        foreach (short surface in new short[] { -20, 0, 32, 100, 223, 300 })
        foreach (int phase in new[] { 0, 7, 15 })
        foreach (ushort currentY in new ushort[] { 400, ushort.MaxValue })
        {
            var fx = new RoomLayer3FxRenderSnapshot(type, blend, new Position16<Bg3Pixels>(65530, 511), new Position16<RoomPixels>(0, currentY),
                WaterBg3WavePhase: phase, WaterSurfaceScreenY: surface);
            Rgba32[] expected = baseline.ToArray();
            SnesGameplayFrameRendererConstants.ApplyRoomLayer3FxColorMath(expected, vram, cgram, fx);
            Bg2BppColorMathRenderLayer? layer = SnesGameplayFrameRenderer.CaptureRoomLayer3Fx(fx);
            var packet = new RenderFrameSnapshot(new(++samples, 1, 0),
                new LayeredRenderSnapshot(memory, layer is null ? [] : new RenderLayer[] { layer }, 3, 15));
            Rgba32[] actual = SoftwareFrameSnapshotRenderer.Render(RoundTripRenderPacket(packet));
            AssertTrue(expected.SequenceEqual(actual), $"room FX {type}/{blend} at {surface}, phase {phase}, Y {currentY}");
            AssertTrue(actual.AsSpan(0, 256 * 32).SequenceEqual(baseline.AsSpan(0, 256 * 32)), "room FX preserves HUD band");
        }
        var lines = new Position16<BgPlanePixels>[224];
        var owned = new Bg2BppColorMathRenderLayer(0x5800, 0x4000, 64, 32, ExpandedColorMathOperation.Add, lines);
        Array.Fill(lines, new Position16<BgPlanePixels>(1234, 5678));
        AssertTrue(owned.Scrolls.All(line => line == default), "BG math owns register rows");
        AssertThrows<ArgumentException>(() => new Bg2BppColorMathRenderLayer(0, 0, 64, 32,
            ExpandedColorMathOperation.Add, new Position16<BgPlanePixels>[223]), "BG math rejects truncated register table");
        AssertThrows<ArgumentOutOfRangeException>(() => new Bg2BppColorMathRenderLayer(0, 0, 64, 32,
            (ExpandedColorMathOperation)255, lines), "BG math rejects unknown equation");
        var encoded = new RenderFrameSnapshot(new(1, 1, 0), new LayeredRenderSnapshot(memory, [owned], 3, 15));
        byte[] data = RenderFrameSnapshotWriter.SerializeBeforeSpriteDraws(encoded);
        AssertThrows<IOException>(() => RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotWriter.Serialize(encoded)[..^1]), "truncated FX packet rejected");
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(RenderPacketFormat.Signature.Length),
            RenderPacketFormat.MessageLayerVersion);
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(data), "older version rejects BG math");
        AssertThrows<InvalidDataException>(() => SnesGameplayFrameRenderer.CaptureRoomLayer3Fx(
            new(RoomFxType.Water, LayerBlendingConfiguration.Rain, Position16<Bg3Pixels>.Zero, new Position16<RoomPixels>(0, ushort.MaxValue))), "capture preserves invalid water configuration failure");
        Console.WriteLine($"  Room FX snapshots: {samples} water/lava/acid/rain/fog cases, HUD exclusion, ownership and codec agree.");
    }
}
