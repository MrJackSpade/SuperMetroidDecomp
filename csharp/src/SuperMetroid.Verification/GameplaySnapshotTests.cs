using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyGameplaySnapshots()
    {
        var random = new Random(32105);
        byte[] bytes = new byte[SnesPpuLayout.VramByteCount];
        random.NextBytes(bytes);
        ushort[] palette = Enumerable.Range(0, SnesPpuLayout.CgramColorCount)
            .Select(_ => (ushort)random.Next(32768)).ToArray();
        byte[] oam = new byte[SnesPpuLayout.OamUploadByteCount];
        random.NextBytes(oam);
        var memory = new PpuMemorySnapshot(bytes, palette, oam, 128);
        var live = new SoftwarePpuSnapshotMemory(memory);
        int samples = 0;
        foreach ((int width, int height) in new[] { (64, 32), (32, 64), (64, 64) })
        for (int mask = 0; mask < 8; mask++)
        for (int hdma = 0; hdma < 4; hdma++)
        {
            ushort[] x = (hdma & 1) == 0 ? [] : Enumerable.Range(0, 192).Select(i => (ushort)(65535 - i * 7)).ToArray();
            ushort[] y = (hdma & 2) == 0 ? [] : Enumerable.Range(0, 192).Select(i => (ushort)(i * 19)).ToArray();
            SnesMainScreenLayers layers = (mask & 1) == 0 ? 0 : SnesMainScreenLayers.Bg1;
            if ((mask & 2) != 0) layers |= SnesMainScreenLayers.Bg2;
            if ((mask & 4) != 0) layers |= SnesMainScreenLayers.Obj;
            var registers = new OrdinaryGameplayRegisters(511, 255, 65535, 511,
                width, height, SnesPpuLayout.GameplayBg2TilemapWord, 0, 0x6000,
                SnesPpuLayout.GameplayHudCharacterBaseWord, layers);
            Rgba32[] expected = SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(
                live.Vram, live.Cgram, live.Oam, registers.Bg1X, registers.Bg1Y, registers.Bg2X, registers.Bg2Y,
                x.Length == 0 ? null : x, y.Length == 0 ? null : y,
                width, height, registers.Bg2TilemapWord, registers.Bg1CharacterWord,
                registers.Bg2CharacterWord, registers.HudCharacterWord, mainScreenLayers: layers);
            var layer = new OrdinaryGameplayRenderLayer(registers, x, y);
            // Mutating both caller arrays after capture must not alter retained HDMA.
            Array.Fill(x, (ushort)1234);
            Array.Fill(y, (ushort)5678);
            var packet = new RenderFrameSnapshot(new(++samples, 1, (ushort)samples),
                new LayeredRenderSnapshot(memory, new RenderLayer[] { layer }, 3, 15));
            AssertTrue(expected.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(RoundTripRenderPacket(packet))),
                $"gameplay snapshot {width}x{height}, mask {mask}, HDMA {hdma}");
            if (samples == 1)
            {
                byte[] old = RenderFrameSnapshotCodec.Serialize(packet);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(
                    old.AsSpan(RenderPacketFormat.Signature.Length), RenderPacketFormat.Bg2ViewportLayerVersion);
                AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(old),
                    "older format cannot contain gameplay layer");
                AssertThrows<ArgumentException>(() => new OrdinaryGameplayRenderLayer(registers, new ushort[1]),
                    "reject partial gameplay HDMA");
                AssertThrows<ArgumentException>(() => new LayeredRenderSnapshot(memory,
                    new RenderLayer[] { new ObjRenderLayer(), layer }, 3, 15), "base cannot erase prior operations");
            }
        }

        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        AssertThrows<InvalidOperationException>(() => GameplayDisplayCapture.CaptureOrdinaryBase(runtime),
            "Mode-7 shaft cannot silently capture as ordinary tiles");
        runtime.LoadCartridgeRoomForDebug(PowerBombRuntimeVerificationDefinitions.AlphaPowerBombRoomHeader, 0, 0);
        runtime.RunNmi(0, true);
        Rgba32[] retail = SuperMetroidRuntimeFrameRenderer.Render(runtime);
        LayeredRenderSnapshot captured = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
        AssertTrue(retail.AsSpan().SequenceEqual(SoftwareLayeredSnapshotRenderer.Render(captured)),
            "ordinary retail room capture matches full renderer with inactive effects");
        runtime.Vram.LoadBytes(0, new byte[SnesPpuLayout.VramByteCount]);
        for (int color = 0; color < SnesPpuLayout.CgramColorCount; color++) runtime.Cgram.SetColor(color, 0);
        AssertTrue(retail.AsSpan().SequenceEqual(SoftwareLayeredSnapshotRenderer.Render(captured)),
            "gameplay packet survives complete live VRAM/CGRAM replacement");
        Console.WriteLine($"  Gameplay snapshots: {samples} priority/mask/HDMA/geometry cases and retained retail base match.");
    }
}
