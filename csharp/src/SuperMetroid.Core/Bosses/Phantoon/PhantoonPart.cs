using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Which Phantoon occupant a slot holds, one per definition the species installs.</summary>
internal enum PhantoonPart
{
    /// <summary>The <see cref="EnemyDefinitionId.PhantoonBody"/> occupant.</summary>
    Body,
    /// <summary>The <see cref="EnemyDefinitionId.PhantoonEye"/> occupant.</summary>
    Eye,
    /// <summary>The <see cref="EnemyDefinitionId.PhantoonMouth"/> occupant.</summary>
    Mouth,
    /// <summary>The <see cref="EnemyDefinitionId.PhantoonTentacles"/> occupant.</summary>
    Tentacles,
}
