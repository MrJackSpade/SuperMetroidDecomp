
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Physical beam/missile muzzle positions, independent of charge-flare artwork.</summary>
internal static class SamusProjectileOriginDefinitions
{
    /// <summary>The bits of a projectile direction that index the muzzle tables: its low nibble.</summary>
    internal const int DirectionMask = 0x0f;
}
