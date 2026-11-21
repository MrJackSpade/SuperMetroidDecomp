namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile family's typed animation (#641).
/// The projectile's frame hold times each frame, so each frame lasts exactly its duration.
/// </summary>
internal abstract partial class EnemyProjectileAnimation
{
    /// <summary>
    /// Advances one projectile pass: the frame expires on the update its timer would. A cue published
    /// on the way may end the animation (deleting the projectile or handing it to another segment); the pass
    /// then stops, as the cue that did so would. An animation that has shown a frame for good, or rests where its timeline
    /// holds, has stopped its hold and takes no step (#2380).
    /// </summary>
    /// <param name="context">The projectile, Samus, the pass's camera and the engine, for cues that act on them.</param>
    internal void Advance(EnemyProjectileCueContext context)
    {
        EnemyProjectile projectile = context.Projectile;
        if (projectile.IsFrameHoldStopped)
            return;
        CountdownStep frameStep = projectile.FrameHold.Decrement();
        projectile.FrameHold = frameStep.Remaining;
        if (!frameStep.IsZero)
            return;
        EnemyProjectileFrameStep step = NextFrame(context);
        if (projectile.Animation != this)
            return;
        switch (step)
        {
            case EnemyProjectileFrameStep.Show shown:
                projectile.FrameHold = WrappingCountdown.Start(shown.Duration);
                projectile.Frame = shown.Frame;
                projectile.FramePlacement = shown.Placement;
                return;
            case EnemyProjectileFrameStep.ShowForGood held:
                // A frame shown for good stops the hold: the projectile shows it until its owner sets the hold again or
                // starts another animation.
                projectile.StopFrameHold();
                projectile.Frame = held.Frame;
                projectile.FramePlacement = held.Placement;
                return;
            case EnemyProjectileFrameStep.Hide hidden:
                projectile.FrameHold = WrappingCountdown.Start(hidden.Duration);
                projectile.Frame = null;
                return;
            case EnemyProjectileFrameStep.Rest:
                // A held timeline rests on the frame it shows: the hold stops until the projectile's owner sets it again or
                // starts another animation.
                projectile.StopFrameHold();
                return;
            case EnemyProjectileFrameStep.Delete:
                projectile.Delete();
                return;
            default:
                throw new InvalidOperationException($"A projectile animation took an undefined step {step}.");
        }
    }

    /// <summary>Advances the timeline to its next frame, a frame shown for good, or its rest.</summary>
    /// <param name="context">
    /// The projectile the animation runs on, Samus, the pass's camera and the engine that carries out the shared cues, for
    /// cues that act on them.
    /// </param>
    private protected abstract EnemyProjectileFrameStep NextFrame(EnemyProjectileCueContext context);
}
