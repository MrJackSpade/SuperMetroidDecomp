using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Rooms.Doors;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

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
        var random = new SharedRandom();
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), renderGameplayFrames: false, random: random);
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true, random: random, audio: game.AudioForVerification);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.LandingSite);
<<<<<<< HEAD
        runtime.Player.Samus.Control.LockOutPlayer();
||||||| 418ca79935
        runtime.Player.IntroducedSamus.Control.LockOutPlayer();
=======
>>>>>>> origin/main
        game.AdoptRuntime(runtime);
        game.Session.LastAudioRoomState = (global::SuperMetroid.Core.World.RoomStates.RoomStateSettings?)(runtime.Room.LoadedRoom.State);
        game.Session.Enter(SuperMetroidGameState.MainGameplay);
        game.Step(0);
        // A real power bomb, laid and set off through gameplay, explodes as Samus enters the door.
        PowerBombDetonation.ExplodeThroughGameplay(runtime, buttons => game.Step(buttons));
        runtime.Player.IntroducedSamus.Control.LockOutPlayer();

        var level = runtime.Room.LoadedLevelData;
        bool selected = false;
        for (int y = 0; y < level.Size.Y && !selected; y++)
        for (int x = 0; x < level.Size.X && !selected; x++)
        {
            var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            runtime.MeetDoorBlock(block, runtime.Player.Samus.Pose.ForRoomCollision());
            selected = true;
        }
        AssertTrue(selected, "Landing Site has an exit");

        AssertTrue(runtime.Room.LoadedScope.BombProjectiles.PowerBombExplosion.IsActive, "the power bomb is still exploding as Samus enters the door");
        var audio = game.AudioForVerification;

        SoundRequestsMark library1 = audio.MarkSoundRequestsForVerification(SoundEffectLibrary.Samus);
        SoundRequestsMark library2 = audio.MarkSoundRequestsForVerification(SoundEffectLibrary.Enemies);
        game.Session.Enter(DoorTransitionStep.EnteringDoor);
        game.Step(0);

        AssertEqual(new DoorTransitionPhase.Running.WaitForSoundQueues(), game.DoorTransitionPhaseForVerification,
            "the entry frame begins the door sound wait");
        AssertEqual(0, audio.SoundsQueuedSinceForVerification(library1).Count, "library one's cancel is dropped");
        AssertEqual(0, audio.SoundsQueuedSinceForVerification(library2).Count, "library two's $71 is dropped");
        game.Step(0);
        AssertEqual(new DoorTransitionPhase.Running.FadeOutSourceRoom(), game.DoorTransitionPhaseForVerification,
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
