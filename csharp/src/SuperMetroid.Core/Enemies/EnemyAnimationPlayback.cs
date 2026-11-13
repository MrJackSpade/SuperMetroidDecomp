using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Plays an enemy's typed animation (#641, #1624) on its <see cref="EnemyAnimationClock"/>: a step plays on the update
/// the clock's hold runs out, a shown frame holds for its duration and is a new frame on that update, a pause holds the
/// frame shown, and a hold rests where it stands, its clock left to wrap from zero. Owners keep the playback and perform
/// its events: <c>if (slot.AnimationClock.Expire()) { ...; Play(...); }</c>.
/// </summary>
internal static class EnemyAnimationPlayback
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
    /// Starts the animation that begins with <paramref name="segment"/> unless it is the one already started:
    /// <paramref name="start"/> records it on the species state, and its first step plays on the next update.
    /// </summary>
    /// <param name="slot">The enemy.</param>
    /// <param name="started">The segment the species state last started, or null before the first.</param>
    /// <param name="segment">The segment the animation to start begins with.</param>
    /// <param name="start">Records the animation and its starting place on the species state.</param>
    internal static void StartOnce<TSegment>(RoomEnemySlot slot, TSegment? started, TSegment segment, Action<TSegment> start)
        where TSegment : struct, Enum
    {
        Ensure.NotNull(slot);
        Ensure.NotNull(start);
        if (started is { } current && EqualityComparer<TSegment>.Default.Equals(current, segment))
            return;
        start(segment);
        slot.AnimationClock.Restart();
    }
}
