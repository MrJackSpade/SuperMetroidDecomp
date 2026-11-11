using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Chozo/Tourian dust's typed animation.</summary>
internal sealed class ChozoTourianDustAnimation(ChozoTourianDustProgram program)
    : EnemyProjectileTypedAnimation<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue>(ChozoTourianDustAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue> Segments => ChozoTourianDustAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<ChozoTourianDustProgram, Animation<ChozoTourianDustProgram>> StartingAt => ChozoTourianDustAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(ChozoTourianDustFrame frame) =>
        ChozoTourianDustFrameDefinitions.ShownProjectileFrames[frame];
}
