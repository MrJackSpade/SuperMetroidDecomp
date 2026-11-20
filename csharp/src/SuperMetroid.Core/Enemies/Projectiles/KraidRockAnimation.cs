using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>A Kraid rock's typed animation.</summary>
/// <param name="segment">The animation to play.</param>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
internal sealed class KraidRockAnimation(IEnemyProjectileCues<KraidRockCue> owner, KraidRockSegment segment)
    : EnemyProjectileFrameAnimation<KraidRockSegment, KraidRockCue>(KraidRockAnimations.StartingWith(segment), EnemyProjectileCueRunner<KraidRockSegment, KraidRockCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<KraidRockSegment, EnemyProjectileFrameId, KraidRockCue> Segments => KraidRockAnimations.Segments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<KraidRockSegment, Animation<KraidRockSegment>>> StartingAt => KraidRockAnimations.StartingAt;
}
