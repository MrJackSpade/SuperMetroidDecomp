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
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Rooms.Artwork;
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
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        VerifyCeresModeTiming(runtime);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.AlphaPowerBomb, new Position16<RoomPixels>(0, 0));
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
        for (int i = 0; i < 20; i++) explosion.StepFrame();
        // A sky room installs a live sky the published frame never had (#1403).
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LandingSite, new Position16<RoomPixels>(0, 0));
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
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.AlphaPowerBomb, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
    }

    /// <summary>Kraid's body on BG2 (#1352) and Mother Brain's BG2 register shadows publish with the accepted NMI's latch.</summary>
    private static void VerifyBossBackgroundTiming(SuperMetroidRuntime runtime)
    {
        // Each boss's body shows in its own room, whose artwork pictures it (#1392).
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Kraid, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        GameplaySceneView before = Registers(runtime);
        var kraid = new KraidEnemyState { ShowsBodyLayer = true, BodyOrigin = new Position16<RoomPixels>(320, 88) };
        runtime.Enemies.KraidBehavior.Access.Kraid = kraid;
        AssertPending(runtime, registers => registers.BackgroundScroll == before.BackgroundScroll, "Kraid's body on the background");
        // The view shows the body row below where his body lies against the frame's camera at the screen's top (#1514, #1617).
        AssertPublished(runtime, registers => registers.BackgroundScroll == GameplaySceneView.ShownAtScreenTop(
            BossBodyPlacement.Shown(kraid.BodyOrigin, runtime.Display.Latched.Scrolls.ForegroundPosition)), "Kraid's body on the background");
        runtime.Enemies.KraidBehavior.Access.Kraid = null;
        runtime.FrameBoundary.BeginAcceptedFrame(0);

        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MotherBrain, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        before = Registers(runtime);
        var motherBrain = new MotherBrainEnemyState(runtime.Enemies.SlotTable.Slots[0]) { PlacesBodyLayer = true, DeathHidesBodyLayer = true };
        runtime.Enemies.MotherBrainEncounter.MotherBrain = motherBrain;
        AssertPending(runtime, registers => registers.ShownLayers == before.ShownLayers, "Mother Brain's hidden background");
        AssertPublished(runtime, registers => registers.ShownLayers == (ScreenLayers.Foreground | ScreenLayers.Sprites),
            "Mother Brain's hidden background");
        AssertEqual(motherBrain.RainbowBeam.Color, runtime.PublishDisplayFrame().Latched.Tints.RainbowBeam,
            "rainbow beam COLDATA is latched with Mother Brain's registers");
        runtime.Enemies.MotherBrainEncounter.MotherBrain = null;
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.AlphaPowerBomb, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
    }

    /// <summary>The scrolling sky's background scroll publishes with the accepted frame's latch.</summary>
    private static void VerifySceneRegisterTiming(SuperMetroidRuntime runtime)
    {
        GameplaySceneView before;
        // $88:AFB2 writes the BG2VOFS mirror while the same pass rebuilds the band table.
        // HDMA reads the table during display; the mirror waits for the NMI upload. A sky room installs its sky, and
        // the sky's image, as it loads (#1403).
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LandingSite, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        before = Registers(runtime);
        ScrollingSkyState sky = runtime.Room.ScrollingSky ?? throw new InvalidOperationException("The Landing Site installs its scrolling sky.");
        sky.ProcessFrame(SkyLayer1Y, timeIsFrozen: false);
        AssertTrue(runtime.PublishDisplayFrame().FrameEnd.ScrollingSky is not null,
            "sky bands are read live during display");
        AssertPending(runtime, registers => registers.BackgroundScroll.Y == before.BackgroundScroll.Y, "the sky's vertical scroll");
        // The view shows the sky row below the published scroll at the screen's top (#1514).
        AssertPublished(runtime, registers => registers.BackgroundScroll.Y == GameplaySceneView.ShownRowAtScreenTop(SkyLayer1Y), "the sky's vertical scroll");
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.AlphaPowerBomb, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
    }

    /// <summary>Each effect's tint publishes at the accepted frame while the same effect's window table is read live.</summary>
    private static void VerifyFixedColorTiming(SuperMetroidRuntime runtime)
    {
        // Fireflea darkness: the effect and the shade it publishes. From the first flash shade (level zero), a step at darkness
        // offset six shades level eighteen and one at offset two level six.
        RoomFxType originalFx = runtime.RoomFxOverlay.Type;
        FirefleaFxState fireflea = runtime.RoomFx.Fireflea;
        runtime.RoomFxOverlay.Type = RoomFxType.Fireflea;
        fireflea.Initialize();
        fireflea.Step(frozen: false, darknessStep: 3);
        AssertTrue(GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0] is GameplaySceneLayer,
            "Fireflea darkness waits for the accepted frame");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var darkness = (GameplayBlendLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertEqual(18, darkness.Blend.Colour.FadeLevels.Red, "Fireflea's shade publishes with its blend");
        fireflea.Step(frozen: false, darknessStep: 1);
        AssertEqual(18, ((GameplayBlendLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0]).Blend.Colour.FadeLevels.Red,
            "a shade chosen in the main loop waits for the next accepted frame");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual(6, ((GameplayBlendLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0]).Blend.Colour.FadeLevels.Red,
            "the next accepted frame publishes the chosen shade");
        runtime.RoomFxOverlay.Type = originalFx;
        fireflea.Initialize();
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual<RgbColor?>(null, runtime.PublishDisplayFrame().Latched.Tints.Fireflea,
            "a room whose effect is not Fireflea darkness publishes no Fireflea shade");

        // Power bomb: the window table is built during the main loop and read live; the
        // pre-instruction's COLDATA write reaches the screen at the following NMI.
        SamusPowerBombExplosionState explosion = runtime.BombProjectiles.PowerBombExplosion;
        explosion.Arm();
        explosion.Spawn(new(128, 120));
        for (int i = 0; i < 4; i++) explosion.StepFrame();
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        int latchedRed = explosion.GlowColor.FadeLevels.Red;
        int steps = 0;
        while (explosion.GlowColor.FadeLevels.Red == latchedRed && steps++ < 64) explosion.StepFrame();
        AssertTrue(explosion.GlowColor.FadeLevels.Red != latchedRed, "fixture reaches a power-bomb COLDATA change");
        GameplayDisplayFrame pending = runtime.PublishDisplayFrame();
        AssertEqual(explosion.RenderedPhase, pending.FrameEnd.PowerBomb.RenderedPhase, "power-bomb window table is read live");
        AssertEqual(latchedRed, GameplayDisplayCapture.PowerBomb(pending).GlowColor.FadeLevels.Red, "power-bomb COLDATA waits for the NMI");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual(explosion.GlowColor.FadeLevels.Red, GameplayDisplayCapture.PowerBomb(runtime.PublishDisplayFrame()).GlowColor.FadeLevels.Red,
            "the accepted NMI publishes power-bomb COLDATA");
        explosion.Reset();
        runtime.FrameBoundary.BeginAcceptedFrame(0);

        // Suit pickup: the light beam is a live HDMA window; its COLDATA follows the NMI.
        SamusState samus = runtime.Player.IntroducedSamus;
        runtime.SuitPickup.Begin(samus, new(0, 0), SamusSuitPickupKind.Varia);
        runtime.SuitPickup.Step(samus, runtime.Player.Colours);
        RgbColor suitBefore = runtime.PublishDisplayFrame().Latched.Tints.SuitPickup;
        AssertTrue(runtime.PublishDisplayFrame().FrameEnd.SuitPickupBeam is not null, "suit light beam window is read live");
        AssertEqual(suitBefore.FadeLevels.Red, (GameplayDisplayCapture.SuitPickup(runtime.PublishDisplayFrame())
            ?? throw new InvalidOperationException("Active suit pickup has no flash.")).Color.FadeLevels.Red, "suit COLDATA waits for the NMI");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual(runtime.SuitPickup.BeamColor.FadeLevels.Red, (GameplayDisplayCapture.SuitPickup(runtime.PublishDisplayFrame())
            ?? throw new InvalidOperationException("Active suit pickup has no flash.")).Color.FadeLevels.Red, "the accepted NMI publishes suit COLDATA");
    }

    /// <summary>Door setup shows the shaft's floor with its transform; room setup's characters and Ridley's floor follow the accepted frame.</summary>
    private static void VerifyCeresModeTiming(SuperMetroidRuntime runtime)
    {
        GameplayDisplayLatch shaft = runtime.PublishDisplayFrame().Latched;
        AssertTrue(shaft.PerspectiveFloor is { Below: BelowPerspectiveFloor.Nothing },
            "door setup $8F:E4E0 shows the elevator shaft's floor");

        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.CeresRidley, new Position16<RoomPixels>(0, 0));
        GameplayDisplayLatch arena = runtime.PublishDisplayFrame().Latched;
        AssertTrue(arena.PerspectiveFloor is null,
            "door setup $8F:E513 removes the elevator shaft's floor");
        AssertTrue(arena.Rooms?.BlockArtwork != RoomBlockArtwork.Sprites, "room setup BG12NBA waits for the NMI");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual(RoomBlockArtwork.Sprites, runtime.PublishDisplayFrame().Latched.Rooms?.BlockArtwork,
            "the accepted NMI publishes the Ceres Ridley BG12NBA");

        RidleyEnemyState getaway = runtime.Enemies.RidleyEncounter.CeresRidley
            ?? throw new InvalidOperationException("Ceres Ridley room has no Ridley.");
        getaway.GetawayActive = true;
        getaway.FloorAngle = RotationAngle.FromSteps(5);
        getaway.FloorZoom = 240;
        getaway.FloorCenter = new Position16<FloorPixels>(128, 120);
        getaway.FloorOffset = new Offset16<FloorPixels>(7, 19);
        AssertTrue(GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0] is GameplaySceneLayer,
            "Ridley's floor and fake BGMODE wait for the NMI");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var mode7 = (TurningRoomLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertEqual(RotationAngle.FromSteps(5).Radians, mode7.Transform.Angle, "the accepted NMI publishes Ridley's floor angle");
        AssertEqual(240 / 256.0, mode7.Transform.Scale, "the accepted NMI publishes Ridley's floor zoom");
        AssertTrue(mode7.Floor is not null, "the accepted NMI publishes Ridley's mixed Mode 7 band");
        getaway.GetawayActive = false;
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
        ((GameplaySceneLayer)GameplayDisplayCapture.CaptureOrdinaryBase(runtime.PublishDisplayFrame()).Layers[0]).View;

    /// <summary>Runs the focused gameplay display frame confirmation.</summary>
    [VerificationCommand("--gameplay-display-frame")]
    internal static int GameplayDisplayFrameCommand()
    {
        Suite(nameof(VerifyGameplayDisplayFrame), () => VerifyGameplayDisplayFrame());
        return 0;
    }
}
