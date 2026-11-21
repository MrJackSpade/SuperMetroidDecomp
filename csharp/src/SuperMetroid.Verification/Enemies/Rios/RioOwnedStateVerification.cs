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
        IReadOnlyList<RoomEnemyPlace> places = enemies.LoadPopulation(PopulationPlacementAccess.Entry(EnemyDefinitionId.Geruta, new EnemyFollowerSpawnSettings(EnemyFollowerRole.Leader)), PopulationPlacementAccess.Entry(EnemyDefinitionId.Geruta, new EnemyFollowerSpawnSettings(EnemyFollowerRole.Follower)));
        RoomEnemyPlace parent = places[0];
        RoomEnemyPlace follower = places[1];
        // The placement builds the part's own entity (#2164): the parent a Rio, the follower its flames.
        AssertTrue(parent.EntityState<GerutaEntity, GerutaEnemyState>() is not null &&
            parent.SpeciesState<GerutaFollowerState>() is null &&
            follower.EntityState<GerutaFollowerEntity, GerutaFollowerState>() is not null &&
            follower.SpeciesState<GerutaEnemyState>() is null,
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
