using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.SpriteObjects;

/// <summary>
/// One room sprite object: an animation-driven decorative effect (dust, an explosion, a Metroid's shell, a Nuclear Waffle
/// link, one of Draygon's Evirs), created fresh each time one is spawned (#1591). Its spawner holds it when it moves,
/// recolours, freezes or ends it; the room's <see cref="RoomSpriteObjects"/> steps and draws it until it ends.
/// </summary>
public sealed class RoomSpriteObject
{
    /// <summary>Spawns an object of <paramref name="kind"/> at <paramref name="position"/>, showing its animation's first frame.</summary>
    /// <param name="kind">What the object is, which selects the animation it plays.</param>
    /// <param name="position">Where it stands, in whole room pixels.</param>
    /// <param name="graphics">The artwork and colours it draws with.</param>
    internal RoomSpriteObject(RoomSpriteObjectKind kind, Position16<RoomPixels> position, SpriteGraphics graphics)
    {
        Kind = kind;
        Position = FixedPosition<RoomPixels>.FromWhole(position);
        Graphics = graphics;
        Playback = new(RoomSpriteObjectAnimations.StartingWith(RoomSpriteObjectDefinitions.Animations[kind]), AnimationPlace.StartOfAnimation);
        AdvanceAnimation();
    }

    /// <summary>What the object is, as its spawner chose it.</summary>
    public RoomSpriteObjectKind Kind { get; }

    /// <summary>Whether the object still runs its animation; once it ends it never runs or shows again.</summary>
    public bool IsActive => Playback is not null;

    /// <summary>
    /// The 1/65536-pixel room position, converted to screen coordinates by subtracting the camera when drawn. Most
    /// sprite objects move only in whole pixels and leave the fractions zero; Draygon's six burial Evirs move by
    /// fractional speeds, so discarding the fractions visibly changes their fan-in.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }

    /// <summary>The artwork and colours the object draws with.</summary>
    public SpriteGraphics Graphics { get; internal set; }

    /// <summary>The animation the object plays and where it stands in it (#1600), or null once it has ended.</summary>
    internal AnimationPlayback<RoomSpriteObjectSegment>? Playback { get; private set; }

    /// <summary>Updates left showing <see cref="Frame"/> before the animation moves on; not counted while the object <see cref="Holds"/>.</summary>
    public int Timer { get; private set; }

    /// <summary>Whether the animation has reached its hold: the object shows <see cref="Frame"/> from then on until it ends.</summary>
    public bool Holds { get; private set; }

    /// <summary>The frame the object shows.</summary>
    internal RoomSpriteObjectPicture Frame { get; private set; }

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
        AnimationPlayback<RoomSpriteObjectSegment> playback = Playback ?? throw new InvalidOperationException(
            $"Sprite object {Kind} has no animation to play.");
        Animation<RoomSpriteObjectSegment> animation = playback.Animation;
        AnimationPlace place = playback.Place;
        AnimationShown<RoomSpriteObjectSegment, RoomSpriteObjectPicture> shown = RoomSpriteObjectAnimations.Segments.PlayToNextFrame(
            ref animation, ref place,
            raised => throw new InvalidOperationException($"Sprite object {Kind}'s event-free animation raised {raised}."));
        switch (shown)
        {
            case AnimationShown<RoomSpriteObjectSegment, RoomSpriteObjectPicture>.Frame frame:
                Playback = new(animation, place);
                Frame = frame.Shown;
                Timer = frame.Duration;
                return;
            case AnimationShown<RoomSpriteObjectSegment, RoomSpriteObjectPicture>.Held:
                Playback = new(animation, place);
                Holds = true;
                return;
            case AnimationShown<RoomSpriteObjectSegment, RoomSpriteObjectPicture>.Ended:
                End();
                return;
            default:
                throw new InvalidOperationException($"Sprite object {Kind}'s animation reached {shown}.");
        }
    }

    /// <summary>
    /// Starts the animation beginning at <paramref name="segment"/> from its first frame, as a spawn starts one (#2047):
    /// the frozen Metroid shell restarts its animation in its frozen colours.
    /// </summary>
    /// <param name="segment">The segment whose animation the object plays from now on.</param>
    internal void StartAnimation(RoomSpriteObjectSegment segment)
    {
        if (!IsActive)
            throw new InvalidOperationException($"Sprite object {Kind} has ended and shows no frame.");
        Holds = false;
        Playback = new(RoomSpriteObjectAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        AdvanceAnimation();
    }
}
