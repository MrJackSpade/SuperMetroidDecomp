using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>Polyp lava rock's typed animation.</summary>
internal sealed class PolypRockAnimation(PolypRockProgram program)
    : EnemyProjectileFrameAnimation<PolypRockProgram, EnemyProjectileCommonCue>(PolypRockAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<PolypRockProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => PolypRockAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<PolypRockProgram, Animation<PolypRockProgram>> StartingAt => PolypRockAnimations.StartingAt;
}
