using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Hardware;

/// <summary>Checks turning-floor gameplay packets keep their floor views, and a captured Ceres Ridley scene draws as the live frame did.</summary>
internal static class Mode7GameplaySnapshotTestsVerification
{
    /// <summary>Checks the band layer across angle, scale and scroll variants, then packet retention after live scene data changes.</summary>
    internal static void VerifyMode7GameplaySnapshots()
    {
        var random = new Random(32109);
        byte[] bytes = new byte[SnesVram.ByteCount]; random.NextBytes(bytes);
        byte[] oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount]; random.NextBytes(oam);
        ushort[] palette = Enumerable.Range(0, SnesCgram.ColorCount).Select(_ => (ushort)random.Next(32768)).ToArray();
        var memory = FramePaint.FromOam(bytes, ToColors(palette), oam, 128, 3);
        // The layer's pictures are drawn from the fixture's own VRAM, which the frame no longer carries (#1403).
        var fixtureVram = new SnesVram();
        fixtureVram.LoadBytes(0, bytes);
        // The fixture's floor is composed once; each variant turns, scales and scrolls the same picture (#1677).
        var floor = AffineFloorFixtures.Floor(fixtureVram.Bytes, ToColors(palette));
        int samples = 0;
        foreach (double scale in new[] { 0, 0.5, 1, 2 })
        foreach (ushort scroll in new ushort[] { 0, 255, 511, ushort.MaxValue })
        {
            // The floor band below shows the room's layer-2 plane (#1403); the retail Ridley capture below draws it.
            var m = new FloorView(0.13, scale, new Offset16<FloorPixels>(128, 96), new Offset16<FloorPixels>(-13, unchecked((short)scroll)));
            var layer = new TurningRoomLayer(m, floor, ScreenGeometry.GameplayHudHeightPixels);
            samples++;
            var packet = new DisplayPacket(0, new LayeredScene(memory, new DisplayLayer[] { layer }, 15));
            AssertEqual(layer, (TurningRoomLayer)(packet.Layers ?? throw new InvalidOperationException("Verification requires packet.Layers.")).Layers[0], "the turning-floor band layer keeps its floor view");
            if (samples == 1)
            {
                AssertThrows<ArgumentException>(() => new LayeredScene(memory,
                    new DisplayLayer[] { new SpritesLayer([]), layer }, 15), "Mode-7 base cannot erase earlier layers");
            }
        }
        // Retail room assets with an explicitly constructed getaway floor exercise
        // the runtime producer, without pretending this is a controller-driven battle.
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug); runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.CeresRidley, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var getaway = runtime.Enemies.RidleyEncounter.CeresRidley;
        (getaway ?? throw new InvalidOperationException("Verification requires getaway.")).GetawayActive = true;
        getaway.FloorAngle = RotationAngle.FromSteps(5);
        getaway.FloorZoom = FloorZoom.FromTwoHundredFiftySixths(240);
        getaway.FloorCenter = new Position16<FloorPixels>(128, 120);
        getaway.FloorOffset = new Offset16<FloorPixels>(7, 19);
        // The getaway's floor reaches the display at the accepted frame boundary (#1307).
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        RgbaPixel[] retail = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame());
        var retained = new DisplayPacket(0, GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()));
        AssertTrue(retail.SequenceEqual(RenderForComparison(retained)), "retail Ridley capture resolves mixed bands and haze");
        getaway.GetawayActive = false; getaway.FloorZoom = default;
        runtime.Vram.LoadBytes(0, new byte[SnesVram.ByteCount]);
        AssertTrue(retail.SequenceEqual(RenderForComparison(retained)), "Ridley packet survives mode exit and live memory replacement");
        AssertThrows<ArgumentOutOfRangeException>(() => new TurningRoomLayer(default, RoomBackgroundsFixtures.BlankMode7Floor(), 31), "HUD must cover complete tile rows");
        AssertThrows<ArgumentOutOfRangeException>(() => new TurningRoomLayer(default, RoomBackgroundsFixtures.BlankMode7Floor(), 32,
            new TurningRoomLowerBand(31, new Position16<BackgroundPixels>(0, 0), RoomBackgroundsFixtures.Blank().Foreground)),
            "floor cannot overlap HUD");
        Console.WriteLine($"  Mode-7 gameplay snapshots: {samples} scale/scroll layers keep their floor views; the Ridley packet is retained.");
    }
}
