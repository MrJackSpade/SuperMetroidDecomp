using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The trail programs a trailing projectile's two sides run.</summary>
public static class ProjectileTrailSelection
{
    /// <summary>The programs the left and right trails behind <paramref name="trailing"/> run.</summary>
    /// <remarks>
    /// Only the ordinary beam updates, the missile and Super Missile updates and the Ice, Wave and Spazer special attacks
    /// spawn trails (#1500): a beam shot or special-attack particle by its beams and charge, a missile or Super Missile,
    /// and the Spazer attack's trailing pair. The Shinespark crash's echo has its own update, which spawns none, so a
    /// trail behind it is rejected, as is one behind a bomb or an explosion.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The projectile leaves no trail.</exception>
    internal static ProjectileTrailPrograms Select(SamusProjectileType trailing) => trailing.Family switch
    {
        SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile => ProjectileTrailDefinitions.MissileTrail,
        SamusProjectileFamily.Beam => trailing.Echo switch
        {
            null => new(Left(trailing.BeamCombination, trailing.IsChargedBeam), Right(trailing.BeamCombination, trailing.IsChargedBeam)),
            SamusBeamEcho.SpazerComboTrail => ProjectileTrailDefinitions.SpazerAttackTrail,
            SamusBeamEcho.ShinesparkEcho => throw new ArgumentOutOfRangeException(nameof(trailing), trailing, "A Shinespark crash's echo spawns no trail."),
            _ => throw new ArgumentOutOfRangeException(nameof(trailing), trailing, "Undefined beam echo."),
        },
        SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb or SamusProjectileFamily.BeamExplosion or
            SamusProjectileFamily.MissileExplosion => throw new ArgumentOutOfRangeException(nameof(trailing), trailing, "The projectile leaves no trail."),
        _ => throw new ArgumentOutOfRangeException(nameof(trailing), trailing, "Undefined projectile family."),
    };

    /// <summary>The left trail of a beam shot: Wave's own trail, otherwise Ice draws its sparkle, and so does a charged Power shot.</summary>
    private static ProjectileTrailProgram Left(SamusBeamCombination beams, bool charged) => beams switch
    {
        SamusBeamCombination.Wave => ProjectileTrailProgram.Wave,
        SamusBeamCombination.Power => charged ? ProjectileTrailProgram.LeftIce : ProjectileTrailProgram.Empty,
        SamusBeamCombination.Ice or SamusBeamCombination.IceWave or SamusBeamCombination.SpazerIce or
            SamusBeamCombination.SpazerIceWave or SamusBeamCombination.PlasmaIce or
            SamusBeamCombination.PlasmaIceWave => ProjectileTrailProgram.LeftIce,
        SamusBeamCombination.Spazer or SamusBeamCombination.SpazerWave or SamusBeamCombination.Plasma or
            SamusBeamCombination.PlasmaWave or SamusBeamCombination.SpazerPlasma or
            SamusBeamCombination.SpazerPlasmaWave or SamusBeamCombination.SpazerPlasmaIce or
            SamusBeamCombination.SpazerPlasmaIceWave => ProjectileTrailProgram.Empty,
        _ => throw new ArgumentOutOfRangeException(nameof(beams), beams, "Undefined beam combination."),
    };

    /// <summary>
    /// The right trail of a beam shot: a charged Wave or Ice-Wave shot's, and Ice's sparkle where Spazer or Plasma
    /// joins it, unless both do.
    /// </summary>
    private static ProjectileTrailProgram Right(SamusBeamCombination beams, bool charged) => beams switch
    {
        SamusBeamCombination.Wave => charged ? ProjectileTrailProgram.Wave : ProjectileTrailProgram.Empty,
        SamusBeamCombination.IceWave => charged ? ProjectileTrailProgram.RightIce : ProjectileTrailProgram.Empty,
        SamusBeamCombination.SpazerIce or SamusBeamCombination.SpazerIceWave or
            SamusBeamCombination.PlasmaIceWave => ProjectileTrailProgram.RightIce,
        SamusBeamCombination.Power or SamusBeamCombination.Ice or SamusBeamCombination.Spazer or
            SamusBeamCombination.SpazerWave or SamusBeamCombination.Plasma or
            SamusBeamCombination.PlasmaWave or SamusBeamCombination.PlasmaIce or
            SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaWave or
            SamusBeamCombination.SpazerPlasmaIce or SamusBeamCombination.SpazerPlasmaIceWave => ProjectileTrailProgram.Empty,
        _ => throw new ArgumentOutOfRangeException(nameof(beams), beams, "Undefined beam combination."),
    };
}
