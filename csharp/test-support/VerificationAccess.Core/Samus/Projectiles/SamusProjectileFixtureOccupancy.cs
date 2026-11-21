using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Fixture occupancy of Samus's shot and bomb places (#2558): a check that places a projectile of a type without firing it
/// gives it the damage a projectile of that type deals as it takes its place, as production's firing, release or placing
/// does, so no fixture invents a damage for a type.
/// </summary>
internal static class SamusProjectileFixtureOccupancy
{
    /// <summary>
    /// The damage a projectile of <paramref name="type"/> deals as it takes its place: an echo's, a beam shot's of its
    /// combination and charge, an explosion's, or a missile's, Super Missile's, bomb's or Power Bomb's.
    /// </summary>
    internal static int DamageAsPlaced(SamusProjectileType type) => type switch
    {
        { Echo: { } echo } => SamusProjectileDamage.Read(SamusProjectileSelection.Echo(echo)),
        { Family: SamusProjectileFamily.Beam } => SamusProjectileDamage.Read(SamusProjectileSelection.BeamShot(type.BeamCombination, type.IsChargedBeam)),
        { Family: SamusProjectileFamily.BeamExplosion or SamusProjectileFamily.MissileExplosion } => SamusProjectileDamageDefinitions.ExplosionDamage,
        _ => SamusProjectileDamage.Read(NonBeam(type.Family)),
    };

    /// <summary>The kind of a missile, Super Missile, bomb or Power Bomb of <paramref name="family"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The family is no missile or bomb family.</exception>
    internal static SamusProjectileKind NonBeam(SamusProjectileFamily family) => family switch
    {
        SamusProjectileFamily.Missile => SamusProjectileSelection.Missile(super: false),
        SamusProjectileFamily.SuperMissile => SamusProjectileSelection.Missile(super: true),
        SamusProjectileFamily.Bomb or SamusProjectileFamily.PowerBomb => SamusProjectileSelection.Bomb(BombKindOf(family)),
        _ => throw new ArgumentOutOfRangeException(nameof(family), family, "The family is no missile or bomb family."),
    };

    /// <summary>The bomb of <paramref name="family"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The family is no bomb family.</exception>
    internal static SamusBombKind BombKindOf(SamusProjectileFamily family) => family switch
    {
        SamusProjectileFamily.Bomb => SamusBombKind.Bomb,
        SamusProjectileFamily.PowerBomb => SamusBombKind.PowerBomb,
        _ => throw new ArgumentOutOfRangeException(nameof(family), family, "The family is no bomb family."),
    };

    /// <summary>
    /// A shot place, the first of the pool's, holding a projectile of <paramref name="type"/> at rest at the room's origin
    /// with the damage its type deals: the place a check that runs one projectile's own steps builds.
    /// </summary>
    /// <param name="type">What the projectile is.</param>
    internal static SamusProjectile ShotPlace(SamusProjectileType type)
    {
        var place = new SamusProjectile(0);
        _ = place.OccupyAsFired(type);
        return place;
    }

    /// <summary>Fixture occupancy of a shot place.</summary>
    extension(SamusProjectile shot)
    {
        /// <summary>Takes the place with a projectile of <paramref name="type"/> dealing the damage its type deals.</summary>
        /// <returns>The projectile now holding the place.</returns>
        internal SamusProjectileOccupant OccupyAsFired(SamusProjectileType type) => shot.Occupy(type, DamageAsPlaced(type));

        /// <summary>
        /// Takes the place with a projectile of <paramref name="type"/> dealing <paramref name="damage"/>, at rest at the
        /// room's origin, as a check's fixture places one.
        /// </summary>
        /// <returns>The projectile now holding the place.</returns>
        internal SamusProjectileOccupant Occupy(SamusProjectileType type, int damage) =>
            shot.Occupy(type, damage, FixedPosition<RoomPixels>.Zero);
    }

    /// <summary>Fixture occupancy of the shot pool.</summary>
    extension(SamusShotPool pool)
    {
        /// <summary>
        /// Takes <paramref name="place"/>, ending the shot there, with a projectile of <paramref name="type"/> dealing the
        /// damage its type deals, as firing does.
        /// </summary>
        /// <returns>The place the projectile now holds.</returns>
        internal SamusProjectile OccupyAsFired(int place, SamusProjectileType type) =>
            pool.Occupy(place, type, DamageAsPlaced(type)).Projectile;
    }

    /// <summary>Fixture occupancy of a bomb place.</summary>
    extension(SamusBomb bomb)
    {
        /// <summary>Takes the place with a bomb of <paramref name="type"/> dealing the damage its family's bomb definition gives.</summary>
        /// <returns>The bomb now holding the place.</returns>
        internal SamusBombOccupant OccupyAsPlaced(SamusProjectileType type) => bomb.Occupy(BombKindOf(type.Family), DamageAsPlaced(type));
    }
}
