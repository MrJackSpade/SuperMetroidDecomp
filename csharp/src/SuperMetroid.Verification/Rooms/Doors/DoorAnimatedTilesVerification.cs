using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Rooms.Doors;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Checks Tourian statue tile animation during a real door transition.</summary>
internal static class DoorAnimatedTilesVerification
{
    /// <summary>
    /// Confirms the statue programs advance at $82:E659 and on every $82:E737 fade step
    /// when entering from the left; omitted calls left each statue unlock 18 frames late (#1269).
    /// </summary>
    internal static void VerifyDoorAnimatedTiles()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var random = new CartridgeRandom();
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true, random: random);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        for (int area = 1; area <= 4; area++)
            runtime.System.SetBossBits(area, BossBits.AreaBoss);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.StatuesHallway);
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.LockHandlers();

        // Select the source room's door into the statue room.
        var level = runtime.Room.LoadedLevelData;
        DoorDefinition? entry = null;
        for (int y = 0; y < level.Size.Y && entry is null; y++)
        for (int x = 0; x < level.Size.X && entry is null; x++)
        {
            var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            DoorDefinition? door = level.ResolveDoorCollision(block.Bts.Value, samus.Pose.ForRoomCollision(), false).Door;
            if (door?.DestinationRoom == RoomHeaderDefinitions.Statues)
                entry = level.ResolveDoorCollision(block.Bts.Value, samus.Pose.ForRoomCollision(), true).Door;
        }
        AssertTrue(entry is not null, "the Tourian entrance has a door into the statue room");

        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false, random: random);
        game.AdoptRuntime(runtime);
        game.Session.LastAudioRoomState = runtime.Room.LoadedRoom.State;
        game.GameState = SuperMetroidGameState.HitDoorBlock;
        int handlerFrames = 0;
        for (int frame = 0; frame < 2000 && game.GameState != SuperMetroidGameState.MainGameplay; frame++)
        {
            if (game.DoorTransitionPhaseForVerification is DoorTransitionPhase.HandleAnimatedTiles or
                DoorTransitionPhase.BuildDestinationOam or DoorTransitionPhase.FadeInDestinationPalette)
                handlerFrames++;
            game.Step(0);
        }
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "the door completes");
        AssertEqual(RoomHeaderDefinitions.Statues, runtime.Room.LoadedRoom.Definition, "the door enters the statue room");

        // A directly loaded statue room stepped by the same number of handler calls.
        var reference = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        reference.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        reference.RoomLoader.InitializeStartingCeresRoom();
        reference.RoomLoader.InitializeCeresStartSamus();
        for (int area = 1; area <= 4; area++)
            reference.System.SetBossBits(area, BossBits.AreaBoss);
        reference.LoadCartridgeRoomThroughDoorForVerification((entry ?? throw new InvalidOperationException("Verification requires entry.")));
        for (int call = 0; call < handlerFrames; call++)
            reference.TourianStatues.StepTiles();

        AssertTrue(handlerFrames > 1, "the door runs the handler at $E659 and on its fade steps");
        AssertEqual(StatueSnapshot(reference), StatueSnapshot(runtime),
            "the statues have advanced by every door-transition handler call");
        Console.WriteLine($"Door animated tiles: {handlerFrames} door-transition handler calls advance the statue programs.");

        static string StatueSnapshot(SuperMetroid.Core.Runtime.SuperMetroidRuntime source)
        {
            // Each statue's typed program step (program and position) and its frame timer.
            return string.Join(" ", source.TourianStatues.objects.Select(o => $"{o.Cursor}/{o.Timer.RawValue}"));
        }
    }

    /// <summary>Runs the statue door-transition animation check against the retail room.</summary>
    [VerificationCommand("--door-animated-tiles")]
    internal static int DoorAnimatedTilesCommand()
    {
        VerifyDoorAnimatedTiles();
        return 0;
    }
}
