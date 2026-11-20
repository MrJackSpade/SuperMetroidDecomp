using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A glass shard's or sparkle's typed animation.</summary>
/// <param name="segment">The animation to play.</param>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
internal sealed class MotherBrainGlassAnimation(IEnemyProjectileCues<MotherBrainGlassCue> owner, MotherBrainGlassSegment segment)
    : EnemyProjectileFrameAnimation<MotherBrainGlassSegment, MotherBrainGlassCue>(MotherBrainGlassAnimations.StartingAt[segment], EnemyProjectileCueRunner<MotherBrainGlassSegment, MotherBrainGlassCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<MotherBrainGlassSegment, EnemyProjectileFrameId, MotherBrainGlassCue> Segments =>
        MotherBrainGlassAnimations.Segments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<MotherBrainGlassSegment, Animation<MotherBrainGlassSegment>>> StartingAt =>
        MotherBrainGlassAnimations.StartingAt;
}
