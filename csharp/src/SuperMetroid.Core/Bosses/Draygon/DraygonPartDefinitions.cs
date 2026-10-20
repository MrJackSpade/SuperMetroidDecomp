using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The definitions the Draygon species installs and the part each one is.</summary>
internal static class DraygonPartDefinitions
{
    /// <summary>Each Draygon definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, DraygonPart> Parts { get; } = new Dictionary<EnemyDefinitionId, DraygonPart>
    {
        [EnemyDefinitionId.DraygonArms] = DraygonPart.Arms,
        [EnemyDefinitionId.DraygonBody] = DraygonPart.Body,
        [EnemyDefinitionId.DraygonEye] = DraygonPart.Eye,
        [EnemyDefinitionId.DraygonTail] = DraygonPart.Tail,
    };
}
