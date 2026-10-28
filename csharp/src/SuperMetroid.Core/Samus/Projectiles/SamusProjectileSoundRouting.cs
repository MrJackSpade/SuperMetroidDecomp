using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Samus.Projectiles.SamusProjectileSoundRoutingDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Which sound each projectile event plays.</summary>
public static class SamusProjectileSoundRouting
{
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
