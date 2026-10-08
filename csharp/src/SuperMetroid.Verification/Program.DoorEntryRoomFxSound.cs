using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    // #1269: MainGameLoop runs the HDMA pass ($88:84B9) before game state $09's handler, so a
    // rising-lava rumble ($88:B21D) on the door-entry frame is queued before the handler's
    // library-two cancel ($71) and DisableSounds. In the 100% movie's Rising Tide exit that
    // extra rumble lengthens the door's sound-queue wait by one drained sound.
    private static void VerifyDoorEntryRoomFxSound()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        FieldInfo timer = typeof(RoomLayer3FxState).GetField("earthquakeSoundTimer", flags)!;
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
        var samus = runtime.Samus!;
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        typeof(SuperMetroidGame).GetField("runtime", flags)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", flags)!.SetValue(game, runtime.ActiveRoom!.State.Pointer);
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.MainGameplay);

        SuperMetroid.Core.Runtime.SuperMetroidRuntime LoadRisingTide(CartridgeDoorHeader door)
        {
            var loaded = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
            loaded.InitializeHud(HudSnapshot.CeresDebug);
            loaded.InitializeStartingCeresRoom();
            loaded.InitializeCeresStartSamus();
            loaded.LoadCartridgeRoomThroughDoorForVerification(door);
            loaded.Samus!.InputLocked = true;
            return loaded;
        }

        // Run real frames until the rising lava's rumble timer will fire on the next HDMA pass.
        StepUntil(() => runtime.RoomLayer3Fx.EarthquakeRequest.HasValue &&
                (ushort)timer.GetValue(runtime.RoomLayer3Fx)! == 0,
            _ => game.Step(0), maximumFrames: 600, context: "Rising Tide rumble timer reaches zero");

        var level = runtime.LevelData!;
        bool selected = false;
        for (int y = 0; y < level.HeightInBlocks && !selected; y++)
        for (int x = 0; x < level.WidthInBlocks && !selected; x++)
        {
            var block = level.GetCollisionBlock(x, y);
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            level.ResolveDoorCollision(bus, block.Behavior, samus.Pose, true);
            selected = true;
        }
        AssertTrue(selected, "Rising Tide has an exit");
        var audio = game.AudioForVerification;
        byte start = audio.SoundQueueForVerification(1).Next;
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.HitDoorBlock);
        game.Step(0);

        AssertEqual(DoorTransitionPhase.WaitForSoundQueues, game.DoorTransitionPhaseForVerification,
            "the entry frame begins the door sound wait");
        AssertEqual((byte)0x46, audio.SoundQueueEntryForVerification(1, start),
            "the HDMA pass's rumble is queued first");
        AssertEqual((byte)0x71, audio.SoundQueueEntryForVerification(1, (start + 1) & 0x0f),
            "the door's library-two cancel follows it");
        Console.WriteLine("Door entry room-FX sound: the entry frame's rumble precedes the door's cancel and DisableSounds.");
    }
}
