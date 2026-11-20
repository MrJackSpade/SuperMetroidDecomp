using System.Collections.Immutable;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The sparkles one projectile selection's trail runs (#2558): none, one on the left, or one on each side. A side with no
/// sparkle runs no animation.
/// </summary>
internal sealed record ProjectileTrailAnimations
{
    /// <summary>Keeps the trail's sparkles.</summary>
    private ProjectileTrailAnimations(ImmutableArray<ProjectileTrailSparkle> sparkles) => Sparkles = sparkles;

    /// <summary>A trail that runs no sparkle on either side.</summary>
    internal static ProjectileTrailAnimations Bare { get; } = new([]);

    /// <summary>The sparkles the trail runs, each on its side.</summary>
    internal ImmutableArray<ProjectileTrailSparkle> Sparkles { get; }

    /// <summary>A trail that runs <paramref name="left"/> on its left side only.</summary>
    internal static ProjectileTrailAnimations LeftOnly(ProjectileTrailSegment left) =>
        new([new(HorizontalDirection.Left, left)]);

    /// <summary>A trail that runs <paramref name="left"/> on its left side and <paramref name="right"/> on its right.</summary>
    internal static ProjectileTrailAnimations Both(ProjectileTrailSegment left, ProjectileTrailSegment right) =>
        new([new(HorizontalDirection.Left, left), new(HorizontalDirection.Right, right)]);
}
