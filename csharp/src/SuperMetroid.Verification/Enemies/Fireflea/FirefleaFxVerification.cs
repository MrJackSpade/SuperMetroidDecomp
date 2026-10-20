using SuperMetroid.Core.Frontend;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Enemies.Fireflea;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

/// <summary>Checks Fireflea fixed-color flashing, darkness, frozen-time behavior, and X-Ray composition.</summary>
internal static class FirefleaFxVerification
{
    /// <summary>Compares native shade tables and frame updates, optionally including a retail X-Ray display capture.</summary>
    internal static void VerifyFirefleaFx(bool includeXrayCapture = true)
    {
        var memory = new TestAddressSpace();
        var bus = new MutableCartridgeReadGuard(memory);
        bus.Forbid(address => address is >= 0x88B058 and < 0x88B07E,
            address => $"Fireflea FX attempted migrated definition read ${address:X6}.");
        var fxSession = new RoomFxSession();
        var fx = new RoomLayer3FxState(fxSession);
        FixedColorState fixedColor = fx.FixedColors;
        var vram = new SnesVram();
        RoomFxRecordDefinition record = RoomFxDefinitions.AllRecords.First(candidate => candidate.Type == (byte)RoomFxType.Fireflea && candidate.DoorPointer == 0);
        // Independent transcription of $88:B058/$88:B070. These are deliberately not
        // generated from the production catalog or implementation.
        ushort[] flash = [0, 0x100, 0x200, 0x300, 0x400, 0x500, 0x600, 0x500, 0x400, 0x300, 0x200, 0x100];
        ushort[] darkness = [0, 0x600, 0xC00, 0x1200, 0x1800, 0x1900, 0xC208];
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        fxSession.PaletteBlendColors = SuperMetroid.Core.Fx.Artwork.RoomFxPaletteBlendCatalog.Load(
            new MemoryStream(SuperMetroid.AssetExtraction.Fx.RoomFxPaletteBlendExtractor.Extract(rom)));
        Suite(nameof(VerifyFirefleaFlashingAlgorithm), () => VerifyFirefleaFlashingAlgorithm(rom));
        Suite(nameof(VerifyFirefleaDarknessAlgorithm), () => VerifyFirefleaDarknessAlgorithm(rom));
        Suite(nameof(VerifyFirefleaFrozenDarkness), VerifyFirefleaFrozenDarkness);
        Suite(nameof(VerifyFirefleaUninitializedTimerWrap), VerifyFirefleaUninitializedTimerWrap);
        fx = RoomLayer3FxState.ForDefinition(fxSession, new SnesCgram(), record, 0);
        AssertEqual(FirefleaFxDefinitions.FlashDuration, fx.Fireflea.Timer.RawValue, "Fireflea load initializes native six-frame timer");
        AssertTrue(!fx.IsRenderable, "Fireflea darkness does not invent a BG3 texture");
        for (int frame = 1; frame <= 144; frame++)
        {
            fx.Step(BgArtworkFixtures.For(vram), new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, MainGameLoopCarry = true });
            int expectedIndex = (frame / 6) % 12;
            int shade = SnesLittleEndian.HighByte(flash[expectedIndex]);
            AssertEqual(expectedIndex, fx.Fireflea.Index, $"Fireflea index frame {frame}");
            AssertEqual(shade | 0x20, fixedColor.Red, $"Fireflea red frame {frame}");
            AssertEqual(shade | 0x80, fixedColor.Green, $"Fireflea second COLDATA write frame {frame}");
            AssertEqual(shade | 0x40, fixedColor.Blue, $"Fireflea third COLDATA write frame {frame}");
        }
        NativeCountdown timer = fx.Fireflea.Timer;
        int red = fixedColor.Red;
        for (int frame = 0; frame < 100; frame++) fx.Step(BgArtworkFixtures.For(vram), new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = true, MainGameLoopCarry = true });
        AssertEqual(timer, fx.Fireflea.Timer, "X-ray frozen time retains flashing phase");
        AssertEqual(red, fixedColor.Red, "X-ray frozen time retains fixed color");
        AssertEqual(LayerBlendingConfiguration.Fireflea, fx.LayerBlendConfiguration, "frozen Fireflea still selects blending");
        for (ushort level = 0; level <= 12; level += 2)
        {
            fx = RoomLayer3FxState.ForDefinition(fxSession, new SnesCgram(), record, 0);
            for (int frame = 1; frame <= 78; frame++)
            {
                fx.Step(BgArtworkFixtures.For(vram), new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, MainGameLoopCarry = true, FirefleaDarknessLevel = level });
                int index = level < 10 ? frame / 6 % 12 : frame < 6 ? 0 : 6;
                byte shade = SnesLittleEndian.HighByte((ushort)(darkness[level / 2] + flash[index]));
                AssertEqual(shade | 0x20, fixedColor.Red, $"death offset {level}, frame {frame}: red");
                AssertEqual(shade | 0x80, fixedColor.Green, $"death offset {level}, frame {frame}: blue");
                AssertEqual(shade | 0x40, fixedColor.Blue, $"death offset {level}, frame {frame}: green");
                AssertEqual(level, fx.Fireflea.Darkness, "enemy death offset reaches the Fireflea FX state");
            }
            int retained = fixedColor.Red;
            fx.Step(BgArtworkFixtures.For(vram), new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = true, MainGameLoopCarry = true, FirefleaDarknessLevel = level });
            AssertEqual(retained, fixedColor.Red, "frozen death level retains its exact shade");
        }
        // Removing the effect must stop its producer, not let the old flashing timer
        // keep changing a subsequent room's fixed color.
        fx = RoomLayer3FxState.ForRoom(fxSession, new SnesCgram(), null, null, 0);
        fixedColor.Red = 0x25;
        fx.Step(BgArtworkFixtures.For(vram), new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, MainGameLoopCarry = true });
        AssertEqual(0x25, fixedColor.Red, "non-Fireflea room does not execute the old producer");
        if (includeXrayCapture) VerifyFirefleaXrayCapture();
        Console.WriteLine("  Fireflea FX: compiled native shades, ROM-read guard, initialization, flash cycles, all seven death offsets, COLDATA order and frozen-time retention agree.");
    }

    /// <summary>Checks all twelve calculated flash shades against native words and rejects invalid indices.</summary>
    private static void VerifyFirefleaFlashingAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (ushort i = 0; i < 12; i++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((FirefleaFxDefinitions.FlashReferenceAddress + 2 * i)),
                FirefleaFxDefinitions.FlashingShade(i), $"Original Fireflea flash {i}");
        AssertThrows<InvalidDataException>(() => FirefleaFxDefinitions.FlashingShade(12), "Flash upper bound");
        AssertThrows<InvalidDataException>(() => FirefleaFxDefinitions.FlashingShade(ushort.MaxValue), "Flash invalid maximum");
    }

    /// <summary>Checks seven darkness offsets, including the native opcode alias, against cartridge words.</summary>
    private static void VerifyFirefleaDarknessAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (ushort offset = 0; offset <= 12; offset += 2)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((FirefleaFxDefinitions.DarknessReferenceAddress + offset)),
                FirefleaFxDefinitions.DarknessShade(offset), $"Original Fireflea darkness including opcode alias {offset}");
        AssertThrows<InvalidDataException>(() => FirefleaFxDefinitions.DarknessShade(1), "Darkness odd offset");
        AssertThrows<InvalidDataException>(() => FirefleaFxDefinitions.DarknessShade(14), "Darkness upper bound");
        AssertThrows<InvalidDataException>(() => FirefleaFxDefinitions.DarknessShade(ushort.MaxValue), "Darkness invalid maximum");
    }

    /// <summary>Checks that frozen time holds the flash clock while the darkness level still follows the enemies.</summary>
    private static void VerifyFirefleaFrozenDarkness()
    {
        var fireflea = new FirefleaFxState();
        var fixedColor = new FixedColorState();
        fireflea.Initialize();
        for (int frame = 0; frame < 10; frame++) fireflea.Step(fixedColor, frozen: false, darkness: 0);
        NativeCountdown timer = fireflea.Timer;
        int index = fireflea.Index;
        fireflea.Step(fixedColor, frozen: true, darkness: 2);
        AssertEqual(timer, fireflea.Timer, "frozen Fireflea holds its flash timer");
        AssertEqual(index, fireflea.Index, "frozen Fireflea holds its flash index");
        AssertEqual(2, fireflea.Darkness, "frozen Fireflea still publishes the enemy-owned darkness counter");
    }

    /// <summary>
    /// Checks the flash clock's wrap: a step from the cleared zero timer, before the FX initializes, wraps to $FFFF
    /// rather than restarting, and the index stays put until the timer later reaches zero.
    /// </summary>
    private static void VerifyFirefleaUninitializedTimerWrap()
    {
        var fireflea = new FirefleaFxState();
        fireflea.Step(new FixedColorState(), frozen: false, darkness: 0);
        AssertEqual(ushort.MaxValue, fireflea.Timer.RawValue, "a step from the cleared timer wraps");
        AssertEqual(0, fireflea.Index, "the wrapped step does not advance the index");
        fireflea.Initialize();
        for (int step = 0; step < FirefleaFxDefinitions.FlashDuration; step++)
            fireflea.Step(new FixedColorState(), frozen: false, darkness: 0);
        AssertEqual(1, fireflea.Index, "the timer reaching zero advances the index");
        AssertEqual(FirefleaFxDefinitions.FlashDuration, fireflea.Timer.RawValue, "the timer reaching zero restarts it");
    }

    /// <summary>Checks the retail Fireflea room's fixed-color subtraction before, during, and after X-Ray.</summary>
    private static void VerifyFirefleaXrayCapture()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.GreenBrinstarFirefleas); // Green Brinstar Firefleas, retail room/FX population.
        AssertEqual(RoomFxType.Fireflea, runtime.RoomLayer3Fx.Type, "retail fixture selects Fireflea FX");
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation();
        // Seed a late darkness shade through the production FX owner. X-ray freezes
        // the producer, so its display must preserve this value rather than substitute
        // ordinary X-ray's constant seven or restart the flash cycle.
        runtime.RoomLayer3Fx.Step(runtime.BgArtwork, new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, MainGameLoopCarry = true, FirefleaDarknessLevel = 6 });
        int shade = runtime.RoomLayer3Fx.FixedColors.Snapshot().RedIntensity;
        AssertEqual(18, shade, "retail darkness table seeds a shade above X-ray's minimum");
        // The blend selector and COLDATA shadows reach the PPU at the accepted NMI (#1307).
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var ordinaryDarkness = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        AssertTrue(ordinaryDarkness.Layers[0] is GameplayColorMathRenderLayer,
            "normal Fireflea display retains source identity for darkness subtraction");
        var darkLayer = (GameplayColorMathRenderLayer)ordinaryDarkness.Layers[0];
        AssertEqual((byte)0xA2, (byte)darkLayer.ColorMath,
            "normal Fireflea darkens BG2/backdrop, not foreground or Samus");
        AssertTrue(darkLayer.Lines.All(line => line.Left > line.Right),
            "ordinary Fireflea darkness has no X-ray exemption window");
        AssertTrue(!darkLayer.RevealBlocks && !darkLayer.AddSubscreen,
            "ordinary Fireflea keeps both room maps and subtracts fixed color");
        AssertTrue(samus.Xray.TryBegin(samus, samus.PoseMetadata.ReadMovementType(), new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }), "Fireflea display fixture activates X-ray");
        for (int frame = 0; frame < 90; frame++) runtime.StepFrame(runtime.Options.ControllerBindings.Dash);
        var packet = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        AssertTrue(packet.Layers[0] is GameplayColorMathRenderLayer, "Fireflea capture uses source-aware X-ray compositor");
        var layer = (GameplayColorMathRenderLayer)packet.Layers[0];
        AssertTrue(!layer.RevealBlocks && !layer.AddSubscreen, "Fireflea preserves room maps and uses fixed-color subtraction");
        AssertEqual((byte)0xB3, (byte)layer.ColorMath, "Fireflea native CGADSUB");
        AssertEqual(shade, layer.FixedRed, "Fireflea fixed red survives 90 frozen runtime frames");
        AssertEqual(shade, layer.FixedGreen, "Fireflea fixed green survives 90 frozen runtime frames");
        AssertEqual(shade, layer.FixedBlue, "Fireflea fixed blue survives 90 frozen runtime frames");
        for (int frame = 0; frame < 8; frame++) runtime.StepFrame(0);
        AssertTrue(!samus.Xray.IsActive && !runtime.Freeze.TimeIsFrozen, "Fireflea X-ray teardown releases gameplay");
        var restored = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        AssertEqual((byte)0xA2, (byte)((GameplayColorMathRenderLayer)restored.Layers[0]).ColorMath,
            "Fireflea teardown restores room subtraction rather than unblended rendering");
    }

    /// <summary>Dispatches the focused Fireflea FX verification command.</summary>
    [VerificationCommand("--fireflea-fx")]
    internal static int FirefleaFxCommand()
    {
        Suite(nameof(VerifyFirefleaFx), () => VerifyFirefleaFx());
        return 0;
    }
}
