using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.EffectActors;

/// <summary>
/// One room effect actor: an animation-driven decorative effect (dust, an explosion, a Metroid's shell, a Nuclear Waffle
/// link, one of Draygon's Evirs), created fresh each time one is spawned (#1591). Its spawner holds it when it moves,
/// recolours, freezes or ends it; the room's <see cref="RoomEffectActors"/> steps and draws it until it ends.
/// </summary>
public sealed class RoomEffectActor
{
    /// <summary>Spawns an object of <paramref name="kind"/> at <paramref name="position"/>, showing its animation's first frame.</summary>
    /// <param name="kind">What the object is, which selects the animation it plays.</param>
    /// <param name="position">Where it stands, in whole room pixels.</param>
    /// <param name="graphics">The artwork and colours it draws with.</param>
    internal RoomEffectActor(RoomEffectActorKind kind, Position16<RoomPixels> position, SpriteGraphics graphics)
    {
        Kind = kind;
        Position = FixedPosition<RoomPixels>.FromWhole(position);
        Graphics = graphics;
        Playback = new(RoomEffectActorAnimations.StartingWith(kind), AnimationPlace.StartOfAnimation);
        AdvanceAnimation();
    }

    /// <summary>What the object shows, as its spawner chose it, which is also the animation it plays and restarts.</summary>
    private RoomEffectActorKind Kind { get; }

    /// <summary>Whether the object still runs its animation; once it ends it never runs or shows again.</summary>
    public bool IsActive => Playback is not null;

    /// <summary>
    /// The room position, with its fraction of a pixel, converted to screen coordinates by subtracting the camera when drawn. Most
    /// effect actors move only in whole pixels; Draygon's six burial Evirs move by fractional speeds, so discarding the
    /// fractions visibly changes their fan-in.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }

    /// <summary>The artwork and colours the object draws with.</summary>
    public SpriteGraphics Graphics { get; internal set; }

    /// <summary>The animation the object plays and where it stands in it (#1600), or null once it has ended.</summary>
    private AnimationPlayback<RoomEffectActorSegment>? Playback { get; set; }

    /// <summary>
    /// The object's animation clock (#2477), as an enemy's is: how long <see cref="Frame"/> holds before the animation moves
    /// on. A still picture, or the animation resting at its hold, stops it, so the object shows that frame until it ends; its
    /// owner suspends it while the object must keep its frame where it stands, drawing it still.
    /// </summary>
    public EnemyAnimationClock AnimationClock { get; } = new();

    /// <summary>The frame the object shows.</summary>
    internal RoomEffectActorPicture Frame { get; private set; }

    /// <summary>Ends the object: it stops running and showing.</summary>
    internal void End() => Playback = null;

    /// <summary>Counts one update off the shown frame on the object's clock and moves the animation on when it runs out.</summary>
    internal void Step()
    {
        if (IsActive && AnimationClock.Expire())
            AdvanceAnimation();
    }

    /// <summary>
    /// Plays the animation to its next frame: a frame shows for its duration, a still picture or the hold keeps the frame for
    /// good, stopping the clock, and the animation's end ends the object. The animations count no repeats and raise no events.
    /// </summary>
    private void AdvanceAnimation()
    {
        AnimationPlayback<RoomEffectActorSegment> playback = Playback ?? throw new InvalidOperationException(
            $"Effect actor {Kind} has no animation to play.");
        Animation<RoomEffectActorSegment> animation = playback.Animation;
        AnimationPlace place = playback.Place;
        AnimationShown<RoomEffectActorSegment, RoomEffectActorPicture>.Unpaused shown = RoomEffectActorAnimations.Segments.PlayToNextFrame(
            ref animation, ref place);
        // The animation's end ends the object; every other outcome stands its animation where the play stopped.
        if (shown is not AnimationShown<RoomEffectActorSegment, RoomEffectActorPicture>.Ended)
            Playback = new(animation, place);
        shown.React(AdvanceAnimationReactions, this);
    }

    /// <summary>What <see cref="AdvanceAnimation"/> does on each outcome of the object's animation, which never pauses, built once (#2553).</summary>
    private static UnpausedAnimationShownReactions<RoomEffectActorSegment, RoomEffectActorPicture, RoomEffectActor> AdvanceAnimationReactions { get; } = new(
        Frame: static (frame, actor) =>
        {
            actor.Frame = frame.Shown;
            actor.AnimationClock.Show(frame.Duration);
        },
        HeldFrame: static (held, actor) =>
        {
            // A still picture shows from now on (#2275).
            actor.Frame = held.Shown;
            actor.AnimationClock.ShowForGood();
        },
        Held: static (_, actor) => actor.AnimationClock.Stop(),
        Ended: static (_, actor) => actor.End());

    /// <summary>
    /// Restarts the object's own animation from its first frame, as its spawn started it (#2047): a frozen Metroid's
    /// electricity and shell restart theirs in their frozen colours. A suspended clock stays suspended.
    /// </summary>
    internal void RestartAnimation()
    {
        if (!IsActive)
            throw new InvalidOperationException($"Effect actor {Kind} has ended and shows no frame.");
        Playback = new(RoomEffectActorAnimations.StartingWith(Kind), AnimationPlace.StartOfAnimation);
        AdvanceAnimation();
    }
}
