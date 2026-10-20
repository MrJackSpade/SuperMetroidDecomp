using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Chozo/Tourian dust's typed animation.</summary>
internal sealed class ChozoTourianDustAnimation(ChozoTourianDustProgram program)
    : EnemyProjectileProgramAnimation<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<ChozoTourianDustProgram, ChozoTourianDustFrame, EnemyProjectileCommonCue> Programs => ChozoTourianDustAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(ChozoTourianDustFrame frame) =>
        ChozoTourianDustFrameDefinitions.ShownProjectileFrames[frame];
}
