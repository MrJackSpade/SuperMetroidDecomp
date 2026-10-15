namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Reads the projectile family stored in a projectile type word.</summary>
public static class SamusProjectileFamilies
{
    /// <summary>The family's table index: bits 8-11 of the type word, the nibble <see cref="SamusProjectileFamily"/> names.</summary>
    public static int IndexOf(ushort type) => SnesLittleEndian.HighByte(type) & 0x0f;
}
