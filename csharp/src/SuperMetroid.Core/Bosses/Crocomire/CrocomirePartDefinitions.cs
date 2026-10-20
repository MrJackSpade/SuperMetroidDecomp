using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The definitions the Crocomire species installs and the part each one is.</summary>
internal static class CrocomirePartDefinitions
{
    /// <summary>Each Crocomire definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, CrocomirePart> Parts { get; } = new Dictionary<EnemyDefinitionId, CrocomirePart>
    {
        [EnemyDefinitionId.Crocomire] = CrocomirePart.Crocomire,
        [EnemyDefinitionId.CrocomireTongue] = CrocomirePart.Tongue,
    };
}
