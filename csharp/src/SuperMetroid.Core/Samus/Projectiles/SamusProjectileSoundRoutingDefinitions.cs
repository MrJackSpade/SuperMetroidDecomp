using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Samus-library sound each beam combination fires with, uncharged and charged, and the missile sounds.
/// </summary>
/// <remarks>
/// The game indexes its uncharged and charged sound tables with the raw four-bit beam combination. Combinations
/// twelve through fifteen therefore read past the twelve authored combinations: an uncharged shot plays the first
/// charged beam family's sounds, and a charged shot plays the following non-beam row (no sound, missile, Super Missile,
/// no sound). Those four overreads are gameplay-visible advanced-technique behaviour and are kept deliberately.
/// </remarks>
public static class SamusProjectileSoundRoutingDefinitions
{
    /// <summary>The missile firing sound, also what charged combination thirteen reads.</summary>
    internal static readonly SoundEffectId MissileRequest = SamusSound.Missile;
    /// <summary>The Super Missile firing sound, also what charged combination fourteen reads.</summary>
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
}
