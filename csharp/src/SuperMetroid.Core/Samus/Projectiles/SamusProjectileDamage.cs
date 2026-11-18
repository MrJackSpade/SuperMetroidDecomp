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
        if (UnchargedBeams.TryGetValue(kind, out SamusBeamCombination uncharged))
            return BeamDamage(uncharged);
        if (ChargedBeams.TryGetValue(kind, out SamusBeamCombination charged))
            return BeamDamage(charged) * ChargedBeamDamageMultiplier;
        return NonBeamDamage.TryGetValue(kind, out int damage)
            ? damage
            : throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined projectile kind.");
    }

    /// <summary>An uncharged shot of <paramref name="beam"/>'s damage.</summary>
    private static int BeamDamage(SamusBeamCombination beam) =>
        UnchargedBeamDamage.TryGetValue(beam, out int damage)
            ? damage
            : throw new InvalidOperationException($"{beam} has no beam shot kind of its own.");
}
