using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The definitions the Kraid species installs and the part each one is.</summary>
internal static class KraidPartDefinitions
{
    /// <summary>Each Kraid definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, KraidPart> Parts { get; } = new Dictionary<EnemyDefinitionId, KraidPart>
    {
        [EnemyDefinitionId.Kraid] = KraidPart.Kraid,
        [EnemyDefinitionId.KraidArm] = KraidPart.Arm,
        [EnemyDefinitionId.KraidFoot] = KraidPart.Foot,
        [EnemyDefinitionId.KraidLintBottom] = KraidPart.LintBottom,
        [EnemyDefinitionId.KraidLintMiddle] = KraidPart.LintMiddle,
        [EnemyDefinitionId.KraidLintTop] = KraidPart.LintTop,
        [EnemyDefinitionId.KraidNail] = KraidPart.Nail,
        [EnemyDefinitionId.KraidNailBad] = KraidPart.NailBad,
    };
}
