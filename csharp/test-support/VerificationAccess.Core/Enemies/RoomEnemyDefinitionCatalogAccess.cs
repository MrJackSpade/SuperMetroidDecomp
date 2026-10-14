
namespace SuperMetroid.Core.Enemies;

/// <summary>Verification access to <see cref="RoomEnemyDefinitionCatalog"/> members production does not use.</summary>
internal static class RoomEnemyDefinitionCatalogAccess
{
    /// <summary>Enumerates every compiled enemy-definition pointer for cartridge-oracle comparisons.</summary>
    extension(RoomEnemyDefinitionCatalog)
    {
        /// <summary>Enumerates every compiled native definition pointer for ROM-oracle tests.</summary>
        internal static IEnumerable<EnemyDefinitionId> Pointers => RoomEnemyDefinitionCatalog.Definitions.Select(entry => entry.Pointer);
    }
}
