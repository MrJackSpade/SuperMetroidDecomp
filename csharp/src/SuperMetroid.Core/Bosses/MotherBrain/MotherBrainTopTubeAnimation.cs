using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A falling ceiling tube's typed animation.</summary>
internal sealed class MotherBrainTopTubeAnimation(MotherBrainTopTubeProgram program)
    : EnemyProjectileTypedAnimation<MotherBrainTopTubeProgram, MotherBrainTopTubeProgram, NoAnimationCue>(MotherBrainTopTubeAnimations.StartingAt[program])
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainTopTubeProgram, MotherBrainTopTubeProgram, NoAnimationCue> Segments =>
        MotherBrainTopTubeAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MotherBrainTopTubeProgram, Animation<MotherBrainTopTubeProgram>> StartingAt =>
        MotherBrainTopTubeAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(MotherBrainTopTubeProgram frame) =>
        MotherBrainTopTubeFrameDefinitions.ShownProjectileFrames[frame];
}
