using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Chozo/Tourian dust's typed animation.</summary>
internal sealed class ChozoTourianDustAnimation(ChozoTourianDustProgram program)
    : EnemyProjectileFrameAnimation<ChozoTourianDustProgram, EnemyProjectileCommonCue>(ChozoTourianDustAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<ChozoTourianDustProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => ChozoTourianDustAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<ChozoTourianDustProgram, Animation<ChozoTourianDustProgram>> StartingAt => ChozoTourianDustAnimations.StartingAt;
}
