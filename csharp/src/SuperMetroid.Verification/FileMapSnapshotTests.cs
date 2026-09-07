using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyFileMapSnapshots()
    {
        VerifyWindowedSceneContract();
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int samples = 0;
        for (int area = 0; area < 6; area++)
        {
            var graphics = new FileSelectAreaMapGraphics(bus, area);
            foreach (bool backdrop in new[] { false, true })
            foreach (ushort used in new ushort[] { 0, 1, ushort.MaxValue })
            {
                ushort[] masks = Enumerable.Repeat(used, 6).ToArray();
                Compare(graphics.Render(masks, backdrop), graphics.CaptureRenderSnapshot(masks, backdrop), "area labels/subscreen");
            }
            var room = new FileSelectRoomMapGraphics(bus, new Bank80SystemState(), (AreaId)area);
            Compare(room.RenderFrameOnly(), room.CaptureRenderSnapshot(frameOnly: true), "room frame only");
            foreach (ushort scroll in new ushort[] { 0, 255, 511, ushort.MaxValue })
                Compare(room.RenderBackgrounds(scroll, scroll), room.CaptureRenderSnapshot(scroll, scroll), "room wrapped scroll");
        }
        foreach (bool cancel in new[] { false, true })
        {
            var saves = new SuperMetroidSaveRam(bus);
            var save = new SuperMetroidSaveSnapshot { Area = 4, SaveStation = 0, Health = 99, MaxHealth = 99 };
            save.MapStationBytes[4] = 1; save.UsedSaveStationBytes[8] = 1;
            saves.SaveSlot(0, save);
            var legacy = new FileSelectMapMenuState(bus, new CartridgeAudioState(), saves.ReadSlot(0)!, 0);
            var capture = new FileSelectMapMenuState(bus, new CartridgeAudioState(), saves.ReadSlot(0)!, 0);
            var phases = new HashSet<FileSelectMapNavigationPhase>();
            LayeredRenderSnapshot? previous = null;
            Rgba32[]? previousPixels = null;
            for (int tick = 0; tick < 300 && !legacy.LoadRequested && !legacy.OptionsRequested; tick++)
            {
                ushort input = tick == 48 ? (ushort)SnesButton.Start
                    : tick == 130 ? (ushort)(cancel ? SnesButton.B : SnesButton.Start)
                    : cancel && tick == 240 ? (ushort)SnesButton.B
                    : tick is >= 110 and <= 119 ? (ushort)SnesButton.Right : (ushort)0;
                legacy.Step(input); capture.Step(input);
                if (previous is not null)
                    AssertTrue(previousPixels.AsSpan().SequenceEqual(SoftwareLayeredSnapshotRenderer.Render(previous)), "map packet survives window/palette/marker update");
                phases.Add(legacy.Phase);
                Rgba32[] expected = legacy.Render();
                LayeredRenderSnapshot packet = capture.CaptureRenderSnapshot();
                Compare(expected, packet, $"map navigation {legacy.Phase}, tick {tick}, cancel {cancel}");
                AssertEqual(legacy.Phase, capture.Phase, "map capture preserves navigation");
                AssertEqual(legacy.LoadRequested, capture.LoadRequested, "map capture preserves load signal");
                previous = packet; previousPixels = expected;
            }
            AssertTrue(phases.Contains(FileSelectMapNavigationPhase.Room) && phases.Contains(FileSelectMapNavigationPhase.ExpandingWindow),
                "map fixture reaches expansion and room map");
            AssertTrue(cancel ? legacy.OptionsRequested : legacy.LoadRequested, "map fixture completes requested route");
        }
        byte[] rom = File.ReadAllBytes(Path.GetFullPath("Super Metroid.smc"));
        var leftBus = new SuperMetroidAddressSpace(rom); var rightBus = new SuperMetroidAddressSpace(rom);
        foreach (var owner in new[] { leftBus, rightBus })
        {
            var save = new SuperMetroidSaveSnapshot { Area = 4, SaveStation = 0, Health = 99, MaxHealth = 99 };
            save.MapStationBytes[4] = 1; save.UsedSaveStationBytes[8] = 1;
            new SuperMetroidSaveRam(owner).SaveSlot(0, save);
        }
        var game = new SuperMetroidGame(leftBus); var capturedGame = new SuperMetroidGame(rightBus);
        bool sawMap = false;
        for (int tick = 0; tick < 2000 && game.GameState != SuperMetroidGameState.MainGameplay; tick++)
        {
            ushort input = tick % 47 == 0 ? (ushort)SnesButton.Start : (ushort)0;
            var reference = game.Step(input); var actual = capturedGame.StepCaptured(input, tick + 1, 1);
            sawMap |= game.GameState == SuperMetroidGameState.FileSelectMap;
            AssertTrue(!actual.UsedLegacyRaster, "saved-file frontend never falls back to raster");
            AssertTrue(reference.Pixels.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(actual.Snapshot!)), "saved-file frontend pixels");
            AssertEqual(reference.GameState, actual.Frame.GameState, "saved-file frontend state");
            AssertEqual(reference.Phase, actual.Frame.Phase, "saved-file frontend phase");
            AssertSequenceEqual(reference.AudioCommands, actual.Frame.AudioCommands, "saved-file frontend audio order");
        }
        AssertTrue(sawMap && game.GameState == SuperMetroidGameState.MainGameplay, "saved-file frontend completes map and load");
        Console.WriteLine($"  File map snapshots: {samples} area/room/entry/window/load/cancel comparisons match.");

        void Compare(Rgba32[] expected, LayeredRenderSnapshot captured, string name)
        {
            var packet = RoundTripRenderPacket(new(new(++samples, 1, 0), captured));
            AssertTrue(expected.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(packet)), name);
        }
    }

    private static void VerifyWindowedSceneContract()
    {
        var palette = new ushort[SnesPpuLayout.CgramColorCount]; palette[0] = 31;
        var red = new PpuMemorySnapshot(new byte[SnesPpuLayout.VramByteCount], palette, new byte[SnesPpuLayout.OamUploadByteCount], 0);
        palette[0] = 31 << 10;
        var blue = new PpuMemorySnapshot(new byte[SnesPpuLayout.VramByteCount], palette, new byte[SnesPpuLayout.OamUploadByteCount], 0);
        var child = new LayeredRenderSnapshot(blue, new RenderLayer[] { new ObjRenderLayer() }, 3, 15);
        var window = new WindowedSceneRenderLayer(child, 127, 111, 130, 113);
        var parent = new LayeredRenderSnapshot(red, new RenderLayer[] { window }, 3, 15);
        var packet = new RenderFrameSnapshot(new(1, 1, 0), parent);
        Rgba32[] pixels = SoftwareFrameSnapshotRenderer.Render(RoundTripRenderPacket(packet));
        for (int y = 0; y < 224; y++)
        for (int x = 0; x < 256; x++)
            AssertEqual(x >= 127 && x < 130 && y >= 111 && y < 113 ? new Rgba32(0, 0, 255) : new Rgba32(255, 0, 0),
                pixels[y * 256 + x], "window replacement includes child backdrop and excludes outside pixels");
        AssertThrows<ArgumentException>(() => new WindowedSceneRenderLayer(parent, 0, 0, 1, 1), "nested scene rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => new WindowedSceneRenderLayer(child, 0, 0, 257, 224), "invalid window rejected");
        byte[] bytes = RenderFrameSnapshotCodec.Serialize(packet);
        bytes[^1] = (byte)RenderPacketLayerKind.WindowedScene;
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(bytes), "nested on-disk scene rejected before recursive parsing");
        bytes = RenderFrameSnapshotCodec.Serialize(packet);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(RenderPacketFormat.Signature.Length), RenderPacketFormat.Mode7GameplayLayerVersion);
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(bytes), "old header rejects window operation");
    }
}
