using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Rendering.RenderPacketCodecTestsVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Hardware;

/// <summary>Checks Mode 7 gameplay packet rendering against the live renderer and a captured Ceres Ridley scene.</summary>
internal static class Mode7GameplaySnapshotTestsVerification
{
    /// <summary>Compares full pixels across matrix, scroll, and floor variants, then checks packet retention after live scene data changes.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables, native PPU geometry, native VRAM words")]
    internal static void VerifyMode7GameplaySnapshots()
    {
        var random = new Random(32109);
        byte[] bytes = new byte[SnesPpuLayout.VramByteCount]; random.NextBytes(bytes);
        byte[] oam = new byte[SnesPpuLayout.OamUploadByteCount]; random.NextBytes(oam);
        ushort[] palette = Enumerable.Range(0, SnesPpuLayout.CgramColorCount).Select(_ => (ushort)random.Next(32768)).ToArray();
        var memory = new PpuMemorySnapshot(bytes, ToColors(palette), oam, 128);
        var live = new SoftwarePpuSnapshotMemory(memory);
        int samples = 0;
        foreach (short matrix in new short[] { 0, 128, 256, -256, 511, short.MinValue })
        foreach (ushort scroll in new ushort[] { 0, 255, 511, ushort.MaxValue })
        foreach (bool floorEnabled in new[] { false, true })
        {
            var m = new Mode7RenderRegisters(matrix, 17, -33, matrix, new Offset16<Mode7Pixels>(128, 96), new Offset16<Mode7Pixels>(-13, unchecked((short)scroll)));
            Mode1FloorBand? floor = floorEnabled ? new(208, GameplayVramLayout.Bg2TilemapWord, 0x6000, new Position16<Bg2Pixels>(scroll, scroll), new Extent16<TilemapCells>(64, 32)) : null;
            var layer = new Mode7GameplayRenderLayer(m, GameplayVramLayout.HudTilemapWord,
                GameplayVramLayout.HudCharacterWord, SnesPpuLayout.GameplayHudHeightPixels, floor);
            Rgba32[] expected = floorEnabled
                ? SnesGameplayFrameRendererConstants.RenderHudCeresRidleyGetawayAndObjs(live.Vram, live.Cgram, live.Sprites, m, new CeresGetawayFloor { Bg2Scroll = new Position16<Bg2Pixels>(scroll, scroll), Bg2CharacterBaseWord = 0x6000 })
                : SnesGameplayFrameRendererConstants.RenderHudMode7AndObjs(live.Vram, live.Cgram, live.Sprites, m);
            var packet = new RenderFrameSnapshot(new(++samples, 1, 0), new LayeredRenderSnapshot(memory, new RenderLayer[] { layer }, 3, 15));
            var restored = RoundTripRenderPacket(packet);
            AssertTrue(expected.SequenceEqual(RenderForComparison(restored)),
                $"Mode-7 gameplay full pixel parity matrix {matrix}, scroll {scroll}, floor {floorEnabled}");
            AssertEqual(layer, (Mode7GameplayRenderLayer)(restored.Layers ?? throw new InvalidOperationException("Verification requires restored.Layers.")).Layers[0], "Mode-7 band codec preserves all register fields");
            if (samples == 1)
            {
                byte[] old = RenderFrameSnapshotWriter.SerializeBeforeSpriteDraws(packet);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(old.AsSpan(RenderPacketFormat.Signature.Length),
                    RenderPacketFormat.BgColorMathLayerVersion);
                AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(old), "old format rejects Mode-7 bands");
                AssertThrows<ArgumentException>(() => new LayeredRenderSnapshot(memory,
                    new RenderLayer[] { new ObjRenderLayer(), layer }, 3, 15), "Mode-7 base cannot erase earlier layers");
            }
        }
        // Retail room assets with explicitly constructed getaway registers exercise
        // the runtime producer, without pretending this is a controller-driven battle.
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug); runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.CeresRidleyRoom, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var getaway = runtime.Enemies.RidleyEncounter.CeresRidley;
        (getaway ?? throw new InvalidOperationException("Verification requires getaway.")).Mode7Active = true;
        getaway.Mode7MatrixA = SnesSignedEightEight.FromSignedRaw(240); getaway.Mode7MatrixB = SnesSignedEightEight.FromSignedRaw(33);
        getaway.Mode7MatrixC = SnesSignedEightEight.FromSignedRaw(-33); getaway.Mode7MatrixD = SnesSignedEightEight.FromSignedRaw(240);
        getaway.Mode7Center = new Position16<Mode7Pixels>(128, 120);
        getaway.Mode7Offset = new Offset16<Mode7Pixels>(7, 19);
        // Room main's M7A..M7Y shadows reach the PPU at the accepted NMI (#1307).
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        Rgba32[] retail = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame());
        var retained = RoundTripRenderPacket(new(new(100, 1, 0), GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame())));
        AssertTrue(retail.SequenceEqual(RenderForComparison(retained)), "retail Ridley capture resolves mixed bands and haze");
        getaway.Mode7Active = false; getaway.Mode7MatrixA = default;
        runtime.Vram.LoadBytes(0, new byte[SnesPpuLayout.VramByteCount]);
        for (int i = 0; i < SnesPpuLayout.CgramColorCount; i++) runtime.Cgram.SetColor(i, new Bgr555(0, 0, 0));
        AssertTrue(retail.SequenceEqual(RenderForComparison(retained)), "Ridley packet survives mode exit and live memory replacement");
        AssertThrows<ArgumentOutOfRangeException>(() => new Mode7GameplayRenderLayer(default, 0, 0, 31), "HUD must cover complete tile rows");
        AssertThrows<ArgumentOutOfRangeException>(() => new Mode7GameplayRenderLayer(default, 0, 0, 32,
            new Mode1FloorBand(31, 0, 0, new Position16<Bg2Pixels>(0, 0), new Extent16<TilemapCells>(64, 32))), "floor cannot overlap HUD");
        Console.WriteLine($"  Mode-7 gameplay snapshots: {samples} full-frame matrix/scroll/floor comparisons match.");
    }
}
