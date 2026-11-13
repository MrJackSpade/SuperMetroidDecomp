using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Cacatac spike's typed animation.</summary>
internal sealed class CacatacProjectileAnimation(CacatacProjectileProgram program)
    : EnemyProjectileFrameAnimation<CacatacProjectileProgram, EnemyProjectileCommonCue>(CacatacProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<CacatacProjectileProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => CacatacProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<CacatacProjectileProgram, Animation<CacatacProjectileProgram>> StartingAt => CacatacProjectileAnimations.StartingAt;
}
