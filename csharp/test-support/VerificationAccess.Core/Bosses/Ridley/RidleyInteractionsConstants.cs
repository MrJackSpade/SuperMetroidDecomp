using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Cartridge values and helpers of <see cref="RidleyInteractions"/> that only verification reads.</summary>
internal static class RidleyInteractionsConstants
{
    /// <summary>Includes both the Ceres and the Lower Norfair Ridley definitions.</summary>
    internal static bool IsRidleyDefinition(EnemyDefinitionId definitionId) =>
        definitionId is EnemyDefinitionId.RidleyCeres or EnemyDefinitionId.Ridley;
}
