using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>A gunship liftoff dust cloud's typed animation.</summary>
internal sealed class GunshipDustAnimation(GunshipDustSegment segment)
    : EnemyProjectileFrameAnimation<GunshipDustSegment, GunshipDustCue>(GunshipDustAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<GunshipDustSegment, EnemyProjectileFrameId, GunshipDustCue> Segments => GunshipDustAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<GunshipDustSegment, Animation<GunshipDustSegment>> StartingAt => GunshipDustAnimations.StartingAt;
}
