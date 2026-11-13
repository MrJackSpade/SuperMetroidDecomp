using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A falling ceiling tube's typed animation.</summary>
internal sealed class MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment segment)
    : EnemyProjectileTypedAnimation<MotherBrainTopTubeSegment, MotherBrainTopTubeSegment, NoAnimationCue>(MotherBrainTopTubeAnimations.StartingAt[segment])
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainTopTubeSegment, MotherBrainTopTubeSegment, NoAnimationCue> Segments =>
        MotherBrainTopTubeAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MotherBrainTopTubeSegment, Animation<MotherBrainTopTubeSegment>> StartingAt =>
        MotherBrainTopTubeAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(MotherBrainTopTubeSegment frame) =>
        MotherBrainTopTubeFrameDefinitions.ShownProjectileFrames[frame];
}
