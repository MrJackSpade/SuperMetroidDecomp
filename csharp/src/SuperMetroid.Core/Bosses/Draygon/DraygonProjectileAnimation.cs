using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>A Draygon goop's or wall-turret shot's typed animation.</summary>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class DraygonProjectileAnimation(DraygonProjectileSegment segment, IEnemyProjectileCues<DraygonProjectileCue> cues)
    : EnemyProjectileFrameAnimation<DraygonProjectileSegment, DraygonProjectileCue>(DraygonProjectileAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<DraygonProjectileSegment, EnemyProjectileFrameId, DraygonProjectileCue> Segments =>
        DraygonProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<DraygonProjectileSegment, Animation<DraygonProjectileSegment>> StartingAt =>
        DraygonProjectileAnimations.StartingAt;
}
