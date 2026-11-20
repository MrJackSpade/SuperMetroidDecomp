using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>A gunship liftoff dust cloud's typed animation.</summary>
/// <param name="segment">The segment the animation starts at.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class GunshipDustAnimation(GunshipDustSegment segment, IEnemyProjectileCues<GunshipDustCue> cues)
    : EnemyProjectileFrameAnimation<GunshipDustSegment, GunshipDustCue>(GunshipDustAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<GunshipDustSegment, EnemyProjectileFrameId, GunshipDustCue> Segments => GunshipDustAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<GunshipDustSegment, Animation<GunshipDustSegment>> StartingAt => GunshipDustAnimations.StartingAt;
}
