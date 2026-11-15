using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The definitions the Torizo species installs and the part each one is.</summary>
internal static class TorizoPartDefinitions
{
    /// <summary>Each Torizo definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, TorizoPart> Parts { get; } = new Dictionary<EnemyDefinitionId, TorizoPart>
    {
        [EnemyDefinitionId.BombTorizo] = TorizoPart.BombTorizo,
        [EnemyDefinitionId.GoldenTorizo] = TorizoPart.GoldenTorizo,
    };
}
