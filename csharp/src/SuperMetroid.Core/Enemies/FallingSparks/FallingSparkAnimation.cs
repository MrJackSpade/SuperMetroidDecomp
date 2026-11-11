using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.FallingSparks.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>Falling spark's typed animation.</summary>
internal sealed class FallingSparkAnimation(FallingSparkProgram program)
    : EnemyProjectileTypedAnimation<FallingSparkProgram, FallingSparkFrame, EnemyProjectileCommonCue>(FallingSparkAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<FallingSparkProgram, FallingSparkFrame, EnemyProjectileCommonCue> Segments => FallingSparkAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<FallingSparkProgram, Animation<FallingSparkProgram>> StartingAt => FallingSparkAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(FallingSparkFrame frame) =>
        FallingSparkFrameDefinitions.ShownProjectileFrames[frame];
}
