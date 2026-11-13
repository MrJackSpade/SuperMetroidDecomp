using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>Stoke projectile's typed animation.</summary>
internal sealed class StokeProjectileAnimation(StokeProjectileProgram program)
    : EnemyProjectileFrameAnimation<StokeProjectileProgram, EnemyProjectileCommonCue>(StokeProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<StokeProjectileProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => StokeProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<StokeProjectileProgram, Animation<StokeProjectileProgram>> StartingAt => StokeProjectileAnimations.StartingAt;
}
