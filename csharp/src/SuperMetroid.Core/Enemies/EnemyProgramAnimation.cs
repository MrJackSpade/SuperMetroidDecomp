using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Runs an enemy's typed program (#641) as the shared enemy animation runs its list: the
/// frame expires on the update its timer would, a newly shown frame raises the new-frame flag, a wait reloads
/// the timer without changing the frame, and a hold is the sleep, whose timer wraps from zero. Owners keep
/// the cursor and perform their cues: <c>if (Expire(slot)) { ...; Run(...); }</c>.
/// </summary>
internal static class EnemyProgramAnimation
{
    /// <summary>
    /// Starts <paramref name="animation"/> on <paramref name="slot"/> (#1624): its first step plays on the next update, with
    /// no repeats counted, from the animation's start. The species state stores the returned playback.
    /// </summary>
    internal static AnimationPlayback<TSegment> Start<TSegment>(RoomEnemySlot slot, Animation<TSegment> animation)
        where TSegment : struct
    {
        Ensure.NotNull(slot).AnimationClock.Restart();
        return new AnimationPlayback<TSegment>(Ensure.NotNull(animation), AnimationPlace.StartOfAnimation);
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
        where TSegment : struct
        where TFrame : struct
        where TEvent : notnull
    {
        Ensure.NotNull(slot);
        Ensure.NotNull(read);
        Ensure.NotNull(write);
        Ensure.NotNull(react);
        if (!slot.AnimationClock.Expire())
            return;
        TState state = slot.RequireSpeciesState<TState>();
        AnimationPlayback<TSegment> playback = read(state);
        Play(slot, segments, ref playback, frame, raised => react(state, raised));
        write(state, playback);
    }

    /// <summary>
    /// One typed-animation step of a species whose animations show its installed frames directly, as the other overload
    /// steps one that shows them through <c>frame</c>.
    /// </summary>
    internal static void Advance<TState, TSegment, TEvent>(
        RoomEnemySlot slot,
        AnimationSegments<TSegment, EnemyFrameId, TEvent> segments,
        Func<TState, AnimationPlayback<TSegment>> read,
        Action<TState, AnimationPlayback<TSegment>> write,
        Func<TState, TEvent, AnimationContinuation<TSegment>?> react)
        where TState : class
        where TSegment : struct
        where TEvent : notnull =>
        Advance(slot, segments, Shown, read, write, react);

    /// <summary>Plays an animation that shows the species' installed frames directly, as the other overload plays one.</summary>
    /// <exception cref="InvalidOperationException">The animation passes every beat without showing a frame.</exception>
    internal static void Play<TSegment, TEvent>(
        RoomEnemySlot slot,
        AnimationSegments<TSegment, EnemyFrameId, TEvent> segments,
        ref AnimationPlayback<TSegment> playback,
        Func<TEvent, AnimationContinuation<TSegment>?> react)
        where TSegment : struct
        where TEvent : notnull =>
        Play(slot, segments, ref playback, Shown, react);

    /// <summary>
    /// Plays an animation that shows the species' installed frames directly, telling <paramref name="react"/> where the
    /// play stands just past each event, as the other overload does.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation passes every beat without showing a frame.</exception>
    internal static void Play<TSegment, TEvent>(
        RoomEnemySlot slot,
        AnimationSegments<TSegment, EnemyFrameId, TEvent> segments,
        ref AnimationPlayback<TSegment> playback,
        Func<TEvent, AnimationPlayback<TSegment>, AnimationContinuation<TSegment>?> react)
        where TSegment : struct
        where TEvent : notnull =>
        Play(slot, segments, ref playback, Shown, react);

    /// <summary>An installed frame an animation names outright, which it shows as it is.</summary>
    private static EnemyFrameId Shown(EnemyFrameId frame) => frame;

    /// <summary>
    /// Plays <paramref name="playback"/> on to its next frame after the slot's frame expired (#1600): a frame becomes the
    /// slot's frame for its duration, a held frame becomes it for good (#1607), a pause holds the slot's frame for its own, a
    /// hold leaves the slot as it is, and the
    /// animation's end deletes the enemy. The enemy's frame timer is the animation's repeat counter, which only its counted
    /// phases load and count down.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation passes every beat without showing a frame.</exception>
    internal static void Play<TSegment, TFrame, TEvent>(
        RoomEnemySlot slot,
        AnimationSegments<TSegment, TFrame, TEvent> segments,
        ref AnimationPlayback<TSegment> playback,
        Func<TFrame, EnemyFrameId> frame,
        Func<TEvent, AnimationContinuation<TSegment>?> react)
        where TSegment : struct
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
        where TSegment : struct
        where TFrame : struct
        where TEvent : notnull
    {
        Ensure.NotNull(slot);
        Ensure.NotNull(segments);
        Ensure.NotNull(frame);
        Animation<TSegment> animation = playback.Animation;
        AnimationPlace place = playback.Place;
        // The clock's repeats are read and written in place as counted phases load and count them, so a restart a cue's
        // reaction makes in between stands.
        EnemyAnimationClock clock = slot.AnimationClock;
        AnimationShown<TSegment, TFrame> shown = segments.PlayToNextFrame(ref animation, ref place, ref clock, react);
        playback = new AnimationPlayback<TSegment>(animation, place);
        switch (shown)
        {
            case AnimationShown<TSegment, TFrame>.Frame shownFrame:
                slot.Frame = frame(shownFrame.Shown);
                slot.AnimationClock.Show(shownFrame.Duration);
                return;
            case AnimationShown<TSegment, TFrame>.HeldFrame held:
                slot.Frame = frame(held.Shown);
                slot.AnimationClock.ShowForGood();
                return;
            case AnimationShown<TSegment, TFrame>.Paused paused:
                slot.AnimationClock.Pause(paused.Duration);
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
    /// on the species state, and its first cue runs on the next frame.
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
        slot.AnimationClock.Restart();
    }
}
