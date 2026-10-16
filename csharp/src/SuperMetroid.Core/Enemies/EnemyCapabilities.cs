namespace SuperMetroid.Core.Enemies;

/// <summary>Collision paths a species takes beyond the common radius-based ones; declared once per species.</summary>
[Flags]
internal enum EnemyCapabilities
{
    /// <summary>Only the common paths.</summary>
    None = 0,

    /// <summary>
    /// Ordinary Samus contact enters <c>$A0:9A5A</c>'s extended-spritemap walker. This differs deliberately from the shot
    /// set: Phantoon uses authored component rectangles for touch, while its weapon collision is owned by the
    /// encounter-specific projectile resolver.
    /// </summary>
    ExtendedSamusHitboxes = 1,

    /// <summary>
    /// Ordinary projectile collision enters <c>$A0:9B7F</c>'s extended-spritemap walker. Extra-property bit $0004 alone is
    /// not sufficient: Mother Brain and several custom-drawn actors set it for scheduling or rendering while their shot
    /// paths use a header radius or encounter-specific geometry.
    /// </summary>
    ExtendedProjectileHitboxes = 2,
}
