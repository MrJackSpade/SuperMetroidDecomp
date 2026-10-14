using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="RoomEnemyAuxiliaryDefinitionCatalog"/> members production does not use.</summary>
internal static class RoomEnemyAuxiliaryDefinitionCatalogAccess
{
    /// <summary>Enumerates five auxiliary enemy headers absent from room-population lists.</summary>
    extension(RoomEnemyAuxiliaryDefinitionCatalog)
    {
        /// <summary>The five auxiliary header identities missing from room lists.</summary>
        internal static IEnumerable<EnemyDefinitionId> Pointers => RoomEnemyAuxiliaryDefinitionCatalog.Definitions.Keys;
    }
}
