using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>The presentation boundary of the pre-Phantoon room projectile (#641).</summary>
internal static class PrePhantoonRoomVisualDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_PrePhantoonRoom</c> at $86:A3AA.</summary>
    internal const ushort Program = 0xa3aa;

    /// <summary>Its blank frame: the operand at $86:A3AC selects the empty projectile spritemap, not an installed frame.</summary>
    internal const ushort BlankFrameKey = EnemyProjectileAnimation.EmptyFrameKey;
}