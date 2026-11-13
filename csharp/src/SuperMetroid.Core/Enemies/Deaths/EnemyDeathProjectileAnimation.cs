using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>Enemy death and drop's typed animation.</summary>
internal sealed class EnemyDeathProjectileAnimation(EnemyDeathProjectileProgram program)
    : EnemyProjectileFrameAnimation<EnemyDeathProjectileProgram, EnemyDeathProjectileCue>(EnemyDeathProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<EnemyDeathProjectileProgram, EnemyProjectileFrameId, EnemyDeathProjectileCue> Segments => EnemyDeathProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<EnemyDeathProjectileProgram, Animation<EnemyDeathProjectileProgram>> StartingAt => EnemyDeathProjectileAnimations.StartingAt;
}
