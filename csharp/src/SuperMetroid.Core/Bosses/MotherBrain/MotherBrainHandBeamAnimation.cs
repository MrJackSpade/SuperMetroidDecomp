using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A hand beam's typed animation.</summary>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
internal sealed class MotherBrainHandBeamAnimation(IEnemyProjectileCues<MotherBrainHandBeamCue> owner)
    : EnemyProjectileFrameAnimation<MotherBrainHandBeamSegment, MotherBrainHandBeamCue>(
        MotherBrainHandBeamSegment.Stage, MotherBrainHandBeamAnimations.StartingAt[MotherBrainHandBeamSegment.Stage], EnemyProjectileCueRunner<MotherBrainHandBeamSegment, MotherBrainHandBeamCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainHandBeamSegment, EnemyProjectileFrameId, MotherBrainHandBeamCue> Segments =>
        MotherBrainHandBeamAnimations.Segments;
}
