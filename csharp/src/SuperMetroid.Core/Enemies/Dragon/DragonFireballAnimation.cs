using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Dragon.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>Dragon fireball's typed animation.</summary>
internal sealed class DragonFireballAnimation(DragonFireballProgram program)
    : EnemyProjectileTypedAnimation<DragonFireballProgram, DragonFireballFrame, EnemyProjectileCommonCue>(DragonFireballAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<DragonFireballProgram, DragonFireballFrame, EnemyProjectileCommonCue> Segments => DragonFireballAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<DragonFireballProgram, Animation<DragonFireballProgram>> StartingAt => DragonFireballAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(DragonFireballFrame frame) =>
        DragonFireballFrameDefinitions.ShownProjectileFrames[frame];
}
