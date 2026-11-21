using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Plays an enemy's typed animation (#641, #1624) on its <see cref="EnemyAnimationClock"/>: a step plays on the update
/// the clock's hold runs out, a shown frame holds for its duration and is a new frame on that update, a pause holds the
/// frame shown, and a frame held for good or a hold rests where it stands, its clock stopped (#2454). Owners keep the
/// playback and perform its events: <c>if (place.AnimationClock.Expire()) { ...; Play(...); }</c>.
/// </summary>
internal static class EnemyAnimationPlayback
{
    /// <summary>
    /// Starts <paramref name="animation"/> on <paramref name="place"/> (#1624): its first step plays on the next update, from
    /// the animation's start. The species state stores the returned playback.
    /// </summary>
    internal static AnimationPlayback<TSegment> Start<TSegment>(RoomEnemyPlace place, Animation<TSegment> animation)
        where TSegment : struct
    {
        Ensure.NotNull(place).AnimationClock.Restart();
        return new AnimationPlayback<TSegment>(Ensure.NotNull(animation), AnimationPlace.StartOfAnimation);
    }

    /// <summary>
    /// Starts <paramref name="animation"/> on <paramref name="place"/> with its first step after <paramref name="updates"/>
    /// updates, from the animation's start: the frame the place shows holds until then. The species state stores the
    /// returned playback.
    /// </summary>
    internal static AnimationPlayback<TSegment> StartAfter<TSegment>(RoomEnemyPlace place, Animation<TSegment> animation, int updates)
        where TSegment : struct
    {
        Ensure.NotNull(place).AnimationClock.RestartAfter(updates);
        return new AnimationPlayback<TSegment>(Ensure.NotNull(animation), AnimationPlace.StartOfAnimation);
    }

    /// <summary>
    /// One typed-animation step of a species whose state holds its playback (#1600): when the place's frame has expired,
    /// plays the state's animation on to its next frame and stores the playback back. <paramref name="react"/> carries out
    /// each event against the state and may continue the play in another animation.
    /// </summary>
    /// <param name="place">The enemy.</param>
    /// <param name="segments">The species' animation segments.</param>
    /// <param name="frame">The installed frame an animation frame shows.</param>
    /// <param name="read">Reads the species state's playback.</param>
    /// <param name="write">Stores the playback back on the species state.</param>
    /// <param name="react">Carries out an event, returning where the play continues, or null to play on.</param>
    internal static void Advance<TState, TSegment, TFrame, TEvent>(
        RoomEnemyPlace place,
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
        Ensure.NotNull(place);
        Ensure.NotNull(read);
        Ensure.NotNull(write);
        Ensure.NotNull(react);
        if (!place.AnimationClock.Expire())
            return;
        TState state = place.RequireSpeciesState<TState>();
        AnimationPlayback<TSegment> playback = read(state);
        Play(place, segments, ref playback, frame, raised => react(state, raised));
        write(state, playback);
    }

    /// <summary>
    /// One typed-animation step of a species whose animations show its installed frames directly, as the other overload
    /// steps one that shows them through <c>frame</c>.
    /// </summary>
    internal static void Advance<TState, TSegment, TEvent>(
        RoomEnemyPlace place,
        AnimationSegments<TSegment, EnemyFrameId, TEvent> segments,
        Func<TState, AnimationPlayback<TSegment>> read,
        Action<TState, AnimationPlayback<TSegment>> write,
        Func<TState, TEvent, AnimationContinuation<TSegment>?> react)
        where TState : class
        where TSegment : struct
        where TEvent : notnull =>
        Advance(place, segments, Shown, read, write, react);

    /// <summary>Plays an animation that shows the species' installed frames directly, as the other overload plays one.</summary>
    /// <exception cref="InvalidOperationException">The animation passes every beat without showing a frame.</exception>
    internal static void Play<TSegment, TEvent>(
        RoomEnemyPlace place,
        AnimationSegments<TSegment, EnemyFrameId, TEvent> segments,
        ref AnimationPlayback<TSegment> playback,
        Func<TEvent, AnimationContinuation<TSegment>?> react)
        where TSegment : struct
        where TEvent : notnull =>
        Play(place, segments, ref playback, Shown, react);

