using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Drawing;
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

/// <summary>Checks Fireflea shade flashing, darkness, frozen-time behavior, and X-Ray composition.</summary>
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
        var fx = new RoomFxOverlayState(fxSession);
        FirefleaFxState state = fxSession.Fireflea;
        var vram = new SnesVram();
        RoomEffectDefinition record = RoomFxDefinitions.AllRecords.First(candidate => candidate.Type == RoomFxType.Fireflea && candidate.DoorPointer == 0);
        // Independent transcription of $88:B058/$88:B070. These are deliberately not
        // generated from the production catalog or implementation.
        ushort[] flash = [0, 0x100, 0x200, 0x300, 0x400, 0x500, 0x600, 0x500, 0x400, 0x300, 0x200, 0x100];
        ushort[] darkness = [0, 0x600, 0xC00, 0x1200, 0x1800, 0x1900, 0xC208];
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyFirefleaFlashingAlgorithm), () => VerifyFirefleaFlashingAlgorithm(rom));
        Suite(nameof(VerifyFirefleaDarknessAlgorithm), () => VerifyFirefleaDarknessAlgorithm(rom));
        Suite(nameof(VerifyFirefleaFrozenDarkness), VerifyFirefleaFrozenDarkness);
        Suite(nameof(VerifyFirefleaUninitializedTimerWrap), VerifyFirefleaUninitializedTimerWrap);
        fx = RoomFxOverlayState.ForDefinition(fxSession, record, 0);
        AssertEqual(FirefleaFxDefinitions.FlashDuration, fx.Fireflea.Timer.RawValue, "Fireflea load initializes native six-frame timer");
        AssertTrue(!fx.IsRenderable, "Fireflea darkness does not invent a BG3 texture");
        for (int frame = 1; frame <= 144; frame++)
        {
            fx.Step(new ArtworkFrameSelections(), new RoomFxOverlayFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame });
            int expectedIndex = (frame / 6) % 12;
            int shade = LittleEndianBytes.HighByte(flash[expectedIndex]);
            AssertEqual(expectedIndex, fx.Fireflea.Index, $"Fireflea index frame {frame}");
            AssertEqual(new RgbColor?(new FadeLevelColour(shade, shade, shade).ToColour()), fx.FirefleaTint,
                $"Fireflea grey shade frame {frame}");
        }
        WrappingCountdown timer = fx.Fireflea.Timer;
        RgbColor tint = state.Tint;
        for (int frame = 0; frame < 100; frame++) fx.Step(new ArtworkFrameSelections(), new RoomFxOverlayFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = true, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame });
        AssertEqual(timer, fx.Fireflea.Timer, "X-ray frozen time retains flashing phase");
        AssertEqual(tint, state.Tint, "X-ray frozen time retains the shade");
        for (int step = 0; step < darkness.Length; step++)
        {
            fx = RoomFxOverlayState.ForDefinition(fxSession, record, 0);
            for (int frame = 1; frame <= 78; frame++)
            {
                fx.Step(new ArtworkFrameSelections(), new RoomFxOverlayFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame, FirefleaDarknessStep = step });
                int index = step < 5 ? frame / 6 % 12 : frame < 6 ? 0 : 6;
                // The native shade's bits above the level name the colour channels each write sets, all to the level.
                int shade = LittleEndianBytes.HighByte((ushort)(darkness[step] + flash[index])) & FadeLevelColour.FullLevel;
                AssertEqual(new FadeLevelColour(shade, shade, shade).ToColour(), state.Tint, $"darkness step {step}, frame {frame}: grey shade");
                AssertEqual(step, fx.Fireflea.DarknessStep, "the enemies' darkness step reaches the Fireflea FX state");
            }
            RgbColor retained = state.Tint;
            fx.Step(new ArtworkFrameSelections(), new RoomFxOverlayFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = true, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame, FirefleaDarknessStep = step });
            AssertEqual(retained, state.Tint, "frozen death level retains its exact shade");
        }
        // Removing the effect stops its producer, and a room without it shows no Fireflea shade.
        fx = RoomFxOverlayState.ForRoom(fxSession, null, null, 0);
        WrappingCountdown leftTimer = state.Timer;
        fx.Step(new ArtworkFrameSelections(), new RoomFxOverlayFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame });
        AssertEqual(leftTimer, state.Timer, "non-Fireflea room does not execute the old producer");
        AssertEqual<RgbColor?>(null, fx.FirefleaTint, "non-Fireflea room shows no Fireflea shade");
        if (includeXrayCapture) VerifyFirefleaXrayCapture();
        Console.WriteLine("  Fireflea FX: compiled native shades, ROM-read guard, initialization, flash cycles, all seven death offsets, grey shades and frozen-time retention agree.");
    }

    /// <summary>
    /// Checks all twelve calculated flash shade levels against the high byte of each native word, and that every
    /// native flash word's low byte is zero, so adding a darkness word never carries into the shade (#1415: the shade
    /// is a level, not the cartridge's word).
    /// </summary>
    private static void VerifyFirefleaFlashingAlgorithm(CartridgeImportAddressSpace rom)
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
    /// Checks the seven darkness steps' fade levels, the sixth death's faint darkness included, against the high byte of
    /// each cartridge word below its channel bits, which for the sixth death name channels the writes set to the same
    /// level; the low byte cannot reach the shade because the flash words' low bytes are zero (#1415).
    /// </summary>
    private static void VerifyFirefleaDarknessAlgorithm(CartridgeImportAddressSpace rom)
    {
        AssertEqual(7, FirefleaFxDefinitions.DarknessLevels.Length, "Fireflea darkness steps");
        for (int step = 0; step < FirefleaFxDefinitions.DarknessLevels.Length; step++)
            AssertEqual(LittleEndianBytes.HighByte(CartridgeImportSource.Require(rom).ReadCartridgeWord((FirefleaFxDefinitions.DarknessReferenceAddress + step * 2))) & FadeLevelColour.FullLevel,
                FirefleaShades.DarknessShade(step), $"Original Fireflea darkness of step {step}");
        AssertThrows<InvalidDataException>(() => FirefleaShades.DarknessShade(7), "Darkness upper bound");
        AssertThrows<InvalidDataException>(() => FirefleaShades.DarknessShade(-1), "Darkness negative step");
    }

    /// <summary>Checks that frozen time holds the flash clock while the darkness level still follows the enemies.</summary>
    private static void VerifyFirefleaFrozenDarkness()
    {
        var fireflea = new FirefleaFxState();
        fireflea.Initialize();
        for (int frame = 0; frame < 10; frame++) fireflea.Step(frozen: false, darknessStep: 0);
        WrappingCountdown timer = fireflea.Timer;
        int index = fireflea.Index;
        fireflea.Step(frozen: true, darknessStep: 1);
        AssertEqual(timer, fireflea.Timer, "frozen Fireflea holds its flash timer");
        AssertEqual(index, fireflea.Index, "frozen Fireflea holds its flash index");
        AssertEqual(1, fireflea.DarknessStep, "frozen Fireflea still publishes the enemies' darkness step");
    }

    /// <summary>
    /// Checks the flash clock's wrap: a step from the cleared zero timer, before the FX initializes, wraps to $FFFF
    /// rather than restarting, and the index stays put until the timer later reaches zero.
    /// </summary>
    private static void VerifyFirefleaUninitializedTimerWrap()
    {
        var fireflea = new FirefleaFxState();
        fireflea.Step(frozen: false, darknessStep: 0);
        AssertEqual(ushort.MaxValue, fireflea.Timer.RawValue, "a step from the cleared timer wraps");
        AssertEqual(0, fireflea.Index, "the wrapped step does not advance the index");
        fireflea.Initialize();
        for (int step = 0; step < FirefleaFxDefinitions.FlashDuration; step++)
            fireflea.Step(frozen: false, darknessStep: 0);
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
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.GreenBrinstarFirefleas); // Green Brinstar Firefleas, retail room/FX population.
        AssertEqual(RoomFxType.Fireflea, runtime.RoomFxOverlay.Type, "retail fixture selects Fireflea FX");
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        // Seed a late darkness shade through the production FX owner. X-ray freezes
        // the producer, so its display must preserve this value rather than substitute
        // ordinary X-ray's constant seven or restart the flash cycle.
        runtime.RoomFxOverlay.Step(runtime.Room.LoadedScope.ArtworkFrames, new RoomFxOverlayFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame, FirefleaDarknessStep = 3 });
        int shade = runtime.RoomFxOverlay.Fireflea.Tint.FadeLevels.Red;
        AssertEqual(18, shade, "retail darkness table seeds a shade above X-ray's minimum");
        // The darkness and its shade reach the screen at the accepted frame (#1307).
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var ordinaryDarkness = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        AssertTrue(ordinaryDarkness.Layers[0] is GameplayBlendLayer,
            "normal Fireflea display retains source identity for darkness subtraction");
        var darkLayer = (GameplayBlendLayer)ordinaryDarkness.Layers[0];
        AssertEqual(BlendedLayers.Background | BlendedLayers.Backdrop, darkLayer.Blend.Blended,
            "normal Fireflea darkens the background and the backdrop, not the foreground or Samus");
        AssertEqual(BlendOperation.Subtract, darkLayer.Blend.Operation, "normal Fireflea darkness subtracts its shade");
        AssertTrue(darkLayer.Window.IsClosed,
            "ordinary Fireflea darkness has no X-ray exemption window");
        AssertTrue(!darkLayer.RevealBlocks && darkLayer.OverlayBehind is null && !darkLayer.BackgroundBehind,
            "ordinary Fireflea keeps both room maps and subtracts its shade, with nothing lying behind");
        AssertTrue(samus.Xray.TryBegin(samus, SamusPoseMetadata.ReadMovementType(samus), new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }), "Fireflea display fixture activates X-ray");
        for (int frame = 0; frame < 90; frame++) runtime.StepFrame(runtime.Options.ControllerBindings.Dash);
        var packet = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        AssertTrue(packet.Layers[0] is GameplayBlendLayer, "Fireflea capture uses source-aware X-ray compositor");
        var layer = (GameplayBlendLayer)packet.Layers[0];
        AssertTrue(!layer.RevealBlocks && layer.OverlayBehind is null && !layer.BackgroundBehind,
            "Fireflea preserves room maps and subtracts its shade, with nothing lying behind");
        AssertEqual(BlendedLayers.Foreground | BlendedLayers.Background | BlendedLayers.Actors | BlendedLayers.Backdrop,
            layer.Blend.Blended, "Fireflea X-ray darkens both backgrounds, the actors and the backdrop");
        AssertTrue(layer.Blend.Operation == BlendOperation.Subtract && !layer.Blend.Halved, "Fireflea X-ray subtracts its shade whole");
        AssertEqual(new FadeLevelColour(shade, shade, shade).ToColour(), layer.Blend.Colour,
            "Fireflea's grey shade survives 90 frozen runtime frames");
        for (int frame = 0; frame < 8; frame++) runtime.StepFrame(0);
        AssertTrue(!samus.Xray.IsActive && !runtime.Freeze.TimeIsFrozen, "Fireflea X-ray teardown releases gameplay");
        var restored = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        AssertEqual(BlendedLayers.Background | BlendedLayers.Backdrop, ((GameplayBlendLayer)restored.Layers[0]).Blend.Blended,
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
