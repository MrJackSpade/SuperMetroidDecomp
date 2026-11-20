using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A falling ceiling tube's typed animation.</summary>
internal sealed class MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment segment)
    : EnemyProjectileFrameAnimation<MotherBrainTopTubeSegment, NoAnimationCue>(MotherBrainTopTubeAnimations.StartingAt[segment],
        static (_, _, cue, _) => EnemyProjectileCuelessAnimations.Undefined<MotherBrainTopTubeSegment>(cue))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainTopTubeSegment, EnemyProjectileFrameId, NoAnimationCue> Segments =>
        MotherBrainTopTubeAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MotherBrainTopTubeSegment, Animation<MotherBrainTopTubeSegment>> StartingAt =>
        MotherBrainTopTubeAnimations.StartingAt;
}
