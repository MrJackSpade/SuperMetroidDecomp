using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Which Ridley occupant a slot holds, one per definition the species installs.</summary>
internal enum RidleyPart
{
    /// <summary>The <see cref="EnemyDefinitionId.Ridley"/> occupant.</summary>
    Ridley,
    /// <summary>The <see cref="EnemyDefinitionId.RidleyCeres"/> occupant.</summary>
    Ceres,
    /// <summary>The <see cref="EnemyDefinitionId.RidleyExplosion"/> occupant.</summary>
    Explosion,
}
