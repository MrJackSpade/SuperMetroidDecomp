using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Samus-library sound each beam combination fires with, uncharged and charged, and the missile sounds.
/// </summary>
public static class SamusProjectileSoundRoutingDefinitions
{
    /// <summary>The missile firing sound.</summary>
    internal static readonly SoundEffectId MissileRequest = SamusSound.Missile;
    /// <summary>The Super Missile firing sound.</summary>
    internal static readonly SoundEffectId SuperMissileRequest = SamusSound.SuperMissile;

    /// <summary>Each authored beam combination's uncharged firing sound.</summary>
    internal static readonly IReadOnlyDictionary<SamusBeamCombination, SoundEffectId> Uncharged = new Dictionary<SamusBeamCombination, SoundEffectId>
    {
        [SamusBeamCombination.Power] = SamusSound.UnchargedPowerBeam,
        [SamusBeamCombination.Ice] = SamusSound.UnchargedIceBeam,
        [SamusBeamCombination.Wave] = SamusSound.UnchargedWaveBeam,
        [SamusBeamCombination.IceWave] = SamusSound.UnchargedIceWaveBeam,
        [SamusBeamCombination.Spazer] = SamusSound.UnchargedSpazer,
        [SamusBeamCombination.SpazerIce] = SamusSound.UnchargedSpazerIce,
        [SamusBeamCombination.SpazerIceWave] = SamusSound.UnchargedSpazerIceWave,
        [SamusBeamCombination.SpazerWave] = SamusSound.UnchargedSpazerWave,
        [SamusBeamCombination.Plasma] = SamusSound.UnchargedPlasma,
        [SamusBeamCombination.PlasmaIce] = SamusSound.UnchargedPlasmaIce,
        [SamusBeamCombination.PlasmaIceWave] = SamusSound.UnchargedPlasmaIceWave,
        [SamusBeamCombination.PlasmaWave] = SamusSound.UnchargedPlasmaWave,
    };

    /// <summary>Each authored beam combination's charged firing sound.</summary>
    internal static readonly Dictionary<SamusBeamCombination, SoundEffectId> Charged = new Dictionary<SamusBeamCombination, SoundEffectId>
    {
        [SamusBeamCombination.Power] = SamusSound.ChargedPowerBeam,
        [SamusBeamCombination.Ice] = SamusSound.ChargedIceBeam,
        [SamusBeamCombination.Wave] = SamusSound.ChargedWaveBeam,
        [SamusBeamCombination.IceWave] = SamusSound.ChargedIceWaveBeam,
        [SamusBeamCombination.Spazer] = SamusSound.ChargedSpazer,
        [SamusBeamCombination.SpazerIce] = SamusSound.ChargedSpazerIce,
        [SamusBeamCombination.SpazerIceWave] = SamusSound.ChargedSpazerIceWave,
        [SamusBeamCombination.SpazerWave] = SamusSound.ChargedSpazerWave,
        [SamusBeamCombination.Plasma] = SamusSound.ChargedPlasma,
        [SamusBeamCombination.PlasmaIce] = SamusSound.ChargedPlasmaIce,
        [SamusBeamCombination.PlasmaIceWave] = SamusSound.ChargedPlasmaIceWave,
        [SamusBeamCombination.PlasmaWave] = SamusSound.ChargedPlasmaWave,
    };

    /// <summary>
    /// The sound each Spazer and Plasma combination fires with: uncharged, the charged sound of the combination without
    /// Spazer and Plasma; charged, no sound for the plain and all-beams pairings, the missile's for the Wave pairing and
    /// the Super Missile's for the Ice pairing.
    /// </summary>
    internal static readonly UnauthoredBeamSounds<SamusBeamCombination, SoundEffectId> UnauthoredSounds = new(
        new Dictionary<SamusBeamCombination, SoundEffectId?>
        {
            [SamusBeamCombination.SpazerPlasma] = SamusSound.ChargedPowerBeam,
            [SamusBeamCombination.SpazerPlasmaWave] = SamusSound.ChargedWaveBeam,
            [SamusBeamCombination.SpazerPlasmaIce] = SamusSound.ChargedIceBeam,
            [SamusBeamCombination.SpazerPlasmaIceWave] = SamusSound.ChargedIceWaveBeam,
        },
        new Dictionary<SamusBeamCombination, SoundEffectId?>
        {
            [SamusBeamCombination.SpazerPlasma] = null,
            [SamusBeamCombination.SpazerPlasmaWave] = SamusSound.Missile,
            [SamusBeamCombination.SpazerPlasmaIce] = SamusSound.SuperMissile,
            [SamusBeamCombination.SpazerPlasmaIceWave] = null,
        });
}
