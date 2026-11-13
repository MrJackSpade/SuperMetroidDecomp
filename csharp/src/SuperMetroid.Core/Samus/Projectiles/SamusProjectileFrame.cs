namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One frame a Samus projectile animation shows (#1301, #1517); see <see cref="SamusProjectileFrameDefinitions"/>.</summary>
/// <param name="Animation">The animation that shows the frame.</param>
/// <param name="Show">The frame's place among the frames the animation shows, from zero.</param>
internal readonly record struct SamusProjectileFrame(SamusProjectileSegment Animation, int Show);
