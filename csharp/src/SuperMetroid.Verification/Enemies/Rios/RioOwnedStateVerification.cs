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
        parent.SetSpawnSettings(new EnemyFollowerSpawnSettings(EnemyFollowerRole.Leader));
        parent.Bind(EnemyDefinitionId.Geruta, parent.KeptDefinition);
        GerutaBehavior.InitializeGeruta();
        RoomEnemyPlace follower = enemies.Places.InUpdateOrder[1];
        follower.SetSpawnParameter1(EnemyDefinitionId.Geruta, EnemyParameterMasks.FollowerBit);
        follower.Bind(EnemyDefinitionId.Geruta, follower.KeptDefinition);
        enemies.GerutaBehavior.InitializeGerutaFollower(follower);
        // The placement builds the part's own entity (#2164): the parent a Rio, the follower its flames.
        AssertTrue(parent.EntityState<GerutaEntity, GerutaEnemyState>() is not null &&
            SuperMetroid.Core.Enemies.EnemyPartners.Listed<GerutaFollowerEntity>(parent).State is null &&
            follower.EntityState<GerutaFollowerEntity, GerutaFollowerState>() is not null &&
            SuperMetroid.Core.Enemies.EnemyPartners.Listed<GerutaEntity>(follower).State is null,
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
