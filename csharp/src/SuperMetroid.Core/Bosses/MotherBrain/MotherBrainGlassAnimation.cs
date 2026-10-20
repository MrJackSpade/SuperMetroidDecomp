using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A glass shard's or sparkle's typed animation.</summary>
/// <param name="program">The program to play.</param>
internal sealed class MotherBrainGlassAnimation(MotherBrainGlassProgram program)
    : EnemyProjectileProgramAnimation<MotherBrainGlassProgram, MotherBrainGlassFrame, MotherBrainGlassCue>(program)
{

    /// <inheritdoc/>
    private protected override AnimationProgramSet<MotherBrainGlassProgram, MotherBrainGlassFrame, MotherBrainGlassCue> Programs =>
        MotherBrainGlassAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(MotherBrainGlassFrame frame) =>
        MotherBrainGlassVisualDefinitions.ShownProjectileFrames[frame];
}
