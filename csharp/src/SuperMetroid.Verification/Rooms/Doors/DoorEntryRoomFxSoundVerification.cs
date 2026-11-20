using SuperMetroid.Core.World;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Rooms.Doors;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

/// <summary>Checks room-FX sound ordering on the Rising Tide door entry frame.</summary>
internal static class DoorEntryRoomFxSoundVerification
{
    /// <summary>
    /// Verifies that MainGameLoop's HDMA pass queues the rising-lava rumble before the
    /// door handler's library-two cancel. This extra sound extends the door queue wait
    /// during the Rising Tide exit in issue #1269's 100% movie.
    /// </summary>
    internal static void VerifyDoorEntryRoomFxSound()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        // Rising Tide's lava rises only under the FX record of a door that requests it.
        DoorDefinition entry = RetailDoorHeaderCatalog.EnumeratePointers()
            .Select(pointer => CartridgeDoorHeaderImporter.Load(bus, pointer))
            .Where(door => door.DestinationRoom == RoomDefinitions.RisingTide)
            .First(door =>
            {
                var candidate = LoadRisingTide(door, new SharedRandom(), audio: null);
                for (int frame = 0; frame < 4; frame++)
                    candidate.StepFrame(0);
                return candidate.RoomFxOverlay.SustainedEarthquake.HasValue;
            });
        var random = new SharedRandom();
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), renderGameplayFrames: false, random: random);
        var runtime = LoadRisingTide(entry, random, game.AudioForVerification);
        var samus = runtime.Player.Samus;
        game.AdoptRuntime(runtime);
        game.Session.LastAudioRoomState = runtime.Room.LoadedRoom.State;
        game.Session.Enter(SuperMetroidGameState.MainGameplay);

        SuperMetroid.Core.Runtime.SuperMetroidRuntime LoadRisingTide(DoorDefinition door, SharedRandom random, AudioQueues? audio)
        {
            var loaded = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true, random: random, audio: audio);
            loaded.HudSetup.InitializeHud(HudSnapshot.NewGame);
            loaded.RoomLoader.InitializeStartingCeresRoom();
            loaded.RoomLoader.InitializeCeresStartSamus(loaded.Room.Loaded);
            loaded.LoadCartridgeRoomThroughDoorForVerification(door);
            loaded.Player.Samus.Control.LockOutPlayer();
            return loaded;
        }

        // Run real frames until the rising lava's rumble timer will fire on the next HDMA pass.
        StepUntil(() => runtime.RoomFxOverlay.SustainedEarthquake.HasValue &&
                runtime.RoomFxOverlay.EarthquakeSoundTimer.IsZero,
            _ => game.Step(0), maximumFrames: 600, context: "Rising Tide rumble timer reaches zero");

        var level = runtime.Room.LoadedLevelData;
        bool selected = false;
        for (int y = 0; y < level.Size.Y && !selected; y++)
        for (int x = 0; x < level.Size.X && !selected; x++)
        {
            var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            runtime.MeetDoorBlock(block, samus.Pose.ForRoomCollision());
            selected = true;
        }
        AssertTrue(selected, "Rising Tide has an exit");
        var audio = game.AudioForVerification;
        SoundRequestsMark start = audio.MarkSoundRequestsForVerification(SoundEffectLibrary.Enemies);
        game.Session.Enter(DoorTransitionStep.EnteringDoor);
        game.Step(0);

        AssertEqual(DoorTransitionPhase.WaitForSoundQueues, game.DoorTransitionPhaseForVerification,
            "the entry frame begins the door sound wait");
        IReadOnlyList<SoundEffectId> queued = audio.SoundsQueuedSinceForVerification(start);
        AssertTrue(queued.Count >= 2, "the entry frame queues at least the rumble and the door's cancel");
        AssertEqual<SoundEffectId>(EnemySound.Lavaquake, queued[0],
            "the HDMA pass's rumble is queued first");
        AssertEqual<SoundEffectId>(EnemySound.Silence, queued[1],
            "the door's library-two cancel follows it");
        Console.WriteLine("Door entry room-FX sound: the entry frame's rumble precedes the door's cancel and DisableSounds.");
    }

    /// <summary>Runs the Rising Tide door entry sound ordering regression.</summary>
    [VerificationCommand("--door-entry-room-fx-sound")]
    internal static int DoorEntryRoomFxSoundCommand()
    {
        VerifyDoorEntryRoomFxSound();
        return 0;
    }
}
