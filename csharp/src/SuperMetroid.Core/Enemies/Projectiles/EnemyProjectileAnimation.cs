namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile family's typed animation (#641).
/// The projectile's frame hold times each frame, so each frame lasts exactly its duration.
/// </summary>
internal abstract class EnemyProjectileAnimation
{
    /// <summary>
    /// Advances one projectile pass: the frame expires on the update its timer would. A cue published
    /// on the way may end the animation (deleting the projectile or starting another animation); the pass then stops, as
    /// the cue that did so would. A cue that hands the projectile over to another family's animation goes on in it. An animation that has shown a frame for good, or rests where its timeline
    /// holds, has stopped its hold and takes no step (#2380).
    /// </summary>
    /// <param name="context">The projectile, Samus and the pass's camera, for cues that act on them.</param>
    /// <param name="projectiles">The projectile engine, which carries out the shared cues.</param>
    internal void Advance(EnemyProjectileCueContext context, EnemyProjectileEngine projectiles)
    {
        EnemyProjectile projectile = context.Projectile;
        if (projectile.IsFrameHoldStopped)
            return;
        CountdownStep frameStep = projectile.FrameHold.Decrement();
        projectile.FrameHold = frameStep.Remaining;
        if (!frameStep.IsZero)
            return;
        // A cue that hands the projectile over to another animation ends this one's play; the projectile plays on in the
        // animation handed to, from its start, within this same pass (#2554).
        EnemyProjectileAnimation playing = this;
        EnemyProjectileFrameStep step = NextFrame(context, projectiles);
        while (step is EnemyProjectileFrameStep.HandOver handOver)
        {
            playing = handOver.Next;
            projectile.InstallAnimation(playing);
            step = playing.NextFrame(context, projectiles);
        }
        if (projectile.Animation != playing)
            return;
        switch (step)
        {
            case EnemyProjectileFrameStep.Show shown:
                projectile.FrameHold = WrappingCountdown.Start(shown.Duration);
                projectile.Frame = shown.Frame;
                return;
            case EnemyProjectileFrameStep.ShowForGood held:
                // A frame shown for good stops the hold: the projectile shows it until its owner sets the hold again or
                // starts another animation.
                projectile.StopFrameHold();
                projectile.Frame = held.Frame;
                return;
            case EnemyProjectileFrameStep.Rest:
                // A held timeline rests on the frame it shows: the hold stops until the projectile's owner sets it again or
                // starts another animation.
                projectile.StopFrameHold();
                return;
            default:
                throw new InvalidOperationException($"A projectile animation took an undefined step {step}.");
        }
    }

    /// <summary>Advances the timeline to its next frame, a frame shown for good, or its rest.</summary>
    /// <param name="context">The projectile the animation runs on, Samus and the pass's camera, for cues that act on them.</param>
    /// <param name="projectiles">The projectile engine, which carries out the shared cues.</param>
    private protected abstract EnemyProjectileFrameStep NextFrame(EnemyProjectileCueContext context, EnemyProjectileEngine projectiles);
}
