using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;

namespace SuperMetroid.Verification.Rooms.Doors;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the door-entry library-one cancel against the configured shot button.</summary>
internal static class DoorEntryShotBindingVerification
{
    /// <summary>
    /// Samus command $1D (`$90:F48A`) skips the library-one cancel while `$8B` holds the shot binding.
    /// The contributed lowc movie binds shot to Y and item select to X, and enters a door with X held:
    /// native queues the cancel and the door waits for it to play, while the port, which tested a fixed
    /// physical X, skipped both and ran ahead of the native door by two updates.
    /// </summary>
    internal static void VerifyDoorEntryShotBinding()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        ControllerBindings lowc = ControllerBindings.Default with
        {
            Shoot = SnesButton.Y, ItemSelect = SnesButton.X, ItemCancel = SnesButton.Select,
            AimUp = SnesButton.L, AimDown = SnesButton.R,
        };
        foreach ((SnesButton held, bool cancels) in new[] { (SnesButton.X, true), (SnesButton.Y, false) })
        {
            var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.Options.ControllerBindings = lowc;
            var samus = runtime.Player.IntroducedSamus;
            samus.InputLocked = true;
            var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
            game.Session.runtime = runtime;
            game.Session.lastAudioRoomStatePointer = runtime.Room.LoadedRoom.State.Pointer;
            game.GameState = SuperMetroidGameState.MainGameplay;

            var level = runtime.Room.LoadedLevelData;
            bool selected = false;
            for (int y = 0; y < level.Size.Y && !selected; y++)
            for (int x = 0; x < level.Size.X && !selected; x++)
            {
                var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
                if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
                level.ResolveDoorCollision(block.Behavior, samus.Pose, true);
                selected = true;
            }
            AssertTrue(selected, "the Ceres start room has an exit");
            var audio = game.AudioForVerification;
            byte start = audio.SoundQueueForVerification(0).Next;
            game.GameState = SuperMetroidGameState.HitDoorBlock;
            game.Step(held);

            AssertEqual(DoorTransitionPhase.WaitForSoundQueues, game.DoorTransitionPhaseForVerification,
                "the entry frame begins the door sound wait");
            byte next = audio.SoundQueueForVerification(0).Next;
            AssertEqual(cancels, next != start,
                $"physical {held} with shot bound to Y {(cancels ? "queues" : "does not queue")} the library-one cancel");
            if (cancels)
                AssertEqual(SoundEffectLibrary1Sounds.CancelAll.Value, audio.SoundQueueEntryForVerification(0, start),
                    "the queued library-one request is the cancel");
        }
        Console.WriteLine("Door entry shot binding: the library-one cancel follows the shot binding, not physical X.");
    }

    /// <summary>Runs the door-entry shot-binding regression.</summary>
    [VerificationCommand("--door-entry-shot-binding")]
    internal static int DoorEntryShotBindingCommand()
    {
        VerifyDoorEntryShotBinding();
        return 0;
    }
}
