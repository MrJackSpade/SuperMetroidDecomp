using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies;

using SuperMetroid.Verification.Harness;

/// <summary>Checks that an enemy's species state lives exactly as long as the occupant it was installed for (#1543).</summary>
internal static class EnemyOccupantStatesVerification
{
    /// <summary>
    /// Confirms a kill, a deletion and a new occupant each leave the place without the previous occupant's state, that
    /// a read of the missing state fails, and that the struck occupant's own set still holds what it installed.
    /// </summary>
    internal static void VerifyEnemyOccupantStates()
    {
        // Each enemy's setup builds its entity, which holds the state its species keeps (#2554): the room's population load
        // sets up a Dachora runner, whose entry stays the place's through its kill or deletion (#2283).
        RoomEnemyPlace place = NewDachora();
        DachoraEnemyState killed = place.RequireSpeciesState<DachoraEnemyState>();
        IEnemyEntity struck = place.Entity ?? throw new InvalidOperationException("The set-up Dachora has no entity.");
        place.Clear();
        AssertEqual<DachoraEnemyState?>(null, place.SpeciesState<DachoraEnemyState>(), "a kill takes the occupant's state with it");
        AssertThrows<InvalidOperationException>(() => place.RequireSpeciesState<DachoraEnemyState>(),
            "a read of a killed occupant's state fails");
        AssertTrue(ReferenceEquals(killed, EnemyEntityStates.Of<DachoraEnemyState>(struck)), "the struck enemy's entity still holds its state");

        place = NewDachora();
        place.Unbind();
        AssertEqual<DachoraEnemyState?>(null, place.SpeciesState<DachoraEnemyState>(), "a deletion takes the occupant's state with it");
        Console.WriteLine("Enemy occupant states: a kill and a deletion each end the occupant's state.");

        static RoomEnemyPlace NewDachora() =>
            EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance)
                .LoadPopulation(PopulationPlacementAccess.Entry(EnemyDefinitionId.Dachora, new DachoraSpawnSettings(HorizontalDirection.Right, DachoraRole.Runner)))[0];
    }

    /// <summary>Runs the focused enemy occupant-state check.</summary>
    [VerificationCommand("--enemy-occupant-states")]
    internal static int EnemyOccupantStatesCommand()
    {
        VerifyEnemyOccupantStates();
        return 0;
    }
}
