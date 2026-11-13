using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>Kago bug's typed animation.</summary>
internal sealed class KagoBugProjectileAnimation(KagoBugProjectileProgram program)
    : EnemyProjectileFrameAnimation<KagoBugProjectileProgram, KagoBugProjectileCue>(KagoBugProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<KagoBugProjectileProgram, EnemyProjectileFrameId, KagoBugProjectileCue> Segments => KagoBugProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<KagoBugProjectileProgram, Animation<KagoBugProjectileProgram>> StartingAt => KagoBugProjectileAnimations.StartingAt;
}
