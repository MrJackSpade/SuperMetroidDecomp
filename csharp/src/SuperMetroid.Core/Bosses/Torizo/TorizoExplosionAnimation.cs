using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>A typed animation of the Torizos' explosions and the bursts of a sonic boom that has struck a wall (#2554), starting at one of its segments.</summary>
/// <param name="segment">The segment the animation starts at.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class TorizoExplosionAnimation(TorizoExplosionSegment segment, IEnemyProjectileBranchingCues<TorizoExplosionSegment, TorizoFamilyCue<TorizoExplosionCueKind>> cues)
    : EnemyProjectileFrameAnimation<TorizoExplosionSegment, TorizoFamilyCue<TorizoExplosionCueKind>>(TorizoExplosionAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<TorizoExplosionSegment, EnemyProjectileFrameId, TorizoFamilyCue<TorizoExplosionCueKind>> Segments => TorizoExplosionAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<TorizoExplosionSegment, Animation<TorizoExplosionSegment>> StartingAt => TorizoExplosionAnimations.StartingAt;
}
