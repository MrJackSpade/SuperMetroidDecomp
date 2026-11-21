using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Projectile damage by kind, independent of the kinds' direction animations.</summary>
internal static partial class SamusProjectileDamageDefinitions
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

    /// <summary>
    /// What each projectile kind starts its projectile with: an uncharged or charged beam shot its beams' damage, and every
    /// other kind its own.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusProjectileKind, SamusProjectileStartingDamage> StartingDamage =
        new Dictionary<SamusProjectileKind, SamusProjectileStartingDamage>
        {
            [SamusProjectileKind.UnchargedPower] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.Power, Charged: false),
            [SamusProjectileKind.UnchargedSpazer] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.Spazer, Charged: false),
            [SamusProjectileKind.UnchargedSpazerIce] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.SpazerIce, Charged: false),
            [SamusProjectileKind.UnchargedSpazerIceWave] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.SpazerIceWave, Charged: false),
            [SamusProjectileKind.UnchargedPlasmaIceWave] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.PlasmaIceWave, Charged: false),
            [SamusProjectileKind.UnchargedIce] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.Ice, Charged: false),
            [SamusProjectileKind.UnchargedWave] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.Wave, Charged: false),
            [SamusProjectileKind.UnchargedPlasma] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.Plasma, Charged: false),
            [SamusProjectileKind.UnchargedIceWave] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.IceWave, Charged: false),
            [SamusProjectileKind.UnchargedSpazerWave] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.SpazerWave, Charged: false),
            [SamusProjectileKind.UnchargedPlasmaWave] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.PlasmaWave, Charged: false),
            [SamusProjectileKind.UnchargedPlasmaIce] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.PlasmaIce, Charged: false),
            [SamusProjectileKind.ChargedPower] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.Power, Charged: true),
            [SamusProjectileKind.ChargedSpazer] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.Spazer, Charged: true),
            [SamusProjectileKind.ChargedSpazerIce] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.SpazerIce, Charged: true),
            [SamusProjectileKind.ChargedSpazerIceWave] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.SpazerIceWave, Charged: true),
            [SamusProjectileKind.ChargedPlasmaIceWave] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.PlasmaIceWave, Charged: true),
            [SamusProjectileKind.ChargedIce] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.Ice, Charged: true),
            [SamusProjectileKind.ChargedPlasma] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.Plasma, Charged: true),
            [SamusProjectileKind.ChargedWave] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.Wave, Charged: true),
            [SamusProjectileKind.ChargedIceWave] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.IceWave, Charged: true),
            [SamusProjectileKind.ChargedSpazerWave] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.SpazerWave, Charged: true),
            [SamusProjectileKind.ChargedPlasmaIce] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.PlasmaIce, Charged: true),
            [SamusProjectileKind.ChargedPlasmaWave] = new SamusProjectileStartingDamage.BeamShot(SamusBeamCombination.PlasmaWave, Charged: true),
            [SamusProjectileKind.Missile] = new SamusProjectileStartingDamage.Own(100),
            [SamusProjectileKind.SuperMissile] = new SamusProjectileStartingDamage.Own(300),
            [SamusProjectileKind.SuperMissileLink] = new SamusProjectileStartingDamage.Own(300),
            [SamusProjectileKind.PowerBomb] = new SamusProjectileStartingDamage.Own(200),
            [SamusProjectileKind.Bomb] = new SamusProjectileStartingDamage.Own(30),
            [SamusProjectileKind.PlasmaSpecialAttack] = new SamusProjectileStartingDamage.Own(300),
            [SamusProjectileKind.WaveSpecialAttack] = new SamusProjectileStartingDamage.Own(300),
            [SamusProjectileKind.SpazerSpecialAttack] = new SamusProjectileStartingDamage.Own(300),
            [SamusProjectileKind.SpazerSpecialAttackTrail] = new SamusProjectileStartingDamage.Own(300),
            [SamusProjectileKind.ShinesparkEcho] = new SamusProjectileStartingDamage.Own(ShinesparkEchoDamage),
        };
}
