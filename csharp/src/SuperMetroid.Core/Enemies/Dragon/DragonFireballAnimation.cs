using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>Dragon fireball's typed animation.</summary>
internal sealed class DragonFireballAnimation(DragonFireballProgram program)
    : EnemyProjectileFrameAnimation<DragonFireballProgram, EnemyProjectileCommonCue>(DragonFireballAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<DragonFireballProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => DragonFireballAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<DragonFireballProgram, Animation<DragonFireballProgram>> StartingAt => DragonFireballAnimations.StartingAt;
}
