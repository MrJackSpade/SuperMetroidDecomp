using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A hand beam's typed animation.</summary>
internal sealed class MotherBrainHandBeamAnimation()
    : EnemyProjectileFrameAnimation<MotherBrainHandBeamSegment, MotherBrainHandBeamCue>(
        MotherBrainHandBeamAnimations.StartingAt[MotherBrainHandBeamSegment.Stage])
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainHandBeamSegment, EnemyProjectileFrameId, MotherBrainHandBeamCue> Segments =>
        MotherBrainHandBeamAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MotherBrainHandBeamSegment, Animation<MotherBrainHandBeamSegment>> StartingAt =>
        MotherBrainHandBeamAnimations.StartingAt;
}
