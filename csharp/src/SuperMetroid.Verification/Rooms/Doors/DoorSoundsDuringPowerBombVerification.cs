using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Rooms.Doors;
using SuperMetroid.Verification.Harness;

/// <summary>Reproduces the door sound-queue timing during a negative-status Power Bomb explosion.</summary>
internal static class DoorSoundsDuringPowerBombVerification
{
    /// <summary>
    /// All three sound-queue routines drop requests while a power-bomb explosion's status is
    /// negative ($80:9072/$90F4/$9176). In the 13% movie Samus enters a door during one, so
    /// the door's $32/$71 cancels never reach a ring and $E29E proceeds on its first check;
    /// the port queued them and waited a frame longer.
    /// </summary>
    internal static void VerifyDoorSoundsDuringPowerBomb()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        runtime.Player.IntroducedSamus.InputLocked = true;
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false);
        game.Session.runtime = runtime;
        game.Session.lastAudioRoomStatePointer = runtime.Room.LoadedRoom.State.Pointer;
        game.GameState = SuperMetroidGameState.MainGameplay;
        game.Step(0);

        var level = runtime.Room.LoadedLevelData;
        bool selected = false;
        for (int y = 0; y < level.Size.Y && !selected; y++)
        for (int x = 0; x < level.Size.X && !selected; x++)
        {
            var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            level.ResolveDoorCollision(block.Behavior, runtime.Player.IntroducedSamus.Pose, true);
            selected = true;
        }
        AssertTrue(selected, "Landing Site has an exit");

        runtime.PowerBombExplosionStatus = 0x8000;
        var audio = game.AudioForVerification;
        byte library1 = audio.SoundQueueForVerification(0).Next;
        byte library2 = audio.SoundQueueForVerification(1).Next;
        game.GameState = SuperMetroidGameState.HitDoorBlock;
        game.Step(0);

        AssertEqual(DoorTransitionPhase.WaitForSoundQueues, game.DoorTransitionPhaseForVerification,
            "the entry frame begins the door sound wait");
        AssertEqual(library1, audio.SoundQueueForVerification(0).Next, "library one's cancel is dropped");
        AssertEqual(library2, audio.SoundQueueForVerification(1).Next, "library two's $71 is dropped");
        game.Step(0);
        AssertEqual(DoorTransitionPhase.FadeOutSourcePalette, game.DoorTransitionPhaseForVerification,
            "$E29E finds the rings empty on its first check");
        Console.WriteLine("  Door sounds during a power bomb: the cancels are dropped and the wait ends at once.");
    }

    /// <summary>Runs the focused door-transition sound cancellation check.</summary>
    [VerificationCommand("--door-sounds-during-power-bomb")]
    internal static int DoorSoundsDuringPowerBombCommand()
    {
        Suite(nameof(VerifyDoorSoundsDuringPowerBomb), () => VerifyDoorSoundsDuringPowerBomb());
        return 0;
    }
}
