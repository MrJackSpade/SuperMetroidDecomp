using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailGameFixtureVerification;
using static RetailRuntimeFixtureVerification;

internal static class DoorAnimatedTilesVerification
{
    // #1269: the door transition runs AnimatedTilesObject_Handler once at $82:E659 and on
    // every $82:E737 fade step. Entering the Tourian statue room from the left (as the 100%
    // movie does) the statue programs have therefore advanced by those frames when gameplay
    // resumes; skipping them left every statue unlock 18 frames late.
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
        var samus = runtime.Player.Samus!;
        samus.InputLocked = true;

        // Select the source room's door into the statue room.
        var level = runtime.Room.LevelData!;
        CartridgeDoorHeader? entry = null;
        for (int y = 0; y < level.Size.Y && entry is null; y++)
        for (int x = 0; x < level.Size.X && entry is null; x++)
        {
            var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            CartridgeDoorHeader? door = level.ResolveDoorCollision(bus, block.Behavior, samus.Pose, false).Door;
            if (door?.DestinationRoomPointer == 0xa66a)
                entry = level.ResolveDoorCollision(bus, block.Behavior, samus.Pose, true).Door;
        }
        AssertTrue(entry is not null, "the Tourian entrance has a door into the statue room");

        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        game.runtime = runtime;
        game.lastAudioRoomStatePointer = runtime.Room.ActiveRoom!.State.Pointer;
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
        AssertEqual((ushort)0xa66a, runtime.Room.ActiveRoom!.Pointer, "the door enters the statue room");

        // A directly loaded statue room stepped by the same number of handler calls.
        var reference = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        reference.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        reference.RoomLoader.InitializeStartingCeresRoom();
        reference.RoomLoader.InitializeCeresStartSamus();
        for (int area = 1; area <= 4; area++)
            reference.System.SetBossBits(area, BossBits.AreaBoss);
        reference.LoadCartridgeRoomThroughDoorForVerification(entry!);
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

    [VerificationCommand("--door-animated-tiles")]
    internal static int DoorAnimatedTilesCommand()
    {
        VerifyDoorAnimatedTiles();
        return 0;
    }
}
