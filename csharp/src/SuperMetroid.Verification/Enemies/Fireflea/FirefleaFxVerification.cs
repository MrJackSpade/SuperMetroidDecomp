using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core;
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
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Enemies.Fireflea;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Fx;

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
        RoomFixedColor fixedColor = fx.FixedColors;
        var vram = new SnesVram();
        RoomFxRecordDefinition record = RoomFxDefinitions.AllRecords.First(candidate => candidate.Type == RoomFxType.Fireflea && candidate.DoorPointer == 0);
        // Independent transcription of $88:B058/$88:B070. These are deliberately not
        // generated from the production catalog or implementation.
        ushort[] flash = [0, 0x100, 0x200, 0x300, 0x400, 0x500, 0x600, 0x500, 0x400, 0x300, 0x200, 0x100];
        ushort[] darkness = [0, 0x600, 0xC00, 0x1200, 0x1800, 0x1900, 0xC208];
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        fxSession.PaletteBlendColors = RoomFxPaletteBlendFile.Read(
            new MemoryStream(SuperMetroid.AssetExtraction.Fx.RoomFxPaletteBlendExtractor.Extract(rom)));
        Suite(nameof(VerifyFirefleaFlashingAlgorithm), () => VerifyFirefleaFlashingAlgorithm(rom));
        Suite(nameof(VerifyFirefleaDarknessAlgorithm), () => VerifyFirefleaDarknessAlgorithm(rom));
        Suite(nameof(VerifyFirefleaFrozenDarkness), VerifyFirefleaFrozenDarkness);
        Suite(nameof(VerifyFirefleaUninitializedTimerWrap), VerifyFirefleaUninitializedTimerWrap);
        fx = RoomLayer3FxState.ForDefinition(fxSession, new GameplayColours(new SpriteColourSets()), record, 0);
        AssertEqual(FirefleaFxDefinitions.FlashDuration, fx.Fireflea.Timer.RawValue, "Fireflea load initializes native six-frame timer");
        AssertTrue(!fx.IsRenderable, "Fireflea darkness does not invent a BG3 texture");
        for (int frame = 1; frame <= 144; frame++)
        {
            fx.Step(new ArtworkFrameSelections(), new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame });
            int expectedIndex = (frame / 6) % 12;
            int shade = LittleEndianBytes.HighByte(flash[expectedIndex]);
            AssertEqual(expectedIndex, fx.Fireflea.Index, $"Fireflea index frame {frame}");
            AssertEqual(shade, fixedColor.Color.Red, $"Fireflea red frame {frame}");
            AssertEqual(shade, fixedColor.Color.Green, $"Fireflea second COLDATA write frame {frame}");
            AssertEqual(shade, fixedColor.Color.Blue, $"Fireflea third COLDATA write frame {frame}");
        }
        WrappingCountdown timer = fx.Fireflea.Timer;
        int red = fixedColor.Color.Red;
        for (int frame = 0; frame < 100; frame++) fx.Step(new ArtworkFrameSelections(), new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = true, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame });
        AssertEqual(timer, fx.Fireflea.Timer, "X-ray frozen time retains flashing phase");
        AssertEqual(red, fixedColor.Color.Red, "X-ray frozen time retains fixed color");
        AssertEqual(LayerBlendingConfiguration.Fireflea, fx.LayerBlendConfiguration, "frozen Fireflea still selects blending");
        for (ushort level = 0; level <= 12; level += 2)
        {
            fx = RoomLayer3FxState.ForDefinition(fxSession, new GameplayColours(new SpriteColourSets()), record, 0);
            for (int frame = 1; frame <= 78; frame++)
            {
                fx.Step(new ArtworkFrameSelections(), new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame, FirefleaDarknessLevel = level });
                int index = level < 10 ? frame / 6 % 12 : frame < 6 ? 0 : 6;
                byte shade = LittleEndianBytes.HighByte((ushort)(darkness[level / 2] + flash[index]));
                AssertEqual(shade & RgbColor.MaxChannel, fixedColor.Color.Red, $"death offset {level}, frame {frame}: red");
                AssertEqual(shade & RgbColor.MaxChannel, fixedColor.Color.Green, $"death offset {level}, frame {frame}: blue");
                AssertEqual(shade & RgbColor.MaxChannel, fixedColor.Color.Blue, $"death offset {level}, frame {frame}: green");
                AssertEqual(level, fx.Fireflea.Darkness, "enemy death offset reaches the Fireflea FX state");
            }
            int retained = fixedColor.Color.Red;
            fx.Step(new ArtworkFrameSelections(), new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = true, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame, FirefleaDarknessLevel = level });
            AssertEqual(retained, fixedColor.Color.Red, "frozen death level retains its exact shade");
        }
        // Removing the effect must stop its producer, not let the old flashing timer
        // keep changing a subsequent room's fixed color.
        fx = RoomLayer3FxState.ForRoom(fxSession, new BackgroundColourSets(), null, null, 0);
        fixedColor.Color = new RgbColor(5, 5, 5);
        fx.Step(new ArtworkFrameSelections(), new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame });
        AssertEqual(5, fixedColor.Color.Red, "non-Fireflea room does not execute the old producer");
        if (includeXrayCapture) VerifyFirefleaXrayCapture();
        Console.WriteLine("  Fireflea FX: compiled native shades, ROM-read guard, initialization, flash cycles, all seven death offsets, COLDATA order and frozen-time retention agree.");
    }

    /// <summary>
    /// Checks all twelve calculated flash shade levels against the high byte of each native word, and that every
    /// native flash word's low byte is zero, so adding a darkness word never carries into the shade (#1415: the shade
    /// is a level, not the cartridge's word).
    /// </summary>
    private static void VerifyFirefleaFlashingAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (ushort i = 0; i < 12; i++)
        {
            ushort word = CartridgeImportSource.Require(rom).ReadCartridgeWord((FirefleaFxDefinitions.FlashReferenceAddress + 2 * i));
            AssertEqual(LittleEndianBytes.HighByte(word), FirefleaShades.FlashingShade(i), $"Original Fireflea flash {i}");
            AssertEqual((byte)0, LittleEndianBytes.LowByte(word), $"Original Fireflea flash {i} carries nothing");
        }
        AssertThrows<InvalidDataException>(() => FirefleaShades.FlashingShade(12), "Flash upper bound");
        AssertThrows<InvalidDataException>(() => FirefleaShades.FlashingShade(ushort.MaxValue), "Flash invalid maximum");
    }

    /// <summary>
    /// Checks seven darkness shade levels, including the level read past the ramp, against the high byte of each
    /// cartridge word; the low byte cannot reach the shade because the flash words' low bytes are zero (#1415).
    /// </summary>
    private static void VerifyFirefleaDarknessAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (ushort offset = 0; offset <= 12; offset += 2)
            AssertEqual(LittleEndianBytes.HighByte(CartridgeImportSource.Require(rom).ReadCartridgeWord((FirefleaFxDefinitions.DarknessReferenceAddress + offset))),
                FirefleaShades.DarknessShade(offset), $"Original Fireflea darkness including the past-ramp level {offset}");
        AssertThrows<InvalidDataException>(() => FirefleaShades.DarknessShade(1), "Darkness odd offset");
        AssertThrows<InvalidDataException>(() => FirefleaShades.DarknessShade(14), "Darkness upper bound");
        AssertThrows<InvalidDataException>(() => FirefleaShades.DarknessShade(ushort.MaxValue), "Darkness invalid maximum");
    }

    /// <summary>Checks that frozen time holds the flash clock while the darkness level still follows the enemies.</summary>
    private static void VerifyFirefleaFrozenDarkness()
    {
        var fireflea = new FirefleaFxState();
        var fixedColor = new RoomFixedColor();
        fireflea.Initialize();
        for (int frame = 0; frame < 10; frame++) fireflea.Step(fixedColor, frozen: false, darkness: 0);
        WrappingCountdown timer = fireflea.Timer;
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
        fireflea.Step(new RoomFixedColor(), frozen: false, darkness: 0);
        AssertEqual(ushort.MaxValue, fireflea.Timer.RawValue, "a step from the cleared timer wraps");
        AssertEqual(0, fireflea.Index, "the wrapped step does not advance the index");
        fireflea.Initialize();
        for (int step = 0; step < FirefleaFxDefinitions.FlashDuration; step++)
            fireflea.Step(new RoomFixedColor(), frozen: false, darkness: 0);
        AssertEqual(1, fireflea.Index, "the timer reaching zero advances the index");
        AssertEqual(FirefleaFxDefinitions.FlashDuration, fireflea.Timer.RawValue, "the timer reaching zero restarts it");
    }

    /// <summary>Checks the retail Fireflea room's fixed-color subtraction before, during, and after X-Ray.</summary>
    private static void VerifyFirefleaXrayCapture()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.GreenBrinstarFirefleas); // Green Brinstar Firefleas, retail room/FX population.
        AssertEqual(RoomFxType.Fireflea, runtime.RoomLayer3Fx.Type, "retail fixture selects Fireflea FX");
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        // Seed a late darkness shade through the production FX owner. X-ray freezes
        // the producer, so its display must preserve this value rather than substitute
        // ordinary X-ray's constant seven or restart the flash cycle.
        runtime.RoomLayer3Fx.Step(runtime.Room.LoadedScope.ArtworkFrames, new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame, FirefleaDarknessLevel = 6 });
        int shade = runtime.RoomLayer3Fx.FixedColors.Color.Red;
        AssertEqual(18, shade, "retail darkness table seeds a shade above X-ray's minimum");
        // The blend selector and COLDATA shadows reach the PPU at the accepted NMI (#1307).
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var ordinaryDarkness = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        AssertTrue(ordinaryDarkness.Layers[0] is GameplayBlendLayer,
            "normal Fireflea display retains source identity for darkness subtraction");
        var darkLayer = (GameplayBlendLayer)ordinaryDarkness.Layers[0];
        AssertEqual(ColorBlend.Bg2 | ColorBlend.Backdrop | ColorBlend.Subtract, darkLayer.Blend.Layers,
            "normal Fireflea darkens BG2/backdrop, not foreground or Samus");
        AssertTrue(darkLayer.Window.IsClosed,
            "ordinary Fireflea darkness has no X-ray exemption window");
        AssertTrue(!darkLayer.RevealBlocks && !darkLayer.Blend.BlendsWithLayersBehind,
            "ordinary Fireflea keeps both room maps and subtracts fixed color");
        AssertTrue(samus.Xray.TryBegin(samus, SamusPoseMetadata.ReadMovementType(samus), new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }), "Fireflea display fixture activates X-ray");
        for (int frame = 0; frame < 90; frame++) runtime.StepFrame(runtime.Options.ControllerBindings.Dash);
        var packet = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        AssertTrue(packet.Layers[0] is GameplayBlendLayer, "Fireflea capture uses source-aware X-ray compositor");
        var layer = (GameplayBlendLayer)packet.Layers[0];
        AssertTrue(!layer.RevealBlocks && !layer.Blend.BlendsWithLayersBehind, "Fireflea preserves room maps and uses fixed-color subtraction");
        AssertEqual(ColorBlend.Bg1 | ColorBlend.Bg2 | ColorBlend.Sprites | ColorBlend.Backdrop | ColorBlend.Subtract,
            layer.Blend.Layers, "Fireflea X-ray subtracts from both backgrounds, the sprites and the backdrop");
        AssertEqual(shade, layer.Blend.FixedColor.Red, "Fireflea fixed red survives 90 frozen runtime frames");
        AssertEqual(shade, layer.Blend.FixedColor.Green, "Fireflea fixed green survives 90 frozen runtime frames");
        AssertEqual(shade, layer.Blend.FixedColor.Blue, "Fireflea fixed blue survives 90 frozen runtime frames");
        for (int frame = 0; frame < 8; frame++) runtime.StepFrame(0);
        AssertTrue(!samus.Xray.IsActive && !runtime.Freeze.TimeIsFrozen, "Fireflea X-ray teardown releases gameplay");
        var restored = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        AssertEqual(ColorBlend.Bg2 | ColorBlend.Backdrop | ColorBlend.Subtract, ((GameplayBlendLayer)restored.Layers[0]).Blend.Layers,
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
