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
        var vram = new SnesVram();
        RoomEffectDefinition record = RoomFxDefinitions.AllRecords.First(candidate => candidate.Type == RoomFxType.Fireflea && candidate.DoorPointer == 0);
        // Independent transcription of $88:B058/$88:B070. These are deliberately not
        // generated from the production catalog or implementation.
        ushort[] flash = [0, 0x100, 0x200, 0x300, 0x400, 0x500, 0x600, 0x500, 0x400, 0x300, 0x200, 0x100];
        // A sixth death's faint darkness is not kept (#2573); the deepest step holds.
        ushort[] darkness = [0, 0x600, 0xC00, 0x1200, 0x1800, 0x1900];
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyFirefleaFlashingAlgorithm), () => VerifyFirefleaFlashingAlgorithm(rom));
        Suite(nameof(VerifyFirefleaDarknessAlgorithm), () => VerifyFirefleaDarknessAlgorithm(rom));
        Suite(nameof(VerifyFirefleaFrozenDarkness), VerifyFirefleaFrozenDarkness);
        Suite(nameof(VerifyFirefleaFlashCount), VerifyFirefleaFlashCount);
        RoomFxOverlayState fx = RoomFxOverlayState.ForDefinition(record, 0);
        FirefleaFxState state = Darkness(fx);
        AssertTrue(!fx.IsRenderable, "Fireflea darkness does not invent a BG3 texture");
        for (int frame = 1; frame <= 144; frame++)
        {
            fx.Step(new ArtworkFrameSelections(), new RoomFxOverlayFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false });
            int expectedPhase = (frame / 6) % 12;
            int shade = LittleEndianBytes.HighByte(flash[expectedPhase]);
            AssertEqual(new FirefleaFlashShade(shade), state.Flash, $"Fireflea flash look frame {frame}");
            AssertEqual(new RgbColor?(new FadeLevelColour(shade, shade, shade).ToColour()), fx.Fireflea?.Tint,
                $"Fireflea grey shade frame {frame}");
        }
        RgbColor tint = state.Tint;
        for (int frame = 0; frame < 100; frame++) fx.Step(new ArtworkFrameSelections(), new RoomFxOverlayFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = true });
        AssertEqual(tint, state.Tint, "X-ray frozen time retains the shade");
        // Frozen time holds the flash where it stood (#2488): the unfrozen updates after it carry on the six-update cadence.
        for (int frame = 145; frame <= 288; frame++)
        {
            fx.Step(new ArtworkFrameSelections(), new RoomFxOverlayFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false });
            int shade = LittleEndianBytes.HighByte(flash[frame / 6 % 12]);
            AssertEqual(new FirefleaFlashShade(shade), state.Flash, $"X-ray frozen time retains flashing phase, unfrozen frame {frame}");
        }
        for (int step = 0; step < darkness.Length; step++)
        {
            fx = RoomFxOverlayState.ForDefinition(record, 0);
            state = Darkness(fx);
            for (int frame = 1; frame <= 78; frame++)
            {
                fx.Step(new ArtworkFrameSelections(), new RoomFxOverlayFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, FirefleaDarkness = global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Fx.FirefleaDarkness>([typeof(int)], new object?[] { step }) });
                int index = step < 5 ? frame / 6 % 12 : frame < 6 ? 0 : 6;
                // The native shade's bits above the level name the colour channels each write sets, all to the level.
                int shade = LittleEndianBytes.HighByte((ushort)(darkness[step] + flash[index])) & FadeLevelColour.FullLevel;
                AssertEqual(new FadeLevelColour(shade, shade, shade).ToColour(), state.Tint, $"darkness step {step}, frame {frame}: grey shade");
                AssertEqual(step, state.Darkness.FirefleasKilled, "the enemies' count of Firefleas killed reaches the Fireflea FX state");
            }
            RgbColor retained = state.Tint;
            fx.Step(new ArtworkFrameSelections(), new RoomFxOverlayFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = true, FirefleaDarkness = global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Fx.FirefleaDarkness>([typeof(int)], new object?[] { step }) });
            AssertEqual(retained, state.Tint, "frozen death level retains its exact shade");
        }
        // A room without the effect has no Fireflea darkness and shows no Fireflea shade; the darkness lives with its room.
        fx = RoomFxOverlayState.ForRoom(null, null, 0, SuperMetroid.Core.Audio.GameplayAudioRequests.Standalone());
        fx.Step(new ArtworkFrameSelections(), new RoomFxOverlayFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false });
        AssertTrue(fx.Fireflea is null, "a non-Fireflea room has no Fireflea darkness");
        if (includeXrayCapture) VerifyFirefleaXrayCapture();
        Console.WriteLine("  Fireflea FX: compiled native shades, ROM-read guard, initialization, flash cycles, all seven death offsets, grey shades and frozen-time retention agree.");
    }

    /// <summary>
    /// Checks the flash's twelve looks, in the order its animation shows them (#2367), against the high byte of each native
    /// word, and that every native flash word's low byte is zero, so adding a darkness word never carries into the shade
    /// (#1415: the shade is a level, not the cartridge's word).
    /// </summary>
    private static void VerifyFirefleaFlashingAlgorithm(CartridgeImportAddressSpace rom)
    {
        FirefleaFlashShade[] looks = [.. FirefleaFlashAnimations.Segments.Beats(FirefleaFlashSegment.Flashing)
            .Select(beat => beat.ShownFrame ?? throw new InvalidOperationException($"The Fireflea flash holds a beat that shows no look: {beat}."))];
        AssertEqual(12, looks.Length, "Fireflea flash looks");
        for (int i = 0; i < looks.Length; i++)
        {
            ushort word = CartridgeImportSource.Require(rom).ReadCartridgeWord((FirefleaFxDefinitions.FlashReferenceAddress + 2 * i));
            AssertEqual(LittleEndianBytes.HighByte(word), looks[i].Level, $"Original Fireflea flash {i}");
            AssertEqual((byte)0, LittleEndianBytes.LowByte(word), $"Original Fireflea flash {i} carries nothing");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => new FirefleaFlashShade(7), "Flash upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => new FirefleaFlashShade(-1), "Flash lower bound");
    }

    /// <summary>
    /// Checks the six darkness steps' fade levels against the high byte of each cartridge word below its channel bits; the
    /// low byte cannot reach the shade because the flash words' low bytes are zero (#1415). A sixth death keeps the
    /// deepest step (#2573).
    /// </summary>
    private static void VerifyFirefleaDarknessAlgorithm(CartridgeImportAddressSpace rom)
    {
        AssertEqual(6, FirefleaFxDefinitions.DarknessLevels.Length, "Fireflea darkness steps");
        for (int step = 0; step < FirefleaFxDefinitions.DarknessLevels.Length; step++)
            AssertEqual(LittleEndianBytes.HighByte(CartridgeImportSource.Require(rom).ReadCartridgeWord((FirefleaFxDefinitions.DarknessReferenceAddress + step * 2))) & FadeLevelColour.FullLevel,
                global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Fx.FirefleaDarkness>([typeof(int)], new object?[] { step }).Level, $"Original Fireflea darkness of step {step}");
        AssertEqual(global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Fx.FirefleaDarkness>([typeof(int)], new object?[] { 5 }), global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Fx.FirefleaDarkness>([typeof(int)], new object?[] { 5 }).Deeper, "a death past the fifth keeps the deepest darkness");
        AssertThrows<ArgumentOutOfRangeException>(() => global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Fx.FirefleaDarkness>([typeof(int)], new object?[] { 6 }), "Darkness upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Fx.FirefleaDarkness>([typeof(int)], new object?[] { -1 }), "Darkness negative step");
    }

    /// <summary>The Fireflea darkness <paramref name="fx"/>'s room effect started, which the check requires.</summary>
    private static FirefleaFxState Darkness(RoomFxOverlayState fx) =>
        fx.Fireflea ?? throw new InvalidOperationException("Verification requires the room's Fireflea darkness.");

    /// <summary>Checks that frozen time holds the flash clock while the darkness level still follows the enemies.</summary>
    private static void VerifyFirefleaFrozenDarkness()
    {
        var fireflea = new FirefleaFxState();
        var unfrozen = new FirefleaFxState();
        for (int frame = 0; frame < 10; frame++)
        {
            fireflea.Step(frozen: false, global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Fx.FirefleaDarkness>([typeof(int)], new object?[] { 0 }));
            unfrozen.Step(frozen: false, global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Fx.FirefleaDarkness>([typeof(int)], new object?[] { 0 }));
        }
        FirefleaFlashShade look = fireflea.Flash;
        fireflea.Step(frozen: true, global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Fx.FirefleaDarkness>([typeof(int)], new object?[] { 1 }));
        AssertEqual(look, fireflea.Flash, "frozen Fireflea holds its flash look");
        AssertEqual(1, fireflea.Darkness.FirefleasKilled, "frozen Fireflea still publishes the enemies' darkness");
        // The frozen update counts nothing off the flash (#2488): it goes on exactly as a flash never frozen.
        for (int frame = 0; frame < 2 * FirefleaFxDefinitions.FlashDuration; frame++)
        {
            fireflea.Step(frozen: false, global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Fx.FirefleaDarkness>([typeof(int)], new object?[] { 0 }));
            unfrozen.Step(frozen: false, global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Fx.FirefleaDarkness>([typeof(int)], new object?[] { 0 }));
            AssertEqual(unfrozen.Flash, fireflea.Flash, $"frozen Fireflea holds its flash timer, unfrozen update {frame} after it");
        }
    }

    /// <summary>Checks that a new Fireflea darkness starts from its first flash and advances the phase when its count reaches zero.</summary>
    private static void VerifyFirefleaFlashCount()
    {
        var fireflea = new FirefleaFxState();
        // A new darkness waits a whole flash on its first look, then the next shows for a whole flash too (#2488).
        for (int look = 0; look < 2; look++)
        {
            for (int step = 1; step < FirefleaFxDefinitions.FlashDuration; step++)
            {
                fireflea.Step(frozen: false, global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Fx.FirefleaDarkness>([typeof(int)], new object?[] { 0 }));
                AssertEqual(new FirefleaFlashShade(look), fireflea.Flash, $"look {look} still shows {step} updates in");
            }
            fireflea.Step(frozen: false, global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Fx.FirefleaDarkness>([typeof(int)], new object?[] { 0 }));
            AssertEqual(new FirefleaFlashShade(look + 1), fireflea.Flash, $"look {look}'s count reaching zero shows the flash's next look");
        }
    }

    /// <summary>Checks the retail Fireflea room's fixed-color subtraction before, during, and after X-Ray.</summary>
    private static void VerifyFirefleaXrayCapture()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
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
        runtime.RoomFxOverlay.Step(runtime.Room.LoadedScope.ArtworkFrames, new RoomFxOverlayFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, FirefleaDarkness = global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Fx.FirefleaDarkness>([typeof(int)], new object?[] { 3 }) });
        int shade = Darkness(runtime.RoomFxOverlay).Tint.FadeLevels.Red;
        AssertEqual(18, shade, "retail darkness table seeds a shade above X-ray's minimum");
        // The darkness and its shade reach the screen at the accepted frame (#1307).
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var ordinaryDarkness = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        AssertTrue(ordinaryDarkness.Layers[0] is GameplayBlendLayer,
            "normal Fireflea display retains source identity for darkness subtraction");
        var darkLayer = (GameplayBlendLayer)ordinaryDarkness.Layers[0];
        AssertTrue(darkLayer.Blend is FirefleaDarknessBlend,
            "normal Fireflea shows its darkness, which darkens the background and the backdrop, not the foreground or Samus");
        AssertTrue(darkLayer.Window.IsClosed,
            "ordinary Fireflea darkness has no X-ray exemption window");
        AssertTrue(!darkLayer.RevealBlocks && darkLayer.OverlayBehind is null && !darkLayer.BackgroundBehind,
            "ordinary Fireflea keeps both room maps and subtracts its shade, with nothing lying behind");
        AssertTrue(samus.Xray.TryBeginAndActivate(samus, SamusPoseMetadata.ReadMovementType(samus), new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay, PowerBombExplosionActive = false, ProjectileCooldownTimer = 0, BombCounter = 0 }), "Fireflea display fixture activates X-ray");
        for (int frame = 0; frame < 90; frame++) runtime.StepFrame(runtime.Options.ControllerBindings[SuperMetroid.Core.Input.ControllerAction.Dash]);
        var packet = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        AssertTrue(packet.Layers[0] is GameplayBlendLayer, "Fireflea capture uses source-aware X-ray compositor");
        var layer = (GameplayBlendLayer)packet.Layers[0];
        AssertTrue(!layer.RevealBlocks && layer.OverlayBehind is null && !layer.BackgroundBehind,
            "Fireflea preserves room maps and subtracts its shade, with nothing lying behind");
        AssertTrue(layer.Blend is XrayDimmingBlend { Mode.KeepsDarkness: true },
            "Fireflea X-ray keeps the darkness, darkening the whole room with its shade whole");
        AssertEqual(new FadeLevelColour(shade, shade, shade).ToColour(), ((XrayDimmingBlend)layer.Blend).Tint,
            "Fireflea's grey shade survives 90 frozen runtime frames");
        for (int frame = 0; frame < 8; frame++) runtime.StepFrame(0);
        AssertTrue(!samus.Xray.IsActive && !runtime.Freeze.TimeIsFrozen, "Fireflea X-ray teardown releases gameplay");
        var restored = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        AssertTrue(((GameplayBlendLayer)restored.Layers[0]).Blend is FirefleaDarknessBlend,
            "Fireflea teardown restores the room's darkness rather than unblended rendering");
    }

    /// <summary>Dispatches the focused Fireflea FX verification command.</summary>
    [VerificationCommand("--fireflea-fx")]
    internal static int FirefleaFxCommand()
    {
        Suite(nameof(VerifyFirefleaFx), () => VerifyFirefleaFx());
        return 0;
    }
}
