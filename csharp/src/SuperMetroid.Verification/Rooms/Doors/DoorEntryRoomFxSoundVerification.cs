using System.Reflection;
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
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        // Rising Tide's lava rises only under the FX record of a door that requests it.
        CartridgeDoorHeader entry = RetailDoorHeaderCatalog.EnumeratePointers()
            .Select(pointer => CartridgeDoorHeaderImporter.Load(bus, pointer))
            .Where(door => door.DestinationRoomPointer == FixtureRoomHeaders.RisingTide)
            .First(door =>
            {
                var candidate = LoadRisingTide(door);
                for (int frame = 0; frame < 4; frame++)
                    candidate.StepFrame(0);
                return candidate.RoomLayer3Fx.EarthquakeRequest.HasValue;
            });
        var runtime = LoadRisingTide(entry);
        var samus = runtime.Player.IntroducedSamus;
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        game.runtime = runtime;
        game.lastAudioRoomStatePointer = runtime.Room.LoadedRoom.State.Pointer;
        game.GameState = SuperMetroidGameState.MainGameplay;

        SuperMetroid.Core.Runtime.SuperMetroidRuntime LoadRisingTide(CartridgeDoorHeader door)
        {
            var loaded = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
            loaded.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            loaded.RoomLoader.InitializeStartingCeresRoom();
            loaded.RoomLoader.InitializeCeresStartSamus();
            loaded.LoadCartridgeRoomThroughDoorForVerification(door);
            loaded.Player.IntroducedSamus.InputLocked = true;
            return loaded;
        }

        // Run real frames until the rising lava's rumble timer will fire on the next HDMA pass.
        StepUntil(() => runtime.RoomLayer3Fx.EarthquakeRequest.HasValue &&
                runtime.RoomLayer3Fx.earthquakeSoundTimer.IsZero,
            _ => game.Step(0), maximumFrames: 600, context: "Rising Tide rumble timer reaches zero");

        var level = runtime.Room.LoadedLevelData;
        bool selected = false;
        for (int y = 0; y < level.Size.Y && !selected; y++)
        for (int x = 0; x < level.Size.X && !selected; x++)
        {
            var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            level.ResolveDoorCollision(bus, block.Behavior, samus.Pose, true);
            selected = true;
        }
        AssertTrue(selected, "Rising Tide has an exit");
        var audio = game.AudioForVerification;
        byte start = audio.SoundQueueForVerification(1).Next;
        game.GameState = SuperMetroidGameState.HitDoorBlock;
        game.Step(0);

        AssertEqual(DoorTransitionPhase.WaitForSoundQueues, game.DoorTransitionPhaseForVerification,
            "the entry frame begins the door sound wait");
        AssertEqual((byte)0x46, audio.SoundQueueEntryForVerification(1, start),
            "the HDMA pass's rumble is queued first");
        AssertEqual((byte)0x71, audio.SoundQueueEntryForVerification(1, (start + 1) & 0x0f),
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
