using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>Yapping Maw body link's typed animation.</summary>
internal sealed class YappingMawBodyProjectileAnimation(YappingMawBodyProjectileProgram program)
    : EnemyProjectileFrameAnimation<YappingMawBodyProjectileProgram, EnemyProjectileCommonCue>(YappingMawBodyProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<YappingMawBodyProjectileProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => YappingMawBodyProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<YappingMawBodyProjectileProgram, Animation<YappingMawBodyProjectileProgram>> StartingAt => YappingMawBodyProjectileAnimations.StartingAt;
}
