using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>Nuclear Waffle body's typed animation.</summary>
internal sealed class NuclearWaffleProjectileAnimation(NuclearWaffleProjectileProgram program)
    : EnemyProjectileFrameAnimation<NuclearWaffleProjectileProgram, EnemyProjectileCommonCue>(NuclearWaffleProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<NuclearWaffleProjectileProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => NuclearWaffleProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<NuclearWaffleProjectileProgram, Animation<NuclearWaffleProjectileProgram>> StartingAt => NuclearWaffleProjectileAnimations.StartingAt;
}
