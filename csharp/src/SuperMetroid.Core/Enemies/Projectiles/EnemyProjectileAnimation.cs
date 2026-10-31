namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile family's typed animation, run in place of the bank-$86 instruction interpreter (#641).
/// The slot keeps the cartridge's timer gate, so each frame lasts exactly as long as the native frame did.
/// </summary>
internal abstract class EnemyProjectileAnimation
{
    /// <summary>
    /// Advances one projectile pass: the frame expires on the update its native timer would. A cue published
    /// on the way may end the animation (deleting the projectile or handing it to another program); the pass
    /// then stops, as the native instruction that did so would.
    /// </summary>
    internal void Advance(EnemyProjectileCueContext context, EnemyProjectileHandlerTable handlers)
    {
        RoomEnemyProjectileSlot projectile = context.Projectile;
        CountdownStep frameStep = projectile.InstructionTimer.Decrement();
        projectile.InstructionTimer = frameStep.Remaining;
        if (!frameStep.IsZero)
            return;
        bool shown = TryShowNextFrame(context, handlers, out int duration, out EnemyProjectileFrameId frame);
        if (projectile.Animation != this)
            return;
        if (!shown)
        {
            // A held timeline is the native sleep: the timer stays zero and wraps, never expiring again.
            projectile.InstructionTimer = WrappingCountdown.Start(0);
            return;
        }
        projectile.InstructionTimer = WrappingCountdown.FromRaw(WordWrap.Unsigned(duration));
        projectile.Frame = frame;
    }

    /// <summary>Advances the timeline to its next frame, or reports that it holds.</summary>
    /// <param name="context">The slot the animation runs on, Samus and the pass's camera, for cues that act on them.</param>
    /// <param name="handlers">The engine's handlers, which carry out published cues.</param>
    /// <param name="duration">How many projectile passes the frame lasts.</param>
    /// <param name="frame">The frame shown.</param>
    private protected abstract bool TryShowNextFrame(
        EnemyProjectileCueContext context, EnemyProjectileHandlerTable handlers, out int duration, out EnemyProjectileFrameId frame);
}
