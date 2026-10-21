using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Projectile damage by header, independent of the headers' direction programs.</summary>
internal static class SamusProjectileDamageDefinitions
{
    /// <summary>The damage <paramref name="header"/> starts its projectile with.</summary>
    internal static ushort Read(SamusProjectileHeader header) => header switch
    {
        SamusProjectileHeader.Missile => 100,
        SamusProjectileHeader.SuperMissile or SamusProjectileHeader.SuperMissileLink => 300,
        SamusProjectileHeader.PowerBomb => 200,
        SamusProjectileHeader.Bomb => 30,
        SamusProjectileHeader.BeamExplosion or SamusProjectileHeader.MissileExplosion or
            SamusProjectileHeader.SuperMissileExplosion => 8,
        SamusProjectileHeader.BombExplosion or SamusProjectileHeader.Projectile27 => 0,
        SamusProjectileHeader.PlasmaSBA or SamusProjectileHeader.WaveSBA or SamusProjectileHeader.SpazerSBA or
            SamusProjectileHeader.SpazerSBATrail => 300,
        SamusProjectileHeader.Projectile25 => 0xf000,
        SamusProjectileHeader.ShinesparkEcho => 0x1000,
        SamusProjectileHeader.UnchargedPower or SamusProjectileHeader.UnchargedSpazer or SamusProjectileHeader.UnchargedSpazerIce or SamusProjectileHeader.UnchargedSpazerIceWave or
            SamusProjectileHeader.UnchargedPlasmaIceWave or SamusProjectileHeader.UnchargedIce or SamusProjectileHeader.UnchargedWave or SamusProjectileHeader.UnchargedPlasma or
            SamusProjectileHeader.UnchargedIceWave or SamusProjectileHeader.UnchargedSpazerWave or SamusProjectileHeader.UnchargedPlasmaWave or SamusProjectileHeader.UnchargedPlasmaIce or
            SamusProjectileHeader.ChargedPower or SamusProjectileHeader.ChargedSpazer or SamusProjectileHeader.ChargedSpazerIce or SamusProjectileHeader.ChargedSpazerIceWave or
            SamusProjectileHeader.ChargedPlasmaIceWave or SamusProjectileHeader.ChargedIce or SamusProjectileHeader.ChargedPlasma or SamusProjectileHeader.ChargedWave or
            SamusProjectileHeader.ChargedIceWave or SamusProjectileHeader.ChargedSpazerWave or SamusProjectileHeader.ChargedPlasmaIce or SamusProjectileHeader.ChargedPlasmaWave => BeamDamage(BeamIdentity(header)),
        _ => throw new ArgumentOutOfRangeException(nameof(header), header, "Undefined projectile header."),
    };

    /// <summary>
    /// $93:8431..8640 ProjectileDataTable_Uncharged_* / Charged_*: the charge state and beams of a beam header. The charged
    /// half swaps Wave/Plasma and Plasma-Wave/Plasma-Ice order.
    /// </summary>
    internal static SamusBeamIdentity BeamIdentity(SamusProjectileHeader header) => new(
        header is SamusProjectileHeader.ChargedPower or SamusProjectileHeader.ChargedSpazer or SamusProjectileHeader.ChargedSpazerIce or SamusProjectileHeader.ChargedSpazerIceWave or
        SamusProjectileHeader.ChargedPlasmaIceWave or SamusProjectileHeader.ChargedIce or SamusProjectileHeader.ChargedPlasma or SamusProjectileHeader.ChargedWave or
        SamusProjectileHeader.ChargedIceWave or SamusProjectileHeader.ChargedSpazerWave or SamusProjectileHeader.ChargedPlasmaIce or SamusProjectileHeader.ChargedPlasmaWave, Beam(header));

