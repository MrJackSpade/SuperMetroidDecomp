using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Rios;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Geruta state the species owns: the follower selection.</summary>
internal static class RioOwnedStateVerification
{
    /// <summary>
    /// Confirms a Geruta keeps its population's follower selection, and its follower follows its parent.
    /// </summary>
    internal static void VerifyRioOwnedState()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace parent = enemies.Places.InUpdateOrder[0];
        parent.BindPlaced(EnemyDefinitionId.Geruta, new EnemyFollowerSpawnSettings(EnemyFollowerRole.Leader));
        parent.SetUpAsPlaced();
        RoomEnemyPlace follower = enemies.Places.InUpdateOrder[1];
        follower.SetSpawnParameter1(EnemyDefinitionId.Geruta, EnemyParameterMasks.FollowerBit);
        follower.Bind(EnemyDefinitionId.Geruta, follower.KeptDefinition);
        follower.SetUpAsPlaced();
        // The placement builds the part's own entity (#2164): the parent a Rio, the follower its flames.
        AssertTrue(parent.Entity is GerutaEntity && follower.Entity is GerutaFollowerEntity,
            "parameter one's sign bit selects the follower, which is built as a follower");
        Console.WriteLine("Rio owned state: the follower selection matches.");
    }

    /// <summary>Runs the focused Rio owned-state check.</summary>
    [VerificationCommand("--rio-owned-state")]
    internal static int RioOwnedStateCommand()
    {
        VerifyRioOwnedState();
        return 0;
    }
}
