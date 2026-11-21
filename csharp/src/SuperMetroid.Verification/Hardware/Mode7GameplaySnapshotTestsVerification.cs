using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Gameplay;
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
            var packet = new DisplayPacket(0, new LayeredScene(memory, new DisplayLayer[] { layer }, DisplayBrightness.Full));
            AssertEqual(layer, (TurningRoomLayer)((LayeredScene)packet.Scene).Layers[0], "the turning-floor band layer keeps its floor view");
            if (samples == 1)
            {
                AssertThrows<ArgumentException>(() => new LayeredScene(memory,
                    new DisplayLayer[] { new SpritesLayer([]), layer }, DisplayBrightness.Full), "Mode-7 base cannot erase earlier layers");
            }
        }
        // Retail room assets with an explicitly constructed getaway floor exercise
        // the runtime producer, without pretending this is a controller-driven battle.
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame); runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.CeresRidley, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        // The station's escape owns the getaway's floor (#2228).
        var getaway = runtime.Enemies.CeresEscape.Floor;
        getaway.Active = true;
        getaway.Angle = RotationAngle.FromSteps(5);
        getaway.Zoom = FloorZoomConstants.FromTwoHundredFiftySixths(240);
        getaway.Center = new Offset16<FloorPixels>(128, 120);
        getaway.Offset = new Offset16<FloorPixels>(7, 19);
        // The getaway's floor reaches the display at the accepted frame boundary (#1307).
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        RgbaPixel[] retail = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame());
        var retained = new DisplayPacket(0, GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()));
        AssertTrue(retail.SequenceEqual(RenderForComparison(retained)), "retail Ridley capture resolves mixed bands and haze");
        // The floor band's background scroll is signed (#2308): a background scroll stored three rows above the background's
        // origin shows as a negative row, moved by the frame's shake, not as a wrapped one.
        runtime.BackgroundScroll.BackgroundPosition = new Position16<BackgroundPixels>(5, ushort.MaxValue - 2);
        runtime.BackgroundScroll.CalculateLayerScrolls();
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var aboveOrigin = (TurningRoomLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertEqual(new Offset16<BackgroundPixels>(5, -3) + runtime.Display.DisplayedGameplayScrolls.RoomShake.Background,
            (aboveOrigin.LowerBand ?? throw new InvalidOperationException("The getaway shows the room's background below its floor.")).Scroll,
            "the floor band's background scroll above the origin is signed");
        getaway.Active = false; getaway.Zoom = default;
        runtime.Vram.LoadBytes(0, new byte[SnesVram.ByteCount]);
        AssertTrue(retail.SequenceEqual(RenderForComparison(retained)), "Ridley packet survives mode exit and live memory replacement");
        AssertThrows<ArgumentOutOfRangeException>(() => new TurningRoomLayer(default, RoomBackgroundsFixtures.BlankMode7Floor(), ScreenGeometry.ScreenHeightPixels + 1), "the HUD cannot cover more than the screen");
        AssertThrows<ArgumentOutOfRangeException>(() => new TurningRoomLayer(default, RoomBackgroundsFixtures.BlankMode7Floor(), 32,
            new TurningRoomLowerBand(31, Offset16<BackgroundPixels>.Zero, RoomBackgroundsFixtures.Blank().Foreground)),
            "floor cannot overlap HUD");
        Console.WriteLine($"  Mode-7 gameplay snapshots: {samples} scale/scroll layers keep their floor views; the Ridley packet is retained.");
    }
}
