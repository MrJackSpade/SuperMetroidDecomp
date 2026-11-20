using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The trail animations a trailing projectile's two sides run.</summary>
public static class ProjectileTrailSelection
{
    /// <summary>The animations the left and right trails behind <paramref name="trailing"/> run.</summary>
    /// <remarks>
    /// Only the ordinary beam updates, the missile and Super Missile updates and the Ice, Wave and Spazer special attacks
    /// spawn trails (#1500): a beam shot or special-attack particle by its beams and charge, a missile or Super Missile,
    /// and the Spazer attack's trailing pair. The Shinespark crash's echo has its own update, which spawns none, so a
    /// trail behind it is rejected, as is one behind a bomb or an explosion.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The projectile leaves no trail.</exception>
    internal static ProjectileTrailAnimations Select(SamusProjectileType trailing) => trailing.Family switch
    {
        SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile => ProjectileTrailDefinitions.MissileTrail,
        SamusProjectileFamily.Beam => trailing.Echo switch
        {
            null => (trailing.IsChargedBeam ? ProjectileTrailDefinitions.ChargedBeamTrails : ProjectileTrailDefinitions.UnchargedBeamTrails)
                .TryGetValue(trailing.BeamCombination, out ProjectileTrailAnimations? beamTrail)
                    ? beamTrail
                    : throw new ArgumentOutOfRangeException(nameof(trailing), trailing, "Undefined beam combination."),
            SamusBeamEcho.SpazerComboTrail => ProjectileTrailDefinitions.SpazerAttackTrail,
            SamusBeamEcho.ShinesparkEcho => throw new ArgumentOutOfRangeException(nameof(trailing), trailing, "A Shinespark crash's echo spawns no trail."),
            _ => throw new ArgumentOutOfRangeException(nameof(trailing), trailing, "Undefined beam echo."),
        },
        SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb or SamusProjectileFamily.BeamExplosion or
            SamusProjectileFamily.MissileExplosion => throw new ArgumentOutOfRangeException(nameof(trailing), trailing, "The projectile leaves no trail."),
        _ => throw new ArgumentOutOfRangeException(nameof(trailing), trailing, "Undefined projectile family."),
    };
}
