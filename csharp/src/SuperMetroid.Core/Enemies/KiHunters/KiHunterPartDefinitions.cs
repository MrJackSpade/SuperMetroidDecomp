namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>The definitions the Ki-Hunter species installs and the part each one is.</summary>
internal static class KiHunterPartDefinitions
{
    /// <summary>Each Ki-Hunter definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, KiHunterPart> Parts { get; } = new Dictionary<EnemyDefinitionId, KiHunterPart>
    {
        [EnemyDefinitionId.KihunterGreen] = KiHunterPart.KihunterGreen,
        [EnemyDefinitionId.KihunterGreenWings] = KiHunterPart.KihunterGreenWings,
        [EnemyDefinitionId.KihunterRed] = KiHunterPart.KihunterRed,
        [EnemyDefinitionId.KihunterRedWings] = KiHunterPart.KihunterRedWings,
        [EnemyDefinitionId.KihunterYellow] = KiHunterPart.KihunterYellow,
        [EnemyDefinitionId.KihunterYellowWings] = KiHunterPart.KihunterYellowWings,
    };
}
