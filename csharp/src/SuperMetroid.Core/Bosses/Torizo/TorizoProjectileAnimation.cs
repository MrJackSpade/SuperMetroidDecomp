using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>A Torizo projectile's typed animation.</summary>
internal sealed class TorizoProjectileAnimation(TorizoProjectileSegment segment)
    : EnemyProjectileTypedAnimation<TorizoProjectileSegment, TorizoProjectileFrame, TorizoProjectileCue>(TorizoProjectileAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<TorizoProjectileSegment, TorizoProjectileFrame, TorizoProjectileCue> Segments => TorizoProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<TorizoProjectileSegment, Animation<TorizoProjectileSegment>> StartingAt => TorizoProjectileAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(TorizoProjectileFrame frame) =>
        TorizoProjectileVisualDefinitions.ShownProjectileFrames[frame];
}
