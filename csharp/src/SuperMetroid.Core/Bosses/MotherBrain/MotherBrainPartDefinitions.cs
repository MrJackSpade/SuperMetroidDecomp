using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The definitions the MotherBrain species installs and the part each one is.</summary>
internal static class MotherBrainPartDefinitions
{
    /// <summary>Each MotherBrain definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, MotherBrainPart> Parts { get; } = new Dictionary<EnemyDefinitionId, MotherBrainPart>
    {
        [EnemyDefinitionId.MotherBrainBody] = MotherBrainPart.Body,
        [EnemyDefinitionId.MotherBrainHead] = MotherBrainPart.Head,
        [EnemyDefinitionId.MotherBrainTubes] = MotherBrainPart.Tubes,
    };
}
