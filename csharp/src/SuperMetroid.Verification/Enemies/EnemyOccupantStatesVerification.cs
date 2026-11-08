using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.World;
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
        var enemies = new RoomEnemySystem(new EnemySession(new UnauthoredBoostCadence()), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];

        slot.Bind(EnemyDefinitionId.Dachora, slot.Definition);
        var killed = new DachoraEnemyState();
        slot.SetSpeciesState(killed);
        AssertTrue(ReferenceEquals(killed, slot.SpeciesState<DachoraEnemyState>()), "the occupant reads the state its species installed");
        EnemyOccupantStates struck = slot.OccupantStates;
        slot.Clear();
        AssertEqual<DachoraEnemyState?>(null, slot.SpeciesState<DachoraEnemyState>(), "a kill takes the occupant's state with it");
        AssertThrows<InvalidOperationException>(() => slot.RequireSpeciesState<DachoraEnemyState>(),
            "a read of a killed occupant's state fails");
        AssertTrue(ReferenceEquals(killed, struck.Find<DachoraEnemyState>()), "the struck occupant's own set still holds its state");

        slot.Bind(EnemyDefinitionId.Dachora, slot.Definition);
        slot.SetSpeciesState(new DachoraEnemyState());
        slot.Unbind();
        AssertEqual<DachoraEnemyState?>(null, slot.SpeciesState<DachoraEnemyState>(), "a deletion takes the occupant's state with it");

        slot.Bind(EnemyDefinitionId.Dachora, slot.Definition);
        slot.SetSpeciesState(new DachoraEnemyState());
        slot.Bind(EnemyDefinitionId.Dachora, slot.Definition);
        AssertEqual<DachoraEnemyState?>(null, slot.SpeciesState<DachoraEnemyState>(), "a new occupant starts without its predecessor's state");
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
