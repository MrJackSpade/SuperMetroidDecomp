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

    /// <summary>What the object shows, as its spawner chose it, which is also the animation it plays.</summary>
    public RoomEffectActorKind Kind { get; }

    /// <summary>Whether the object still runs its animation; once it ends it never runs or shows again.</summary>
    public bool IsActive => Playback is not null;

    /// <summary>
    /// The 1/65536-pixel room position, converted to screen coordinates by subtracting the camera when drawn. Most
    /// effect actors move only in whole pixels and leave the fractions zero; Draygon's six burial Evirs move by
    /// fractional speeds, so discarding the fractions visibly changes their fan-in.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }

    /// <summary>The artwork and colours the object draws with.</summary>
    public SpriteGraphics Graphics { get; internal set; }

    /// <summary>The animation the object plays and where it stands in it (#1600), or null once it has ended.</summary>
    internal AnimationPlayback<RoomEffectActorSegment>? Playback { get; private set; }

    /// <summary>Updates left showing <see cref="Frame"/> before the animation moves on; not counted while the object <see cref="Holds"/>.</summary>
    public int Timer { get; private set; }

    /// <summary>Whether the animation has reached its hold: the object shows <see cref="Frame"/> from then on until it ends.</summary>
    public bool Holds { get; private set; }

    /// <summary>The frame the object shows.</summary>
    internal RoomEffectActorPicture Frame { get; private set; }

    /// <summary>Suppresses animation updates but leaves drawing intact.</summary>
    public bool UpdatesDisabled { get; internal set; }

    /// <summary>Ends the object: it stops running and showing.</summary>
    internal void End() => Playback = null;

    /// <summary>Counts one update off the shown frame and moves the animation on when it runs out, unless updates are disabled or the object holds.</summary>
    internal void Step()
    {
        if (!IsActive || UpdatesDisabled || Holds)
            return;
        if (--Timer == 0)
            AdvanceAnimation();
    }

    /// <summary>
    /// Plays the animation to its next frame: a frame installs itself and its duration, the hold keeps the frame for good,
    /// and the animation's end ends the object. The animations count no repeats and raise no events.
    /// </summary>
    private void AdvanceAnimation()
    {
        AnimationPlayback<RoomEffectActorSegment> playback = Playback ?? throw new InvalidOperationException(
            $"Effect actor {Kind} has no animation to play.");
        Animation<RoomEffectActorSegment> animation = playback.Animation;
        AnimationPlace place = playback.Place;
        AnimationShown<RoomEffectActorSegment, RoomEffectActorPicture> shown = RoomEffectActorAnimations.Segments.PlayToNextFrame(
            ref animation, ref place,
            raised => throw new InvalidOperationException($"Effect actor {Kind}'s event-free animation raised {raised}."));
        switch (shown)
        {
            case AnimationShown<RoomEffectActorSegment, RoomEffectActorPicture>.Frame frame:
                Playback = new(animation, place);
                Frame = frame.Shown;
                Timer = frame.Duration;
                return;
            case AnimationShown<RoomEffectActorSegment, RoomEffectActorPicture>.HeldFrame held:
                // A still picture shows from now on (#2275).
                Playback = new(animation, place);
                Frame = held.Shown;
                Holds = true;
                return;
            case AnimationShown<RoomEffectActorSegment, RoomEffectActorPicture>.Held:
                Playback = new(animation, place);
                Holds = true;
                return;
            case AnimationShown<RoomEffectActorSegment, RoomEffectActorPicture>.Ended:
                End();
                return;
            default:
                throw new InvalidOperationException($"Effect actor {Kind}'s animation reached {shown}.");
        }
    }

    /// <summary>
    /// Restarts the object's own animation from its first frame, as its spawn started it (#2047): a frozen Metroid's
    /// electricity and shell restart theirs in their frozen colours.
    /// </summary>
    internal void RestartAnimation()
    {
        if (!IsActive)
            throw new InvalidOperationException($"Effect actor {Kind} has ended and shows no frame.");
        Holds = false;
        Playback = new(RoomEffectActorAnimations.StartingWith(Kind), AnimationPlace.StartOfAnimation);
        AdvanceAnimation();
    }
}
