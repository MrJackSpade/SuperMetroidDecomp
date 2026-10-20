using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.FallingSparks.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>Falling spark's typed animation.</summary>
internal sealed class FallingSparkAnimation(FallingSparkProgram program)
    : EnemyProjectileProgramAnimation<FallingSparkProgram, FallingSparkFrame, EnemyProjectileCommonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<FallingSparkProgram, FallingSparkFrame, EnemyProjectileCommonCue> Programs => FallingSparkAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(FallingSparkFrame frame) =>
        FallingSparkFrameDefinitions.ShownProjectileFrames[frame];
}
