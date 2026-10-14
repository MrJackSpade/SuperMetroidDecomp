using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>A Ridley fireball's or afterburn's typed animation.</summary>
internal sealed class RidleyFireballAnimation(RidleyFireballProgram program)
    : EnemyProjectileProgramAnimation<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue> Programs =>
        RidleyFireballAnimations.Programs;

    /// <inheritdoc/>
    private protected override ushort PresentationKey(RidleyFireballFrame frame) => RidleyFireballVisualDefinitions.PresentationKey(frame);
}
