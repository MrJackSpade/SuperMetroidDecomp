using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Samus.Projectiles.SamusProjectileDamageDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The damage each projectile kind starts its projectile with (<see cref="SamusProjectileDamageDefinitions"/>).</summary>
internal static class SamusProjectileDamage
{
    /// <summary>
    /// The damage a projectile of <paramref name="kind"/> starts with: a beam shot's beams' damage, tripled when it is
    /// charged, or the kind's own.
    /// </summary>
    internal static int Read(SamusProjectileKind kind)
    {
        if (Beams.ContainsKey(kind))
            return BeamDamage(BeamIdentity(kind));
        return NonBeamDamage.TryGetValue(kind, out int damage)
            ? damage
            : throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined projectile kind.");
    }

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

    /// <summary>A beam shot's beams' damage, multiplied when the shot is charged.</summary>
    private static int BeamDamage(SamusBeamIdentity identity)
    {
        var (charged, beam) = identity;
        int damage = UnchargedBeamDamage.TryGetValue(beam, out int uncharged)
            ? uncharged
            : throw new InvalidOperationException($"{beam} has no beam shot kind of its own.");
        return charged ? damage * ChargedBeamDamageMultiplier : damage;
    }
}
