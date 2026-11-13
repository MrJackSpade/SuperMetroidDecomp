using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>
/// Reads a compiled enemy definition: an auxiliary definition the retail population needs beyond the main definition
/// table answers first, and every other identity is a main definition.
/// </summary>
internal static class CompiledEnemyHeaders
{
    /// <summary>The compiled definition for <paramref name="definitionId"/>.</summary>
    internal static RoomEnemyDefinition Read(EnemyDefinitionId definitionId) =>
        RoomEnemyAuxiliaryDefinitionCatalog.TryGet(definitionId, out RoomEnemyDefinition auxiliary)
            ? auxiliary
            : RoomEnemyHeaders.Get(definitionId);
}
