using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>Compiled fixed animation selectors for enemy drops.</summary>
internal static class EnemyPickupDefinitions
{
    /// <summary>
    /// Each pickup kind's drop animation. <see cref="EnemyPickupKind.None"/> has no animation, and
    /// <see cref="EnemyPickupKind.NoDrop"/> has no entry; the dormant pickup path handles both.
    /// </summary>
    internal static readonly Dictionary<EnemyPickupKind, EnemyPickupAnimationDefinition> Animations = new()
    {
        [EnemyPickupKind.None] = new(null),
        [EnemyPickupKind.SmallEnergy] = new(EnemyDeathProjectileSegment.SmallEnergy),
        [EnemyPickupKind.BigEnergy] = new(EnemyDeathProjectileSegment.BigEnergy),
        [EnemyPickupKind.PowerBomb] = new(EnemyDeathProjectileSegment.PowerBombs),
        [EnemyPickupKind.Missile] = new(EnemyDeathProjectileSegment.Missiles),
        [EnemyPickupKind.SuperMissile] = new(EnemyDeathProjectileSegment.SuperMissiles),
    };
}
