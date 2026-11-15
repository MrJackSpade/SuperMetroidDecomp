using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.FaceBlocks;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;

/// <summary>Checks the face block state the species owns: the distance it records.</summary>
internal static class BlueBrinstarFaceBlockOwnedStateVerification
{
    /// <summary>
    /// Runs one main-AI update with Samus level with the block and 32 pixels to its left and confirms the recorded
    /// distance.
    /// </summary>
    internal static void VerifyBlueBrinstarFaceBlockOwnedState()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.FaceBlock, place.Definition);
        place.Bind(place.DefinitionId, default(RoomEnemyDefinition));
        place.SetSpawnParameter1(EnemyDefinitionId.FaceBlock, 64);
        place.SetSpawnParameter2(EnemyDefinitionId.FaceBlock, 0);
        place.Position = place.Position.WithWhole(new(256, 256));
        enemies.BlueBrinstarFaceBlockBehavior.InitializeBlueBrinstarFaceBlock(place, RoomEnemyFrameFixtures.StandaloneSamus());
        BlueBrinstarFaceBlockEnemyState state = enemies.BlueBrinstarFaceBlockStates[0]
            ?? throw new InvalidOperationException("The face block has no state.");
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), new SuperMetroid.Core.Samus.Artwork.SamusArtworkBinding())
        {
            CollectedItems = SamusEquipmentFlags.MorphBall,
            Position = FixedPosition<RoomPixels>.FromWhole(new(224, 256)),
        };
        enemies.BlueBrinstarFaceBlockBehavior.RunBlueBrinstarFaceBlockMain(place, state, samus);
        AssertEqual(!state.FacesRight, state.Activated, "the face block wakes for Samus 32 pixels left only when it faces left");
        Console.WriteLine("Face block owned state: the block wakes for the side it faces.");
    }

    /// <summary>Runs the focused face block owned-state check.</summary>
    [VerificationCommand("--face-block-owned-state")]
    internal static int FaceBlockOwnedStateCommand()
    {
        VerifyBlueBrinstarFaceBlockOwnedState();
        return 0;
    }
}
