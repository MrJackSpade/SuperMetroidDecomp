using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;
using static SuiteTimingVerification;
using static VerificationHelpersVerification;

/// <summary>
/// Confirms the published gameplay display frame (#1307): repainting never advances or changes it,
/// it shares no storage with simulation, and each register shadow the cartridge uploads at NMI
/// reaches the screen at the same accepted NMI as the rest of that frame's latch.
/// </summary>
internal static class GameplayDisplayFrameTestsVerification
{
    /// <summary>A WOBJLOG value used to mark which accepted NMI published a frame's latch.</summary>
    private const byte LatchMarker = 90;

    /// <summary>The BG3 character base word Kraid's room selects.</summary>
    private const ushort KraidHudCharacterWord = 8192;

    /// <summary>A layer-one Y position the scrolling sky mirrors into BG2VOFS.</summary>
    private const ushort SkyLayer1Y = 291;

    /// <summary>Runs the repaint, immutability and per-input NMI timing confirmations.</summary>
    internal static void VerifyGameplayDisplayFrame()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.NmiHandler.RunNmi(0, true);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        VerifyCeresModeTiming(runtime);
        runtime.LoadCartridgeRoomForDebug(PowerBombRuntimeVerificationDefinitions.AlphaPowerBombRoomHeader, new Position16<RoomPixels>(0, 0));
        runtime.NmiHandler.RunNmi(0, true);