    /// <summary>The beams a beam header fires.</summary>
    private static SamusBeamCombination Beam(SamusProjectileHeader header) => header switch
    {
        SamusProjectileHeader.UnchargedPower => SamusBeamCombination.Power,
        SamusProjectileHeader.UnchargedSpazer => SamusBeamCombination.Spazer,
        SamusProjectileHeader.UnchargedSpazerIce => SamusBeamCombination.SpazerIce,
        SamusProjectileHeader.UnchargedSpazerIceWave => SamusBeamCombination.SpazerIceWave,
        SamusProjectileHeader.UnchargedPlasmaIceWave => SamusBeamCombination.PlasmaIceWave,
        SamusProjectileHeader.UnchargedIce => SamusBeamCombination.Ice,
        SamusProjectileHeader.UnchargedWave => SamusBeamCombination.Wave,
        SamusProjectileHeader.UnchargedPlasma => SamusBeamCombination.Plasma,
        SamusProjectileHeader.UnchargedIceWave => SamusBeamCombination.IceWave,
        SamusProjectileHeader.UnchargedSpazerWave => SamusBeamCombination.SpazerWave,
        SamusProjectileHeader.UnchargedPlasmaWave => SamusBeamCombination.PlasmaWave,
        SamusProjectileHeader.UnchargedPlasmaIce => SamusBeamCombination.PlasmaIce,
        SamusProjectileHeader.ChargedPower => SamusBeamCombination.Power,
        SamusProjectileHeader.ChargedSpazer => SamusBeamCombination.Spazer,
        SamusProjectileHeader.ChargedSpazerIce => SamusBeamCombination.SpazerIce,
        SamusProjectileHeader.ChargedSpazerIceWave => SamusBeamCombination.SpazerIceWave,
        SamusProjectileHeader.ChargedPlasmaIceWave => SamusBeamCombination.PlasmaIceWave,
        SamusProjectileHeader.ChargedIce => SamusBeamCombination.Ice,
        SamusProjectileHeader.ChargedPlasma => SamusBeamCombination.Plasma,
        SamusProjectileHeader.ChargedWave => SamusBeamCombination.Wave,
        SamusProjectileHeader.ChargedIceWave => SamusBeamCombination.IceWave,
        SamusProjectileHeader.ChargedSpazerWave => SamusBeamCombination.SpazerWave,
        SamusProjectileHeader.ChargedPlasmaIce => SamusBeamCombination.PlasmaIce,
        SamusProjectileHeader.ChargedPlasmaWave => SamusBeamCombination.PlasmaWave,
        SamusProjectileHeader.Missile or SamusProjectileHeader.SuperMissile or SamusProjectileHeader.SuperMissileLink or SamusProjectileHeader.PowerBomb or
            SamusProjectileHeader.Bomb or SamusProjectileHeader.BeamExplosion or SamusProjectileHeader.MissileExplosion or SamusProjectileHeader.BombExplosion or
            SamusProjectileHeader.PlasmaSBA or SamusProjectileHeader.WaveSBA or SamusProjectileHeader.SpazerSBA or SamusProjectileHeader.SuperMissileExplosion or
            SamusProjectileHeader.Projectile25 or SamusProjectileHeader.SpazerSBATrail or SamusProjectileHeader.ShinesparkEcho or SamusProjectileHeader.Projectile27 =>
            throw new ArgumentOutOfRangeException(nameof(header), header, "Only the twenty-four beam headers name beams."),
        _ => throw new ArgumentOutOfRangeException(nameof(header), header, "Undefined projectile header."),
    };

    /// <summary>Maps a beam header to its native base damage and triples charged shots.</summary>
    private static ushort BeamDamage(SamusBeamIdentity identity)
    {
        var (charged, beam) = identity;
        int damage = beam switch
        {
            SamusBeamCombination.Power => 20,
            SamusBeamCombination.Ice => 30,
            SamusBeamCombination.Wave => 50,
            SamusBeamCombination.IceWave => 60,
            SamusBeamCombination.Spazer => 40,
            SamusBeamCombination.SpazerIce => 60,
            SamusBeamCombination.SpazerWave => 70,
            SamusBeamCombination.SpazerIceWave => 100,
            SamusBeamCombination.Plasma => 150,
            SamusBeamCombination.PlasmaIce => 200,
            SamusBeamCombination.PlasmaWave => 250,
            SamusBeamCombination.PlasmaIceWave => 300,
            SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaWave or
                SamusBeamCombination.SpazerPlasmaIce or SamusBeamCombination.SpazerPlasmaIceWave =>
                throw new InvalidOperationException($"{beam} has no beam header."),
            _ => throw new ArgumentOutOfRangeException(nameof(identity), beam, "Undefined beam combination."),
        };
        return (ushort)(damage * (charged ? 3 : 1));
    }
}
