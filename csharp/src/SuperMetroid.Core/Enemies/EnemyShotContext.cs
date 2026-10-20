using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>One Samus projectile overlapping one enemy in the ordinary shot pass.</summary>
/// <param name="Enemy">The enemy whose shot callback runs.</param>
/// <param name="Projectile">The overlapping beam or missile slot.</param>
/// <param name="Projectiles">Samus's projectile owner, for collision preludes, impacts and reflection.</param>
/// <param name="Samus">Samus, when the pass has one.</param>
/// <param name="ProjectileType">The incoming projectile type, sampled before any impact rewrites it.</param>
/// <param name="ProjectileDamage">The incoming projectile damage, a copy of the projectile's damage (0 through 65535).</param>
/// <param name="ProjectileDirection">The incoming projectile direction and lifecycle state.</param>
/// <param name="Family">The incoming projectile family.</param>
/// <param name="UsesExtendedHitboxes">Whether the overlap came from the enemy's authored extended hitboxes.</param>
/// <param name="HitboxShotAi">The named shot callback of the overlapping hitbox, if named.</param>
/// <param name="HealthBefore">Enemy health after impact and before vulnerability damage (set for the later stages).</param>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly record struct EnemyShotContext(
    RoomEnemySlot Enemy,
    SamusProjectileSlot Projectile,
    SamusProjectileSystem Projectiles,
    SamusState? Samus,
    SamusProjectileTypeWord ProjectileType,
    int ProjectileDamage,
    SamusProjectileDirectionWord ProjectileDirection,
    SamusProjectileFamily Family,
    bool UsesExtendedHitboxes,
    EnemyInteractionCallback? HitboxShotAi,
    int HealthBefore = 0)
{
    /// <summary>Whether common shot AI took the immediate-freeze path; set for <see cref="EnemyShotReaction.AfterCommon"/>.</summary>
    internal bool Froze { get; init; }

    /// <summary>
    /// The bank-$A0 rule for marking the projectile as collided: a non-plasma shot, or any shot when the enemy
    /// blocks plasma.
    /// </summary>
    internal bool MarksCollision =>
        Enemy.Properties.HasAny(EnemyProperties.BlocksPlasmaBeam) || !Projectile.PackedType.BeamCombination.HasPlasma;
}
