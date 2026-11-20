using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.FaceBlocks;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;

/// <summary>Checks the side the face block's main AI finds Samus on once she is in vertical range.</summary>
internal static class BlueBrinstarFaceBlockHorizontalDistanceVerification
{
    /// <summary>Room X of the face block in the ordinary cases.</summary>
    private const int BlockX = 256;

    /// <summary>Room Y shared by the face block and Samus, so the vertical range test always passes.</summary>
    private const int RowY = 256;

    /// <summary>
    /// Main AI finds Samus's side from her X minus the block's X: to the left and to the right. Samus never lies left of
    /// the room's origin (#2430), so the distance is a plain one. A block wakes only for Samus on the side it faces, so
    /// each side is seen by whether the block wakes.
    /// </summary>
    internal static void VerifyBlueBrinstarFaceBlockHorizontalDistance()
    {
        (bool leftWoke, bool facesRight) = Wakes(BlockX, BlockX - 32);
        AssertEqual(!facesRight, leftWoke, "face block finds Samus 32 pixels left on its left");
        (bool rightWoke, _) = Wakes(BlockX, BlockX + 32);
        AssertEqual(facesRight, rightWoke, "face block finds Samus 32 pixels right on its right");
        Console.WriteLine("  Face block horizontal distance: left and right sides pass.");
    }

    /// <summary>
    /// Runs one main-AI update with Morph Ball collected and Samus level with the block at <paramref name="samusX"/>; returns
    /// whether the block woke, and which way it faces.
    /// </summary>
    private static (bool Woke, bool FacesRight) Wakes(int blockX, int samusX)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.FaceBlock, place.KeptDefinition);
        place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        place.SetSpawnParameter1(EnemyDefinitionId.FaceBlock, 64);
        place.SetSpawnParameter2(EnemyDefinitionId.FaceBlock, 0);
        place.Position = place.Position.WithWhole(new(blockX, RowY));
        enemies.BlueBrinstarFaceBlockBehavior.InitializeBlueBrinstarFaceBlock(place, RoomEnemyFrameFixtures.StandaloneSamus());
        BlueBrinstarFaceBlockEnemyState state = enemies.BlueBrinstarFaceBlockStates[0]
            ?? throw new InvalidOperationException("The face block has no state.");
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            CollectedItems = SamusEquipmentFlags.MorphBall,
            Position = FixedPosition<RoomPixels>.FromWhole(new(samusX, RowY)),
        };
        enemies.BlueBrinstarFaceBlockBehavior.RunBlueBrinstarFaceBlockMain(place.HeldOccupant(), place, state, samus);
        return (state.Activated, place.HeldOccupant().SpawnSettings<FaceBlockSpawnSettings>().Facing == HorizontalDirection.Right);
    }

    /// <summary>Runs the face-block horizontal distance check through its dedicated command-line switch.</summary>
    [VerificationCommand("--face-block-horizontal-distance")]
    internal static int FaceBlockHorizontalDistanceCommand()
    {
        Suite(nameof(VerifyBlueBrinstarFaceBlockHorizontalDistance), () => VerifyBlueBrinstarFaceBlockHorizontalDistance());
        return 0;
    }
}
