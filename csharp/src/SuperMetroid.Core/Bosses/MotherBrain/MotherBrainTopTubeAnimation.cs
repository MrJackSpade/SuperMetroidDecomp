using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A falling ceiling tube's typed animation.</summary>
internal sealed class MotherBrainTopTubeAnimation(MotherBrainTopTubeProgram program)
    : EnemyProjectileProgramAnimation<MotherBrainTopTubeProgram, MotherBrainTopTubeProgram, NoAnimationCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<MotherBrainTopTubeProgram, MotherBrainTopTubeProgram, NoAnimationCue> Programs =>
        MotherBrainTopTubeAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFramePresentation Present(MotherBrainTopTubeProgram frame) =>
        EnemyProjectileFramePresentation.Installed(MotherBrainTopTubeFrameDefinitions.PresentationKeys[frame]);
}