    /// <summary>
    /// Plays an animation whose cues never start another, as <see cref="Play{TSegment, TEvent}(RoomEnemyPlace, AnimationSegments{TSegment, EnemyFrameId, TEvent}, ref AnimationPlayback{TSegment}, Func{TEvent, AnimationContinuation{TSegment}?})"/>
    /// plays one: <paramref name="apply"/> carries out each cue and the animation plays on.
    /// </summary>
    internal static void PlayApplyingCues<TSegment, TEvent>(
        RoomEnemyPlace place,
        AnimationSegments<TSegment, EnemyFrameId, TEvent> segments,
        ref AnimationPlayback<TSegment> playback,
        Action<TEvent> apply)
        where TSegment : struct
        where TEvent : notnull =>
        Play(place, segments, ref playback, Shown, (TEvent cue) =>
        {
            apply(cue);
            return null;
        });

    /// <summary>
    /// One typed-animation step of a species whose cues never start another animation, as
    /// <see cref="Advance{TState, TSegment, TEvent}(RoomEnemyPlace, AnimationSegments{TSegment, EnemyFrameId, TEvent}, Func{TState, AnimationPlayback{TSegment}}, Action{TState, AnimationPlayback{TSegment}}, Func{TState, TEvent, AnimationContinuation{TSegment}?})"/>
    /// takes one: <paramref name="apply"/> carries out each cue and the animation plays on.
    /// </summary>
    internal static void AdvanceApplyingCues<TState, TSegment, TEvent>(
        RoomEnemyPlace place,
        AnimationSegments<TSegment, EnemyFrameId, TEvent> segments,
        Func<TState, AnimationPlayback<TSegment>> read,
        Action<TState, AnimationPlayback<TSegment>> write,
        Action<TState, TEvent> apply)
        where TState : class
        where TSegment : struct
        where TEvent : notnull =>
        Advance(place, segments, read, write, (TState state, TEvent cue) =>
        {
            apply(state, cue);
            return null;
        });

    /// <summary>
    /// Plays an animation that shows the species' installed frames directly, telling <paramref name="react"/> where the
    /// play stands just past each event, as the other overload does.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation passes every beat without showing a frame.</exception>
    internal static void Play<TSegment, TEvent>(
        RoomEnemyPlace place,
        AnimationSegments<TSegment, EnemyFrameId, TEvent> segments,
        ref AnimationPlayback<TSegment> playback,
        Func<TEvent, AnimationPlayback<TSegment>, AnimationContinuation<TSegment>?> react)
        where TSegment : struct
        where TEvent : notnull =>
        Play(place, segments, ref playback, Shown, react);

    /// <summary>
    /// One typed-animation step of a species whose animations raise no event (#2553): frames, holds and repeats only, as
    /// <see cref="Advance{TState, TSegment, TEvent}(RoomEnemyPlace, AnimationSegments{TSegment, EnemyFrameId, TEvent}, Func{TState, AnimationPlayback{TSegment}}, Action{TState, AnimationPlayback{TSegment}}, Func{TState, TEvent, AnimationContinuation{TSegment}?})"/>
    /// steps one with events. <see cref="NoAnimationCue"/> has no member, so the step takes no reaction to carry one out.
    /// </summary>
    internal static void AdvanceSilently<TState, TSegment>(
        RoomEnemyPlace place,
        AnimationSegments<TSegment, EnemyFrameId, NoAnimationCue> segments,
        Func<TState, AnimationPlayback<TSegment>> read,
        Action<TState, AnimationPlayback<TSegment>> write)
        where TState : class
        where TSegment : struct =>
        Advance(place, segments, read, write, static (TState _, NoAnimationCue _) => null);

