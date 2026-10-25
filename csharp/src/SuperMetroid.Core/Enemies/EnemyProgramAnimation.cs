using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

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
        int oldTimer = slot.InstructionTimer.RawValue;
        slot.InstructionTimer = slot.InstructionTimer.Decrement().Remaining;
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
    /// <param name="frame">The installed frame a program frame shows.</param>
    /// <param name="publish">Carries out a cue, returning the program a goto cue continues at.</param>
    internal static void Run<TProgram, TFrame, TCue>(
        RoomEnemySlot slot,
        AnimationProgramSet<TProgram, TFrame, TCue> programs,
        ref AnimationProgramCursor<TProgram> cursor,
        Func<TFrame, EnemyFrameId> frame,
        Func<TCue, AnimationProgramCursor<TProgram>, TProgram?> publish)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        Ensure.NotNull(publish);
        RunTo(slot, programs, ref cursor, frame,
            (cue, continuation) => publish(cue, continuation) is { } program ? AnimationProgramCursor<TProgram>.Start(program) : null);
    }

    /// <summary>
    /// One typed-animation step of a species whose state holds its program cursor: when the slot's frame has expired,
    /// runs the program the state's cursor names and stores the cursor back.
    /// </summary>
    /// <param name="slot">The enemy.</param>
    /// <param name="programs">The species' program set.</param>
    /// <param name="frame">The installed frame a program frame shows.</param>
    /// <param name="read">Reads the species state's program cursor.</param>
    /// <param name="write">Stores the cursor back on the species state.</param>
    internal static void Advance<TState, TProgram, TFrame>(
        RoomEnemySlot slot,
        AnimationProgramSet<TProgram, TFrame, NoAnimationCue> programs,
        Func<TFrame, EnemyFrameId> frame,
        Func<TState, AnimationProgramCursor<TProgram>> read,
        Action<TState, AnimationProgramCursor<TProgram>> write)
        where TState : class
        where TProgram : struct, Enum
        where TFrame : struct
    {
        Ensure.NotNull(slot);
        if (!Expire(slot))
            return;
        TState state = slot.RequireSpeciesState<TState>();
        AnimationProgramCursor<TProgram> cursor = read(state);
        Run(slot, programs, ref cursor, frame);
        write(state, cursor);
    }

    /// <summary>Like the cue-free step, for a program set whose cues <paramref name="publish"/> carries out against the state.</summary>
    /// <param name="slot">The enemy.</param>
    /// <param name="programs">The species' program set.</param>
    /// <param name="frame">The installed frame a program frame shows.</param>
    /// <param name="read">Reads the species state's program cursor.</param>
    /// <param name="write">Stores the cursor back on the species state.</param>
    /// <param name="publish">Carries out a cue, returning the program a goto cue continues at.</param>
    internal static void Advance<TState, TProgram, TFrame, TCue>(
        RoomEnemySlot slot,
        AnimationProgramSet<TProgram, TFrame, TCue> programs,
        Func<TFrame, EnemyFrameId> frame,
        Func<TState, AnimationProgramCursor<TProgram>> read,
        Action<TState, AnimationProgramCursor<TProgram>> write,
        Func<TState, TCue, AnimationProgramCursor<TProgram>, TProgram?> publish)
        where TState : class
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        Ensure.NotNull(slot);
        if (!Expire(slot))
            return;
        TState state = slot.RequireSpeciesState<TState>();
        AnimationProgramCursor<TProgram> cursor = read(state);
        Run(slot, programs, ref cursor, frame, (cue, continuation) => publish(state, cue, continuation));
        write(state, cursor);
    }

    /// <summary>
    /// Installs <paramref name="program"/> unless it is already the installed one: <paramref name="install"/> records it
    /// on the species state, and its first instruction runs on the next frame.
    /// </summary>
    /// <param name="slot">The enemy.</param>
    /// <param name="installed">The program the species state last installed, or null before the first.</param>
    /// <param name="program">The program to install.</param>
    /// <param name="install">Records the program and its starting cursor on the species state.</param>
    internal static void InstallOnce<TProgram>(RoomEnemySlot slot, TProgram? installed, TProgram program, Action<TProgram> install)
        where TProgram : struct, Enum
    {
        Ensure.NotNull(slot);
        if (installed is { } current && EqualityComparer<TProgram>.Default.Equals(current, program))
            return;
        install(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Runs a program set whose lists publish no cues, after the slot's frame expired.</summary>
    internal static void Run<TProgram, TFrame>(
        RoomEnemySlot slot,
        AnimationProgramSet<TProgram, TFrame, NoAnimationCue> programs,
        ref AnimationProgramCursor<TProgram> cursor,
        Func<TFrame, EnemyFrameId> frame)
        where TProgram : struct, Enum
        where TFrame : struct =>
        Run(slot, programs, ref cursor, frame,
            static (cue, _) => throw new InvalidOperationException($"A cue-free program published {cue}."));

    /// <summary>
    /// Like <see cref="Run{TProgram, TFrame, TCue}"/>, but a cue may resume anywhere: <paramref name="publish"/> returns the cursor the program
    /// continues at, as a native return to a saved instruction pointer does, or null to run on.
    /// </summary>
    internal static void RunTo<TProgram, TFrame, TCue>(
        RoomEnemySlot slot,
        AnimationProgramSet<TProgram, TFrame, TCue> programs,
        ref AnimationProgramCursor<TProgram> cursor,
        Func<TFrame, EnemyFrameId> frame,
        Func<TCue, AnimationProgramCursor<TProgram>, AnimationProgramCursor<TProgram>?> publish)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        Ensure.NotNull(slot);
        Ensure.NotNull(programs);
        Ensure.NotNull(frame);
        // A counting set keeps its loop counter in the enemy's timer word, as the native timer-load and countdown
        // commands do; a set that never counts leaves that word to its owner.
        WrappingCountdown loopCounter = programs.CountsLoops ? slot.Timer : WrappingCountdown.Start(0);
        AnimationProgramAdvance<TFrame> advance = programs.AdvanceTo(ref cursor, ref loopCounter, publish);
        if (programs.CountsLoops)
            slot.Timer = loopCounter;
        switch (advance)
        {
            case AnimationProgramAdvance<TFrame>.Show shown:
                slot.InstructionTimer = WrappingCountdown.FromRaw((ushort)shown.Duration);
                slot.Frame = frame(shown.Frame);
                slot.ExtraProperties = slot.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
                return;
            case AnimationProgramAdvance<TFrame>.Wait waiting:
                slot.InstructionTimer = WrappingCountdown.FromRaw((ushort)waiting.Duration);
                return;
            case AnimationProgramAdvance<TFrame>.Hold:
                return;
            case AnimationProgramAdvance<TFrame>.Stop:
                // The common stop deletes the enemy.
                slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
                return;
            default:
                throw new InvalidOperationException($"Enemy {slot.DefinitionId}'s programs have no {advance} step.");
        }
    }
}
