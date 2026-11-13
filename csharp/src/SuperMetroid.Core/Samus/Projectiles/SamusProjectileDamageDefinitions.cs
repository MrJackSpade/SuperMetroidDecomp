using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Projectile damage by kind, independent of the kinds' direction programs.</summary>
internal static class SamusProjectileDamageDefinitions
{
    /// <summary>The damage a Shinespark echo starts with.</summary>
    internal const int ShinesparkEchoDamage = 4096;

    /// <summary>The beams each beam shot kind fires; Beam looks it up.</summary>
    internal static readonly IReadOnlyDictionary<SamusProjectileKind, SamusBeamCombination> Beams = new Dictionary<SamusProjectileKind, SamusBeamCombination>
    {
        [SamusProjectileKind.UnchargedPower] = SamusBeamCombination.Power,
        [SamusProjectileKind.UnchargedSpazer] = SamusBeamCombination.Spazer,
        [SamusProjectileKind.UnchargedSpazerIce] = SamusBeamCombination.SpazerIce,
        [SamusProjectileKind.UnchargedSpazerIceWave] = SamusBeamCombination.SpazerIceWave,
        [SamusProjectileKind.UnchargedPlasmaIceWave] = SamusBeamCombination.PlasmaIceWave,
        [SamusProjectileKind.UnchargedIce] = SamusBeamCombination.Ice,
        [SamusProjectileKind.UnchargedWave] = SamusBeamCombination.Wave,
        [SamusProjectileKind.UnchargedPlasma] = SamusBeamCombination.Plasma,
        [SamusProjectileKind.UnchargedIceWave] = SamusBeamCombination.IceWave,
        [SamusProjectileKind.UnchargedSpazerWave] = SamusBeamCombination.SpazerWave,
        [SamusProjectileKind.UnchargedPlasmaWave] = SamusBeamCombination.PlasmaWave,
        [SamusProjectileKind.UnchargedPlasmaIce] = SamusBeamCombination.PlasmaIce,
        [SamusProjectileKind.ChargedPower] = SamusBeamCombination.Power,
        [SamusProjectileKind.ChargedSpazer] = SamusBeamCombination.Spazer,
        [SamusProjectileKind.ChargedSpazerIce] = SamusBeamCombination.SpazerIce,
        [SamusProjectileKind.ChargedSpazerIceWave] = SamusBeamCombination.SpazerIceWave,
        [SamusProjectileKind.ChargedPlasmaIceWave] = SamusBeamCombination.PlasmaIceWave,
        [SamusProjectileKind.ChargedIce] = SamusBeamCombination.Ice,
        [SamusProjectileKind.ChargedPlasma] = SamusBeamCombination.Plasma,
        [SamusProjectileKind.ChargedWave] = SamusBeamCombination.Wave,
        [SamusProjectileKind.ChargedIceWave] = SamusBeamCombination.IceWave,
        [SamusProjectileKind.ChargedSpazerWave] = SamusBeamCombination.SpazerWave,
        [SamusProjectileKind.ChargedPlasmaIce] = SamusBeamCombination.PlasmaIce,
        [SamusProjectileKind.ChargedPlasmaWave] = SamusBeamCombination.PlasmaWave,
    };
}
