using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static ColorWordsVerification;
using static GraphDigestVerification;
using static RenderPacketCodecTestsVerification;
using static RetailRuntimeFixtureVerification;

/// <summary>Checks gameplay render snapshots retain VRAM, CGRAM, OAM, and
/// per-line scroll independently of later live-memory changes.</summary>
internal static class GameplaySnapshotTestsVerification
{
    /// <summary>Round-trips ordinary layered packets across geometry, priority,
    /// and HDMA combinations, then compares a captured retail room frame.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables, native PPU geometry, native VRAM words")]
    internal static void VerifyGameplaySnapshots()
    {
        var random = new Random(32105);
        byte[] bytes = new byte[SnesPpuLayout.VramByteCount];
        random.NextBytes(bytes);
        ushort[] palette = Enumerable.Range(0, SnesPpuLayout.CgramColorCount)
            .Select(_ => (ushort)random.Next(32768)).ToArray();
        byte[] oam = new byte[SnesPpuLayout.OamUploadByteCount];
        random.NextBytes(oam);
        var memory = new PpuMemorySnapshot(bytes, ToColors(palette), oam, 128);
        var live = new SoftwarePpuSnapshotMemory(memory);
        int samples = 0;
        foreach (Extent16<TilemapCells> mapSize in new Extent16<TilemapCells>[] { new(64, 32), new(32, 64), new(64, 64) })
        for (int mask = 0; mask < 8; mask++)
        for (int hdma = 0; hdma < 4; hdma++)
        {
            ushort[] x = (hdma & 1) == 0 ? [] : Enumerable.Range(0, 192).Select(i => (ushort)(65535 - i * 7)).ToArray();
            ushort[] y = (hdma & 2) == 0 ? [] : Enumerable.Range(0, 192).Select(i => (ushort)(i * 19)).ToArray();
            SnesMainScreenLayers layers = (mask & 1) == 0 ? 0 : SnesMainScreenLayers.Bg1;
            if ((mask & 2) != 0) layers |= SnesMainScreenLayers.Bg2;
            if ((mask & 4) != 0) layers |= SnesMainScreenLayers.Obj;
            var registers = new OrdinaryGameplayRegisters(new Position16<RoomPixels>(511, 255), new Position16<Bg2Pixels>(65535, 511),
                mapSize, GameplayVramLayout.Bg2TilemapWord, 0, 0x6000,
                GameplayVramLayout.HudCharacterWord, layers);
            var layer = new OrdinaryGameplayRenderLayer(registers, new Bg2HdmaScrollTables(x, y));
            Rgba32[] expected = SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(
                live.Vram, live.Cgram, live.Oam, layer);
            // Mutating both caller arrays after capture must not alter retained HDMA.
            Array.Fill(x, (ushort)1234);
            Array.Fill(y, (ushort)5678);
            var packet = new RenderFrameSnapshot(new(++samples, 1, (ushort)samples),
                new LayeredRenderSnapshot(memory, new RenderLayer[] { layer }, 3, 15));
            AssertTrue(expected.SequenceEqual(RenderForComparison(RoundTripRenderPacket(packet))),
                $"gameplay snapshot {mapSize.X}x{mapSize.Y}, mask {mask}, HDMA {hdma}");
            if (samples == 1)
            {
                byte[] old = RenderFrameSnapshotWriter.Serialize(packet);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(
                    old.AsSpan(RenderPacketFormat.Signature.Length), RenderPacketFormat.Bg2ViewportLayerVersion);
                AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(old),
                    "older format cannot contain gameplay layer");
                AssertThrows<ArgumentException>(() => new Bg2HdmaScrollTables(new ushort[1], default),
                    "reject partial gameplay HDMA");
                AssertThrows<ArgumentException>(() => new LayeredRenderSnapshot(memory,
                    new RenderLayer[] { new ObjRenderLayer(), layer }, 3, 15), "base cannot erase prior operations");
            }
        }

        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.NmiHandler.RunNmi(0, true);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        AssertThrows<InvalidOperationException>(() => GameplayDisplayCapture.CaptureOrdinaryBase(runtime),
            "Mode-7 shaft cannot silently capture as ordinary tiles");
        runtime.LoadCartridgeRoomForDebug(PowerBombRuntimeVerificationDefinitions.AlphaPowerBombRoomHeader, new Position16<RoomPixels>(0, 0));
        runtime.NmiHandler.RunNmi(0, true);
        Rgba32[] retail = SuperMetroidRuntimeFrameRenderer.Render(runtime);
        LayeredRenderSnapshot captured = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
        AssertTrue(retail.SequenceEqual(RenderForComparison(captured)),
            "ordinary retail room capture matches full renderer with inactive effects");
        runtime.Vram.LoadBytes(0, new byte[SnesPpuLayout.VramByteCount]);
        for (int color = 0; color < SnesPpuLayout.CgramColorCount; color++) runtime.Cgram.SetColor(color, new Bgr555(0, 0, 0));
        AssertTrue(retail.SequenceEqual(RenderForComparison(captured)),
            "gameplay packet survives complete live VRAM/CGRAM replacement");
        Console.WriteLine($"  Gameplay snapshots: {samples} priority/mask/HDMA/geometry cases and retained retail base match.");
    }
}
