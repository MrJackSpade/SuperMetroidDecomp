using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>A Ridley fireball's or afterburn's typed animation.</summary>
internal sealed class RidleyFireballAnimation(RidleyFireballProgram program)
    : EnemyProjectileTypedAnimation<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>(RidleyFireballAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue> Segments =>
        RidleyFireballAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<RidleyFireballProgram, Animation<RidleyFireballProgram>> StartingAt =>
        RidleyFireballAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(RidleyFireballFrame frame) =>
        RidleyFireballVisualDefinitions.ShownProjectileFrames[frame];
}
