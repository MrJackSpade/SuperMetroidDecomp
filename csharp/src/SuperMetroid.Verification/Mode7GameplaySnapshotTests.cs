using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyMode7GameplaySnapshots()
    {
        var random = new Random(32109);
        byte[] bytes = new byte[SnesPpuLayout.VramByteCount]; random.NextBytes(bytes);
        byte[] oam = new byte[SnesPpuLayout.OamUploadByteCount]; random.NextBytes(oam);
        ushort[] palette = Enumerable.Range(0, SnesPpuLayout.CgramColorCount).Select(_ => (ushort)random.Next(32768)).ToArray();
        var memory = new PpuMemorySnapshot(bytes, palette, oam, 128);
        var live = new SoftwarePpuSnapshotMemory(memory);
        int samples = 0;
        foreach (short matrix in new short[] { 0, 128, 256, -256, 511, short.MinValue })
        foreach (ushort scroll in new ushort[] { 0, 255, 511, ushort.MaxValue })
        foreach (bool floorEnabled in new[] { false, true })
        {
            var m = new Mode7RenderRegisters(matrix, 17, -33, matrix, 128, 96, -13, unchecked((short)scroll));
            Mode1FloorBand? floor = floorEnabled ? new(208, SnesPpuLayout.GameplayBg2TilemapWord, 0x6000, scroll, scroll, 64, 32) : null;
            var layer = new Mode7GameplayRenderLayer(m, SnesPpuLayout.GameplayHudTilemapWord,
                SnesPpuLayout.GameplayHudCharacterBaseWord, SnesPpuLayout.GameplayHudHeightPixels, floor);
            Rgba32[] expected = floorEnabled
                ? SnesGameplayFrameRenderer.RenderHudCeresRidleyGetawayAndObjs(live.Vram, live.Cgram, live.Oam,
                    m.MatrixA, m.MatrixB, m.MatrixC, m.MatrixD, m.CenterX, m.CenterY, m.HorizontalOffset, m.VerticalOffset,
                    scroll, scroll, 0x6000)
                : SnesGameplayFrameRenderer.RenderHudMode7AndObjs(live.Vram, live.Cgram, live.Oam,
                    m.MatrixA, m.MatrixB, m.MatrixC, m.MatrixD, m.CenterX, m.CenterY, m.HorizontalOffset, m.VerticalOffset);
            var packet = new RenderFrameSnapshot(new(++samples, 1, 0), new LayeredRenderSnapshot(memory, new RenderLayer[] { layer }, 3, 15));
            var restored = RoundTripRenderPacket(packet);
            AssertTrue(expected.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(restored)),
                $"Mode-7 gameplay full pixel parity matrix {matrix}, scroll {scroll}, floor {floorEnabled}");
            AssertEqual(layer, (Mode7GameplayRenderLayer)restored.Layers!.Layers[0], "Mode-7 band codec preserves all register fields");
            if (samples == 1)
            {
                byte[] old = RenderFrameSnapshotCodec.Serialize(packet);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(old.AsSpan(RenderPacketFormat.Signature.Length),
                    RenderPacketFormat.BgColorMathLayerVersion);
                AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(old), "old format rejects Mode-7 bands");
                AssertThrows<ArgumentException>(() => new LayeredRenderSnapshot(memory,
                    new RenderLayer[] { new ObjRenderLayer(), layer }, 3, 15), "Mode-7 base cannot erase earlier layers");
            }
        }
        // Retail room assets with explicitly constructed getaway registers exercise
        // the runtime producer, without pretending this is a controller-driven battle.
        var runtime = new SuperMetroidRuntime(SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
        runtime.InitializeHud(HudSnapshot.CeresDebug); runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.CeresRidleyRoom, 0, 0);
        runtime.RunNmi(0, true);
        var getaway = runtime.Enemies.CeresRidley!;
        getaway.Mode7Active = true;
        getaway.Mode7MatrixA = 240; getaway.Mode7MatrixB = 33;
        getaway.Mode7MatrixC = unchecked((ushort)-33); getaway.Mode7MatrixD = 240;
        getaway.Mode7CenterX = 128; getaway.Mode7CenterY = 120;
        getaway.Mode7HorizontalOffset = 7; getaway.Mode7VerticalOffset = 19;
        Rgba32[] retail = SuperMetroidRuntimeFrameRenderer.Render(runtime);
        var retained = RoundTripRenderPacket(new(new(100, 1, 0), GameplayDisplayCapture.TryCaptureFrame(runtime)!));
        AssertTrue(retail.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(retained)), "retail Ridley capture resolves mixed bands and haze");
        getaway.Mode7Active = false; getaway.Mode7MatrixA = 0;
        runtime.Vram.LoadBytes(0, new byte[SnesPpuLayout.VramByteCount]);
        for (int i = 0; i < SnesPpuLayout.CgramColorCount; i++) runtime.Cgram.SetColor(i, 0);
        AssertTrue(retail.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(retained)), "Ridley packet survives mode exit and live memory replacement");
        AssertThrows<ArgumentOutOfRangeException>(() => new Mode7GameplayRenderLayer(default, 0, 0, 31), "HUD must cover complete tile rows");
        AssertThrows<ArgumentOutOfRangeException>(() => new Mode7GameplayRenderLayer(default, 0, 0, 32,
            new Mode1FloorBand(31, 0, 0, 0, 0, 64, 32)), "floor cannot overlap HUD");
        Console.WriteLine($"  Mode-7 gameplay snapshots: {samples} full-frame matrix/scroll/floor comparisons match.");
    }
}
