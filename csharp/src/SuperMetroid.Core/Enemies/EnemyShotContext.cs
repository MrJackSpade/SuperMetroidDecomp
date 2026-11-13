using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies;

/// <summary>One Samus projectile overlapping one enemy in the ordinary shot pass.</summary>
/// <param name="Enemy">The enemy whose shot reaction runs.</param>
/// <param name="Projectile">The overlapping beam or missile slot.</param>
/// <param name="Projectiles">Samus's projectile owner, for collision preludes, impacts and reflection.</param>
/// <param name="Samus">Samus, when the pass has one.</param>
/// <param name="ProjectileType">The incoming projectile type, sampled before any impact rewrites it.</param>
/// <param name="ProjectileDamage">The incoming projectile damage, a copy of the projectile's damage.</param>
/// <param name="ProjectileDirection">The incoming projectile's direction.</param>
/// <param name="Family">The incoming projectile family.</param>
/// <param name="UsesFrameHitboxes">Whether the overlap came from the enemy's authored extended hitboxes.</param>
/// <param name="HitboxShotAi">The named shot reaction of the overlapping hitbox, if named.</param>
/// <param name="HealthBefore">Enemy health after impact and before vulnerability damage (set for the later stages).</param>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly record struct EnemyShotContext(
    RoomEnemySlot Enemy,
    SamusProjectile Projectile,
    SamusProjectileSystem Projectiles,
    SamusState? Samus,
    SamusProjectileType ProjectileType,
    int ProjectileDamage,
    SamusProjectileDirection ProjectileDirection,
    SamusProjectileFamily Family,
    bool UsesFrameHitboxes,
    EnemyReaction? HitboxShotAi,
    int HealthBefore = 0)
{
    /// <summary>Whether common shot AI took the immediate-freeze path; set for <see cref="EnemyShotReaction.AfterCommon"/>.</summary>
    internal bool Froze { get; init; }

    /// <summary>
    /// The species states of the occupant the shot struck, taken as the pass finds the overlap (#1543). A killing hit
    /// removes the occupant from its place before <see cref="EnemyShotReaction.AfterCommon"/> runs; a tail that still
    /// acts on the killed enemy reaches its states here.
    /// </summary>
    internal required EnemyOccupantStates StruckOccupant { get; init; }

    /// <summary>
    /// The rule for marking the projectile as collided: a non-plasma shot, or any shot when the enemy
    /// blocks plasma.
    /// </summary>
    internal bool MarksCollision =>
        Enemy.Contact.HasAny(EnemyContact.BlocksPlasmaBeam) || !Projectile.Type.BeamCombination.HasPlasma;
}