    /// <summary>
    /// Plays an animation that raises no event and shows the species' installed frames directly (#2553), as
    /// <see cref="Play{TSegment, TEvent}(RoomEnemyPlace, AnimationSegments{TSegment, EnemyFrameId, TEvent}, ref AnimationPlayback{TSegment}, Func{TEvent, AnimationContinuation{TSegment}?})"/>
    /// plays one with events.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation passes every beat without showing a frame.</exception>
    internal static void PlaySilently<TSegment>(
        RoomEnemyPlace place,
        AnimationSegments<TSegment, EnemyFrameId, NoAnimationCue> segments,
        ref AnimationPlayback<TSegment> playback)
        where TSegment : struct =>
        Play(place, segments, ref playback, Shown, static (NoAnimationCue _) => null);

    /// <summary>
    /// Plays an animation that raises no event and shows its frames through <paramref name="frame"/> (#2553), as the other
    /// overload plays one that shows the installed frames directly.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation passes every beat without showing a frame.</exception>
    internal static void PlaySilently<TSegment, TFrame>(
        RoomEnemyPlace place,
        AnimationSegments<TSegment, TFrame, NoAnimationCue> segments,
        ref AnimationPlayback<TSegment> playback,
        Func<TFrame, EnemyFrameId> frame)
        where TSegment : struct
        where TFrame : struct =>
        Play(place, segments, ref playback, frame, static (NoAnimationCue _) => null);

    /// <summary>An installed frame an animation names outright, which it shows as it is.</summary>
    private static EnemyFrameId Shown(EnemyFrameId frame) => frame;

    /// <summary>
    /// Plays <paramref name="playback"/> on to its next frame after the place's frame expired (#1600): a frame becomes the
    /// place's frame for its duration, a held frame becomes it for good (#1607), a pause holds the place's frame for its own, a
    /// hold leaves the place as it is with its clock stopped (#2454), and the animation's end deletes the enemy and stops
    /// its clock (#2532).
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation passes every beat without showing a frame.</exception>
    internal static void Play<TSegment, TFrame, TEvent>(
        RoomEnemyPlace place,
        AnimationSegments<TSegment, TFrame, TEvent> segments,
        ref AnimationPlayback<TSegment> playback,
        Func<TFrame, EnemyFrameId> frame,
        Func<TEvent, AnimationContinuation<TSegment>?> react)
        where TSegment : struct
        where TFrame : struct
        where TEvent : notnull
    {
        Ensure.NotNull(react);
        Play(place, segments, ref playback, frame, (raised, _) => react(raised));
    }

    /// <summary>
    /// Plays <paramref name="playback"/> as the other overload does, also telling <paramref name="react"/> where the play
    /// stands just past each event: the place an owner keeps to come back to later.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation passes every beat without showing a frame.</exception>
    internal static void Play<TSegment, TFrame, TEvent>(
        RoomEnemyPlace enemy,
        AnimationSegments<TSegment, TFrame, TEvent> segments,
        ref AnimationPlayback<TSegment> playback,
        Func<TFrame, EnemyFrameId> frame,
        Func<TEvent, AnimationPlayback<TSegment>, AnimationContinuation<TSegment>?> react)
        where TSegment : struct
        where TFrame : struct
        where TEvent : notnull
    {
        Ensure.NotNull(enemy);
        Ensure.NotNull(segments);
        Ensure.NotNull(frame);
        Animation<TSegment> animation = playback.Animation;
        AnimationPlace place = playback.Place;
        AnimationShown<TSegment, TFrame> shown = segments.PlayToNextFrame(ref animation, ref place, react);
        playback = new AnimationPlayback<TSegment>(animation, place);
        Show(enemy, shown, frame);
    }

    /// <summary>
    /// Shows on <paramref name="enemy"/> what one step of its animation came to (#2553): a frame for its updates or for good
    /// through <paramref name="frame"/>, a pause, a rest on the frame shown last, or the common end, which deletes the enemy.
    /// </summary>
    internal static void Show<TSegment, TFrame>(RoomEnemyPlace enemy, AnimationShown<TSegment, TFrame> shown, Func<TFrame, EnemyFrameId> frame)
        where TSegment : struct
        where TFrame : struct
    {
        Ensure.NotNull(frame);
        Show(enemy, shown, static (shownOn, picture, frameOf) => shownOn.Frame = frameOf(picture), frame);
    }

