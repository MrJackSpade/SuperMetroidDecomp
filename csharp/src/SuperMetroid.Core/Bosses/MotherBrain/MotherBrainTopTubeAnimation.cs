using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A falling ceiling tube's typed animation.</summary>
internal sealed class MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment segment)
    : EnemyProjectileSilentFrameAnimation<MotherBrainTopTubeSegment>(segment, MotherBrainTopTubeAnimations.StartingAt[segment])
{
    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<MotherBrainTopTubeSegment, EnemyProjectileFrameId, NoAnimationCue> Segments =>
        MotherBrainTopTubeAnimations.Segments;
}
