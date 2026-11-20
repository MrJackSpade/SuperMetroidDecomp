namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One sparkle a projectile's trail runs: the side it runs on and its animation.</summary>
/// <param name="Side">The trail side the sparkle runs on.</param>
/// <param name="Segment">The animation the sparkle runs.</param>
internal readonly record struct ProjectileTrailSparkle(HorizontalDirection Side, ProjectileTrailSegment Segment);