        VerifyRepaintAndImmutability(runtime, bus);
        VerifyBossBackgroundTiming(runtime);
        VerifySceneRegisterTiming(runtime);
        VerifyFixedColorTiming(runtime, bus);
        Console.WriteLine("  Gameplay display frame: repaint invariance, immutability and NMI timing of every re-timed register shadow confirmed.");
    }

    /// <summary>Publishing twice without stepping, and capturing one frame twice, yield identical pictures that later simulation cannot change.</summary>
    [PpuLayoutBoundary("exact hardware test: replaces PPU memory after publication")]
    private static void VerifyRepaintAndImmutability(SuperMetroidRuntime runtime, ISnesAddressSpace bus)
    {
        GameplayDisplayFrame frame = runtime.PublishDisplayFrame();
        Rgba32[] pixels = SoftwareLayeredSnapshotRenderer.Render(GameplayDisplayCapture.Capture(frame));
        AssertTrue(pixels.SequenceEqual(SoftwareLayeredSnapshotRenderer.Render(GameplayDisplayCapture.Capture(frame))),
            "capturing one published frame twice is identical");
        AssertTrue(pixels.SequenceEqual(SoftwareLayeredSnapshotRenderer.Render(GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()))),
            "publishing again without stepping is identical");
        AssertTrue(pixels.SequenceEqual(SuperMetroidRuntimeFrameRenderer.Render(frame)),
            "legacy raster of the published frame matches its capture");

        // Mutate frame-end sources, NMI-latched sources and PPU memory after publication.
        GameplayFrameEndDisplayInputs frameEnd = frame.FrameEnd;
        GameplayNmiDisplayLatch latched = frame.Latched;
        SamusPowerBombExplosionState explosion = runtime.BombProjectiles.PowerBombExplosion;
        explosion.Arm();
        explosion.Spawn(new(128, 120));
        for (int i = 0; i < 20; i++) explosion.StepFrame(bus);
        runtime.Room.ScrollingSky = new ScrollingSkyState();
        runtime.Display.GameplayHudCharacterBaseWord = KraidHudCharacterWord;
        runtime.Vram.LoadBytes(0, new byte[SnesPpuLayout.VramByteCount]);
        for (int color = 0; color < SnesPpuLayout.CgramColorCount; color++) runtime.Cgram.SetColor(color, new Bgr555(31, 0, 31));
        runtime.NmiHandler.RunNmi(0, true);
        AssertTrue(ReferenceEquals(frameEnd, frame.FrameEnd) && ReferenceEquals(latched, frame.Latched),
            "a published frame keeps its own latch and frame-end inputs");
        AssertEqual(false, frame.FrameEnd.PowerBomb.IsActive, "published power bomb inputs are a value copy");
        AssertTrue(frame.FrameEnd.ScrollingSkyHorizontalScrolls is null, "published sky table is not the live owner");
        AssertTrue(pixels.SequenceEqual(SoftwareLayeredSnapshotRenderer.Render(GameplayDisplayCapture.Capture(frame))),
            "a published frame survives later simulation, latch and PPU memory changes");

        explosion.Reset();
        runtime.Room.ScrollingSky = null;
        runtime.Display.GameplayHudCharacterBaseWord = GameplayVramLayout.HudCharacterWord;
        runtime.NmiHandler.RunNmi(0, true);
    }

    /// <summary>Kraid's and Mother Brain's BG2 register shadows publish with the accepted NMI's latch.</summary>
    [PpuLayoutBoundary("exact hardware test: checks PPU register numbers")]
    private static void VerifyBossBackgroundTiming(SuperMetroidRuntime runtime)
    {
        OrdinaryGameplayRegisters before = Registers(runtime);
        var kraid = new KraidEnemyState { OwnsBg2Tilemap = true, Bg2Scroll = new Position16<Bg2Pixels>(320, 88) };
        runtime.Enemies.KraidBehavior.Kraid = kraid;
        MarkMainLoopWrite(runtime);
        AssertPending(runtime, registers => registers.Bg2TilemapWord == before.Bg2TilemapWord &&
            registers.Bg2Scroll == before.Bg2Scroll, "Kraid BG2SC/BG2HOFS/BG2VOFS");
        AssertPublished(runtime, registers => registers.Bg2TilemapWord == KraidBackgroundRomData.LiveBg2TilemapWord &&
            registers.Bg2Scroll == kraid.Bg2Scroll, "Kraid BG2SC/BG2HOFS/BG2VOFS");
        runtime.Enemies.KraidBehavior.Kraid = null;
        runtime.NmiHandler.RunNmi(0, true);

        var motherBrain = new MotherBrainEnemyState(runtime.Enemies.SlotTable.Slots[0]) { HasBg2ScrollOverride = true, DeathBg2Hidden = true };
        runtime.Enemies.MotherBrainEncounter.MotherBrain = motherBrain;
        MarkMainLoopWrite(runtime);
        AssertPending(runtime, registers => registers.Bg2MapSize == before.Bg2MapSize &&
            registers.MainScreenLayers == before.MainScreenLayers, "Mother Brain BG2SC and TM BG2 bit");
        AssertPublished(runtime, registers => registers.Bg2MapSize == new Extent16<TilemapCells>(32, 32) &&
            registers.MainScreenLayers == (SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj), "Mother Brain BG2SC and TM BG2 bit");
        AssertEqual(motherBrain.RainbowBeamHdma.Color, runtime.PublishDisplayFrame().Latched.FixedColors.RainbowBeam,
            "rainbow beam COLDATA is latched with Mother Brain's registers");
        runtime.Enemies.MotherBrainEncounter.MotherBrain = null;
        runtime.NmiHandler.RunNmi(0, true);
    }

    /// <summary>BG34NBA, the FX blend selectors and the scrolling sky's BG2 shadows publish with the accepted NMI's latch.</summary>
    [PpuLayoutBoundary("exact hardware test: checks PPU register numbers")]
    private static void VerifySceneRegisterTiming(SuperMetroidRuntime runtime)
    {
        OrdinaryGameplayRegisters before = Registers(runtime);
        runtime.Display.GameplayHudCharacterBaseWord = KraidHudCharacterWord;
        MarkMainLoopWrite(runtime);
        AssertPending(runtime, registers => registers.HudCharacterWord == before.HudCharacterWord, "BG34NBA");
        AssertPublished(runtime, registers => registers.HudCharacterWord == KraidHudCharacterWord, "BG34NBA");
        runtime.Display.GameplayHudCharacterBaseWord = GameplayVramLayout.HudCharacterWord;
        runtime.NmiHandler.RunNmi(0, true);

        RoomFxType originalFx = runtime.RoomLayer3Fx.Type;
        runtime.RoomLayer3Fx.Type = RoomFxType.TourianEntranceStatue;
        MarkMainLoopWrite(runtime);
        AssertPending(runtime, registers => registers.Bg2MapSize == before.Bg2MapSize, "FX-selected BG2SC");
        AssertPublished(runtime, registers => registers.Bg2MapSize == new Extent16<TilemapCells>(32, 64), "FX-selected BG2SC");
        runtime.RoomLayer3Fx.Type = originalFx;
        runtime.NmiHandler.RunNmi(0, true);

        // $88:AFB2 writes the BG2VOFS mirror while the same pass rebuilds the band table.
        // HDMA reads the table during display; the mirror waits for the NMI upload.
        var sky = new ScrollingSkyState();
        runtime.Room.ScrollingSky = sky;
        sky.ProcessFrame(SkyLayer1Y, timeIsFrozen: false, runtime.VramWrites);
        MarkMainLoopWrite(runtime);
        AssertTrue(runtime.PublishDisplayFrame().FrameEnd.ScrollingSkyHorizontalScrolls is not null,
            "sky band table is read live during display");
        AssertPending(runtime, registers => registers.Bg2Scroll.Y == before.Bg2Scroll.Y &&
            registers.Bg2MapSize == before.Bg2MapSize, "sky BG2VOFS and BG2SC");
        AssertPublished(runtime, registers => registers.Bg2Scroll.Y == SkyLayer1Y &&
            registers.Bg2MapSize == new Extent16<TilemapCells>(32, 64), "sky BG2VOFS and BG2SC");
        runtime.Room.ScrollingSky = null;
        runtime.NmiHandler.RunNmi(0, true);
    }

    /// <summary>COLDATA shadows publish at the accepted NMI while the same effect's window table is read live.</summary>
    [PpuLayoutBoundary("exact hardware test: checks PPU register numbers")]
    private static void VerifyFixedColorTiming(SuperMetroidRuntime runtime, ISnesAddressSpace bus)
    {
        // Fireflea darkness: FX selector and the shared COLDATA shadows.
        RoomFxType originalFx = runtime.RoomLayer3Fx.Type;
        var fixedColor = new FixedColorMemory(runtime.MemoryBus);
        runtime.RoomLayer3Fx.Type = RoomFxType.Fireflea;
        fixedColor.Red = 32 + 9; fixedColor.Green = 64 + 9; fixedColor.Blue = 128 + 9;
        MarkMainLoopWrite(runtime);
        AssertTrue(GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0] is OrdinaryGameplayRenderLayer,
            "Fireflea blend selector waits for the NMI");
        runtime.NmiHandler.RunNmi(0, mainLoopRequestedNmi: true);
        var darkness = (GameplayColorMathRenderLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertEqual((byte)9, darkness.FixedRed, "Fireflea COLDATA publishes with its blend selector");
        AssertEqual(LatchMarker, darkness.Gameplay.Registers.Windows.ObjectColorLogic, "Fireflea COLDATA publishes with the frame's window latch");
        fixedColor.Red = 32 + 4;
        AssertEqual((byte)9, ((GameplayColorMathRenderLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0]).FixedRed,
            "a main-loop COLDATA write waits for the next accepted NMI");
        runtime.NmiHandler.RunNmi(0, true);
        AssertEqual((byte)4, ((GameplayColorMathRenderLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0]).FixedRed,
            "the next accepted NMI publishes the COLDATA write");
        runtime.RoomLayer3Fx.Type = originalFx;
        fixedColor.Red = 32; fixedColor.Green = 64; fixedColor.Blue = 128;
        runtime.NmiHandler.RunNmi(0, true);

        // Power bomb: the window table is built during the main loop and read live; the
        // pre-instruction's COLDATA write reaches the screen at the following NMI.
        SamusPowerBombExplosionState explosion = runtime.BombProjectiles.PowerBombExplosion;
        explosion.Arm();
        explosion.Spawn(new(128, 120));
        for (int i = 0; i < 4; i++) explosion.StepFrame(bus);
        runtime.NmiHandler.RunNmi(0, true);
        byte latchedRed = explosion.FixedColorRed;
        int steps = 0;
        while (explosion.FixedColorRed == latchedRed && steps++ < 64) explosion.StepFrame(bus);
        AssertTrue(explosion.FixedColorRed != latchedRed, "fixture reaches a power-bomb COLDATA change");
        GameplayDisplayFrame pending = runtime.PublishDisplayFrame();
        AssertEqual(explosion.RenderedPhase, pending.FrameEnd.PowerBomb.RenderedPhase, "power-bomb window table is read live");
        AssertEqual(latchedRed, GameplayDisplayCapture.PowerBomb(pending).FixedColorRed, "power-bomb COLDATA waits for the NMI");
        runtime.NmiHandler.RunNmi(0, true);
        AssertEqual(explosion.FixedColorRed, GameplayDisplayCapture.PowerBomb(runtime.PublishDisplayFrame()).FixedColorRed,
            "the accepted NMI publishes power-bomb COLDATA");
        explosion.Reset();
        runtime.NmiHandler.RunNmi(0, true);

        // Suit pickup: the light beam is a live HDMA window; its COLDATA follows the NMI.
        SamusState samus = runtime.Player.IntroducedSamus;
        runtime.SuitPickup.Begin(bus, samus, new(0, 0), SamusSuitPickupKind.Varia);
        runtime.SuitPickup.Step(bus, samus, runtime.Cgram);
        FixedColorRegisterSnapshot suitBefore = runtime.PublishDisplayFrame().Latched.FixedColors.SuitPickup;
        AssertTrue(runtime.PublishDisplayFrame().FrameEnd.SuitPickupBeam is not null, "suit light beam window is read live");
        AssertEqual(suitBefore.Red, (GameplayDisplayCapture.SuitPickup(runtime.PublishDisplayFrame())
            ?? throw new InvalidOperationException("Active suit pickup has no flash.")).Red, "suit COLDATA waits for the NMI");
        runtime.NmiHandler.RunNmi(0, true);
        AssertEqual(runtime.SuitPickup.FixedColorRed, (GameplayDisplayCapture.SuitPickup(runtime.PublishDisplayFrame())
            ?? throw new InvalidOperationException("Active suit pickup has no flash.")).Red, "the accepted NMI publishes suit COLDATA");
    }

    /// <summary>Door setup publishes BGMODE with its Mode 7 matrix; room setup BG12NBA and Ridley's matrix follow the NMI.</summary>
    [PpuLayoutBoundary("exact hardware test: checks PPU register numbers")]
    private static void VerifyCeresModeTiming(SuperMetroidRuntime runtime)
    {
        GameplayNmiDisplayLatch shaft = runtime.PublishDisplayFrame().Latched;
        AssertTrue(shaft.Scene.CeresElevatorMode7 && shaft.SamusMode7Transform is not null,
            "door setup $8F:E4E0 publishes BGMODE with its Mode 7 matrix");

        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.CeresRidleyRoom, new Position16<RoomPixels>(0, 0));
        GameplayNmiDisplayLatch arena = runtime.PublishDisplayFrame().Latched;
        AssertTrue(!arena.Scene.CeresElevatorMode7 && arena.SamusMode7Transform is null,
            "door setup $8F:E513 clears BGMODE with its Mode 7 matrix");
        AssertEqual((ushort)0, arena.Scene.Bg12CharacterWord, "room setup BG12NBA waits for the NMI");
        runtime.NmiHandler.RunNmi(0, true);
        AssertEqual(GameplayRenderDefinitions.CeresCharacterWord, Registers(runtime).Bg1CharacterWord,
            "the accepted NMI publishes the Ceres Ridley BG12NBA");

        RidleyEnemyState getaway = runtime.Enemies.RidleyEncounter.CeresRidley
            ?? throw new InvalidOperationException("Ceres Ridley room has no Ridley.");
        getaway.Mode7Active = true;
        getaway.Mode7MatrixA = 240; getaway.Mode7MatrixB = 33;
        getaway.Mode7MatrixC = unchecked((ushort)-33); getaway.Mode7MatrixD = 240;
        getaway.Mode7Center = new Position16<Mode7Pixels>(128, 120);
        getaway.Mode7Offset = new Offset16<Mode7Pixels>(7, 19);
        AssertTrue(GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0] is OrdinaryGameplayRenderLayer,
            "Ridley's M7A..M7Y and fake BGMODE wait for the NMI");
        runtime.NmiHandler.RunNmi(0, mainLoopRequestedNmi: false);
        AssertTrue(GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0] is OrdinaryGameplayRenderLayer,
            "a lag NMI does not publish Ridley's Mode 7");
        runtime.NmiHandler.RunNmi(0, true);
        var mode7 = (Mode7GameplayRenderLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertEqual((short)240, mode7.Registers.MatrixA, "the accepted NMI publishes Ridley's M7A");
        AssertEqual((short)33, mode7.Registers.MatrixB, "the accepted NMI publishes Ridley's M7B");
        AssertTrue(mode7.Floor is not null, "the accepted NMI publishes Ridley's mixed Mode 7 band");
        getaway.Mode7Active = false;
        runtime.NmiHandler.RunNmi(0, true);
    }

    /// <summary>Writes a WOBJLOG marker as the same main loop's write, so publication can be tied to one NMI latch.</summary>
    [PpuLayoutBoundary("exact hardware test: checks PPU register numbers")]
    private static void MarkMainLoopWrite(SuperMetroidRuntime runtime) =>
        runtime.GameplayWindows.Cache.WriteByte(GameplayWindowRegisterAddresses.ObjectColorLogic, LatchMarker);

    /// <summary>Asserts a main-loop write is not yet displayed, including after a lag NMI.</summary>
    private static void AssertPending(SuperMetroidRuntime runtime, Func<OrdinaryGameplayRegisters, bool> unchanged, string input)
    {
        AssertTrue(unchanged(Registers(runtime)), $"{input} written by the main loop waits for the NMI");
        runtime.NmiHandler.RunNmi(0, mainLoopRequestedNmi: false);
        AssertTrue(unchanged(Registers(runtime)), $"a lag NMI does not publish {input}");
    }

    /// <summary>Asserts an accepted NMI publishes the write in the same latch as the marked window register.</summary>
    [PpuLayoutBoundary("exact hardware test: checks PPU register numbers")]
    private static void AssertPublished(SuperMetroidRuntime runtime, Func<OrdinaryGameplayRegisters, bool> published, string input)
    {
        AssertTrue(Registers(runtime).Windows.ObjectColorLogic != LatchMarker, $"{input}: marker is not yet displayed");
        runtime.NmiHandler.RunNmi(0, mainLoopRequestedNmi: true);
        OrdinaryGameplayRegisters registers = Registers(runtime);
        AssertTrue(published(registers), $"the accepted NMI publishes {input}");
        AssertEqual(LatchMarker, registers.Windows.ObjectColorLogic, $"{input} publishes with the same NMI's window latch");
        runtime.GameplayWindows.Cache.WriteByte(GameplayWindowRegisterAddresses.ObjectColorLogic, 0);
    }

    /// <summary>The ordinary gameplay registers of a freshly published frame.</summary>
    private static OrdinaryGameplayRegisters Registers(SuperMetroidRuntime runtime) =>
        ((OrdinaryGameplayRenderLayer)GameplayDisplayCapture.CaptureOrdinaryBase(runtime.PublishDisplayFrame()).Layers[0]).Registers;

    /// <summary>Runs the focused gameplay display frame confirmation.</summary>
    [VerificationCommand("--gameplay-display-frame")]
    internal static int GameplayDisplayFrameCommand()
    {
        Suite(nameof(VerifyGameplayDisplayFrame), () => VerifyGameplayDisplayFrame());
        return 0;
    }
}
