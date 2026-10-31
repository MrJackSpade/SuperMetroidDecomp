using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Projectile damage by header, independent of the headers' direction programs.</summary>
internal static class SamusProjectileDamageDefinitions
{
    /// <summary>The damage unused projectile $25 starts with.</summary>
    [SuppressMessage("Design", "SME6447", Justification =
        "A projectile's damage, 61440 hit points; damage is a plain count that no typed fixed-point or address form expresses.")]
    internal const int Projectile25Damage = 61440;
    /// <summary>The damage a Shinespark echo starts with.</summary>
    internal const int ShinesparkEchoDamage = 0x1000;

    /// <summary>The value each key selects; Beam looks it up.</summary>
    internal static readonly IReadOnlyDictionary<SamusProjectileHeader, SamusBeamCombination> Beams = new Dictionary<SamusProjectileHeader, SamusBeamCombination>
    {
        [SamusProjectileHeader.UnchargedPower] = SamusBeamCombination.Power,
        [SamusProjectileHeader.UnchargedSpazer] = SamusBeamCombination.Spazer,
        [SamusProjectileHeader.UnchargedSpazerIce] = SamusBeamCombination.SpazerIce,
        [SamusProjectileHeader.UnchargedSpazerIceWave] = SamusBeamCombination.SpazerIceWave,
        [SamusProjectileHeader.UnchargedPlasmaIceWave] = SamusBeamCombination.PlasmaIceWave,
        [SamusProjectileHeader.UnchargedIce] = SamusBeamCombination.Ice,
        [SamusProjectileHeader.UnchargedWave] = SamusBeamCombination.Wave,
        [SamusProjectileHeader.UnchargedPlasma] = SamusBeamCombination.Plasma,
        [SamusProjectileHeader.UnchargedIceWave] = SamusBeamCombination.IceWave,
        [SamusProjectileHeader.UnchargedSpazerWave] = SamusBeamCombination.SpazerWave,
        [SamusProjectileHeader.UnchargedPlasmaWave] = SamusBeamCombination.PlasmaWave,
        [SamusProjectileHeader.UnchargedPlasmaIce] = SamusBeamCombination.PlasmaIce,
        [SamusProjectileHeader.ChargedPower] = SamusBeamCombination.Power,
        [SamusProjectileHeader.ChargedSpazer] = SamusBeamCombination.Spazer,
        [SamusProjectileHeader.ChargedSpazerIce] = SamusBeamCombination.SpazerIce,
        [SamusProjectileHeader.ChargedSpazerIceWave] = SamusBeamCombination.SpazerIceWave,
        [SamusProjectileHeader.ChargedPlasmaIceWave] = SamusBeamCombination.PlasmaIceWave,
        [SamusProjectileHeader.ChargedIce] = SamusBeamCombination.Ice,
        [SamusProjectileHeader.ChargedPlasma] = SamusBeamCombination.Plasma,
        [SamusProjectileHeader.ChargedWave] = SamusBeamCombination.Wave,
        [SamusProjectileHeader.ChargedIceWave] = SamusBeamCombination.IceWave,
        [SamusProjectileHeader.ChargedSpazerWave] = SamusBeamCombination.SpazerWave,
        [SamusProjectileHeader.ChargedPlasmaIce] = SamusBeamCombination.PlasmaIce,
        [SamusProjectileHeader.ChargedPlasmaWave] = SamusBeamCombination.PlasmaWave,
    };
}
