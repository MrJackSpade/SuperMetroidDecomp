namespace SuperMetroid.Core.Game;

/// <summary>
/// Explicit source for constructed verifier rooms whose enemy definitions, populations,
/// and graphics sets are intentionally different from the pinned retail cartridge.
/// Production address spaces never implement this test-only interface.
/// </summary>
internal interface IRoomEnemyFixtureSource
{
    RoomEnemyDefinition ReadEnemyDefinition(ushort pointer);
    RoomEnemyPopulationDefinition ReadEnemyPopulation(ushort pointer);
    RoomEnemyGraphicsSetDefinition ReadEnemyGraphicsSet(ushort pointer);
}
