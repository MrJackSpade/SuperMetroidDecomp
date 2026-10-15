namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Bits the cartridge keeps in a Samus projectile's direction word beside the direction itself.</summary>
internal static class SamusProjectileDirectionMasks
{
    /// <summary>Bit $10: the projectile already struck something, so later collision walkers skip it.</summary>
    internal const ushort Spent = 0x0010;

    /// <summary>The low nibble: the direction itself.</summary>
    internal const ushort Direction = 0x000f;
}
