namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Which KiHunter occupant a slot holds, one per definition the species installs.</summary>
internal enum KiHunterPart
{
    /// <summary>The <see cref="EnemyDefinitionId.KihunterGreen"/> occupant.</summary>
    KihunterGreen,
    /// <summary>The <see cref="EnemyDefinitionId.KihunterGreenWings"/> occupant.</summary>
    KihunterGreenWings,
    /// <summary>The <see cref="EnemyDefinitionId.KihunterRed"/> occupant.</summary>
    KihunterRed,
    /// <summary>The <see cref="EnemyDefinitionId.KihunterRedWings"/> occupant.</summary>
    KihunterRedWings,
    /// <summary>The <see cref="EnemyDefinitionId.KihunterYellow"/> occupant.</summary>
    KihunterYellow,
    /// <summary>The <see cref="EnemyDefinitionId.KihunterYellowWings"/> occupant.</summary>
    KihunterYellowWings,
}
