namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The definitions the ChozoStatue species installs and the part each one is.</summary>
internal static class ChozoStatuePartDefinitions
{
    /// <summary>Each ChozoStatue definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, ChozoStatuePart> Parts { get; } = new Dictionary<EnemyDefinitionId, ChozoStatuePart>
    {
        [EnemyDefinitionId.Chozo] = ChozoStatuePart.Chozo,
        [EnemyDefinitionId.NoobTubeCrack] = ChozoStatuePart.NoobTubeCrack,
    };
}
