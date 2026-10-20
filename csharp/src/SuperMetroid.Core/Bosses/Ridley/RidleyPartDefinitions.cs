using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The definitions the Ridley species installs and the part each one is.</summary>
internal static class RidleyPartDefinitions
{
    /// <summary>Each Ridley definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, RidleyPart> Parts { get; } = new Dictionary<EnemyDefinitionId, RidleyPart>
    {
        [EnemyDefinitionId.Ridley] = RidleyPart.Ridley,
        [EnemyDefinitionId.RidleyCeres] = RidleyPart.Ceres,
        [EnemyDefinitionId.RidleyExplosion] = RidleyPart.Explosion,
    };
}
