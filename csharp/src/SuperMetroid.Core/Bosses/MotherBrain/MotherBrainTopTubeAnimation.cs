using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A falling ceiling tube's typed animation.</summary>
internal sealed class MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment segment)
    : EnemyProjectileFrameAnimation<MotherBrainTopTubeSegment, NoAnimationCue>(MotherBrainTopTubeAnimations.StartingAt[segment], EnemyProjectileCueRunners.None<MotherBrainTopTubeSegment>())
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainTopTubeSegment, EnemyProjectileFrameId, NoAnimationCue> Segments =>
        MotherBrainTopTubeAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MotherBrainTopTubeSegment, Animation<MotherBrainTopTubeSegment>> StartingAt =>
        MotherBrainTopTubeAnimations.StartingAt;
}
