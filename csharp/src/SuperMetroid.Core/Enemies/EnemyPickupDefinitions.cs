using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>Compiled fixed program selectors for enemy drops.</summary>
internal static class EnemyPickupDefinitions
{
    /// <summary>
    /// Each pickup kind's drop program. <see cref="EnemyPickupKind.None"/> has no program, and
    /// <see cref="EnemyPickupKind.NoDrop"/> has no entry; the dormant pickup path handles both.
    /// </summary>
    internal static readonly Dictionary<EnemyPickupKind, EnemyPickupAnimationDefinition> Animations = new()
    {
        [EnemyPickupKind.None] = new(null),
        [EnemyPickupKind.SmallEnergy] = new(EnemyDeathProjectileProgram.SmallEnergy),
        [EnemyPickupKind.BigEnergy] = new(EnemyDeathProjectileProgram.BigEnergy),
        [EnemyPickupKind.PowerBomb] = new(EnemyDeathProjectileProgram.PowerBombs),
        [EnemyPickupKind.Missile] = new(EnemyDeathProjectileProgram.Missiles),
        [EnemyPickupKind.SuperMissile] = new(EnemyDeathProjectileProgram.SuperMissiles),
    };
}
