using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    // #1269: the door transition runs AnimatedTilesObject_Handler once at $82:E659 and on
    // every $82:E737 fade step. Entering the Tourian statue room from the left (as the 100%
    // movie does) the statue programs have therefore advanced by those frames when gameplay
    // resumes; skipping them left every statue unlock 18 frames late.
    /// <summary>Verifies door-transition handler calls advance Tourian statue tiles through destination construction and fade-in.</summary>
    private static void VerifyDoorAnimatedTiles()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        for (int area = 1; area <= 4; area++)
            runtime.System.SetBossBits(area, BossBits.AreaBoss);
        runtime.LoadCartridgeRoomForDebug(0xa5ed);
        var samus = runtime.Samus!;
        samus.InputLocked = true;

        // Select the source room's door into the statue room.
        var level = runtime.LevelData!;
        CartridgeDoorHeader? entry = null;
        for (int y = 0; y < level.HeightInBlocks && entry is null; y++)
        for (int x = 0; x < level.WidthInBlocks && entry is null; x++)
        {
            var block = level.GetCollisionBlock(x, y);
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            CartridgeDoorHeader door = level.ResolveDoorCollision(bus, block.Behavior, samus.Pose, false);
            if (door.DestinationRoomPointer == 0xa66a)
                entry = level.ResolveDoorCollision(bus, block.Behavior, samus.Pose, true);
        }
        AssertTrue(entry is not null, "the Tourian entrance has a door into the statue room");

        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(SuperMetroidGame).GetField("runtime", flags)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", flags)!.SetValue(game, runtime.ActiveRoom!.State.Pointer);
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.HitDoorBlock);
        int handlerFrames = 0;
        for (int frame = 0; frame < 2000 && game.GameState != SuperMetroidGameState.MainGameplay; frame++)
        {
            if (game.DoorTransitionPhaseForVerification is DoorTransitionPhase.HandleAnimatedTiles or
                DoorTransitionPhase.BuildDestinationOam or DoorTransitionPhase.FadeInDestinationPalette)
                handlerFrames++;
            game.Step(0);
        }
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "the door completes");
        AssertEqual((ushort)0xa66a, runtime.ActiveRoom!.Pointer, "the door enters the statue room");

        // A directly loaded statue room stepped by the same number of handler calls.
        var reference = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        reference.InitializeHud(HudSnapshot.CeresDebug);
        reference.InitializeStartingCeresRoom();
        reference.InitializeCeresStartSamus();
        for (int area = 1; area <= 4; area++)
            reference.System.SetBossBits(area, BossBits.AreaBoss);
        reference.LoadCartridgeRoomThroughDoorForVerification(entry!);
        for (int call = 0; call < handlerFrames; call++)
            reference.TourianStatues.StepTiles(reference);

        AssertTrue(handlerFrames > 1, "the door runs the handler at $E659 and on its fade steps");
        AssertEqual(StatueSnapshot(reference), StatueSnapshot(runtime),
            "the statues have advanced by every door-transition handler call");
        Console.WriteLine($"Door animated tiles: {handlerFrames} door-transition handler calls advance the statue programs.");

        static string StatueSnapshot(SuperMetroid.Core.Runtime.SuperMetroidRuntime source)
        {
            var objects = (System.Collections.IList)typeof(TourianStatueSequence)
                .GetField("objects", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(source.TourianStatues)!;
            return string.Join(" ", objects.Cast<object>().Select(o =>
            {
                Type t = o.GetType();
                return $"{t.GetField("Pointer")!.GetValue(o)}/{t.GetField("Timer")!.GetValue(o)}";
            }));
        }
    }
}
