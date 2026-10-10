using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    // #1269: Mother Brain's room Rinkas choose spawn points against layer 1 ($A2:B69B). The
    // door loader initializes them while the door IRQ is still scrolling; in the 100% movie
    // it reaches them with layer 1 at X $38C, where only the $3E7 points are on screen, so
    // the third Rinka falls back to the first free point. The port chose against the
    // finished camera ($300), where every point is on screen.
    /// <summary>Replays the shaft-to-Mother-Brain door transition with the loader camera held at the reported scroll position and checks all three Rinka spawn points.</summary>
    private static void VerifyMotherBrainRinkaDoorSpawn()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.RinkaShaft);
        var samus = runtime.Samus!;
        samus.PoseId = SamusPoseId.FacingLeftNormalPose;
        samus.XPosition = 0x14; samus.YPosition = 0x028b;
        samus.InputLocked = true;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(SuperMetroidGame).GetField("runtime", flags)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", flags)!.SetValue(game, runtime.ActiveRoom!.State.Pointer);
        game.DoorLoaderProgress = new CameraReachedLoaderProgress(runtime, 0x038c);

        var level = runtime.LevelData!;
        bool selected = false;
        for (int y = 0; y < level.HeightInBlocks && !selected; y++)
        for (int x = 0; x < level.WidthInBlocks && !selected; x++)
        {
            var block = level.GetCollisionBlock(x, y);
            if (block.CollisionType != RoomCollisionType.DoorBlock ||
                level.ResolveDoorCollision(bus, block.Behavior, samus.Pose, publishDoorSideEffects: false)
                    .DestinationRoomPointer != FixtureRoomHeaders.MotherBrain)
                continue;
            level.ResolveDoorCollision(bus, block.Behavior, samus.Pose, publishDoorSideEffects: true);
            selected = true;
        }
        AssertTrue(selected, "the shaft has a door into Mother Brain's room");
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.HitDoorBlock);
        for (int guard = 0; game.GameState != SuperMetroidGameState.MainGameplay && guard < 600; guard++)
            game.Step(0);
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "the door transition completes");
        AssertEqual(FixtureRoomHeaders.MotherBrain, runtime.ActiveRoom!.Pointer, "Samus is in Mother Brain's room");

        RoomEnemySlot[] rinkas = runtime.Enemies.Slots
            .Where(slot => slot.EnemyDefinitionPointer == RoomEnemySystem.RinkaDefinition && slot.Parameter1 != 0)
            .OrderBy(slot => slot.SlotIndex)
            .ToArray();
        AssertEqual(3, rinkas.Length, "Mother Brain's room has three special Rinkas");
        (ushort X, ushort Y)[] expected = [(0x03e7, 0x0026), (0x03e7, 0x00a6), (0x0337, 0x0036)];
        for (int index = 0; index < rinkas.Length; index++)
        {
            AssertEqual(expected[index].X, rinkas[index].XPosition, $"Rinka {index} X follows the loader's camera");
            AssertEqual(expected[index].Y, rinkas[index].YPosition, $"Rinka {index} Y follows the loader's camera");
        }
        Console.WriteLine("Mother Brain Rinka door spawn: spawn points are chosen against the loader's scrolling camera.");
    }

    /// <summary>Reports every enemy initialized once the door scroll has reached a layer-1 X.</summary>
    private sealed class CameraReachedLoaderProgress(SuperMetroidRuntime runtime, ushort cameraX) : IDoorLoaderProgressSource
    {
        /// <summary>Signals that enemy initialization may proceed after the runtime camera reaches the configured X coordinate.</summary>
        /// <param name="slot">Enemy slot being checked; this fixture applies the same camera threshold to each slot.</param>
        public bool HasInitializedEnemySlot(int slot) => runtime.Camera!.XPosition <= cameraX;
    }
}