    /// <summary>
    /// Shows on <paramref name="enemy"/> what one step of its animation came to, as the other overload does, each frame it
    /// shows through <paramref name="showFrame"/>, which reads <paramref name="context"/> besides the enemy and the frame
    /// (#2553), so that no step builds a reaction of its own.
    /// </summary>
    internal static void Show<TSegment, TFrame, TContext>(
        RoomEnemyPlace enemy, AnimationShown<TSegment, TFrame> shown, Action<RoomEnemyPlace, TFrame, TContext> showFrame, TContext context)
        where TSegment : struct
        where TFrame : struct
    {
        Ensure.NotNull(enemy);
        Ensure.NotNull(showFrame);
        Ensure.NotNull(shown).React(ShownOn<TSegment, TFrame, TContext>.Reactions, new EnemyFrameShowing<TFrame, TContext>(enemy, showFrame, context));
    }

    /// <summary>
    /// An enemy a step of its animation shows on (#2553): the enemy, how it shows a frame, and what that reads besides the
    /// enemy and the frame.
    /// </summary>
    /// <param name="Enemy">The enemy.</param>
    /// <param name="ShowFrame">Shows a frame on the enemy.</param>
    /// <param name="Context">What showing a frame reads besides the enemy and the frame.</param>
    private readonly record struct EnemyFrameShowing<TFrame, TContext>(RoomEnemyPlace Enemy, Action<RoomEnemyPlace, TFrame, TContext> ShowFrame, TContext Context)
        where TFrame : struct;

    /// <summary>What a step of an enemy's animation shows on it, for each outcome, built once per animation (#2553).</summary>
    private static class ShownOn<TSegment, TFrame, TContext>
        where TSegment : struct
        where TFrame : struct
    {
        /// <summary>
        /// A frame for its updates or for good, a pause, a rest on the frame shown last, or the common end, which deletes the
        /// enemy.
        /// </summary>
        internal static AnimationShownReactions<TSegment, TFrame, EnemyFrameShowing<TFrame, TContext>> Reactions { get; } = new(
            Frame: static (shownFrame, showing) =>
            {
                showing.ShowFrame(showing.Enemy, shownFrame.Shown, showing.Context);
                showing.Enemy.AnimationClock.Show(shownFrame.Duration);
            },
            HeldFrame: static (held, showing) =>
            {
                showing.ShowFrame(showing.Enemy, held.Shown, showing.Context);
                showing.Enemy.AnimationClock.ShowForGood();
            },
            Paused: static (paused, showing) => showing.Enemy.AnimationClock.Pause(paused.Duration),
            Held: static (_, showing) => showing.Enemy.AnimationClock.Stop(),
            Ended: static (_, showing) =>
            {
                // The common end deletes the enemy, and its animation takes no further step (#2532).
                showing.Enemy.Presence = showing.Enemy.Presence.With(EnemyPresence.Deleted);
                showing.Enemy.AnimationClock.Stop();
            });
    }

    /// <summary>
    /// Starts <paramref name="animation"/> on <paramref name="place"/> unless <paramref name="playing"/> already plays it
    /// (#2145): asking for the animation playing leaves it running where it stands, and only a different animation starts
    /// from its beginning and restarts the clock, its first step playing on the next update. The species state stores the
    /// returned playback.
    /// </summary>
    internal static AnimationPlayback<TSegment> StartUnlessPlaying<TSegment>(
        RoomEnemyPlace place, AnimationPlayback<TSegment> playing, Animation<TSegment> animation)
        where TSegment : struct =>
        ReferenceEquals(playing.Animation, Ensure.NotNull(animation)) ? playing : Start(place, animation);
}
