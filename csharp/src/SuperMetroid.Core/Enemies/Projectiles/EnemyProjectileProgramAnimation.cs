using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile animation that runs programs of an <see cref="AnimationProgramSet{TProgram, TFrame, TCue}"/>: for
/// families whose native lists run on into, or jump between, one another. Each cue goes to the handler its family
/// registered with <see cref="EnemyProjectileHandlerTable.RegisterCue{TCue}"/>, or with
/// <see cref="EnemyProjectileHandlerTable.RegisterBranchingCue{TProgram, TCue}"/> when a cue may redirect the program,
/// as a native conditional goto does. Counter steps count the projectile's general timer.
/// </summary>
/// <typeparam name="TProgram">The family's program identity.</typeparam>
/// <typeparam name="TFrame">The family's frame identity.</typeparam>
/// <typeparam name="TCue">The family's cue type.</typeparam>
internal abstract class EnemyProjectileProgramAnimation<TProgram, TFrame, TCue> : EnemyProjectileAnimation
    where TProgram : struct
    where TFrame : struct
    where TCue : notnull
{
    /// <summary>Starts <paramref name="program"/>.</summary>
    private protected EnemyProjectileProgramAnimation(TProgram program) => Cursor = AnimationProgramCursor<TProgram>.Start(program);

    /// <summary>Where the program resumes; an imported projectile resumes where its recording stood.</summary>
    internal AnimationProgramCursor<TProgram> Cursor { get; set; }

    /// <summary>The program set this animation runs.</summary>
    private protected abstract AnimationProgramSet<TProgram, TFrame, TCue> Programs { get; }

    /// <summary>The projectile frame <paramref name="frame"/> shows.</summary>
    private protected abstract EnemyProjectileFrameId Present(TFrame frame);

    /// <summary>Runs the program, publishing each cue to the family's registered handler.</summary>
    private protected sealed override bool TryShowNextFrame(
        EnemyProjectileCueContext context, EnemyProjectileHandlerTable handlers, out int duration, out EnemyProjectileFrameId frame)
    {
        RoomEnemyProjectileSlot projectile = context.Projectile;
        AnimationProgramCursor<TProgram> cursor = Cursor;
        // Counter steps use the projectile's general timer, as the native TimerInY and DecrementTimer_GotoYIfNonZero do.
        ushort timerBefore = checked((ushort)projectile.GeneralTimer);
        WrappingCountdown counter = WrappingCountdown.FromRaw(timerBefore);
        AnimationProgramAdvance<TFrame> advance = Programs.Advance(ref cursor, ref counter,
            (cue, continuation) => handlers.PublishProgramCue<TProgram, TCue>(context, cue));
        Cursor = cursor;
        // Only a counter step changes the counter; a deleted projectile keeps its cleared timer.
        if (projectile.Animation == this && counter.RawValue != timerBefore)
            projectile.GeneralTimer = counter.RawValue;
        switch (advance)
        {
            case AnimationProgramAdvance<TFrame>.Show shown:
                duration = shown.Duration;
                frame = Present(shown.Frame);
                return true;
            case AnimationProgramAdvance<TFrame>.Hold:
                duration = 0;
                frame = default;
                return false;
            default:
                throw new InvalidOperationException($"A projectile program ended an update with {advance}.");
        }
    }
}
