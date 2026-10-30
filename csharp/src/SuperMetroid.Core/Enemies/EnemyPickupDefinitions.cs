using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>Compiled fixed program selectors for enemy drops.</summary>
internal static class EnemyPickupDefinitions
{
    /// <summary>
    /// The none, small energy, big energy, Power Bomb, missile and Super Missile entries of the pickup table at
    /// <c>$86:EF04-$86:EF0F</c>: each kind's drop program. <see cref="EnemyPickupKind.None"/> has no
    /// program, and <see cref="EnemyPickupKind.NoDrop"/> is not part of the table; the dormant pickup path handles both.
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
