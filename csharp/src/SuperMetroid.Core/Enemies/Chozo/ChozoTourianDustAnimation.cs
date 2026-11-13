using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Chozo/Tourian dust's typed animation.</summary>
internal sealed class ChozoTourianDustAnimation(ChozoTourianDustSegment segment)
    : EnemyProjectileFrameAnimation<ChozoTourianDustSegment, EnemyProjectileCommonCue>(ChozoTourianDustAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<ChozoTourianDustSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => ChozoTourianDustAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<ChozoTourianDustSegment, Animation<ChozoTourianDustSegment>> StartingAt => ChozoTourianDustAnimations.StartingAt;
}
