using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies;

/// <summary>One Samus projectile overlapping one enemy in the ordinary shot pass.</summary>
/// <param name="Enemy">The enemy whose shot callback runs.</param>
/// <param name="Projectile">The overlapping beam or missile slot.</param>
/// <param name="Projectiles">Samus's projectile owner, for collision preludes, impacts and reflection.</param>
/// <param name="Samus">Samus, when the pass has one.</param>
/// <param name="ProjectileType">The incoming projectile type word, sampled before any impact rewrites it.</param>
/// <param name="ProjectileDamage">The incoming projectile damage.</param>
/// <param name="ProjectileDirection">The incoming projectile direction word.</param>
/// <param name="Family">The incoming projectile family.</param>
/// <param name="UsesExtendedHitboxes">Whether the overlap came from the enemy's authored extended hitboxes.</param>
/// <param name="HitboxShotAi">The named shot callback of the overlapping hitbox, if named.</param>
/// <param name="HitboxShotAiPointer">The raw bank-local shot callback word of the overlapping hitbox.</param>
/// <param name="HealthBefore">Enemy health after impact and before vulnerability damage (set for the later stages).</param>
internal readonly record struct EnemyShotContext(
    RoomEnemySlot Enemy,
    SamusProjectileSlot Projectile,
    SamusProjectileSystem Projectiles,
    SamusState? Samus,
    ushort ProjectileType,
    ushort ProjectileDamage,
    ushort ProjectileDirection,
    SamusProjectileFamily Family,
    bool UsesExtendedHitboxes,
    EnemyInteractionCallback? HitboxShotAi,
    ushort HitboxShotAiPointer,
    int HealthBefore = 0)
{
    /// <summary>
    /// The bank-$A0 rule for marking the projectile as collided: a non-plasma shot, or any shot when the enemy
    /// blocks plasma.
    /// </summary>
    internal bool MarksCollision =>
        Enemy.Properties.HasAny(EnemyProperties.BlocksPlasmaBeam) || !new SamusProjectileTypeWord(Projectile.Type).BeamCombination.HasPlasma;
}
