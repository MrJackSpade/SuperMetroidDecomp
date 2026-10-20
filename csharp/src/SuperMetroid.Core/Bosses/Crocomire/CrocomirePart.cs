using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Which Crocomire occupant a slot holds, one per definition the species installs.</summary>
internal enum CrocomirePart
{
    /// <summary>The <see cref="EnemyDefinitionId.Crocomire"/> occupant.</summary>
    Crocomire,
    /// <summary>The <see cref="EnemyDefinitionId.CrocomireTongue"/> occupant.</summary>
    Tongue,
}
