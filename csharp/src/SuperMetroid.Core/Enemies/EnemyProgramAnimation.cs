using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Runs an enemy's typed program (#641) where the enemy instruction interpreter would run its native list: the
/// frame expires on the update its native timer would, a newly shown frame raises the new-frame flag, a wait reloads
/// the timer without changing the frame, and a hold is the native sleep, whose timer wraps from zero. Owners keep
/// the cursor and carry out their cues: <c>if (Expire(slot)) { ...; Run(...); }</c>.
/// </summary>
internal static class EnemyProgramAnimation
{
    /// <summary>
    /// Counts <paramref name="slot"/>'s frame timer down and reports whether its frame expired this update, clearing
    /// the new-frame flag while the frame holds.
    /// </summary>
    internal static bool Expire(RoomEnemySlot slot)
    {
        Ensure.NotNull(slot);
        ushort oldTimer = slot.InstructionTimer.RawValue;
        slot.InstructionTimer = NativeCountdown.FromRaw(unchecked((ushort)(oldTimer - 1)));
        if (oldTimer == 1)
            return true;
        slot.ExtraProperties = slot.ExtraProperties.Without(EnemyExtraProperties.NewInstructionFrame);
        return false;
    }

    /// <summary>
    /// Runs <paramref name="slot"/>'s program after its frame expired, until it shows a frame, waits or holds. Each
    /// cue goes to <paramref name="publish"/>; a program it returns starts at once, as a native goto does.
    /// </summary>
    /// <param name="slot">The enemy the program animates.</param>
    /// <param name="programs">The owner's program set.</param>
    /// <param name="cursor">Where the program resumes; updated in place.</param>
    /// <param name="spritemap">The spritemap a frame shows.</param>
    /// <param name="publish">Carries out a cue, returning the program a goto cue continues at.</param>
    internal static void Run<TProgram, TFrame, TCue>(
        RoomEnemySlot slot,
        AnimationProgramSet<TProgram, TFrame, TCue> programs,
        ref AnimationProgramCursor<TProgram> cursor,
        Func<TFrame, ushort> spritemap,
        Func<TCue, AnimationProgramCursor<TProgram>, TProgram?> publish)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        Ensure.NotNull(publish);
        RunTo(slot, programs, ref cursor, spritemap,
            (cue, continuation) => publish(cue, continuation) is { } program ? AnimationProgramCursor<TProgram>.Start(program) : null);
    }

    /// <summary>
    /// Like <see cref="Run"/>, but a cue may resume anywhere: <paramref name="publish"/> returns the cursor the program
    /// continues at, as a native return to a saved instruction pointer does, or null to run on.
    /// </summary>
    internal static void RunTo<TProgram, TFrame, TCue>(
        RoomEnemySlot slot,
        AnimationProgramSet<TProgram, TFrame, TCue> programs,
        ref AnimationProgramCursor<TProgram> cursor,
        Func<TFrame, ushort> spritemap,
        Func<TCue, AnimationProgramCursor<TProgram>, AnimationProgramCursor<TProgram>?> publish)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        Ensure.NotNull(slot);
        Ensure.NotNull(programs);
        Ensure.NotNull(spritemap);
        // A counting set keeps its loop counter in the enemy's timer word, as the native timer-load and countdown
        // commands do; a set that never counts leaves that word to its owner.
        NativeCountdown loopCounter = programs.CountsLoops ? slot.Timer : NativeCountdown.Start(0);
        AnimationProgramAdvance<TFrame> advance = programs.AdvanceTo(ref cursor, ref loopCounter, publish);
        if (programs.CountsLoops)
            slot.Timer = loopCounter;
        switch (advance.Kind)
        {
            case AnimationProgramAdvanceKind.Show:
                slot.InstructionTimer = NativeCountdown.FromRaw((ushort)advance.Duration);
                slot.SpritemapPointer = spritemap(advance.Frame);
                slot.ExtraProperties = slot.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
                return;
            case AnimationProgramAdvanceKind.Wait:
                slot.InstructionTimer = NativeCountdown.FromRaw((ushort)advance.Duration);
                return;
            case AnimationProgramAdvanceKind.Hold:
                return;
            case AnimationProgramAdvanceKind.Stop:
                // The common stop deletes the enemy.
                slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
                return;
            default:
                throw new InvalidOperationException($"Enemy {slot.EnemyDefinitionPointer}'s programs have no {advance.Kind} step.");
        }
    }
}
