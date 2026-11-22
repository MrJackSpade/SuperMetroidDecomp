using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Dragon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Dragon state each record owns: the body/wing selection.</summary>
internal static class DragonOwnedStateVerification
{
    /// <summary>
    /// Initializes a body and its wing and confirms parameter one selects the wing and the body starts waiting to rise.
    /// </summary>
    internal static void VerifyDragonOwnedState()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        // Both parts are set up as their room's population sets them up, the wing after the body that offers it.
        IReadOnlyList<RoomEnemyPlace> places = enemies.LoadPopulation(PopulationPlacementAccess.Entry(EnemyDefinitionId.Dragon, new DragonSpawnSettings(DragonPart.Body)), PopulationPlacementAccess.Entry(EnemyDefinitionId.Dragon, new DragonSpawnSettings(DragonPart.Wing)));
        RoomEnemyPlace body = places[0];
        RoomEnemyPlace wing = places[1];
        DragonEnemyState bodyState = enemies.DragonStates[0] ?? throw new InvalidOperationException("The Dragon body has no state.");
        AssertTrue(body.Entity is DragonEntity && wing.Entity is DragonWingEntity,
            "parameter one selects the wing's own entity, and its absence the body's");
        AssertEqual(DragonAction.WaitToRise, bodyState.Action, "the body starts waiting to rise");
        Console.WriteLine("Dragon owned state: the body/wing selection matches.");
    }

    /// <summary>Runs the focused Dragon owned-state check.</summary>
    [VerificationCommand("--dragon-owned-state")]
    internal static int DragonOwnedStateCommand()
    {
        VerifyDragonOwnedState();
        return 0;
    }
}
