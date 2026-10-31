using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Samus.Projectiles.SamusProjectileDamageDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The damage each projectile header starts its projectile with.</summary>
internal static class SamusProjectileDamage
{
    /// <summary>The damage <paramref name="header"/> starts its projectile with.</summary>
    internal static int Read(SamusProjectileHeader header) => header switch
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
        SamusProjectileHeader.Projectile25 => Projectile25Damage,
        SamusProjectileHeader.ShinesparkEcho => ShinesparkEchoDamage,
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
    private static SamusBeamCombination Beam(SamusProjectileHeader header) =>
        Beams.TryGetValue(header, out SamusBeamCombination value) ? value : throw new ArgumentOutOfRangeException(nameof(header), header, "Undefined projectile header.");

    /// <summary>Maps a beam header to its native base damage and triples charged shots.</summary>
    private static int BeamDamage(SamusBeamIdentity identity)
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
        return WordWrap.Unsigned(damage * (charged ? 3 : 1));
    }
}
