using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Which projectile kind each shot, bomb, special attack and echo is, by what fires it: a beam combination and
/// whether the shot was charged, a projectile family, or an echo.
/// </summary>
internal static class SamusProjectileSelectionDefinitions
{
    /// <summary>The kind of each retail beam combination's uncharged shot.</summary>
    internal static readonly IReadOnlyDictionary<SamusBeamCombination, SamusProjectileKind> UnchargedBeamShots =
        new Dictionary<SamusBeamCombination, SamusProjectileKind>
        {
            [SamusBeamCombination.Power] = SamusProjectileKind.UnchargedPower,
            [SamusBeamCombination.Wave] = SamusProjectileKind.UnchargedWave,
            [SamusBeamCombination.Ice] = SamusProjectileKind.UnchargedIce,
            [SamusBeamCombination.IceWave] = SamusProjectileKind.UnchargedIceWave,
            [SamusBeamCombination.Spazer] = SamusProjectileKind.UnchargedSpazer,
            [SamusBeamCombination.SpazerWave] = SamusProjectileKind.UnchargedSpazerWave,
            [SamusBeamCombination.SpazerIce] = SamusProjectileKind.UnchargedSpazerIce,
            [SamusBeamCombination.SpazerIceWave] = SamusProjectileKind.UnchargedSpazerIceWave,
            [SamusBeamCombination.Plasma] = SamusProjectileKind.UnchargedPlasma,
            [SamusBeamCombination.PlasmaWave] = SamusProjectileKind.UnchargedPlasmaWave,
            [SamusBeamCombination.PlasmaIce] = SamusProjectileKind.UnchargedPlasmaIce,
            [SamusBeamCombination.PlasmaIceWave] = SamusProjectileKind.UnchargedPlasmaIceWave,
        };

    /// <summary>The kind of each retail beam combination's charged shot.</summary>
    internal static readonly IReadOnlyDictionary<SamusBeamCombination, SamusProjectileKind> ChargedBeamShots =
        new Dictionary<SamusBeamCombination, SamusProjectileKind>
        {
            [SamusBeamCombination.Power] = SamusProjectileKind.ChargedPower,
            [SamusBeamCombination.Wave] = SamusProjectileKind.ChargedWave,
            [SamusBeamCombination.Ice] = SamusProjectileKind.ChargedIce,
            [SamusBeamCombination.IceWave] = SamusProjectileKind.ChargedIceWave,
            [SamusBeamCombination.Spazer] = SamusProjectileKind.ChargedSpazer,
            [SamusBeamCombination.SpazerWave] = SamusProjectileKind.ChargedSpazerWave,
            [SamusBeamCombination.SpazerIce] = SamusProjectileKind.ChargedSpazerIce,
            [SamusBeamCombination.SpazerIceWave] = SamusProjectileKind.ChargedSpazerIceWave,
            [SamusBeamCombination.Plasma] = SamusProjectileKind.ChargedPlasma,
            [SamusBeamCombination.PlasmaWave] = SamusProjectileKind.ChargedPlasmaWave,
            [SamusBeamCombination.PlasmaIce] = SamusProjectileKind.ChargedPlasmaIce,
            [SamusBeamCombination.PlasmaIceWave] = SamusProjectileKind.ChargedPlasmaIceWave,
        };

    /// <summary>
    /// What each Spazer and Plasma combination fires: its uncharged shot is the charged shot of the combination without
    /// Spazer and Plasma, and its charged shot a missile, a Super Missile or a Power Bomb.
    /// </summary>
    internal static readonly UnauthoredBeamShots<SamusBeamCombination, SamusProjectileKind> UnauthoredShots = new(
        new Dictionary<SamusBeamCombination, SamusProjectileKind>
        {
            [SamusBeamCombination.SpazerPlasma] = SamusProjectileKind.ChargedPower,
            [SamusBeamCombination.SpazerPlasmaWave] = SamusProjectileKind.ChargedWave,
            [SamusBeamCombination.SpazerPlasmaIce] = SamusProjectileKind.ChargedIce,
            [SamusBeamCombination.SpazerPlasmaIceWave] = SamusProjectileKind.ChargedIceWave,
        },
        new Dictionary<SamusBeamCombination, SamusProjectileKind>
        {
            [SamusBeamCombination.SpazerPlasma] = SamusProjectileKind.Missile,
            [SamusBeamCombination.SpazerPlasmaWave] = SamusProjectileKind.Missile,
            [SamusBeamCombination.SpazerPlasmaIce] = SamusProjectileKind.SuperMissile,
            [SamusBeamCombination.SpazerPlasmaIceWave] = SamusProjectileKind.PowerBomb,
        });

