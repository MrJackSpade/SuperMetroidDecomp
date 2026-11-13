using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>Falling spark's typed animation.</summary>
internal sealed class FallingSparkAnimation(FallingSparkProgram program)
    : EnemyProjectileFrameAnimation<FallingSparkProgram, EnemyProjectileCommonCue>(FallingSparkAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<FallingSparkProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => FallingSparkAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<FallingSparkProgram, Animation<FallingSparkProgram>> StartingAt => FallingSparkAnimations.StartingAt;
}
