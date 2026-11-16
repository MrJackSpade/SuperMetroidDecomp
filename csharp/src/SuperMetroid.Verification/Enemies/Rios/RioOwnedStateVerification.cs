using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Rios;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Norfair Rio state the species owns: the follower selection.</summary>
internal static class RioOwnedStateVerification
{
    /// <summary>
    /// Confirms a Norfair Rio keeps its population's follower selection, and its follower follows its parent.
    /// </summary>
    internal static void VerifyRioOwnedState()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace parent = enemies.Places.InUpdateOrder[0];
        parent.Bind(EnemyDefinitionId.Geruta, parent.Definition);
        parent.SetSpawnSettings(new EnemyFollowerSpawnSettings(EnemyFollowerRole.Leader));
        enemies.NorfairRioBehavior.InitializeNorfairRio(parent);
        RoomEnemyPlace follower = enemies.Places.InUpdateOrder[1];
        follower.Bind(EnemyDefinitionId.Geruta, follower.Definition);
        follower.SetSpawnParameter1(EnemyDefinitionId.Geruta, EnemyParameterMasks.FollowerBit);
        enemies.NorfairRioBehavior.InitializeNorfairRio(follower);
        AssertTrue(parent.EntityState<NorfairRioEntity, NorfairRioEnemyState>() is not null &&
            parent.EntityState<NorfairRioEntity, NorfairRioFollowerState>() is null &&
            follower.EntityState<NorfairRioEntity, NorfairRioFollowerState>() is not null &&
            follower.EntityState<NorfairRioEntity, NorfairRioEnemyState>() is null,
            "parameter one's sign bit selects the follower, which keeps only its own follower state");
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
