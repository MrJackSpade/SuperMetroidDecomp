using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain effect's typed animation.</summary>
internal sealed class MotherBrainEffectAnimation(MotherBrainEffectProgram program)
    : EnemyProjectileFrameAnimation<MotherBrainEffectProgram, MotherBrainEffectCue>(MotherBrainEffectAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainEffectProgram, EnemyProjectileFrameId, MotherBrainEffectCue> Segments => MotherBrainEffectAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MotherBrainEffectProgram, Animation<MotherBrainEffectProgram>> StartingAt => MotherBrainEffectAnimations.StartingAt;
}
