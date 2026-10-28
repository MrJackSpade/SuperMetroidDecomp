using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Reads the projectile family stored in a projectile type word.</summary>
public static class SamusProjectileFamilies
{
    /// <summary>The family's table index: bits 8-11 of the type word, the nibble <see cref="SamusProjectileFamily"/> names.</summary>
    public static int IndexOf(int type) => new SamusProjectileTypeWord(checked((ushort)Ensure.BetweenInclusive(0, ushort.MaxValue, type))).FamilyIndex;
}
