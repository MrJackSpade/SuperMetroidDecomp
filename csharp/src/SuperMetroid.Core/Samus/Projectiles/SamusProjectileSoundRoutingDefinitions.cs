using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The library-one sound each beam combination fires with, uncharged and charged, and the missile sounds.
/// </summary>
/// <remarks>
/// The cartridge indexes its uncharged and charged sound tables with the raw four-bit beam combination. Combinations
/// twelve through fifteen therefore read past the twelve authored combinations: an uncharged shot plays the first
/// charged beam family's sounds, and a charged shot plays the following non-beam row (no sound, missile, Super Missile,
/// no sound). Those four overreads are gameplay-visible advanced-technique behaviour and are kept deliberately.
/// </remarks>
public static class SamusProjectileSoundRoutingDefinitions
{
    /// <summary>The missile firing sound, also what charged combination thirteen reads.</summary>
    internal static readonly SoundEffectId MissileRequest = Library1Sound.Missile;
    /// <summary>The Super Missile firing sound, also what charged combination fourteen reads.</summary>
    internal static readonly SoundEffectId SuperMissileRequest = Library1Sound.SuperMissile;

    /// <summary>Each authored beam combination's uncharged firing sound.</summary>
    private static readonly IReadOnlyDictionary<SamusBeamCombination, SoundEffectId> Uncharged = new Dictionary<SamusBeamCombination, SoundEffectId>
    {
        [SamusBeamCombination.Power] = Library1Sound.UnchargedPowerBeam,
        [SamusBeamCombination.Ice] = Library1Sound.UnchargedIceBeam,
        [SamusBeamCombination.Wave] = Library1Sound.UnchargedWaveBeam,
        [SamusBeamCombination.IceWave] = Library1Sound.UnchargedIceWaveBeam,
        [SamusBeamCombination.Spazer] = Library1Sound.UnchargedSpazer,
        [SamusBeamCombination.SpazerIce] = Library1Sound.UnchargedSpazerIce,
        [SamusBeamCombination.SpazerIceWave] = Library1Sound.UnchargedSpazerIceWave,
        [SamusBeamCombination.SpazerWave] = Library1Sound.UnchargedSpazerWave,
        [SamusBeamCombination.Plasma] = Library1Sound.UnchargedPlasma,
        [SamusBeamCombination.PlasmaIce] = Library1Sound.UnchargedPlasmaIce,
        [SamusBeamCombination.PlasmaIceWave] = Library1Sound.UnchargedPlasmaIceWave,
        [SamusBeamCombination.PlasmaWave] = Library1Sound.UnchargedPlasmaWave,
    };

    /// <summary>Each authored beam combination's charged firing sound.</summary>
    private static readonly Dictionary<SamusBeamCombination, SoundEffectId> Charged = new Dictionary<SamusBeamCombination, SoundEffectId>
    {
        [SamusBeamCombination.Power] = Library1Sound.ChargedPowerBeam,
        [SamusBeamCombination.Ice] = Library1Sound.ChargedIceBeam,
        [SamusBeamCombination.Wave] = Library1Sound.ChargedWaveBeam,
        [SamusBeamCombination.IceWave] = Library1Sound.ChargedIceWaveBeam,
        [SamusBeamCombination.Spazer] = Library1Sound.ChargedSpazer,
        [SamusBeamCombination.SpazerIce] = Library1Sound.ChargedSpazerIce,
        [SamusBeamCombination.SpazerIceWave] = Library1Sound.ChargedSpazerIceWave,
        [SamusBeamCombination.SpazerWave] = Library1Sound.ChargedSpazerWave,
        [SamusBeamCombination.Plasma] = Library1Sound.ChargedPlasma,
        [SamusBeamCombination.PlasmaIce] = Library1Sound.ChargedPlasmaIce,
        [SamusBeamCombination.PlasmaIceWave] = Library1Sound.ChargedPlasmaIceWave,
        [SamusBeamCombination.PlasmaWave] = Library1Sound.ChargedPlasmaWave,
    };

    /// <summary>The library-one sound a shot of <paramref name="combination"/> fires with, or none.</summary>
    public static SoundEffectId? Resolve(bool charged, SamusBeamCombination combination) => combination switch
    {
        SamusBeamCombination.Power or SamusBeamCombination.Wave or SamusBeamCombination.Ice or
            SamusBeamCombination.IceWave or SamusBeamCombination.Spazer or SamusBeamCombination.SpazerWave or
            SamusBeamCombination.SpazerIce or SamusBeamCombination.SpazerIceWave or SamusBeamCombination.Plasma or
            SamusBeamCombination.PlasmaWave or SamusBeamCombination.PlasmaIce or SamusBeamCombination.PlasmaIceWave =>
            (charged ? Charged : Uncharged)[combination],
        // The uncharged overread reaches the first charged beam family. The
        // charged overread reaches the non-beam no-sound/missile/super/no-sound row.
        SamusBeamCombination.SpazerPlasma => charged ? null : Charged[SamusBeamCombination.Power],
        SamusBeamCombination.SpazerPlasmaWave => charged ? MissileRequest : Charged[SamusBeamCombination.Wave],
        SamusBeamCombination.SpazerPlasmaIce => charged ? SuperMissileRequest : Charged[SamusBeamCombination.Ice],
        SamusBeamCombination.SpazerPlasmaIceWave => charged ? null : Charged[SamusBeamCombination.IceWave],
        _ => throw new ArgumentOutOfRangeException(nameof(combination), combination, "Undefined beam combination."),
    };
}
