namespace SuperMetroid.Core.Game;

/// <summary>
/// Explicit source for constructed verifier rooms whose enemy definitions, populations,
/// and graphics sets are intentionally different from the pinned retail cartridge.
/// Production address spaces never implement this test-only interface.
/// </summary>
internal interface IRoomEnemyFixtureSource
{
    /// <summary>Supplies a constructed enemy definition for a verifier room instead of reading the retail catalog.</summary>
    /// <param name="pointer">Enemy-definition identity referenced by the fixture room data.</param>
    /// <returns>The synthetic definition associated with that identity.</returns>
    RoomEnemyDefinition ReadEnemyDefinition(ushort pointer);

    /// <summary>Supplies constructed enemy population records for a verifier room instead of reading the retail catalog.</summary>
    /// <param name="pointer">Bank-$A1 population identity referenced by the fixture room data.</param>
    /// <returns>The synthetic population and quota associated with that identity.</returns>
    RoomEnemyPopulationDefinition ReadEnemyPopulation(ushort pointer);

    /// <summary>Supplies a constructed enemy graphics set for a verifier room instead of reading the retail catalog.</summary>
    /// <param name="pointer">Bank-$B4 graphics-set identity referenced by the fixture room data.</param>
    /// <returns>The ordered synthetic graphics records associated with that identity.</returns>
    RoomEnemyGraphicsSetDefinition ReadEnemyGraphicsSet(ushort pointer);
}
