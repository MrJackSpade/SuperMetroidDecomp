using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain effect's typed animation.</summary>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
internal sealed class MotherBrainEffectAnimation(IEnemyProjectileCues<MotherBrainEffectCue> owner, MotherBrainEffectSegment segment)
    : EnemyProjectileFrameAnimation<MotherBrainEffectSegment, MotherBrainEffectCue>(MotherBrainEffectAnimations.StartingWith(segment), EnemyProjectileCueRunner<MotherBrainEffectSegment, MotherBrainEffectCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainEffectSegment, EnemyProjectileFrameId, MotherBrainEffectCue> Segments => MotherBrainEffectAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MotherBrainEffectSegment, Animation<MotherBrainEffectSegment>> StartingAt => MotherBrainEffectAnimations.StartingAt;
}
