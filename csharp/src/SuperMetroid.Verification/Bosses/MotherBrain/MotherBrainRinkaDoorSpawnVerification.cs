using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;

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
        var random = new SharedRandom();
        var progress = new CameraReachedLoaderProgress(0x038c);
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), renderGameplayFrames: false,
            hostServices: SuperMetroidGameHostServices.Default with { RoomLoadProgress = progress }, random: random);
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true, random: random, audio: game.AudioForVerification);
        progress.Runtime = runtime;
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.RinkaShaft);
        var samus = runtime.Player.IntroducedSamus;
        samus.PoseId = SamusPoseId.FacingLeftNormalPose;
        samus.Position = samus.Position.WithWhole(new(0x14, 0x028b));
        samus.Control.LockOutPlayer();
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        game.AdoptRuntime(runtime);
        game.Session.LastAudioRoomState = runtime.Room.LoadedRoom.State;

        var level = runtime.Room.LoadedLevelData;
        bool selected = false;
        for (int y = 0; y < level.Size.Y && !selected; y++)
        for (int x = 0; x < level.Size.X && !selected; x++)
        {
            var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock ||
                level.DoorExitAt(block)
                    .Door?.DestinationRoom != RoomDefinitions.MotherBrain)
                continue;
            runtime.MeetDoorBlock(block, samus.Pose.ForRoomCollision());
            selected = true;
        }
        AssertTrue(selected, "the shaft has a door into Mother Brain's room");
        game.Session.Enter(DoorTransitionStep.EnteringDoor);
        for (int guard = 0; game.GameState != SuperMetroidGameState.MainGameplay && guard < 600; guard++)
            game.Step(0);
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "the door transition completes");
        AssertEqual(RoomDefinitions.MotherBrain, runtime.Room.LoadedRoom.Definition, "Samus is in Mother Brain's room");

        RoomEnemyPlace[] rinkas = runtime.Enemies.Places.InUpdateOrder
            .Where(place => place.DefinitionId == EnemyDefinitionId.Rinka && place.SpawnSettings<RinkaSpawnSettings>().SharedSpawnPoint is not null)
            .OrderBy(place => place.Ordinal)
            .ToArray();
        AssertEqual(3, rinkas.Length, "Mother Brain's room has three special Rinkas");
        MotherBrainRinkaDoorSpawnExpected[] expected = [new MotherBrainRinkaDoorSpawnExpected(0x03e7, 0x0026), new MotherBrainRinkaDoorSpawnExpected(0x03e7, 0x00a6), new MotherBrainRinkaDoorSpawnExpected(0x0337, 0x0036)];
        Position16<RoomPixels>[] populated = [new(823, 54), new(823, 166), new(631, 28)];
        for (int index = 0; index < rinkas.Length; index++)
        {
            AssertEqual(expected[index].X, rinkas[index].Position.X.Whole, $"Rinka {index} X follows the loader's camera");
            AssertEqual(expected[index].Y, rinkas[index].Position.Y.Whole, $"Rinka {index} Y follows the loader's camera");
            // The point a Rinka moves to becomes the spawn point it respawns at; its population entry is never rewritten (#2256).
            RinkaSpawnPoint point = rinkas[index].SpeciesState<RinkaEnemyState>()?.SpawnPoint
                ?? throw new InvalidOperationException($"Special Rinka {index} holds no spawn point.");
            AssertEqual(rinkas[index].Position.Whole, RinkaSpawnReservations.Centre(point), $"Rinka {index} respawns at the point it moved to");
            AssertEqual(populated[index], rinkas[index].Spawn.Population.Position, $"Rinka {index}'s population entry keeps its placement");
        }
        Console.WriteLine("Mother Brain Rinka door spawn: spawn points are chosen against the loader's scrolling camera.");
    }

    /// <summary>Reports every enemy initialized once the door scroll has reached a layer-1 X.</summary>
    internal sealed class CameraReachedLoaderProgress(ushort cameraX) : IRoomLoadProgress
    {
        /// <summary>The runtime whose camera the progress follows, built over the game that reports it.</summary>
        internal SuperMetroidRuntime? Runtime { get; set; }

        /// <summary>Allows enemy initialization once scrolling reaches the recorded layer-1 X.</summary>
        /// <exception cref="InvalidOperationException">No runtime has been given.</exception>
        public bool HasInitializedEnemyPlace(int slot) =>
            (Runtime ?? throw new InvalidOperationException("The loader progress follows no runtime.")).Room.LoadedCamera.PixelPosition.X <= cameraX;
    }

    /// <summary>Runs the focused scrolling-camera Rinka spawn check.</summary>
    [VerificationCommand("--mother-brain-rinka-door-spawn")]
    internal static int MotherBrainRinkaDoorSpawnCommand()
    {
        VerifyMotherBrainRinkaDoorSpawn();
        return 0;
    }
}
