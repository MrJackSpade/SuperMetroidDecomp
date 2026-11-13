using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Shaktool attack circle's typed animation.</summary>
internal sealed class ShaktoolProjectileAnimation(ShaktoolProjectileProgram program)
    : EnemyProjectileFrameAnimation<ShaktoolProjectileProgram, EnemyProjectileCommonCue>(ShaktoolProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<ShaktoolProjectileProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => ShaktoolProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<ShaktoolProjectileProgram, Animation<ShaktoolProjectileProgram>> StartingAt => ShaktoolProjectileAnimations.StartingAt;
}
