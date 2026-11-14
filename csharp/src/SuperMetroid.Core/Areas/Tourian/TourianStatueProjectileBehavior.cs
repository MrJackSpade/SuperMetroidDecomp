using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// One of the Tourian statue scene's projectiles (#1760): the entrance statue's decorations, and the unlock's eye glows,
/// souls, particles, tails, splashes and descent dust. Each draws in the statue scene's look. This handles the cues of
/// their animations that act on the projectile alone; the unlock's own kinds also handle the release's cues.
/// </summary>
internal abstract class TourianStatueProjectileBehavior : EnemyProjectileBehavior, IEnemyProjectileCues<TourianStatueProjectileCue>
{
    /// <summary>Handles a statue projectile animation's cue: deletion, a step down, a movement, or a release cue.</summary>
    /// <param name="context">The projectile, Samus and the camera of the pass running the animation.</param>
    /// <param name="cue">The cue the animation published.</param>
    public void RunCue(EnemyProjectileCueContext context, TourianStatueProjectileCue cue)
    {
        EnemyProjectile projectile = context.Projectile;
        switch (cue)
        {
            case TourianStatueProjectileCue.Signal { Kind: TourianStatueProjectileCueKind.Delete }:
                projectile.Delete();
                break;
            case TourianStatueProjectileCue.MoveDown moveDown:
                projectile.Position = projectile.Position.WithWrappedWholeY(projectile.Position.Y.Whole + moveDown.Pixels);
                break;
            case TourianStatueProjectileCue.StartMoving startMoving:
                projectile.FrameBehavior = startMoving.Movement;
                break;
            default:
                RunReleaseCue(projectile, cue);
                break;
        }
    }

    /// <summary>
    /// Handles a cue of the statues' release: its sound, its earthquake, or a particle or tail spawned at the
    /// projectile. Only the unlock's own kinds' animations publish these.
    /// </summary>
    /// <param name="projectile">The projectile whose animation published the cue.</param>
    /// <param name="cue">The cue the animation published.</param>
    /// <exception cref="InvalidOperationException">The kind publishes no such cue.</exception>
    private protected virtual void RunReleaseCue(EnemyProjectile projectile, TourianStatueProjectileCue cue) =>
        throw new InvalidOperationException($"The Tourian statue projectiles have no {cue} cue.");
}
