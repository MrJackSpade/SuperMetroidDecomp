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
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        // A Dachora placement names the runner or one of its echoes (#2283), which every bind below reads; the entry
        // stays the place's through kills, deletions and new occupants.
        LeadingPartPlacement.Prepare(place, EnemyDefinitionId.Dachora);

        place.Bind(EnemyDefinitionId.Dachora, place.KeptDefinition);
        var killed = new DachoraEnemyState(DachoraAction.WaitingForSamus);
        place.SetSpeciesState(killed);
        AssertTrue(ReferenceEquals(killed, place.SpeciesState<DachoraEnemyState>()), "the occupant reads the state its species installed");
        EnemyOccupantStates struck = place.OccupantStates;
        place.Clear();
        AssertEqual<DachoraEnemyState?>(null, place.SpeciesState<DachoraEnemyState>(), "a kill takes the occupant's state with it");
        AssertThrows<InvalidOperationException>(() => place.RequireSpeciesState<DachoraEnemyState>(),
            "a read of a killed occupant's state fails");
        AssertTrue(ReferenceEquals(killed, struck.Find<DachoraEnemyState>()), "the struck occupant's own set still holds its state");

        place.Bind(EnemyDefinitionId.Dachora, place.KeptDefinition);
        place.SetSpeciesState(new DachoraEnemyState(DachoraAction.WaitingForSamus));
        place.Unbind();
        AssertEqual<DachoraEnemyState?>(null, place.SpeciesState<DachoraEnemyState>(), "a deletion takes the occupant's state with it");

        place.Bind(EnemyDefinitionId.Dachora, place.KeptDefinition);
        place.SetSpeciesState(new DachoraEnemyState(DachoraAction.WaitingForSamus));
        place.Bind(EnemyDefinitionId.Dachora, place.KeptDefinition);
        AssertEqual<DachoraEnemyState?>(null, place.SpeciesState<DachoraEnemyState>(), "a new occupant starts without its predecessor's state");
        Console.WriteLine("Enemy occupant states: a kill, a deletion and a new occupant each end the previous occupant's state.");
    }

    /// <summary>Runs the focused enemy occupant-state check.</summary>
    [VerificationCommand("--enemy-occupant-states")]
    internal static int EnemyOccupantStatesCommand()
    {
        VerifyEnemyOccupantStates();
        return 0;
    }
}
