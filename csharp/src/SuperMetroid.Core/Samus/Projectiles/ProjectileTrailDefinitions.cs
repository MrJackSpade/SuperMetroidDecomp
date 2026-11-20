using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Samus.Projectiles.ProjectileTrailSegment;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The trail animations the trail spawner selects for each projectile that leaves a trail.</summary>
public static class ProjectileTrailDefinitions
{
    /// <summary>The trail a missile or Super Missile leaves: the missile animation on the left, none on the right.</summary>
    internal static readonly ProjectileTrailAnimations MissileTrail = new(Missile, EndsWithoutSparkle);

    /// <summary>The trail the Spazer special attack's trailing pair leaves: the left ice animation on both sides.</summary>
    internal static readonly ProjectileTrailAnimations SpazerAttackTrail = new(LeftIce, LeftIce);

    /// <summary>
    /// The trail an uncharged beam shot of each combination leaves: Wave's own on the left of Wave alone; Ice's sparkle on
    /// the left wherever Ice is fired without both Spazer and Plasma, and on the right too with Spazer, or with Plasma and
    /// Wave.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusBeamCombination, ProjectileTrailAnimations> UnchargedBeamTrails =
        new Dictionary<SamusBeamCombination, ProjectileTrailAnimations>
        {
            [SamusBeamCombination.Power] = new(EndsWithoutSparkle, EndsWithoutSparkle),
            [SamusBeamCombination.Wave] = new(Wave, EndsWithoutSparkle),
            [SamusBeamCombination.Ice] = new(LeftIce, EndsWithoutSparkle),
            [SamusBeamCombination.IceWave] = new(LeftIce, EndsWithoutSparkle),
            [SamusBeamCombination.Spazer] = new(EndsWithoutSparkle, EndsWithoutSparkle),
            [SamusBeamCombination.SpazerWave] = new(EndsWithoutSparkle, EndsWithoutSparkle),
            [SamusBeamCombination.SpazerIce] = new(LeftIce, RightIce),
            [SamusBeamCombination.SpazerIceWave] = new(LeftIce, RightIce),
            [SamusBeamCombination.Plasma] = new(EndsWithoutSparkle, EndsWithoutSparkle),
            [SamusBeamCombination.PlasmaWave] = new(EndsWithoutSparkle, EndsWithoutSparkle),
            [SamusBeamCombination.PlasmaIce] = new(LeftIce, EndsWithoutSparkle),
            [SamusBeamCombination.PlasmaIceWave] = new(LeftIce, RightIce),
            [SamusBeamCombination.SpazerPlasma] = new(EndsWithoutSparkle, EndsWithoutSparkle),
            [SamusBeamCombination.SpazerPlasmaWave] = new(EndsWithoutSparkle, EndsWithoutSparkle),
            [SamusBeamCombination.SpazerPlasmaIce] = new(EndsWithoutSparkle, EndsWithoutSparkle),
            [SamusBeamCombination.SpazerPlasmaIceWave] = new(EndsWithoutSparkle, EndsWithoutSparkle),
        };

    /// <summary>
    /// The trail a charged beam shot of each combination leaves: as the uncharged shot's, except that a charged Power
    /// shot draws Ice's sparkle on the left, a charged Wave shot Wave's trail on both sides, and a charged Ice-Wave shot
    /// Ice's sparkle on both.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusBeamCombination, ProjectileTrailAnimations> ChargedBeamTrails =
        new Dictionary<SamusBeamCombination, ProjectileTrailAnimations>
        {
            [SamusBeamCombination.Power] = new(LeftIce, EndsWithoutSparkle),
            [SamusBeamCombination.Wave] = new(Wave, Wave),
            [SamusBeamCombination.Ice] = new(LeftIce, EndsWithoutSparkle),
            [SamusBeamCombination.IceWave] = new(LeftIce, RightIce),
            [SamusBeamCombination.Spazer] = new(EndsWithoutSparkle, EndsWithoutSparkle),
            [SamusBeamCombination.SpazerWave] = new(EndsWithoutSparkle, EndsWithoutSparkle),
            [SamusBeamCombination.SpazerIce] = new(LeftIce, RightIce),
            [SamusBeamCombination.SpazerIceWave] = new(LeftIce, RightIce),
            [SamusBeamCombination.Plasma] = new(EndsWithoutSparkle, EndsWithoutSparkle),
            [SamusBeamCombination.PlasmaWave] = new(EndsWithoutSparkle, EndsWithoutSparkle),
            [SamusBeamCombination.PlasmaIce] = new(LeftIce, EndsWithoutSparkle),
            [SamusBeamCombination.PlasmaIceWave] = new(LeftIce, RightIce),
            [SamusBeamCombination.SpazerPlasma] = new(EndsWithoutSparkle, EndsWithoutSparkle),
            [SamusBeamCombination.SpazerPlasmaWave] = new(EndsWithoutSparkle, EndsWithoutSparkle),
            [SamusBeamCombination.SpazerPlasmaIce] = new(EndsWithoutSparkle, EndsWithoutSparkle),
            [SamusBeamCombination.SpazerPlasmaIceWave] = new(EndsWithoutSparkle, EndsWithoutSparkle),
        };
}
