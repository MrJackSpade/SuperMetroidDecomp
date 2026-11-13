using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.SpriteObjects;

/// <summary>
/// One room sprite object: a program-driven decorative effect (dust, an explosion, a Metroid's shell, a Nuclear Waffle
/// link, one of Draygon's Evirs), created fresh each time one is spawned (#1591). Its spawner holds it when it moves,
/// recolours, freezes or ends it; the room's <see cref="RoomSpriteObjects"/> steps and draws it until it ends.
/// </summary>
public sealed class RoomSpriteObject
{
    /// <summary>Spawns an object of <paramref name="kind"/> at <paramref name="position"/>, showing its program's first frame.</summary>
    /// <param name="kind">What the object is, which selects the program it plays.</param>
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

    /// <summary>Whether the object still runs its program; once it ends it never runs or shows again.</summary>
    public bool IsActive => Playback is not null;

    /// <summary>
    /// The 16.16 room position, converted to screen coordinates by subtracting the camera when drawn. Most
    /// sprite objects move only in whole pixels and leave the fractions zero; Draygon's six burial Evirs move by
    /// fractional speeds, so discarding the fractions visibly changes their fan-in.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }

    /// <summary>The artwork and colours the object draws with.</summary>
    public SpriteGraphics Graphics { get; internal set; }

    /// <summary>The animation the object plays and where it stands in it (#1600), or null once it has ended.</summary>
    internal AnimationPlayback<RoomSpriteObjectSegment>? Playback { get; private set; }

    /// <summary>Updates left showing <see cref="Frame"/> before the program moves on; not counted while the object <see cref="Holds"/>.</summary>
    public int Timer { get; private set; }

    /// <summary>Whether the program has reached its hold: the object shows <see cref="Frame"/> from then on until it ends.</summary>
    public bool Holds { get; private set; }

    /// <summary>The frame the object shows.</summary>
    internal RoomSpriteObjectFrame Frame { get; private set; }

    /// <summary>Suppresses program updates but leaves drawing intact.</summary>
    public bool UpdatesDisabled { get; internal set; }

    /// <summary>Ends the object: it stops running and showing.</summary>
    internal void End() => Playback = null;

    /// <summary>Counts one update off the shown frame and moves the program on when it runs out, unless updates are disabled or the object holds.</summary>
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
        WrappingCountdown repeats = default;
        AnimationShown<RoomSpriteObjectSegment, RoomSpriteObjectFrame> shown = RoomSpriteObjectAnimations.Segments.PlayToNextFrame(
            ref animation, ref place, ref repeats,
            raised => throw new InvalidOperationException($"Sprite object {Kind}'s event-free animation raised {raised}."));
        switch (shown)
        {
            case AnimationShown<RoomSpriteObjectSegment, RoomSpriteObjectFrame>.Frame frame:
                Playback = new(animation, place);
                Frame = frame.Shown;
                Timer = frame.Duration;
                return;
            case AnimationShown<RoomSpriteObjectSegment, RoomSpriteObjectFrame>.Held:
                Playback = new(animation, place);
                Holds = true;
                return;
            case AnimationShown<RoomSpriteObjectSegment, RoomSpriteObjectFrame>.Ended:
                End();
                return;
            default:
                throw new InvalidOperationException($"Sprite object {Kind}'s animation reached {shown}.");
        }
    }

    /// <summary>
    /// Points the object at the animation starting at <paramref name="segment"/>, past its first frame, and shows that
    /// frame, keeping the current countdown: the frozen Metroid shell shows the animation's first frame without reloading
    /// the timer, so the next expiry continues at the animation's second beat.
    /// </summary>
    /// <param name="segment">The segment whose animation the object plays from now on.</param>
    internal void ShowFirstFrameKeepingTimer(RoomSpriteObjectSegment segment)
    {
        if (!IsActive)
            throw new InvalidOperationException($"Sprite object {Kind} has ended and shows no frame.");
        if (Holds)
            throw new InvalidOperationException($"Sprite object {Kind} holds its frame and has no countdown to keep.");
        if (RoomSpriteObjectAnimations.Segments.Beats(segment)[0] is not AnimationBeat<RoomSpriteObjectFrame, NoAnimationCue>.Frame first)
            throw new InvalidOperationException($"Sprite object animation {segment} does not begin with a timed frame.");
        // The animation's first phase plays the segment from its first beat: past that beat is the phase's second.
        Playback = new(RoomSpriteObjectAnimations.StartingWith(segment), new AnimationPlace(0, 1));
        Frame = first.Shown;
    }
}
