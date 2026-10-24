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
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms.Backgrounds;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.TestSupport.FrameRendering;
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
    [PpuLayoutBoundary("exact hardware test: replaces PPU memory after publication")]
    private static void VerifyRepaintAndImmutability(SuperMetroidRuntime runtime)
    {
        GameplayDisplayFrame frame = runtime.PublishDisplayFrame();
        Rgba32[] pixels = DisplayRendering.Render(GameplayDisplayCapture.Capture(frame));
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
        runtime.Vram.LoadBytes(0, new byte[SnesPpuLayout.VramByteCount]);
        for (int color = 0; color < SnesPpuLayout.CgramColorCount; color++) runtime.Cgram.AsPaletteColors().SetColor(color, new Bgr555(31, 0, 31));
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
    [PpuLayoutBoundary("exact hardware test: checks PPU register numbers")]
    private static void VerifyBossBackgroundTiming(SuperMetroidRuntime runtime)
    {
        // Each boss's body shows in its own room, whose artwork pictures it (#1392).
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Kraid, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        OrdinaryGameplayView before = Registers(runtime);
        var kraid = new KraidEnemyState { OwnsBg2Tilemap = true, Bg2Scroll = new Position16<Bg2Pixels>(320, 88) };
        runtime.Enemies.KraidBehavior.Access.Kraid = kraid;
        AssertPending(runtime, registers => registers.Bg2MapSize == before.Bg2MapSize &&
            registers.Bg2Scroll == before.Bg2Scroll, "Kraid's body on BG2");
        AssertPublished(runtime, registers => registers.Bg2MapSize == new Extent16<TilemapCells>(64, 64) &&
            registers.Bg2Scroll == kraid.Bg2Scroll, "Kraid's body on BG2");
        runtime.Enemies.KraidBehavior.Access.Kraid = null;
        runtime.FrameBoundary.BeginAcceptedFrame(0);

        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MotherBrain, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        before = Registers(runtime);
        var motherBrain = new MotherBrainEnemyState(runtime.Enemies.SlotTable.Slots[0]) { HasBg2ScrollOverride = true, DeathBg2Hidden = true };
        runtime.Enemies.MotherBrainEncounter.MotherBrain = motherBrain;
        AssertPending(runtime, registers => registers.Bg2MapSize == before.Bg2MapSize &&
            registers.MainScreenLayers == before.MainScreenLayers, "Mother Brain BG2SC and TM BG2 bit");
        AssertPublished(runtime, registers => registers.Bg2MapSize == new Extent16<TilemapCells>(32, 32) &&
            registers.MainScreenLayers == (ScreenLayers.Bg1 | ScreenLayers.Sprites), "Mother Brain BG2SC and TM BG2 bit");
        AssertEqual(motherBrain.RainbowBeam.Color, runtime.PublishDisplayFrame().Latched.FixedColors.RainbowBeam,
            "rainbow beam COLDATA is latched with Mother Brain's registers");
        runtime.Enemies.MotherBrainEncounter.MotherBrain = null;
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.AlphaPowerBomb, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
    }

    /// <summary>The FX blend selectors and the scrolling sky's BG2 shadows publish with the accepted NMI's latch.</summary>
    [PpuLayoutBoundary("exact hardware test: checks PPU register numbers")]
    private static void VerifySceneRegisterTiming(SuperMetroidRuntime runtime)
    {
        OrdinaryGameplayView before = Registers(runtime);
        RoomFxType originalFx = runtime.RoomLayer3Fx.Type;
        runtime.RoomLayer3Fx.Type = RoomFxType.TourianEntranceStatue;
        AssertPending(runtime, registers => registers.Bg2MapSize == before.Bg2MapSize, "FX-selected BG2SC");
        AssertPublished(runtime, registers => registers.Bg2MapSize == new Extent16<TilemapCells>(32, 64), "FX-selected BG2SC");
        runtime.RoomLayer3Fx.Type = originalFx;
        runtime.FrameBoundary.BeginAcceptedFrame(0);

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
        AssertPending(runtime, registers => registers.Bg2Scroll.Y == before.Bg2Scroll.Y &&
            registers.Bg2MapSize == before.Bg2MapSize, "sky BG2VOFS and BG2SC");
        AssertPublished(runtime, registers => registers.Bg2Scroll.Y == SkyLayer1Y &&
            registers.Bg2MapSize == new Extent16<TilemapCells>(32, 64), "sky BG2VOFS and BG2SC");
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.AlphaPowerBomb, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
    }

    /// <summary>COLDATA shadows publish at the accepted NMI while the same effect's window table is read live.</summary>
    [PpuLayoutBoundary("exact hardware test: checks PPU register numbers")]
    private static void VerifyFixedColorTiming(SuperMetroidRuntime runtime)
    {
        // Fireflea darkness: FX selector and the shared COLDATA shadows.
        RoomFxType originalFx = runtime.RoomLayer3Fx.Type;
        RoomFixedColor fixedColor = runtime.RoomLayer3Fx.FixedColors;
        runtime.RoomLayer3Fx.Type = RoomFxType.Fireflea;
        fixedColor.Color = new Bgr555(9, 9, 9);
        AssertTrue(GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0] is OrdinaryGameplayRenderLayer,
            "Fireflea blend selector waits for the NMI");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var darkness = (GameplayColorMathRenderLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertEqual(9, darkness.Blend.FixedColor.Red, "Fireflea's fixed colour publishes with its blend");
        fixedColor.Color = fixedColor.Color.WithRed(4);
        AssertEqual(9, ((GameplayColorMathRenderLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0]).Blend.FixedColor.Red,
            "a fixed colour chosen in the main loop waits for the next accepted frame");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual(4, ((GameplayColorMathRenderLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0]).Blend.FixedColor.Red,
            "the next accepted frame publishes the chosen fixed colour");
        runtime.RoomLayer3Fx.Type = originalFx;
        fixedColor.Color = Bgr555.Black;
        runtime.FrameBoundary.BeginAcceptedFrame(0);

        // Power bomb: the window table is built during the main loop and read live; the
        // pre-instruction's COLDATA write reaches the screen at the following NMI.
        SamusPowerBombExplosionState explosion = runtime.BombProjectiles.PowerBombExplosion;
        explosion.Arm();
        explosion.Spawn(new(128, 120));
        for (int i = 0; i < 4; i++) explosion.StepFrame();
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        int latchedRed = explosion.FixedColor.Red;
        int steps = 0;
        while (explosion.FixedColor.Red == latchedRed && steps++ < 64) explosion.StepFrame();
        AssertTrue(explosion.FixedColor.Red != latchedRed, "fixture reaches a power-bomb COLDATA change");
        GameplayDisplayFrame pending = runtime.PublishDisplayFrame();
        AssertEqual(explosion.RenderedPhase, pending.FrameEnd.PowerBomb.RenderedPhase, "power-bomb window table is read live");
        AssertEqual(latchedRed, GameplayDisplayCapture.PowerBomb(pending).FixedColor.Red, "power-bomb COLDATA waits for the NMI");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual(explosion.FixedColor.Red, GameplayDisplayCapture.PowerBomb(runtime.PublishDisplayFrame()).FixedColor.Red,
            "the accepted NMI publishes power-bomb COLDATA");
        explosion.Reset();
        runtime.FrameBoundary.BeginAcceptedFrame(0);

        // Suit pickup: the light beam is a live HDMA window; its COLDATA follows the NMI.
        SamusState samus = runtime.Player.IntroducedSamus;
        runtime.SuitPickup.Begin(samus, new(0, 0), SamusSuitPickupKind.Varia);
        runtime.SuitPickup.Step(samus, runtime.Player.Colours);
        Bgr555 suitBefore = runtime.PublishDisplayFrame().Latched.FixedColors.SuitPickup;
        AssertTrue(runtime.PublishDisplayFrame().FrameEnd.SuitPickupBeam is not null, "suit light beam window is read live");
        AssertEqual(suitBefore.Red, (GameplayDisplayCapture.SuitPickup(runtime.PublishDisplayFrame())
            ?? throw new InvalidOperationException("Active suit pickup has no flash.")).Color.Red, "suit COLDATA waits for the NMI");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual(runtime.SuitPickup.FixedColor.Red, (GameplayDisplayCapture.SuitPickup(runtime.PublishDisplayFrame())
            ?? throw new InvalidOperationException("Active suit pickup has no flash.")).Color.Red, "the accepted NMI publishes suit COLDATA");
    }

    /// <summary>Door setup shows the shaft's floor with its transform; room setup's characters and Ridley's floor follow the accepted frame.</summary>
    [PpuLayoutBoundary("exact hardware test: checks PPU register numbers")]
    private static void VerifyCeresModeTiming(SuperMetroidRuntime runtime)
    {
        GameplayDisplayLatch shaft = runtime.PublishDisplayFrame().Latched;
        AssertTrue(shaft.PerspectiveFloor is { Below: BelowPerspectiveFloor.Nothing },
            "door setup $8F:E4E0 shows the elevator shaft's floor");

        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.CeresRidley, new Position16<RoomPixels>(0, 0));
        GameplayDisplayLatch arena = runtime.PublishDisplayFrame().Latched;
        AssertTrue(arena.PerspectiveFloor is null,
            "door setup $8F:E513 removes the elevator shaft's floor");
        AssertTrue(arena.Rooms?.Characters != RoomBackgroundCharacters.ObjectCharacterPage, "room setup BG12NBA waits for the NMI");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual(RoomBackgroundCharacters.ObjectCharacterPage, runtime.PublishDisplayFrame().Latched.Rooms?.Characters,
            "the accepted NMI publishes the Ceres Ridley BG12NBA");

        RidleyEnemyState getaway = runtime.Enemies.RidleyEncounter.CeresRidley
            ?? throw new InvalidOperationException("Ceres Ridley room has no Ridley.");
        getaway.Mode7Active = true;
        getaway.Mode7MatrixA = SnesSignedEightEight.FromSignedRaw(240); getaway.Mode7MatrixB = SnesSignedEightEight.FromSignedRaw(33);
        getaway.Mode7MatrixC = SnesSignedEightEight.FromSignedRaw(-33); getaway.Mode7MatrixD = SnesSignedEightEight.FromSignedRaw(240);
        getaway.Mode7Center = new Position16<Mode7Pixels>(128, 120);
        getaway.Mode7Offset = new Offset16<Mode7Pixels>(7, 19);
        AssertTrue(GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0] is OrdinaryGameplayRenderLayer,
            "Ridley's M7A..M7Y and fake BGMODE wait for the NMI");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var mode7 = (Mode7GameplayRenderLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertEqual((short)240, mode7.Transform.MatrixA, "the accepted NMI publishes Ridley's M7A");
        AssertEqual((short)33, mode7.Transform.MatrixB, "the accepted NMI publishes Ridley's M7B");
        AssertTrue(mode7.Floor is not null, "the accepted NMI publishes Ridley's mixed Mode 7 band");
        getaway.Mode7Active = false;
        runtime.FrameBoundary.BeginAcceptedFrame(0);
    }


    /// <summary>Asserts a main-loop write is not yet displayed.</summary>
    private static void AssertPending(SuperMetroidRuntime runtime, Func<OrdinaryGameplayView, bool> unchanged, string input)
    {
        AssertTrue(unchanged(Registers(runtime)), $"{input} written by the main loop waits for the NMI");
    }

    /// <summary>Asserts an accepted NMI publishes the write.</summary>
    [PpuLayoutBoundary("exact hardware test: checks PPU register numbers")]
    private static void AssertPublished(SuperMetroidRuntime runtime, Func<OrdinaryGameplayView, bool> published, string input)
    {

        runtime.FrameBoundary.BeginAcceptedFrame(0);
        OrdinaryGameplayView registers = Registers(runtime);
        AssertTrue(published(registers), $"the accepted NMI publishes {input}");

    }

    /// <summary>The ordinary gameplay registers of a freshly published frame.</summary>
    private static OrdinaryGameplayView Registers(SuperMetroidRuntime runtime) =>
        ((OrdinaryGameplayRenderLayer)GameplayDisplayCapture.CaptureOrdinaryBase(runtime.PublishDisplayFrame()).Layers[0]).View;

    /// <summary>Runs the focused gameplay display frame confirmation.</summary>
    [VerificationCommand("--gameplay-display-frame")]
    internal static int GameplayDisplayFrameCommand()
    {
        Suite(nameof(VerifyGameplayDisplayFrame), () => VerifyGameplayDisplayFrame());
        return 0;
    }
}
