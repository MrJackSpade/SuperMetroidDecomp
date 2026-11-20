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
        _ => SamusProjectileDamage.Read(SamusProjectileSelection.NonBeam(type.Family)),
    };

    /// <summary>Fixture occupancy of a shot place.</summary>
    extension(SamusProjectile shot)
    {
        /// <summary>Takes the place with a projectile of <paramref name="type"/> dealing the damage its type deals.</summary>
        /// <returns>The projectile now holding the place.</returns>
        internal SamusProjectileOccupant OccupyAsFired(SamusProjectileType type) => shot.Occupy(type, DamageAsPlaced(type));
    }

    /// <summary>Fixture occupancy of a bomb place.</summary>
    extension(SamusBomb bomb)
    {
        /// <summary>Takes the place with a bomb of <paramref name="type"/> dealing the damage its family's bomb definition gives.</summary>
        /// <returns>The bomb now holding the place.</returns>
        internal SamusBombOccupant OccupyAsPlaced(SamusProjectileType type) => bomb.Occupy(type, DamageAsPlaced(type));
    }
}
