using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Which projectile definition each shot, bomb, special attack and echo starts from, by what fires it: a beam combination and
/// whether the shot was charged, a projectile family, or an echo.
/// </summary>
internal static class SamusProjectileSelectionDefinitions
{
    /// <summary>The definition each retail beam combination's uncharged shot starts from.</summary>
    internal static readonly IReadOnlyDictionary<SamusBeamCombination, SamusProjectileHeader> UnchargedBeamShots =
        new Dictionary<SamusBeamCombination, SamusProjectileHeader>
        {
            [SamusBeamCombination.Power] = SamusProjectileHeader.UnchargedPower,
            [SamusBeamCombination.Wave] = SamusProjectileHeader.UnchargedWave,
            [SamusBeamCombination.Ice] = SamusProjectileHeader.UnchargedIce,
            [SamusBeamCombination.IceWave] = SamusProjectileHeader.UnchargedIceWave,
            [SamusBeamCombination.Spazer] = SamusProjectileHeader.UnchargedSpazer,
            [SamusBeamCombination.SpazerWave] = SamusProjectileHeader.UnchargedSpazerWave,
            [SamusBeamCombination.SpazerIce] = SamusProjectileHeader.UnchargedSpazerIce,
            [SamusBeamCombination.SpazerIceWave] = SamusProjectileHeader.UnchargedSpazerIceWave,
            [SamusBeamCombination.Plasma] = SamusProjectileHeader.UnchargedPlasma,
            [SamusBeamCombination.PlasmaWave] = SamusProjectileHeader.UnchargedPlasmaWave,
            [SamusBeamCombination.PlasmaIce] = SamusProjectileHeader.UnchargedPlasmaIce,
            [SamusBeamCombination.PlasmaIceWave] = SamusProjectileHeader.UnchargedPlasmaIceWave,
        };

    /// <summary>The definition each retail beam combination's charged shot starts from.</summary>
    internal static readonly IReadOnlyDictionary<SamusBeamCombination, SamusProjectileHeader> ChargedBeamShots =
        new Dictionary<SamusBeamCombination, SamusProjectileHeader>
        {
            [SamusBeamCombination.Power] = SamusProjectileHeader.ChargedPower,
            [SamusBeamCombination.Wave] = SamusProjectileHeader.ChargedWave,
            [SamusBeamCombination.Ice] = SamusProjectileHeader.ChargedIce,
            [SamusBeamCombination.IceWave] = SamusProjectileHeader.ChargedIceWave,
            [SamusBeamCombination.Spazer] = SamusProjectileHeader.ChargedSpazer,
            [SamusBeamCombination.SpazerWave] = SamusProjectileHeader.ChargedSpazerWave,
            [SamusBeamCombination.SpazerIce] = SamusProjectileHeader.ChargedSpazerIce,
            [SamusBeamCombination.SpazerIceWave] = SamusProjectileHeader.ChargedSpazerIceWave,
            [SamusBeamCombination.Plasma] = SamusProjectileHeader.ChargedPlasma,
            [SamusBeamCombination.PlasmaWave] = SamusProjectileHeader.ChargedPlasmaWave,
            [SamusBeamCombination.PlasmaIce] = SamusProjectileHeader.ChargedPlasmaIce,
            [SamusBeamCombination.PlasmaIceWave] = SamusProjectileHeader.ChargedPlasmaIceWave,
        };

    /// <summary>
    /// What each Spazer and Plasma combination fires: its uncharged shot is the charged shot of the combination without
    /// Spazer and Plasma, and its charged shot a missile, a Super Missile or a Power Bomb.
    /// </summary>
    internal static readonly UnauthoredBeamShots<SamusBeamCombination, SamusProjectileHeader> UnauthoredShots = new(
        new Dictionary<SamusBeamCombination, SamusProjectileHeader>
        {
            [SamusBeamCombination.SpazerPlasma] = SamusProjectileHeader.ChargedPower,
            [SamusBeamCombination.SpazerPlasmaWave] = SamusProjectileHeader.ChargedWave,
            [SamusBeamCombination.SpazerPlasmaIce] = SamusProjectileHeader.ChargedIce,
            [SamusBeamCombination.SpazerPlasmaIceWave] = SamusProjectileHeader.ChargedIceWave,
        },
        new Dictionary<SamusBeamCombination, SamusProjectileHeader>
        {
            [SamusBeamCombination.SpazerPlasma] = SamusProjectileHeader.Missile,
            [SamusBeamCombination.SpazerPlasmaWave] = SamusProjectileHeader.Missile,
            [SamusBeamCombination.SpazerPlasmaIce] = SamusProjectileHeader.SuperMissile,
            [SamusBeamCombination.SpazerPlasmaIceWave] = SamusProjectileHeader.PowerBomb,
        });

