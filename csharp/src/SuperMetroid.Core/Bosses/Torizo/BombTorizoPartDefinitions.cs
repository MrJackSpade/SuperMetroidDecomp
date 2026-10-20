using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The definitions the BombTorizo species installs and the part each one is.</summary>
internal static class BombTorizoPartDefinitions
{
    /// <summary>Each BombTorizo definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, BombTorizoPart> Parts { get; } = new Dictionary<EnemyDefinitionId, BombTorizoPart>
    {
        [EnemyDefinitionId.BombTorizo] = BombTorizoPart.BombTorizo,
        [EnemyDefinitionId.GoldenTorizo] = BombTorizoPart.GoldenTorizo,
    };
}
