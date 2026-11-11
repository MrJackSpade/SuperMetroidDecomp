using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>A Tourian statue projectile's typed animation.</summary>
internal sealed class TourianStatueProjectileAnimation(TourianStatueProjectileProgram program)
    : EnemyProjectileTypedAnimation<TourianStatueProjectileProgram, TourianStatueProjectileFrame, TourianStatueProjectileCue>(TourianStatueProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<TourianStatueProjectileProgram, TourianStatueProjectileFrame, TourianStatueProjectileCue> Segments => TourianStatueProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<TourianStatueProjectileProgram, Animation<TourianStatueProjectileProgram>> StartingAt => TourianStatueProjectileAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(TourianStatueProjectileFrame frame) =>
        TourianStatueProjectileFrameDefinitions.ShownProjectileFrames[frame];
}
