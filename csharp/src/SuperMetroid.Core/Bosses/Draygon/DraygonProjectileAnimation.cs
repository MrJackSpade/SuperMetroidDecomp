using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>A Draygon goop's or wall-turret shot's typed animation.</summary>
internal sealed class DraygonProjectileAnimation(DraygonProjectileSegment segment)
    : EnemyProjectileFrameAnimation<DraygonProjectileSegment, DraygonProjectileCue>(DraygonProjectileAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<DraygonProjectileSegment, EnemyProjectileFrameId, DraygonProjectileCue> Segments =>
        DraygonProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<DraygonProjectileSegment, Animation<DraygonProjectileSegment>> StartingAt =>
        DraygonProjectileAnimations.StartingAt;
}
