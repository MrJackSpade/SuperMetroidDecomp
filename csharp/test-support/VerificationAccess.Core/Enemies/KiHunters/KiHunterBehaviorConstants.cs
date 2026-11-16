namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Cartridge values and helpers of <see cref="KiHunterBehavior"/> that only verification reads.</summary>
internal static class KiHunterBehaviorConstants
{
    /// <summary>Recognizes the green, yellow and red body definitions.</summary>
    internal static bool IsKiHunterBodyDefinition(EnemyDefinitionId definition) =>
        definition is EnemyDefinitionId.KihunterGreen or EnemyDefinitionId.KihunterYellow or EnemyDefinitionId.KihunterRed;
    /// <summary>Recognizes the three corresponding wing definitions.</summary>
    internal static bool IsKiHunterWingDefinition(EnemyDefinitionId definition) =>
        definition is EnemyDefinitionId.KihunterGreenWings or EnemyDefinitionId.KihunterYellowWings or EnemyDefinitionId.KihunterRedWings;
}
