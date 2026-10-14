using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using static RetailGameFixtureVerification;
using static RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;

/// <summary>Reproduces special Rinka placement during the door scroll into Mother Brain's room.</summary>
internal static class MotherBrainRinkaDoorSpawnVerification
{
    /// <summary>
    /// Confirms $A2:B69B chooses spawn points against layer 1 during door scrolling.
    /// At camera X $38C, only the $3E7 points are visible and the third Rinka
    /// falls back to the first free point; the finished X $300 view differs.
    /// </summary>
    internal static void VerifyMotherBrainRinkaDoorSpawn()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.RinkaShaft);
        var samus = runtime.Player.IntroducedSamus;
        samus.PoseId = SamusPoseId.FacingLeftNormalPose;
        samus.Position = samus.Position.WithWhole(new(0x14, 0x028b));
        samus.InputLocked = true;
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        game.runtime = runtime;
        game.lastAudioRoomStatePointer = runtime.Room.LoadedRoom.State.Pointer;
        game.DoorLoaderProgress = new CameraReachedLoaderProgress(runtime, 0x038c);

        var level = runtime.Room.LoadedLevelData;
        bool selected = false;
        for (int y = 0; y < level.Size.Y && !selected; y++)
        for (int x = 0; x < level.Size.X && !selected; x++)
        {
            var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock ||
                level.ResolveDoorCollision(bus, block.Behavior, samus.Pose, publishDoorSideEffects: false)
                    .Door?.DestinationRoomPointer != FixtureRoomHeaders.MotherBrain)
                continue;
            level.ResolveDoorCollision(bus, block.Behavior, samus.Pose, publishDoorSideEffects: true);
            selected = true;
        }
        AssertTrue(selected, "the shaft has a door into Mother Brain's room");
        game.GameState = SuperMetroidGameState.HitDoorBlock;
        for (int guard = 0; game.GameState != SuperMetroidGameState.MainGameplay && guard < 600; guard++)
            game.Step(0);
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "the door transition completes");
        AssertEqual(FixtureRoomHeaders.MotherBrain, runtime.Room.LoadedRoom.Pointer, "Samus is in Mother Brain's room");

        RoomEnemySlot[] rinkas = runtime.Enemies.SlotTable.Slots
            .Where(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Rinka && slot.Parameter1 != 0)
            .OrderBy(slot => slot.SlotIndex)
            .ToArray();
        AssertEqual(3, rinkas.Length, "Mother Brain's room has three special Rinkas");
        (ushort X, ushort Y)[] expected = [(0x03e7, 0x0026), (0x03e7, 0x00a6), (0x0337, 0x0036)];
        for (int index = 0; index < rinkas.Length; index++)
        {
            AssertEqual(expected[index].X, rinkas[index].Position.X.Whole, $"Rinka {index} X follows the loader's camera");
            AssertEqual(expected[index].Y, rinkas[index].Position.Y.Whole, $"Rinka {index} Y follows the loader's camera");
        }
        Console.WriteLine("Mother Brain Rinka door spawn: spawn points are chosen against the loader's scrolling camera.");
    }

    /// <summary>Reports every enemy initialized once the door scroll has reached a layer-1 X.</summary>
    internal sealed class CameraReachedLoaderProgress(SuperMetroidRuntime runtime, ushort cameraX) : IDoorLoaderProgressSource
    {
        /// <summary>Allows enemy initialization once scrolling reaches the recorded layer-1 X.</summary>
        public bool HasInitializedEnemySlot(int slot) => runtime.Room.LoadedCamera.PixelPosition.X <= cameraX;
    }

    /// <summary>Runs the focused scrolling-camera Rinka spawn check.</summary>
    [VerificationCommand("--mother-brain-rinka-door-spawn")]
    internal static int MotherBrainRinkaDoorSpawnCommand()
    {
        VerifyMotherBrainRinkaDoorSpawn();
        return 0;
    }
}
