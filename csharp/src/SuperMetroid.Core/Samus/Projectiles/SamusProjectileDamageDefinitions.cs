using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Projectile damage by kind, independent of the kinds' direction animations.</summary>
internal static class SamusProjectileDamageDefinitions
{
    /// <summary>The damage a Shinespark echo starts with.</summary>
    internal const int ShinesparkEchoDamage = 4096;

    /// <summary>The damage a Hyper Beam shot deals.</summary>
    internal const int HyperBeamDamage = 1000;

    /// <summary>The damage a beam's or missile's explosion deals while it plays.</summary>
    internal const int ExplosionDamage = 8;

    /// <summary>A charged beam shot deals its beams' uncharged damage this many times over.</summary>
    internal const int ChargedBeamDamageMultiplier = 3;

    /// <summary>The damage an uncharged beam shot of each combination deals.</summary>
    internal static readonly IReadOnlyDictionary<SamusBeamCombination, int> UnchargedBeamDamage = new Dictionary<SamusBeamCombination, int>
    {
        [SamusBeamCombination.Power] = 20,
        [SamusBeamCombination.Ice] = 30,
        [SamusBeamCombination.Wave] = 50,
        [SamusBeamCombination.IceWave] = 60,
        [SamusBeamCombination.Spazer] = 40,
        [SamusBeamCombination.SpazerIce] = 60,
        [SamusBeamCombination.SpazerWave] = 70,
        [SamusBeamCombination.SpazerIceWave] = 100,
        [SamusBeamCombination.Plasma] = 150,
        [SamusBeamCombination.PlasmaIce] = 200,
        [SamusBeamCombination.PlasmaWave] = 250,
        [SamusBeamCombination.PlasmaIceWave] = 300,
    };

    /// <summary>The damage each projectile kind that is no beam shot starts with.</summary>
    internal static readonly IReadOnlyDictionary<SamusProjectileKind, int> NonBeamDamage = new Dictionary<SamusProjectileKind, int>
    {
        [SamusProjectileKind.Missile] = 100,
        [SamusProjectileKind.SuperMissile] = 300,
        [SamusProjectileKind.SuperMissileLink] = 300,
        [SamusProjectileKind.PowerBomb] = 200,
        [SamusProjectileKind.Bomb] = 30,
        [SamusProjectileKind.PlasmaSpecialAttack] = 300,
        [SamusProjectileKind.WaveSpecialAttack] = 300,
        [SamusProjectileKind.SpazerSpecialAttack] = 300,
        [SamusProjectileKind.SpazerSpecialAttackTrail] = 300,
        [SamusProjectileKind.ShinesparkEcho] = ShinesparkEchoDamage,
    };

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
