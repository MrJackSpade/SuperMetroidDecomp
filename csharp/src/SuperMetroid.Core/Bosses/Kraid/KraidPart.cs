using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Which Kraid occupant a slot holds, one per definition the species installs.</summary>
internal enum KraidPart
{
    /// <summary>The <see cref="EnemyDefinitionId.Kraid"/> occupant.</summary>
    Kraid,
    /// <summary>The <see cref="EnemyDefinitionId.KraidArm"/> occupant.</summary>
    Arm,
    /// <summary>The <see cref="EnemyDefinitionId.KraidFoot"/> occupant.</summary>
    Foot,
    /// <summary>The <see cref="EnemyDefinitionId.KraidLintBottom"/> occupant.</summary>
    LintBottom,
    /// <summary>The <see cref="EnemyDefinitionId.KraidLintMiddle"/> occupant.</summary>
    LintMiddle,
    /// <summary>The <see cref="EnemyDefinitionId.KraidLintTop"/> occupant.</summary>
    LintTop,
    /// <summary>The <see cref="EnemyDefinitionId.KraidNail"/> occupant.</summary>
    Nail,
    /// <summary>The <see cref="EnemyDefinitionId.KraidNailBad"/> occupant.</summary>
    NailBad,
}
