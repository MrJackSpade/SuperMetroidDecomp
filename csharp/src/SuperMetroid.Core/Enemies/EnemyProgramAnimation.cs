using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Runs an enemy's typed program (#641) where the enemy instruction interpreter would run its list: the
/// frame expires on the update its timer would, a newly shown frame raises the new-frame flag, a wait reloads
/// the timer without changing the frame, and a hold is the sleep, whose timer wraps from zero. Owners keep
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
        CountdownStep frameStep = slot.FrameHold.Decrement();
        slot.FrameHold = frameStep.Remaining;
        if (frameStep.IsZero)
            return true;
        slot.NewInstructionFrame = false;
        return false;
    }

    /// <summary>
    /// Runs <paramref name="slot"/>'s program after its frame expired, until it shows a frame, waits or holds. Each
    /// cue goes to <paramref name="publish"/>; a program it returns starts at once, as a goto does.
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
    /// One typed-animation step of a species whose state holds its playback (#1600): when the slot's frame has expired,
    /// plays the state's animation on to its next frame and stores the playback back. <paramref name="react"/> carries out
    /// each event against the state and may continue the play in another animation.
    /// </summary>
    /// <param name="slot">The enemy.</param>
    /// <param name="segments">The species' animation segments.</param>
    /// <param name="frame">The installed frame an animation frame shows.</param>
    /// <param name="read">Reads the species state's playback.</param>
    /// <param name="write">Stores the playback back on the species state.</param>
    /// <param name="react">Carries out an event, returning where the play continues, or null to play on.</param>
    internal static void Advance<TState, TSegment, TFrame, TEvent>(
        RoomEnemySlot slot,
        AnimationSegments<TSegment, TFrame, TEvent> segments,
        Func<TFrame, EnemyFrameId> frame,
        Func<TState, AnimationPlayback<TSegment>> read,
        Action<TState, AnimationPlayback<TSegment>> write,
        Func<TState, TEvent, AnimationContinuation<TSegment>?> react)
        where TState : class
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        Ensure.NotNull(slot);
        Ensure.NotNull(read);
        Ensure.NotNull(write);
        Ensure.NotNull(react);
        if (!Expire(slot))
            return;
        TState state = slot.RequireSpeciesState<TState>();
        AnimationPlayback<TSegment> playback = read(state);
        Play(slot, segments, ref playback, frame, raised => react(state, raised));
        write(state, playback);
    }

    /// <summary>
    /// Plays <paramref name="playback"/> on to its next frame after the slot's frame expired (#1600): a frame becomes the
    /// slot's frame for its duration, a pause holds the slot's frame for its own, a hold leaves the slot as it is, and the
    /// animation's end deletes the enemy. The enemy's timer word is the animation's repeat counter, which only its counted
    /// phases load and count down.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation passes every beat without showing a frame.</exception>
    internal static void Play<TSegment, TFrame, TEvent>(
        RoomEnemySlot slot,
        AnimationSegments<TSegment, TFrame, TEvent> segments,
        ref AnimationPlayback<TSegment> playback,
        Func<TFrame, EnemyFrameId> frame,
        Func<TEvent, AnimationContinuation<TSegment>?> react)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        Ensure.NotNull(react);
        Play(slot, segments, ref playback, frame, (raised, _) => react(raised));
    }

    /// <summary>
    /// Plays <paramref name="playback"/> as the other overload does, also telling <paramref name="react"/> where the play
    /// stands just past each event: the place an owner's call saves, to return to later.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation passes every beat without showing a frame.</exception>
    internal static void Play<TSegment, TFrame, TEvent>(
        RoomEnemySlot slot,
        AnimationSegments<TSegment, TFrame, TEvent> segments,
        ref AnimationPlayback<TSegment> playback,
        Func<TFrame, EnemyFrameId> frame,
        Func<TEvent, AnimationPlayback<TSegment>, AnimationContinuation<TSegment>?> react)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        Ensure.NotNull(slot);
        Ensure.NotNull(segments);
        Ensure.NotNull(frame);
        Animation<TSegment> animation = playback.Animation;
        AnimationPlace place = playback.Place;
        // The enemy's timer word is read and written in place, as the load and countdown instructions do, so a write a cue's
        // reaction makes to it in between stands.
        var timer = new EnemyTimerRepeats(slot);
        AnimationShown<TSegment, TFrame> shown = segments.PlayToNextFrame(ref animation, ref place, ref timer, react);
        playback = new AnimationPlayback<TSegment>(animation, place);
        switch (shown)
        {
            case AnimationShown<TSegment, TFrame>.Frame shownFrame:
                slot.FrameHold = WrappingCountdown.FromRaw(shownFrame.Duration);
                slot.Frame = frame(shownFrame.Shown);
                slot.NewInstructionFrame = true;
                return;
            case AnimationShown<TSegment, TFrame>.Paused paused:
                slot.FrameHold = WrappingCountdown.FromRaw(paused.Duration);
                return;
            case AnimationShown<TSegment, TFrame>.Held:
                return;
            case AnimationShown<TSegment, TFrame>.Ended:
                // The common end deletes the enemy.
                slot.Presence = slot.Presence.With(EnemyPresence.Deleted);
                return;
            default:
                throw new InvalidOperationException($"Enemy {slot.DefinitionId}'s animation reached an undefined {shown}.");
        }
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
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>
    /// Like <see cref="Run{TProgram, TFrame, TCue}"/>, but a cue may resume anywhere: <paramref name="publish"/> returns the cursor the program
    /// continues at, as a return to a saved position in the animation does, or null to run on.
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
        // A counting set keeps its loop counter in the enemy's timer word, as the timer-load and countdown
        // commands do; a set that never counts leaves that word to its owner.
        WrappingCountdown loopCounter = programs.CountsLoops ? slot.Timer : WrappingCountdown.Start(0);
        AnimationProgramAdvance<TFrame> advance = programs.AdvanceTo(ref cursor, ref loopCounter, publish);
        if (programs.CountsLoops)
            slot.Timer = loopCounter;
        switch (advance)
        {
            case AnimationProgramAdvance<TFrame>.Show shown:
                slot.FrameHold = WrappingCountdown.FromRaw(shown.Duration);
                slot.Frame = frame(shown.Frame);
                slot.NewInstructionFrame = true;
                return;
            case AnimationProgramAdvance<TFrame>.Wait waiting:
                slot.FrameHold = WrappingCountdown.FromRaw(waiting.Duration);
                return;
            case AnimationProgramAdvance<TFrame>.Hold:
                return;
            case AnimationProgramAdvance<TFrame>.Stop:
                // The common stop deletes the enemy.
                slot.Presence = slot.Presence.With(EnemyPresence.Deleted);
                return;
            default:
                throw new InvalidOperationException($"Enemy {slot.DefinitionId}'s programs have no {advance} step.");
        }
    }
}
