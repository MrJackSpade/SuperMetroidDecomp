namespace SuperMetroid.Core.Animation;

/// <summary>
/// An animation that shows a frame from the moment it starts (#2554): an <see cref="AnimationPlayer{TSegment, TFrame, TEvent}"/>
/// started and restarted only on a frame, as the menus', maps' and title's pictures, the glows and the grapple's beam are,
/// so the frame it shows is always present and its owner reads it without a check. It offers no start that shows nothing
/// and no backward play, which can leave a player with no frame, and it plays only segments no beat of which hides the frame,
/// which it checks once when it is started or rebound.
/// </summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
internal sealed partial class ShowingAnimationPlayer<TSegment, TFrame, TEvent>
    where TSegment : struct
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>The play, which always stands on a frame.</summary>
    private readonly AnimationPlayer<TSegment, TFrame, TEvent> player;

    /// <summary>Holds <paramref name="player"/>, which shows <paramref name="first"/>.</summary>
    private ShowingAnimationPlayer(AnimationPlayer<TSegment, TFrame, TEvent> player, TFrame first)
    {
        this.player = player;
        Showing = first;
    }

    /// <summary>The frame shown.</summary>
    internal TFrame Showing { get; private set; }

    /// <summary>The animation playing and where its play stands.</summary>
    internal AnimationPlayback<TSegment> Playback => player.Playback;

    /// <summary>The updates the shown frame, or the pause, has left before the play moves on.</summary>
    internal int UpdatesLeft => player.UpdatesLeft;

    /// <summary>Whether the animation rests for good: on a held frame, or past its end where it holds or stops.</summary>
    internal bool IsResting => player.IsResting;

    /// <summary>Whether the animation has stopped: it played its last phase and ends there, its last frame still shown.</summary>
    internal bool HasStopped => player.HasStopped;

    /// <summary>
    /// Starts <paramref name="animation"/> on its first frame, shown at once for <paramref name="firstFrameUpdates"/> updates,
    /// as <see cref="AnimationPlayer{TSegment, TFrame, TEvent}.StartShowing(AnimationSegments{TSegment, TFrame, TEvent}, Animation{TSegment}, int)"/> does.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="firstFrameUpdates"/> is below one.</exception>
    /// <exception cref="InvalidOperationException">The animation raises an event or pauses before its first frame.</exception>
    internal static ShowingAnimationPlayer<TSegment, TFrame, TEvent> StartShowing(
        AnimationSegments<TSegment, TFrame, TEvent> segments, Animation<TSegment> animation, int firstFrameUpdates)
    {
        var player = AnimationPlayer<TSegment, TFrame, TEvent>.Starting(segments, animation);
        return new(player, player.Show(animation, firstFrameUpdates));
    }

    /// <summary>
    /// Starts <paramref name="animation"/> on its first frame, shown at once for that frame's own updates, as
    /// <see cref="AnimationPlayer{TSegment, TFrame, TEvent}.StartShowing(AnimationSegments{TSegment, TFrame, TEvent}, Animation{TSegment})"/> does.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation raises an event or pauses before its first frame.</exception>
    internal static ShowingAnimationPlayer<TSegment, TFrame, TEvent> StartShowing(
        AnimationSegments<TSegment, TFrame, TEvent> segments, Animation<TSegment> animation)
    {
        var player = AnimationPlayer<TSegment, TFrame, TEvent>.Starting(segments, animation);
        return new(player, player.Show(animation));
    }

    /// <summary>
    /// Starts <paramref name="animation"/> at <paramref name="place"/>, showing at once the frame it plays to from there for
    /// <paramref name="firstFrameUpdates"/> updates, as
    /// <see cref="AnimationPlayer{TSegment, TFrame, TEvent}.ShowAt(Animation{TSegment}, AnimationPlace, int)"/> does.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="firstFrameUpdates"/> is below one.</exception>
    /// <exception cref="InvalidOperationException">The animation raises an event or pauses before the frame.</exception>
    internal static ShowingAnimationPlayer<TSegment, TFrame, TEvent> StartShowingAt(
        AnimationSegments<TSegment, TFrame, TEvent> segments, Animation<TSegment> animation, AnimationPlace place, int firstFrameUpdates)
    {
        var player = AnimationPlayer<TSegment, TFrame, TEvent>.Starting(segments, animation);
        return new(player, player.ShowAt(animation, place, firstFrameUpdates));
    }

    /// <summary>Starts <paramref name="animation"/> again on its first frame, for <paramref name="firstFrameUpdates"/> updates.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="firstFrameUpdates"/> is below one.</exception>
    /// <exception cref="InvalidOperationException">The animation raises an event or pauses before its first frame.</exception>
    internal void Show(Animation<TSegment> animation, int firstFrameUpdates) => Showing = player.Show(animation, firstFrameUpdates);

    /// <summary>Starts <paramref name="animation"/> again on its first frame, for that frame's own updates.</summary>
    /// <exception cref="InvalidOperationException">The animation raises an event or pauses before its first frame.</exception>
    internal void Show(Animation<TSegment> animation) => Showing = player.Show(animation);

    /// <summary>Starts <paramref name="animation"/> again at <paramref name="place"/>, on the frame it plays to from there, for that frame's own updates.</summary>
    /// <exception cref="InvalidOperationException">The animation raises an event or pauses before the frame.</exception>
    internal void ShowFrom(Animation<TSegment> animation, AnimationPlace place) => Showing = player.ShowFrom(animation, place);

    /// <summary>Runs one update of an animation that raises no event.</summary>
    /// <exception cref="InvalidOperationException">The animation raises an event, which this owner has nothing to act on.</exception>
    internal void Step() => _ = StepToNewFrame();

    /// <summary>
    /// Runs one update of an animation that raises no event, and returns the frame it shows anew, or null while the shown
    /// frame holds, while the play pauses and once it rests.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation raises an event, which this owner has nothing to act on.</exception>
    internal TFrame? StepToNewFrame() => Shown(player.StepToNewFrame());

    /// <summary>Runs one update, raising the events it passes in <paramref name="raise"/>.</summary>
    /// <exception cref="InvalidOperationException">The animation passes every beat without showing a frame.</exception>
    internal void Step(Action<TEvent> raise) => _ = Shown(player.StepToNewFrame(raise));

    /// <summary>
    /// Runs one update of an animation that raises no event, but once the shown frame's updates run out plays on from
    /// <paramref name="place"/>, as <see cref="AnimationPlayer{TSegment, TFrame, TEvent}.StepPlayingOnFrom"/> does.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation raises an event, which this owner has nothing to act on.</exception>
    internal void StepPlayingOnFrom(AnimationPlace place) => _ = Shown(player.StepPlayingOnFrom(place));

    /// <summary>Shows the shown frame for <paramref name="updates"/> more updates from now.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is below one.</exception>
    /// <exception cref="InvalidOperationException">The animation rests.</exception>
    internal void HoldShownFrameFor(int updates) => player.HoldShownFrameFor(updates);

    /// <summary>A copy of the play as it stands, which plays on from here on its own (a count that takes the play over).</summary>
    internal AnimationPlayer<TSegment, TFrame, TEvent> Copy() => player.Copy();

    /// <summary>
    /// Plays the animation's segments as <paramref name="rebound"/> holds them from now on, keeping where the play stands,
    /// as <see cref="AnimationPlayer{TSegment, TFrame, TEvent}.Rebind"/> does; the shown frame is the rebound one.
    /// </summary>
    internal void Rebind(AnimationSegments<TSegment, TFrame, TEvent> rebound, Animation<TSegment> animation) =>
        _ = Shown(player.Rebind(rebound, animation));

    /// <summary>Keeps <paramref name="shown"/> as the frame shown, when the play showed one anew, and returns it.</summary>
    private TFrame? Shown(TFrame? shown)
    {
        if (shown is { } frame)
            Showing = frame;
        return shown;
    }
}
