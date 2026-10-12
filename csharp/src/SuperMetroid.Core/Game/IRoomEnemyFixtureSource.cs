namespace SuperMetroid.Core.Game;

/// <summary>
/// Explicit source for constructed verifier rooms whose enemy definitions, populations,
/// and graphics sets are intentionally different from the pinned retail cartridge.
/// Production address spaces never implement this test-only interface.
/// </summary>
internal interface IRoomEnemyFixtureSource
{
    /// <summary>read enemy definition.</summary>
    RoomEnemyDefinition ReadEnemyDefinition(EnemyDefinitionId pointer);
    /// <summary>read enemy population.</summary>
    RoomEnemyPopulationDefinition ReadEnemyPopulation(ushort pointer);
    /// <summary>read enemy graphics set.</summary>
    RoomEnemyGraphicsSetDefinition ReadEnemyGraphicsSet(ushort pointer);
}