    /// <summary>The combination whose charged shot is the Murder Beam: all four beams.</summary>
    internal const SamusBeamCombination MurderBeam = SamusBeamCombination.SpazerPlasmaIceWave;

    /// <summary>
    /// The animation the Murder Beam's shot runs in each direction whose read names one, or none fired left; the four
    /// directions left out read a damage value as their animation.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusProjectileDirection, SamusProjectileSegment?> MurderBeamDirectionAnimations =
        new Dictionary<SamusProjectileDirection, SamusProjectileSegment?>
        {
            [SamusProjectileDirection.UpFacingRight] = SamusProjectileSegment.PowerBomb,
            [SamusProjectileDirection.Right] = SamusProjectileSegment.Bomb,
            [SamusProjectileDirection.DownFacingRight] = SamusProjectileSegment.BeamExplosion,
            [SamusProjectileDirection.DownLeft] = SamusProjectileSegment.MissileExplosion,
            [SamusProjectileDirection.Left] = null,
            [SamusProjectileDirection.UpLeft] = SamusProjectileSegment.BombExplosion,
        };

    /// <summary>The Murder Beam's shot's animation by direction (<see cref="MurderBeamDirectionAnimations"/>).</summary>
    internal static readonly MurderBeamDirectionAnimations<SamusProjectileDirection, SamusProjectileSegment> MurderBeamAnimations =
        new(MurderBeamDirectionAnimations);

    /// <summary>The definition each fired missile and placed bomb starts from, by its projectile family.</summary>
    internal static readonly IReadOnlyDictionary<SamusProjectileFamily, SamusProjectileHeader> NonBeamProjectiles =
        new Dictionary<SamusProjectileFamily, SamusProjectileHeader>
        {
            [SamusProjectileFamily.Missile] = SamusProjectileHeader.Missile,
            [SamusProjectileFamily.SuperMissile] = SamusProjectileHeader.SuperMissile,
            [SamusProjectileFamily.PowerBomb] = SamusProjectileHeader.PowerBomb,
            [SamusProjectileFamily.Bomb] = SamusProjectileHeader.Bomb,
        };

    /// <summary>
    /// The definition each special attack's particles start from, by the retail beam combination that fires it; the Ice
    /// attack's particles start from the charged Ice shot instead, and the other combinations have no particle definition.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusBeamCombination, SamusProjectileHeader> SpecialAttacks =
        new Dictionary<SamusBeamCombination, SamusProjectileHeader>
        {
            [SamusBeamCombination.Wave] = SamusProjectileHeader.WaveSBA,
            [SamusBeamCombination.Spazer] = SamusProjectileHeader.SpazerSBA,
            [SamusBeamCombination.SpazerWave] = SamusProjectileHeader.SpazerSBA,
            [SamusBeamCombination.Plasma] = SamusProjectileHeader.PlasmaSBA,
            [SamusBeamCombination.PlasmaWave] = SamusProjectileHeader.PlasmaSBA,
        };

    /// <summary>The definition each echo starts from.</summary>
    internal static readonly IReadOnlyDictionary<SamusBeamEcho, SamusProjectileHeader> Echoes =
        new Dictionary<SamusBeamEcho, SamusProjectileHeader>
        {
            [SamusBeamEcho.SpazerComboTrail] = SamusProjectileHeader.SpazerSBATrail,
            [SamusBeamEcho.ShinesparkEcho] = SamusProjectileHeader.ShinesparkEcho,
        };
}
