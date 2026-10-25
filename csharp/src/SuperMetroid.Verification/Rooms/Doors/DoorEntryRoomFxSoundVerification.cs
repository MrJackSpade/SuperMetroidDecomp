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
            .Where(door => door.DestinationRoom == RoomHeaderDefinitions.RisingTide)
            .First(door =>
            {
                var candidate = LoadRisingTide(door, new SharedRandom());
                for (int frame = 0; frame < 4; frame++)
                    candidate.StepFrame(0);
                return candidate.RoomLayer3Fx.EarthquakeRequest.HasValue;
            });
        var random = new SharedRandom();
        var runtime = LoadRisingTide(entry, random);
        var samus = runtime.Player.IntroducedSamus;
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false, random: random);
        game.AdoptRuntime(runtime);
        game.Session.LastAudioRoomState = runtime.Room.LoadedRoom.State;
        game.GameState = SuperMetroidGameState.MainGameplay;

        SuperMetroid.Core.Runtime.SuperMetroidRuntime LoadRisingTide(DoorDefinition door, SharedRandom random)
        {
            var loaded = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true, random: random);
            loaded.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            loaded.RoomLoader.InitializeStartingCeresRoom();
            loaded.RoomLoader.InitializeCeresStartSamus();
            loaded.LoadCartridgeRoomThroughDoorForVerification(door);
            loaded.Player.IntroducedSamus.Control.LockHandlers();
            return loaded;
        }

        // Run real frames until the rising lava's rumble timer will fire on the next HDMA pass.
        StepUntil(() => runtime.RoomLayer3Fx.EarthquakeRequest.HasValue &&
                runtime.RoomLayer3Fx.EarthquakeSoundTimer.IsZero,
            _ => game.Step(0), maximumFrames: 600, context: "Rising Tide rumble timer reaches zero");

        var level = runtime.Room.LoadedLevelData;
        bool selected = false;
        for (int y = 0; y < level.Size.Y && !selected; y++)
        for (int x = 0; x < level.Size.X && !selected; x++)
        {
            var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            level.ResolveDoorCollision(block.Bts.Value, samus.Pose.ForRoomCollision(), true);
            selected = true;
        }
        AssertTrue(selected, "Rising Tide has an exit");
        var audio = game.AudioForVerification;
        byte start = audio.SoundQueueForVerification(1).Next;
        game.GameState = SuperMetroidGameState.HitDoorBlock;
        game.Step(0);

        AssertEqual(DoorTransitionPhase.WaitForSoundQueues, game.DoorTransitionPhaseForVerification,
            "the entry frame begins the door sound wait");
        AssertEqual<SoundEffectId?>(Library2Sound.Lavaquake, audio.SoundQueueEntryForVerification(1, start),
            "the HDMA pass's rumble is queued first");
        AssertEqual<SoundEffectId?>(Library2Sound.Silence, audio.SoundQueueEntryForVerification(1, (start + 1) & 0x0f),
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
