namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The damage each projectile kind starts its projectile with (<see cref="SamusProjectileDamageDefinitions"/>).</summary>
internal static class SamusProjectileDamage
{
    /// <summary>
    /// The damage a projectile of <paramref name="kind"/> starts with: a beam shot's beams' damage, tripled when it is
    /// charged, or the kind's own (<see cref="SamusProjectileDamageDefinitions.StartingDamage"/>).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The kind is undefined.</exception>
    internal static int Read(SamusProjectileKind kind) => SamusProjectileDamageDefinitions.StartingDamage[kind].Damage;
}
