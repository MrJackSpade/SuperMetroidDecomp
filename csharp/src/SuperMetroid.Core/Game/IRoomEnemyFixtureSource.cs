namespace SuperMetroid.Core.Game;

/// <summary>
/// Explicit source for constructed verifier rooms whose enemy definitions, populations,
/// and graphics sets are intentionally different from the pinned retail cartridge.
/// Production address spaces never implement this test-only interface.
/// </summary>
internal interface IRoomEnemyFixtureSource
{
    /// <summary>Reads a constructed enemy definition instead of consulting the pinned retail catalog.</summary>
    RoomEnemyDefinition ReadEnemyDefinition(EnemyDefinitionId pointer);
    /// <summary>Reads the constructed room's population by its fixture pointer.</summary>
    RoomEnemyPopulationDefinition ReadEnemyPopulation(ushort pointer);
    /// <summary>Reads the constructed room's graphics set by its fixture pointer.</summary>
    RoomEnemyGraphicsSetDefinition ReadEnemyGraphicsSet(ushort pointer);
}
