namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile family's typed animation (#641).
/// The slot keeps the game's timer gate, so each frame lasts exactly as long as the frame did.
/// </summary>
internal abstract class EnemyProjectileAnimation
{
    /// <summary>
    /// Advances one projectile pass: the frame expires on the update its timer would. A cue published
    /// on the way may end the animation (deleting the projectile or handing it to another segment); the pass
    /// then stops, as the cue that did so would.
    /// </summary>
    /// <param name="context">The projectile, Samus and the pass's camera, for cues that act on them.</param>
    /// <param name="projectiles">The projectile engine, which carries out the shared cues.</param>
    internal void Advance(EnemyProjectileCueContext context, EnemyProjectileEngine projectiles)
    {
        EnemyProjectile projectile = context.Projectile;
        CountdownStep frameStep = projectile.FrameHold.Decrement();
        projectile.FrameHold = frameStep.Remaining;
        if (!frameStep.IsZero)
            return;
        EnemyProjectileFrameStep step = NextFrame(context, projectiles);
        if (projectile.Animation != this)
            return;
        switch (step)
        {
            case EnemyProjectileFrameStep.Show shown:
                projectile.FrameHold = WrappingCountdown.Start(shown.Duration);
                projectile.Frame = shown.Frame;
                return;
            case EnemyProjectileFrameStep.ShowForGood held:
                // A frame shown for good rests as a held timeline does: the timer stays zero and wraps, and no later
                // expiry shows anything.
                projectile.FrameHold = WrappingCountdown.Start(0);
                projectile.Frame = held.Frame;
                return;
            case EnemyProjectileFrameStep.Rest:
                // A held timeline is the sleep: the timer stays zero and wraps, never expiring again.
                projectile.FrameHold = WrappingCountdown.Start(0);
                return;
            default:
                throw new InvalidOperationException($"A projectile animation took an undefined step {step}.");
        }
    }

    /// <summary>Advances the timeline to its next frame, a frame shown for good, or its rest.</summary>
    /// <param name="context">The slot the animation runs on, Samus and the pass's camera, for cues that act on them.</param>
    /// <param name="projectiles">The projectile engine, which carries out the shared cues.</param>
    private protected abstract EnemyProjectileFrameStep NextFrame(EnemyProjectileCueContext context, EnemyProjectileEngine projectiles);
}
