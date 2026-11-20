using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Samus.Projectiles.ProjectileTrailSegment;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The trail animations the trail spawner selects for each projectile that leaves a trail.</summary>
public static class ProjectileTrailDefinitions
{
    /// <summary>The trail a missile or Super Missile leaves: the missile animation on the left, none on the right.</summary>
    internal static readonly ProjectileTrailAnimations MissileTrail = ProjectileTrailAnimations.LeftOnly(Missile);

    /// <summary>The trail the Spazer special attack's trailing pair leaves: the left ice animation on both sides.</summary>
    internal static readonly ProjectileTrailAnimations SpazerAttackTrail = ProjectileTrailAnimations.Both(LeftIce, LeftIce);

    /// <summary>
    /// The trail an uncharged beam shot of each combination leaves: Wave's own on the left of Wave alone; Ice's sparkle on
    /// the left wherever Ice is fired without both Spazer and Plasma, and on the right too with Spazer, or with Plasma and
    /// Wave.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusBeamCombination, ProjectileTrailAnimations> UnchargedBeamTrails =
        new Dictionary<SamusBeamCombination, ProjectileTrailAnimations>
        {
            [SamusBeamCombination.Power] = ProjectileTrailAnimations.Bare,
            [SamusBeamCombination.Wave] = ProjectileTrailAnimations.LeftOnly(Wave),
            [SamusBeamCombination.Ice] = ProjectileTrailAnimations.LeftOnly(LeftIce),
            [SamusBeamCombination.IceWave] = ProjectileTrailAnimations.LeftOnly(LeftIce),
            [SamusBeamCombination.Spazer] = ProjectileTrailAnimations.Bare,
            [SamusBeamCombination.SpazerWave] = ProjectileTrailAnimations.Bare,
            [SamusBeamCombination.SpazerIce] = ProjectileTrailAnimations.Both(LeftIce, RightIce),
            [SamusBeamCombination.SpazerIceWave] = ProjectileTrailAnimations.Both(LeftIce, RightIce),
            [SamusBeamCombination.Plasma] = ProjectileTrailAnimations.Bare,
            [SamusBeamCombination.PlasmaWave] = ProjectileTrailAnimations.Bare,
            [SamusBeamCombination.PlasmaIce] = ProjectileTrailAnimations.LeftOnly(LeftIce),
            [SamusBeamCombination.PlasmaIceWave] = ProjectileTrailAnimations.Both(LeftIce, RightIce),
            [SamusBeamCombination.SpazerPlasma] = ProjectileTrailAnimations.Bare,
            [SamusBeamCombination.SpazerPlasmaWave] = ProjectileTrailAnimations.Bare,
            [SamusBeamCombination.SpazerPlasmaIce] = ProjectileTrailAnimations.Bare,
            [SamusBeamCombination.SpazerPlasmaIceWave] = ProjectileTrailAnimations.Bare,
        };

    /// <summary>
    /// The trail a charged beam shot of each combination leaves: as the uncharged shot's, except that a charged Power
    /// shot draws Ice's sparkle on the left, a charged Wave shot Wave's trail on both sides, and a charged Ice-Wave shot
    /// Ice's sparkle on both.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusBeamCombination, ProjectileTrailAnimations> ChargedBeamTrails =
        new Dictionary<SamusBeamCombination, ProjectileTrailAnimations>
        {
            [SamusBeamCombination.Power] = ProjectileTrailAnimations.LeftOnly(LeftIce),
            [SamusBeamCombination.Wave] = ProjectileTrailAnimations.Both(Wave, Wave),
            [SamusBeamCombination.Ice] = ProjectileTrailAnimations.LeftOnly(LeftIce),
            [SamusBeamCombination.IceWave] = ProjectileTrailAnimations.Both(LeftIce, RightIce),
            [SamusBeamCombination.Spazer] = ProjectileTrailAnimations.Bare,
            [SamusBeamCombination.SpazerWave] = ProjectileTrailAnimations.Bare,
            [SamusBeamCombination.SpazerIce] = ProjectileTrailAnimations.Both(LeftIce, RightIce),
            [SamusBeamCombination.SpazerIceWave] = ProjectileTrailAnimations.Both(LeftIce, RightIce),
            [SamusBeamCombination.Plasma] = ProjectileTrailAnimations.Bare,
            [SamusBeamCombination.PlasmaWave] = ProjectileTrailAnimations.Bare,
            [SamusBeamCombination.PlasmaIce] = ProjectileTrailAnimations.LeftOnly(LeftIce),
            [SamusBeamCombination.PlasmaIceWave] = ProjectileTrailAnimations.Both(LeftIce, RightIce),
            [SamusBeamCombination.SpazerPlasma] = ProjectileTrailAnimations.Bare,
            [SamusBeamCombination.SpazerPlasmaWave] = ProjectileTrailAnimations.Bare,
            [SamusBeamCombination.SpazerPlasmaIce] = ProjectileTrailAnimations.Bare,
            [SamusBeamCombination.SpazerPlasmaIceWave] = ProjectileTrailAnimations.Bare,
        };
}
