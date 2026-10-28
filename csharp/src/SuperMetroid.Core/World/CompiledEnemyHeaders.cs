using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>
/// Reads a compiled enemy header: an auxiliary header the retail population needs beyond the main header table answers
/// first, and every other identity is a main header.
/// </summary>
internal static class CompiledEnemyHeaders
{
    /// <summary>The compiled header for <paramref name="definitionId"/>.</summary>
    internal static RoomEnemyDefinition Read(EnemyDefinitionId definitionId) =>
        RoomEnemyAuxiliaryDefinitionCatalog.TryGet(definitionId, out RoomEnemyDefinition auxiliary)
            ? auxiliary
            : RoomEnemyDefinitionCatalog.Get(definitionId);
}
