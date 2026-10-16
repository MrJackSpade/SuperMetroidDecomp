using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Rooms.Doors;
using SuperMetroid.Verification.Harness;

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
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        for (int area = 1; area <= 4; area++)
            runtime.System.SetBossBits(area, BossBits.AreaBoss);
        runtime.LoadCartridgeRoomForDebug(0xa5ed);
        var samus = runtime.Player.IntroducedSamus;
        samus.InputLocked = true;

        // Select the source room's door into the statue room.
        var level = runtime.Room.LoadedLevelData;
        CartridgeDoorHeader? entry = null;
        for (int y = 0; y < level.Size.Y && entry is null; y++)
        for (int x = 0; x < level.Size.X && entry is null; x++)
        {
            var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            CartridgeDoorHeader? door = level.ResolveDoorCollision(block.Behavior, samus.Pose, false).Door;
            if (door?.DestinationRoomPointer == 0xa66a)
                entry = level.ResolveDoorCollision(block.Behavior, samus.Pose, true).Door;
        }
        AssertTrue(entry is not null, "the Tourian entrance has a door into the statue room");

        var game = CreateRetailGameFixture(bus, new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        game.Session.runtime = runtime;
        game.Session.lastAudioRoomStatePointer = runtime.Room.LoadedRoom.State.Pointer;
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
        AssertEqual((ushort)0xa66a, runtime.Room.LoadedRoom.Pointer, "the door enters the statue room");

        // A directly loaded statue room stepped by the same number of handler calls.
        var reference = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
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
            return string.Join(" ", source.TourianStatues.objects.Select(o => $"{o.Pointer}/{o.Timer.RawValue}"));
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
