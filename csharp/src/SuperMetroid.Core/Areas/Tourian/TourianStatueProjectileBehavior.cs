using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// One of the Tourian statue scene's projectiles (#1760): the entrance statue's decorations, and the unlock's eye glows,
/// souls, particles, tails, splashes and descent dust. Each draws in the statue scene's look. This handles the cues of
/// their animations that act on the projectile alone; the unlock's own kinds also handle the release's cues.
/// </summary>
/// <typeparam name="TMotion">The kinds' motions (#1843).</typeparam>
internal abstract class TourianStatueProjectileBehavior<TMotion>
    : EnemyProjectileBehavior<TMotion>, IEnemyProjectileCues<TourianStatueProjectileCue>
    where TMotion : struct, Enum
{
    /// <summary>Kinds that all start in <paramref name="initial"/>.</summary>
    /// <param name="initial">The motion every kind of this behaviour starts with.</param>
    private protected TourianStatueProjectileBehavior(TMotion initial)
        : base(initial)
    {
    }

    /// <summary>Kinds that start in the motions <paramref name="startingMotions"/> names for each.</summary>
    /// <param name="startingMotions">Every kind this behaviour is bound to, with the motion it starts with, or null when it starts holding still.</param>
    private protected TourianStatueProjectileBehavior(IReadOnlyDictionary<RoomEnemyProjectileKind, TMotion?> startingMotions)
        : base(startingMotions)
    {
    }

    /// <summary>Handles a statue projectile animation's cue: deletion, a step down, following the reveal, or a release cue.</summary>
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
            case TourianStatueProjectileCue.Signal { Kind: TourianStatueProjectileCueKind.FollowReveal }:
                FollowReveal(projectile);
                break;
            default:
                RunReleaseCue(projectile, cue);
                break;
        }
    }

    /// <summary>Starts a decoration following the statue's reveal. Only the decorations' animations publish this.</summary>
    /// <param name="projectile">The projectile whose animation published the cue.</param>
    /// <exception cref="InvalidOperationException">The kind publishes no such cue.</exception>
    private protected virtual void FollowReveal(EnemyProjectile projectile) =>
        throw new InvalidOperationException($"Enemy projectile {projectile.Kind} does not follow the statue's reveal.");

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
