using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A falling ceiling tube's typed animation.</summary>
internal sealed partial class MotherBrainTopTubeAnimation
    : EnemyProjectileSilentFrameAnimation<MotherBrainTopTubeSegment>
{
    /// <summary>Creates a <see cref="MotherBrainTopTubeAnimation"/>.</summary>
    /// <param name="segment">segment</param>
    public MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment segment)
        : base(segment, MotherBrainTopTubeAnimations.StartingAt[segment])
    {
    }

    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<MotherBrainTopTubeSegment, EnemyProjectileFrameId, NoAnimationCue> Segments =>
        MotherBrainTopTubeAnimations.Segments;
}
