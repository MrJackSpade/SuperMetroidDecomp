using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Samus.Projectiles.SamusProjectileDamageDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The damage each projectile kind starts its projectile with.</summary>
internal static class SamusProjectileDamage
{
    /// <summary>The damage a projectile of <paramref name="kind"/> starts with.</summary>
    internal static int Read(SamusProjectileKind kind) => kind switch
    {
        SamusProjectileKind.Missile => 100,
        SamusProjectileKind.SuperMissile or SamusProjectileKind.SuperMissileLink => 300,
        SamusProjectileKind.PowerBomb => 200,
        SamusProjectileKind.Bomb => 30,
        SamusProjectileKind.PlasmaSBA or SamusProjectileKind.WaveSBA or SamusProjectileKind.SpazerSBA or
            SamusProjectileKind.SpazerSBATrail => 300,
        SamusProjectileKind.ShinesparkEcho => ShinesparkEchoDamage,
        SamusProjectileKind.UnchargedPower or SamusProjectileKind.UnchargedSpazer or SamusProjectileKind.UnchargedSpazerIce or SamusProjectileKind.UnchargedSpazerIceWave or
            SamusProjectileKind.UnchargedPlasmaIceWave or SamusProjectileKind.UnchargedIce or SamusProjectileKind.UnchargedWave or SamusProjectileKind.UnchargedPlasma or
            SamusProjectileKind.UnchargedIceWave or SamusProjectileKind.UnchargedSpazerWave or SamusProjectileKind.UnchargedPlasmaWave or SamusProjectileKind.UnchargedPlasmaIce or
            SamusProjectileKind.ChargedPower or SamusProjectileKind.ChargedSpazer or SamusProjectileKind.ChargedSpazerIce or SamusProjectileKind.ChargedSpazerIceWave or
            SamusProjectileKind.ChargedPlasmaIceWave or SamusProjectileKind.ChargedIce or SamusProjectileKind.ChargedPlasma or SamusProjectileKind.ChargedWave or
            SamusProjectileKind.ChargedIceWave or SamusProjectileKind.ChargedSpazerWave or SamusProjectileKind.ChargedPlasmaIce or SamusProjectileKind.ChargedPlasmaWave => BeamDamage(BeamIdentity(kind)),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined projectile kind."),
    };

    /// <summary>
    /// The charge state and beams of an uncharged or charged beam shot's kind. The charged kinds swap the Wave/Plasma and
    /// Plasma-Wave/Plasma-Ice order.
    /// </summary>
    internal static SamusBeamIdentity BeamIdentity(SamusProjectileKind kind) => new(
        kind is SamusProjectileKind.ChargedPower or SamusProjectileKind.ChargedSpazer or SamusProjectileKind.ChargedSpazerIce or SamusProjectileKind.ChargedSpazerIceWave or
        SamusProjectileKind.ChargedPlasmaIceWave or SamusProjectileKind.ChargedIce or SamusProjectileKind.ChargedPlasma or SamusProjectileKind.ChargedWave or
        SamusProjectileKind.ChargedIceWave or SamusProjectileKind.ChargedSpazerWave or SamusProjectileKind.ChargedPlasmaIce or SamusProjectileKind.ChargedPlasmaWave, Beam(kind));

    /// <summary>The beams a beam shot of <paramref name="kind"/> fires.</summary>
    private static SamusBeamCombination Beam(SamusProjectileKind kind) =>
        Beams.TryGetValue(kind, out SamusBeamCombination value) ? value : throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined beam shot kind.");

    /// <summary>Maps a beam shot's beams to its base damage and triples charged shots.</summary>
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
                throw new InvalidOperationException($"{beam} has no beam shot kind of its own."),
            _ => throw new ArgumentOutOfRangeException(nameof(identity), beam, "Undefined beam combination."),
        };
        return damage * (charged ? 3 : 1);
    }
}
