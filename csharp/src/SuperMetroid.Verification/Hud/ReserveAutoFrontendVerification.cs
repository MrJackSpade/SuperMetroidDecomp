using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Diagnostics.SaveStates;
namespace SuperMetroid.Verification.Hud;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Audio;

/// <summary>Checks automatic reserve recovery through the game frontend, including freeze timing, HUD updates, and health-warning audio.</summary>
internal static class ReserveAutoFrontendVerification
{
    /// <summary>The native HUD digit tilemap word for <paramref name="digit"/> at $80:9DBF.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the HUD health-digit tilemap words")]
    private static ushort ReadRetailHudDigitTile(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus, int digit)
    {
        int address = 0x809dbf + digit * 2;
        return bus.ReadCartridgeWord(address);
    }

    /// <summary>The native empty AUTO reserve-indicator tilemap word at byte <paramref name="offset"/> from $80:9997.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the HUD empty AUTO reserve-indicator tilemap")]
    private static ushort ReadRetailEmptyAutoReserveTile(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus, int offset)
    {
        int address = 0x809997 + offset;
        return bus.ReadCartridgeWord(address);
    }

    /// <summary>Drives zero-health entry and a complete refill through state eight, comparing visible digits, AUTO indicator, movement, and warning cues.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyReserveAutoFrontend()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var random = new CartridgeRandom();
        var runtime = CreateRetailRuntimeFixture(random: random);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.AlphaPowerBomb);
        var samus = runtime.Player.IntroducedSamus;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        samus.Health = 0; samus.MaxHealth = 99;
        samus.ReserveEnergy = 2; samus.MaxReserveEnergy = 100; samus.ReserveTankMode = ReserveTankMode.Auto;
        var game = new SuperMetroid.Core.Frontend.SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(), null, SuperMetroid.Core.Frontend.SuperMetroidGameHostServices.Default, random);
        game.AdoptRuntime(runtime);
        var recovery = game.Session.reserveRecovery;
        // Enter through actual state-eight routing, not by calling recovery.Begin:
        // the zero-health gameplay frame must publish recovery without consuming
        // the first reserve point early or letting Start replace it with pause.
        game.GameState = SuperMetroidGameState.MainGameplay;
        long hostSequence = 0;
        var entry = game.StepCaptured(ControllerButton.Start, ++hostSequence, 1);
        AssertEqual(SuperMetroidGameState.ReserveTanksAuto, game.GameState,
            "zero-health gameplay routes to automatic reserves before pause admission");
        AssertEqual(0, samus.Health, "entry frame does not transfer reserve energy early");
        AssertEqual(2, samus.ReserveEnergy, "entry frame retains the full reserve supply");
        AssertTrue(runtime.Freeze.TimeIsFrozen && samus.Control.AlphaLocked,
            "entry publishes both recovery locks for the next frame");
        AssertTrue(entry.Snapshot is not null,
            "entry preserves the completed gameplay presentation instead of a blank frame");
        runtime.BombProjectiles.SetSharedCooldown(10);
        int lockedAnimationFrame = samus.AnimationFrame;
        ushort lockedAnimationTimer = samus.AnimationFrameTimer.RawValue;
        game.StepCaptured(0, ++hostSequence, 1);
        AssertEqual(lockedAnimationFrame, samus.AnimationFrame,
            "automatic refill command-zero beta preserves the current Samus animation frame");
        AssertEqual(lockedAnimationTimer, samus.AnimationFrameTimer.RawValue,
            "automatic refill command-zero beta does not advance Samus animation timer");
        AssertEqual(1, samus.Health, "automatic refill first frame");
        AssertTrue(samus.HealthWarning.IsActive, "native external health check starts warning during frozen refill");
        AssertTrue(runtime.Freeze.TimeIsFrozen && samus.Control.AlphaLocked, "nonfinal refill keeps native freeze and input lock");
        AssertTrue(runtime.Player.LastGroundedSamusMovement is null, "nonfinal refill does not execute grounded movement");
        AssertEqual(10, runtime.BombProjectiles.CooldownTimer.RawValue, "nonfinal refill freezes shared projectile clock");
        game.StepCaptured(ControllerButton.Right, ++hostSequence, 1);
        AssertEqual(2, samus.Health, "automatic refill completion frame");
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "completion restores outer gameplay state");
        AssertTrue(!runtime.Freeze.TimeIsFrozen && !samus.Control.AlphaLocked, "completion clears native freeze and input lock");
        AssertEqual(9, runtime.BombProjectiles.CooldownTimer.RawValue,
            "completion unfreezes the gameplay projectile pass on the same frame");
        AssertTrue(runtime.Player.LastGroundedSamusMovement is not null,
            "native $82:DC18 unfreezes BEFORE the completion frame gameplay/movement pass");
        var audio = game.Session.audio;
        samus.HealthWarning.Update(99, audio);
        samus.Health = 0; samus.ReserveEnergy = 33;
        // Seed the display at the pre-transfer health. Each subsequent gameplay
        // pass updates the HUD for the next accepted NMI to latch, including while
        // automatic recovery freezes movement. Inspect presentation, not just HP.
        runtime.Hud.UpdateGameplayCounters(samus);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var digitImages = new Dictionary<int, Rgba32[]>();
        recovery.Begin(samus); runtime.Freeze.GameplayTimeFrozen = true;
        lockedAnimationFrame = samus.AnimationFrame;
        lockedAnimationTimer = samus.AnimationFrameTimer.RawValue;
        game.GameState = SuperMetroidGameState.ReserveTanksAuto;
        var positions = audio.SoundWritePositions;
        var queues = audio.SoundQueues;
        for (int frame = 0; frame < 33; frame++)
        {
            // Drain the unrelated host transport between observations. Production
            // refill and warning producers still own every request being inspected.
            audio.Reset();
            var capture = game.StepCaptured(0, ++hostSequence, 1);
            if (frame < 32)
            {
                AssertEqual(lockedAnimationFrame, samus.AnimationFrame,
                    "all nonfinal recovery frames retain Samus animation cursor");
                AssertEqual(lockedAnimationTimer, samus.AnimationFrameTimer.RawValue,
                    "all nonfinal recovery frames retain Samus animation delay");
            }
            else
                AssertTrue(samus.Control.Mode.Beta != SamusBetaHandler.ScriptLocked,
                    "completion restores the normal animation handler on the same frame");
            var pixels = DisplayRendering.Render(capture.Snapshot);
            foreach ((int index, int digit) in new[] { new VerifyReserveAutoFrontendCase(70, frame / 10), new VerifyReserveAutoFrontendCase(71, frame % 10) })
            {
                AssertEqual(ReadRetailHudDigitTile(bus, digit), runtime.AcceptedLayer3Plane.Words[Layer3Plane.Columns + index],
                    "automatic refill digit becomes visible on the following accepted NMI");
            }
            var onesPixels = Enumerable.Range(24, 8).SelectMany(y =>
                Enumerable.Range(56, 8).Select(x => pixels[y * 256 + x])).ToArray();
            if (digitImages.TryGetValue(frame % 10, out var prior))
                AssertTrue(prior.SequenceEqual(onesPixels), "repeated health digit has identical visible pixels");
            else
                digitImages.Add(frame % 10, onesPixels);
            AssertEqual(frame + 1, samus.Health, "full automatic refill progression");
            AssertEqual(frame < 30, samus.HealthWarning.IsActive, "warning switches off exactly at health 31");
            bool Has(int command) => Enumerable.Range(0, positions[2]).Any(i => queues[2, i] == SoundEffectCommands.Decode(SoundEffectLibrary.Library3, command));
            AssertEqual(frame == 0, Has(2), "warning start issued only once through real frontend");
            AssertEqual(frame == 30, Has(1), "warning stop issued on 30-to-31 refill crossing");
        }
        AssertEqual(10, digitImages.Count, "automatic refill visibly traverses all ten health digits");
        foreach (var left in digitImages)
            foreach (var right in digitImages.Where(pair => pair.Key > left.Key))
                AssertTrue(!left.Value.SequenceEqual(right.Value), "different health digits render differently during recovery");
        game.StepCaptured(0, ++hostSequence, 1);
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "presentation sequence returns to gameplay");
        foreach ((int index, int offset) in new[] { new VerifyReserveAutoFrontendCase(8, 0), new VerifyReserveAutoFrontendCase(9, 2), new VerifyReserveAutoFrontendCase(40, 4), new VerifyReserveAutoFrontendCase(41, 6), new VerifyReserveAutoFrontendCase(72, 8), new VerifyReserveAutoFrontendCase(73, 10) })
        {
            AssertEqual(ReadRetailEmptyAutoReserveTile(bus, offset), runtime.AcceptedLayer3Plane.Words[Layer3Plane.Columns + index],
                "completed automatic refill publishes the native empty AUTO indicator");
        }
        samus.HealthWarning.Update(30, audio);
        using var saved = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(saved, samus); saved.Position = 0;
        AssertTrue(DebuggerObjectGraphSerializer.Deserialize<SamusState>(saved).HealthWarning.IsActive,
            "Samus debugger graph retains warning latch");
        // Continue in actual state eight: ordinary beta must own the same latch,
        // while a generic locked handler must not acquire that responsibility.
        samus.Health = 31; audio.Reset(); game.StepCaptured(0, ++hostSequence, 1);
        AssertTrue(!samus.HealthWarning.IsActive, "ordinary beta stops warning at healthy threshold");
        samus.Control.LockHandlers(); samus.Health = 30;
        audio.Reset(); game.StepCaptured(0, ++hostSequence, 1);
        AssertTrue(!samus.HealthWarning.IsActive, "locked handler does not run ordinary health check");
        samus.Control.Unlock();
        audio.Reset(); game.StepCaptured(0, ++hostSequence, 1);
        AssertTrue(samus.HealthWarning.IsActive, "ordinary gameplay acquires critical warning after unlock");
        AssertTrue(Enumerable.Range(0, positions[2]).Any(i => queues[2, i] == SoundEffectCommands.Decode(SoundEffectLibrary.Library3, 2)), "ordinary gameplay publishes actual warning command");
        runtime.RoomLoader.InitializePostCeresZebesRoom();
        // The Zebes handoff installs a fresh Samus that carries over only the inventory.
        samus = runtime.Player.Samus;
        AssertTrue(runtime.Player.Samus?.Control.Mode.Beta != SamusBetaHandler.Gunship, "initial post-Ceres descent does not install command 1A");
        var ship = runtime.Enemies.SlotTable.Slots[0];
        GunshipPartState shipPart = GunshipPartState.Of(ship);
        shipPart.Function = GunshipFunction.WaitForEntranceToOpen;
        shipPart.FunctionTimer = 100;
        (samus ?? throw new InvalidOperationException("Verification requires samus.")).HealthWarning.Update(99, audio);
        samus.Health = 30; samus.Control.EnterGunship();
        audio.Reset(); game.StepCaptured(0, ++hostSequence, 1);
        AssertTrue(samus.Control.Mode.Beta == SamusBetaHandler.Gunship && samus.HealthWarning.IsActive,
            "gunship entry handler checks low health despite locked input");
        AssertTrue(Enumerable.Range(0, positions[2]).Any(i => queues[2, i] == SoundEffectCommands.Decode(SoundEffectLibrary.Library3, 2)), "gunship handler publishes warning audio");
        Console.WriteLine("Automatic reserve frontend: nonfinal freeze and same-frame completion movement pass.");
    }

    /// <summary>Runs the reserve AUTO frontend verification command.</summary>
    [VerificationCommand("--reserve-auto-frontend")]
    internal static int ReserveAutoFrontendCommand()
    {
        Suite(nameof(VerifyReserveAutoFrontend), () => VerifyReserveAutoFrontend());
        return 0;
    }
}
