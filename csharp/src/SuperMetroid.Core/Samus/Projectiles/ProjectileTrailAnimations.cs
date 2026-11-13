namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The trail animations one projectile selection runs on its left and right sides.</summary>
/// <param name="Left">The animation the left trail runs.</param>
/// <param name="Right">The animation the right trail runs.</param>
internal readonly record struct ProjectileTrailAnimations(ProjectileTrailSegment Left, ProjectileTrailSegment Right);
