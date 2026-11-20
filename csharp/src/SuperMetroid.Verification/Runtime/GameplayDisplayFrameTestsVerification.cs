using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.World;
namespace SuperMetroid.Verification.Runtime;

/// <summary>
/// Confirms the published gameplay display frame (#1307): repainting never advances or changes it,
/// it shares no storage with simulation, and each register shadow the cartridge uploads at NMI
/// reaches the screen at the same accepted NMI as the rest of that frame's latch.
/// </summary>
internal static class GameplayDisplayFrameTestsVerification
{
    /// <summary>A WOBJLOG value used to mark which accepted NMI published a frame's latch.</summary>



    /// <summary>A layer-one Y position the scrolling sky mirrors into BG2VOFS.</summary>
    private const ushort SkyLayer1Y = 291;

    /// <summary>Runs the repaint, immutability and per-input NMI timing confirmations.</summary>
    internal static void VerifyGameplayDisplayFrame()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        VerifyCeresModeTiming(runtime);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.AlphaPowerBomb, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);

        VerifyRepaintAndImmutability(runtime);
        VerifyBossBackgroundTiming(runtime);
        VerifySceneRegisterTiming(runtime);
        VerifyFixedColorTiming(runtime);
        Console.WriteLine("  Gameplay display frame: repaint invariance, immutability and NMI timing of every re-timed register shadow confirmed.");
    }

    /// <summary>Publishing twice without stepping, and capturing one frame twice, yield identical pictures that later simulation cannot change.</summary>
    private static void VerifyRepaintAndImmutability(SuperMetroidRuntime runtime)
    {
        GameplayDisplayFrame frame = runtime.PublishDisplayFrame();
        RgbaPixel[] pixels = DisplayRendering.Render(GameplayDisplayCapture.Capture(frame));
        AssertTrue(pixels.SequenceEqual(DisplayRendering.Render(GameplayDisplayCapture.Capture(frame))),
            "capturing one published frame twice is identical");
        AssertTrue(pixels.SequenceEqual(DisplayRendering.Render(GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()))),
            "publishing again without stepping is identical");

        // Mutate frame-end sources, NMI-latched sources and PPU memory after publication.
        GameplayFrameEndDisplayInputs frameEnd = frame.FrameEnd;
        GameplayDisplayLatch latched = frame.Latched;
        SamusPowerBombExplosionState explosion = runtime.BombProjectiles.PowerBombExplosion;
        explosion.Arm();
        explosion.Spawn(new(128, 120));
        for (int i = 0; i < 20; i++) explosion.StepFrame(runtime.Artwork.SamusArtwork.PowerBombGlowColors);
        // A sky room installs a live sky the published frame never had (#1403).
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.LandingSite, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        // Wiping VRAM unbinds the room's artwork, so it follows the next frame's latch; the room reloads below.
        runtime.Vram.LoadBytes(0, new byte[SnesVram.ByteCount]);
        AssertTrue(ReferenceEquals(frameEnd, frame.FrameEnd) && ReferenceEquals(latched, frame.Latched),
            "a published frame keeps its own latch and frame-end inputs");
        AssertEqual(false, frame.FrameEnd.PowerBomb.IsActive, "published power bomb inputs are a value copy");
        AssertTrue(frame.FrameEnd.ScrollingSky is null, "published sky layer is not the live owner");
        AssertTrue(pixels.SequenceEqual(DisplayRendering.Render(GameplayDisplayCapture.Capture(frame))),
            "a published frame survives later simulation, latch and PPU memory changes");

        explosion.Reset();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.AlphaPowerBomb, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
    }

    /// <summary>Kraid's body on BG2 (#1352) and Mother Brain's BG2 register shadows publish with the accepted NMI's latch.</summary>
    private static void VerifyBossBackgroundTiming(SuperMetroidRuntime runtime)
    {
        // Each boss's body shows in its own room, whose artwork pictures it (#1392).
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Kraid, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        GameplaySceneView before = Registers(runtime);
        RoomEnemyPlace kraidBody = runtime.Enemies.KraidBehavior.Parts.Body;
        kraidBody.Position = kraidBody.Position.WithWhole(new(320, 88));
        KraidEnemyState kraid = runtime.Enemies.Kraid ?? throw new InvalidOperationException("Kraid's room loads Kraid.");
        Offset16<BackgroundPixels>? bodyBefore = runtime.PublishDisplayFrame().OrdinaryBase.Room.BossBody?.Scroll;
        kraid.ShowsBodyLayer = true;
        AssertEqual(bodyBefore, runtime.PublishDisplayFrame().OrdinaryBase.Room.BossBody?.Scroll,
            "Kraid's body layer written by the main loop waits for the NMI");
        // His body's layer shows the body row below where his body lies against the frame's camera at the screen's top
        // (#1514, #1617, #2573).
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual<Offset16<BackgroundPixels>?>(RoomView.Shown(BossBodyPlacement.Shown(kraidBody.Position.Whole,
            runtime.Display.LatchedRoomFrame.Scrolls.ForegroundPosition)), runtime.PublishDisplayFrame().OrdinaryBase.Room.BossBody?.Scroll,
            "the accepted NMI publishes Kraid's body layer");
        kraid.ShowsBodyLayer = false;
        runtime.FrameBoundary.BeginAcceptedFrame(0);

        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.MotherBrain, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        before = Registers(runtime);
        var motherBrain = new MotherBrainEnemyState(runtime.Enemies.Places.InUpdateOrder[0], runtime.Enemies.Places.InUpdateOrder[1], new MotherBrainWalkCounter(), new MotherBrainBurstCycle(), new MotherBrainBrainAnimation()) { BodyPlacement = MotherBrainBodyPlacement.FollowsBody, DeathHidesBodyLayer = true };
        runtime.Enemies.MotherBrainEncounter.BeginAsSetUp(motherBrain);
        // The room's display reads the body the encounter attached as it set up (#2194): this one's.
        runtime.Enemies.Presentation.AttachBossBody(BossBodyParts.Hidden(BossBodyOwner.MotherBrain, motherBrain));
        AssertPending(runtime, registers => registers.ShownLayers == before.ShownLayers, "Mother Brain's hidden background");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertTrue(!Registers(runtime).ShownLayers.HasFlag(ScreenLayers.BossBody), "the accepted NMI publishes Mother Brain's hidden body");
        AssertEqual(motherBrain.RainbowBeam.Color, motherBrain.RainbowBeam.DisplayedColor,
            "the rainbow beam's colour is latched with Mother Brain's registers");
        motherBrain.DeathHidesBodyLayer = false;
        motherBrain.BodyPlacement = MotherBrainBodyPlacement.Unplaced;
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.AlphaPowerBomb, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
    }

    /// <summary>The scrolling sky's background scroll publishes with the accepted frame's latch.</summary>
    private static void VerifySceneRegisterTiming(SuperMetroidRuntime runtime)
    {
        GameplaySceneView before;
        // $88:AFB2 writes the BG2VOFS mirror while the same pass rebuilds the band table.
        // HDMA reads the table during display; the mirror waits for the NMI upload. A sky room installs its sky, and
        // the sky's image, as it loads (#1403).
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.LandingSite, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        before = Registers(runtime);
        ScrollingSkyState sky = runtime.Room.ScrollingSky ?? throw new InvalidOperationException("The Landing Site installs its scrolling sky.");
        sky.ProcessFrame(new Position16<RoomPixels>(0, SkyLayer1Y), timeIsFrozen: false);
        AssertTrue(runtime.PublishDisplayFrame().FrameEnd.ScrollingSky is not null,
            "sky bands are read live during display");
        AssertPending(runtime, registers => registers.BackgroundScroll.Y == before.BackgroundScroll.Y, "the sky's vertical scroll");
        // The view shows the sky row below the published scroll at the screen's top (#1514).
        AssertPublished(runtime, registers => registers.BackgroundScroll.Y == RoomView.ShownRow(SkyLayer1Y), "the sky's vertical scroll");
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.AlphaPowerBomb, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
    }

    /// <summary>Each effect's tint publishes at the accepted frame while the same effect's window table is read live.</summary>
    private static void VerifyFixedColorTiming(SuperMetroidRuntime runtime)
    {
        // Fireflea darkness: the effect and the shade it publishes. From the first flash shade (level zero), a step at darkness
        // offset six shades level eighteen and one at offset two level six.
        RoomFxType originalFx = runtime.RoomFxOverlay.Type;
        FirefleaFxState fireflea = runtime.RoomFxOverlay.StartFirefleaDarknessForCheck();
        fireflea.Step(frozen: false, global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Fx.FirefleaDarkness>([typeof(int)], new object?[] { (int)(3) }));
        AssertTrue(GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0] is GameplaySceneLayer,
            "Fireflea darkness waits for the accepted frame");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var darkness = (GameplayBlendLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertEqual(18, darkness.Blend.Colour.FadeLevels.Red, "Fireflea's shade publishes with its blend");
        fireflea.Step(frozen: false, global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Fx.FirefleaDarkness>([typeof(int)], new object?[] { (int)(1) }));
        AssertEqual(18, ((GameplayBlendLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0]).Blend.Colour.FadeLevels.Red,
            "a shade chosen in the main loop waits for the next accepted frame");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual(6, ((GameplayBlendLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0]).Blend.Colour.FadeLevels.Red,
            "the next accepted frame publishes the chosen shade");
        runtime.RoomFxOverlay.EndFirefleaDarknessForCheck(originalFx);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual<LatchedFirefleaDarkness?>(null, runtime.PublishDisplayFrame().OrdinaryLatch.Fireflea,
            "a room whose effect is not Fireflea darkness publishes no Fireflea shade");

        // Power bomb: the window table is built during the main loop and read live; the
        // pre-instruction's COLDATA write reaches the screen at the following NMI.
        SamusPowerBombExplosionState explosion = runtime.BombProjectiles.PowerBombExplosion;
        explosion.Arm();
        explosion.Spawn(new(128, 120));
        for (int i = 0; i < 4; i++) explosion.StepFrame(runtime.Artwork.SamusArtwork.PowerBombGlowColors);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        int latchedRed = explosion.Blast.GlowColor.FadeLevels.Red;
        int steps = 0;
        while (explosion.Blast.GlowColor.FadeLevels.Red == latchedRed && steps++ < 64) explosion.StepFrame(runtime.Artwork.SamusArtwork.PowerBombGlowColors);
        AssertTrue(explosion.Blast.GlowColor.FadeLevels.Red != latchedRed, "fixture reaches a power-bomb COLDATA change");
        GameplayDisplayFrame pending = runtime.PublishDisplayFrame();
        AssertEqual(explosion.ShownOval.Radii, pending.FrameEnd.PowerBomb.Oval.Radii, "power-bomb oval is read live");
        AssertEqual(latchedRed, pending.PowerBomb.Oval.Colour.FadeLevels.Red, "power-bomb COLDATA waits for the NMI");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual(explosion.Blast.GlowColor.FadeLevels.Red, runtime.PublishDisplayFrame().PowerBomb.Oval.Colour.FadeLevels.Red,
            "the accepted NMI publishes power-bomb COLDATA");
        explosion.Reset();
        runtime.FrameBoundary.BeginAcceptedFrame(0);

        // Suit pickup: the light beam is a live HDMA window; its COLDATA follows the NMI.
        SamusState samus = runtime.Player.IntroducedSamus;
        runtime.SuitPickup.Begin(samus, new(0, 0), SamusSuitPickupKind.Varia);
        runtime.SuitPickup.Step(samus, runtime.Player.Colours);
        RgbColor suitBefore = runtime.PublishDisplayFrame().RoomLatch.Tints.SuitPickup;
        AssertTrue(runtime.PublishDisplayFrame().FrameEnd.SuitPickupBeam is not null, "suit light beam window is read live");
        AssertEqual(suitBefore.FadeLevels.Red, (runtime.PublishDisplayFrame().SuitPickup
            ?? throw new InvalidOperationException("Active suit pickup has no flash.")).Color.FadeLevels.Red, "suit COLDATA waits for the NMI");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual(runtime.SuitPickup.BeamColor.FadeLevels.Red, (runtime.PublishDisplayFrame().SuitPickup
            ?? throw new InvalidOperationException("Active suit pickup has no flash.")).Color.FadeLevels.Red, "the accepted NMI publishes suit COLDATA");
    }

    /// <summary>
    /// The shaft's floor its door's setup shows, its removal and Ridley's floor all reach the display at the accepted frame
    /// after they are set, which alone latches the display (#2498).
    /// </summary>
    private static void VerifyCeresModeTiming(SuperMetroidRuntime runtime)
    {
        AssertTrue(runtime.PublishDisplayFrame().Latched is LatchBeforeFirstRoom,
            "the shaft's door setup shows nothing before the accepted frame");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertTrue(runtime.PublishDisplayFrame().Latched is PerspectiveFloorFrameLatch { BackgroundBand: null },
            "the accepted frame after the shaft's door setup shows the elevator shaft's floor filling the view");

        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.CeresRidley, new Position16<RoomPixels>(0, 0));
        AssertTrue(runtime.PublishDisplayFrame().Latched is PerspectiveFloorFrameLatch,
            "leaving the shaft keeps its floor until the accepted frame");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertTrue(runtime.PublishDisplayFrame().Latched is OrdinaryRoomFrameLatch,
            "the accepted frame shows Ceres Ridley's room without the elevator shaft's floor");

        // The station's escape owns the getaway's floor (#2228).
        CeresTurningFloor getaway = runtime.Enemies.CeresEscape.Floor;
        getaway.Active = true;
        getaway.Angle = RotationAngle.FromSteps(5);
        getaway.Zoom = FloorZoomConstants.FromTwoHundredFiftySixths(240);
        getaway.Center = new Offset16<FloorPixels>(128, 120);
        getaway.Offset = new Offset16<FloorPixels>(7, 19);
        AssertTrue(GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0] is GameplaySceneLayer,
            "Ridley's floor and fake BGMODE wait for the NMI");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var mode7 = (TurningRoomLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertEqual(RotationAngle.FromSteps(5).Radians, mode7.Transform.Angle, "the accepted NMI publishes Ridley's floor angle");
        AssertEqual(240 / 256.0, mode7.Transform.Scale, "the accepted NMI publishes Ridley's floor zoom");
        AssertTrue(mode7.LowerBand is not null, "the accepted NMI publishes Ridley's mixed Mode 7 band");
        getaway.Active = false;
        runtime.FrameBoundary.BeginAcceptedFrame(0);
    }


    /// <summary>Asserts a main-loop write is not yet displayed.</summary>
    private static void AssertPending(SuperMetroidRuntime runtime, Func<GameplaySceneView, bool> unchanged, string input)
    {
        AssertTrue(unchanged(Registers(runtime)), $"{input} written by the main loop waits for the NMI");
    }

    /// <summary>Asserts an accepted NMI publishes the write.</summary>
    private static void AssertPublished(SuperMetroidRuntime runtime, Func<GameplaySceneView, bool> published, string input)
    {

        runtime.FrameBoundary.BeginAcceptedFrame(0);
        GameplaySceneView registers = Registers(runtime);
        AssertTrue(published(registers), $"the accepted NMI publishes {input}");

    }

    /// <summary>The ordinary gameplay registers of a freshly published frame.</summary>
    private static GameplaySceneView Registers(SuperMetroidRuntime runtime) =>
        (runtime.PublishDisplayFrame().OrdinaryBase.Room).View;

    /// <summary>Runs the focused gameplay display frame confirmation.</summary>
    [VerificationCommand("--gameplay-display-frame")]
    internal static int GameplayDisplayFrameCommand()
    {
        Suite(nameof(VerifyGameplayDisplayFrame), () => VerifyGameplayDisplayFrame());
        return 0;
    }
}
