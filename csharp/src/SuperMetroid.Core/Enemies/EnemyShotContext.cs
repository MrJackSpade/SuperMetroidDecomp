using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies;

/// <summary>One Samus projectile overlapping one enemy in the ordinary shot pass.</summary>
/// <param name="Occupant">The enemy whose shot reaction runs, which a killing hit kills.</param>
/// <param name="Shot">The overlapping beam or missile, holding its place.</param>
/// <param name="Projectiles">Samus's projectile owner, for a shot's impact and reflection.</param>
/// <param name="Samus">Samus.</param>
/// <param name="ProjectileType">The incoming projectile type, sampled before any impact rewrites it.</param>
/// <param name="ProjectileDamage">The incoming projectile damage, a copy of the projectile's damage.</param>
/// <param name="ProjectileDirection">The incoming projectile's direction.</param>
/// <param name="Family">The incoming projectile family.</param>
/// <param name="UsesFrameHitboxes">Whether the overlap came from the enemy's authored extended hitboxes.</param>
/// <param name="HitboxShot">The shot reaction that runs: the overlapping hitbox's, or the definition's on the radius path.</param>
/// <param name="Camera">The camera's position as the shot pass runs.</param>
/// <remarks>The stages from vulnerability on receive it as an <see cref="EnemyShotHitContext"/>, which adds the sampled health.</remarks>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
[ArchitectureReview("context-bag", "One shot's hit on one enemy (#2558): every member is a fact of that hit (the struck occupant, the shot holding its place, its sampled type, damage and direction, the reaction it selects, the pass's camera) that the hit's reaction stages read while the hit runs.")]
internal record EnemyShotContext(
    RoomEnemyOccupant Occupant,
    SamusProjectileOccupant Shot,
    SamusProjectileSystem Projectiles,
    SamusState Samus,
    SamusProjectileType ProjectileType,
    int ProjectileDamage,
    SamusProjectileDirection ProjectileDirection,
    SamusProjectileFamily Family,
    bool UsesFrameHitboxes,
    EnemyReaction HitboxShot,
    Position16<RoomPixels> Camera)
{
    /// <summary>The place the struck occupant was spawned in, which the reaction reads and writes.</summary>
    internal RoomEnemyPlace Enemy => Occupant.Place;

    /// <summary>The place the overlapping shot holds, with its position, damage and animation.</summary>
    internal SamusProjectile Projectile => Shot.Projectile;

    /// <summary>Whether the common shot reaction froze the enemy at once; set for <see cref="EnemyShotReaction.AfterCommon"/>.</summary>
    internal bool Froze { get; init; }

    /// <summary>
    /// The rule for marking the projectile as collided: a non-plasma shot, or any shot when the enemy
    /// blocks plasma.
    /// </summary>
    internal bool MarksCollision =>
        Enemy.Contact.HasAny(EnemyContact.BlocksPlasmaBeam) || !Shot.Type.BeamCombination.HasPlasma;
}
