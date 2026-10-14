using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>A Torizo projectile's typed animation.</summary>
internal sealed class TorizoProjectileAnimation(TorizoProjectileProgram program)
    : EnemyProjectileProgramAnimation<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue> Programs =>
        TorizoProjectileAnimations.Programs;

    /// <inheritdoc/>
    private protected override ushort PresentationKey(TorizoProjectileFrame frame) => TorizoProjectileVisualDefinitions.PresentationKey(frame);
}