    /// <summary>The combination whose charged shot is the Murder Beam: all four beams.</summary>
    internal const SamusBeamCombination MurderBeam = SamusBeamCombination.SpazerPlasmaIceWave;

    /// <summary>
    /// The animation the Murder Beam's shot runs in each direction whose read names one, or none fired left; the four
    /// directions left out read a damage value as their animation.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusProjectileDirection, SamusProjectileProgram?> MurderBeamDirectionAnimations =
        new Dictionary<SamusProjectileDirection, SamusProjectileProgram?>
        {
            [SamusProjectileDirection.UpFacingRight] = SamusProjectileProgram.PowerBomb,
            [SamusProjectileDirection.Right] = SamusProjectileProgram.Bomb,
            [SamusProjectileDirection.DownFacingRight] = SamusProjectileProgram.BeamExplosion,
            [SamusProjectileDirection.DownLeft] = SamusProjectileProgram.MissileExplosion,
            [SamusProjectileDirection.Left] = null,
            [SamusProjectileDirection.UpLeft] = SamusProjectileProgram.BombExplosion,
        };

    /// <summary>The Murder Beam's shot's animation by direction (<see cref="MurderBeamDirectionAnimations"/>).</summary>
    internal static readonly MurderBeamDirectionPrograms<SamusProjectileDirection, SamusProjectileProgram> MurderBeamPrograms =
        new(MurderBeamDirectionAnimations);

    /// <summary>The kind of each fired missile and placed bomb, by its projectile family.</summary>
    internal static readonly IReadOnlyDictionary<SamusProjectileFamily, SamusProjectileKind> NonBeamProjectiles =
        new Dictionary<SamusProjectileFamily, SamusProjectileKind>
        {
            [SamusProjectileFamily.Missile] = SamusProjectileKind.Missile,
            [SamusProjectileFamily.SuperMissile] = SamusProjectileKind.SuperMissile,
            [SamusProjectileFamily.PowerBomb] = SamusProjectileKind.PowerBomb,
            [SamusProjectileFamily.Bomb] = SamusProjectileKind.Bomb,
        };

    /// <summary>
    /// The kind of each special attack's particles, by the retail beam combination that fires it; the Ice attack's
    /// particles are charged Ice shots instead, and the other combinations' attacks have no particles of their own.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusBeamCombination, SamusProjectileKind> SpecialAttacks =
        new Dictionary<SamusBeamCombination, SamusProjectileKind>
        {
            [SamusBeamCombination.Wave] = SamusProjectileKind.WaveSBA,
            [SamusBeamCombination.Spazer] = SamusProjectileKind.SpazerSBA,
            [SamusBeamCombination.SpazerWave] = SamusProjectileKind.SpazerSBA,
            [SamusBeamCombination.Plasma] = SamusProjectileKind.PlasmaSBA,
            [SamusBeamCombination.PlasmaWave] = SamusProjectileKind.PlasmaSBA,
        };

    /// <summary>The kind of each echo.</summary>
    internal static readonly IReadOnlyDictionary<SamusBeamEcho, SamusProjectileKind> Echoes =
        new Dictionary<SamusBeamEcho, SamusProjectileKind>
        {
            [SamusBeamEcho.SpazerComboTrail] = SamusProjectileKind.SpazerSBATrail,
            [SamusBeamEcho.ShinesparkEcho] = SamusProjectileKind.ShinesparkEcho,
        };
}
