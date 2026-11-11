using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>A Torizo projectile's typed animation.</summary>
internal sealed class TorizoProjectileAnimation(TorizoProjectileProgram program)
    : EnemyProjectileTypedAnimation<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>(TorizoProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue> Segments => TorizoProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<TorizoProjectileProgram, Animation<TorizoProjectileProgram>> StartingAt => TorizoProjectileAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(TorizoProjectileFrame frame) =>
        TorizoProjectileVisualDefinitions.ShownProjectileFrames[frame];
}
