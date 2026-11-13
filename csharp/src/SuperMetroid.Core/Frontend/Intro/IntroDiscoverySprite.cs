using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// A cinematic sprite object: its position, colouring and frame behaviour, playing one of the typed cinematic sprite
/// animations (#641, #1600) where the generic cinematic sprite handler ran its animation.
/// </summary>
/// <remarks>
/// The shared frame-behaviour install is carried out here; every other cue goes to the owner that steps the actor, and
/// an owner-less cue fails rather than being skipped.
/// </remarks>
internal sealed class IntroDiscoverySprite
{
    /// <summary>
    /// Countdown before the next sprite cue is read; setup may stagger the first frame. A hold leaves it at
    /// zero, so it keeps counting past zero and the animation resumes when the countdown comes round to zero again.
    /// </summary>
    private WrappingCountdown frameHold = WrappingCountdown.Start(1);

    /// <summary>Creates an actor at a whole-pixel origin with the drawn shade of the colouring it starts drawing in (#1512) and its animation.</summary>
    public IntroDiscoverySprite(
        Position16<ScreenPixels> position,
        PictureShade colouring,
        CinematicSpriteSegment segment)
    {
        Position = new(new(position.X, 0), new(position.Y, 0));
        Colouring = colouring;
        Playback = new AnimationPlayback<CinematicSpriteSegment>(CinematicSpriteAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Cleared when the animation's end retires the actor.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>The whole and subpixel parts of the sprite's position on both axes.</summary>
    public FixedPosition<ScreenPixels> Position { get; set; }

    /// <summary>The whole pixels of <see cref="Position"/>; assigning replaces them and keeps both subpixel parts.</summary>
    public Position16<ScreenPixels> WholePosition
    {
        get => Position.Whole;
        set => Position = new(Position.X.WithWhole(value.X), Position.Y.WithWhole(value.Y));
    }

    /// <summary>The drawn shade of the colouring the current pose's palette-inheriting parts draw in (#1512).</summary>
    public PictureShade Colouring { get; private set; }

    /// <summary>Applies a cinematic callback's change of the colours the actor draws in.</summary>
    public void SetColouring(PictureShade colouring) => Colouring = colouring;

    /// <summary>The pose the animation most recently showed; null before its first frame and after deletion.</summary>
    internal CinematicSpritePose? Pose { get; private set; }

    /// <summary>The actor's animation and where it stands in it (#1600).</summary>
    internal AnimationPlayback<CinematicSpriteSegment> Playback { get; private set; }

    /// <summary>
    /// The segment the actor's animation rests in: resting after the last frame of a segment that runs on into the next, it
    /// already stands in the next (<see cref="CinematicSpriteAnimations.RunsOnInto"/>).
    /// </summary>
    internal CinematicSpriteSegment CurrentAnimation => AnimationResidence.ResidentSegment(
        CinematicSpriteAnimations.Segments, CinematicSpriteAnimations.StartingAt, CinematicSpriteAnimations.RunsOnInto, Playback);

    /// <summary>
    /// The count the animation's counted phases repeat by: a counted phase loads it as it starts and each of its plays
    /// counts it down. Only those phases change it; it starts at zero.
    /// </summary>
    internal WrappingCountdown LoopCountdown { get; private set; } = WrappingCountdown.Start(0);

    /// <summary>The frame behaviour the actor runs, or none.</summary>
    internal CinematicSpriteFrameBehavior? FrameBehavior { get; private set; }

    /// <summary>The frame behaviour the actor's animation installed, until an owner installs its own.</summary>
    internal CinematicSpriteFrameBehavior? ListFrameBehavior { get; private set; }

    /// <summary>Starts the animation of <paramref name="segment"/> on the next handler call, discarding the old frame countdown.</summary>
    public void Start(CinematicSpriteSegment segment)
    {
        Playback = new AnimationPlayback<CinematicSpriteSegment>(CinematicSpriteAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        frameHold = WrappingCountdown.Start(1);
    }

    /// <summary>
    /// Seeds the private frame hold used by the next generic sprite-handler
    /// call. Cinematic object setups write this hold directly when several actors
    /// share one animation but must begin on staggered frames.
    /// </summary>
    /// <remarks>
    /// This is deliberately not <see cref="LoopCountdown"/>, which only the animation's counted phases use; conflating the
    /// two makes a delayed actor show its first frame immediately while merely changing an unrelated loop count.
    /// </remarks>
    public void DelayFirstStep(int frames)
    {
        if (frames == 0)
            throw new ArgumentOutOfRangeException(nameof(frames), "A frame hold must be nonzero.");
        frameHold = WrappingCountdown.Start(frames);
    }

    /// <summary>Installs the frame behaviour the actor's owner selects, or none.</summary>
    internal void InstallFrameBehavior(CinematicSpriteFrameBehavior? frameBehavior)
    {
        FrameBehavior = frameBehavior;
        ListFrameBehavior = null;
    }

    /// <summary>Retires the actor and clears its visible frame.</summary>
    public void Delete()
    {
        IsActive = false;
        Pose = null;
    }

    /// <summary>
    /// Advances one generic cinematic-sprite handler call: when the frame expires, plays the animation on until it shows a
    /// frame, holds or ends. Owner cues go to <paramref name="ownerCue"/>; an animation that raises one with no owner
    /// fails.
    /// </summary>
    public void Step(Action<CinematicSpriteCueKind>? ownerCue = null)
    {
        if (!IsActive)
            return;

        CountdownStep hold = frameHold.Decrement();
        frameHold = hold.Remaining;
        if (!hold.IsZero)
            return;

        Animation<CinematicSpriteSegment> animation = Playback.Animation;
        AnimationPlace place = Playback.Place;
        WrappingCountdown repeats = LoopCountdown;
        AnimationShown<CinematicSpriteSegment, CinematicSpritePose> shown = CinematicSpriteAnimations.Segments.PlayToNextFrame(
            animation, ref place, ref repeats, cue => ApplyCue(cue, ownerCue));
        Playback = new AnimationPlayback<CinematicSpriteSegment>(animation, place);
        LoopCountdown = repeats;
        switch (shown)
        {
            case AnimationShown<CinematicSpriteSegment, CinematicSpritePose>.Frame frame:
                frameHold = WrappingCountdown.Start(frame.Duration);
                Pose = frame.Shown;
                return;
            case AnimationShown<CinematicSpriteSegment, CinematicSpritePose>.Held:
                return;
            case AnimationShown<CinematicSpriteSegment, CinematicSpritePose>.Ended:
                Delete();
                return;
            default:
                throw new InvalidOperationException($"A cinematic sprite animation ended an update with {shown}.");
        }
    }

    /// <summary>Installs a frame behaviour, or hands any other cue to the actor's owner.</summary>
    private void ApplyCue(CinematicSpriteCue cue, Action<CinematicSpriteCueKind>? ownerCue)
    {
        if (cue.Kind == CinematicSpriteCueKind.SetFrameBehavior)
        {
            FrameBehavior = cue.FrameBehavior;
            ListFrameBehavior = cue.FrameBehavior;
            return;
        }
        (ownerCue ?? throw new InvalidOperationException(
            $"Cinematic sprite animation in {CurrentAnimation} raised {cue.Kind} with no owner to carry it out."))(cue.Kind);
    }

    /// <summary>
    /// The width, in pixels, of the horizontal loop the actor's own motion carries it around, or null for an actor that
    /// travels no loop. A looping actor's position wraps within the loop, and the half of the loop past its middle lies
    /// left of the screen, where the actor comes round into view again.
    /// </summary>
    internal int? HorizontalLoopWidth { get; init; }

    /// <summary>
    /// Draws the current frame at the whole position minus <paramref name="scroll"/>, wherever that lies: the display clips
    /// a frame off the screen (#1587).
    /// </summary>
    public void Draw(ScreenSprites objects, Offset16<ScreenPixels> scroll = default,
        IIntroCinematicSpritePresentation? installedArt = null)
    {
        // The blank pose draws nothing.
        if (!IsActive || Pose is not { } pose || pose == CinematicSpritePose.Blank)
            return;
        Position16<ScreenPixels> origin = Position.Whole - scroll;
        if (HorizontalLoopWidth is int loop)
        {
            int alongLoop = origin.X % loop;
            origin = origin.WithX(CoordinateAxis.Wrap(alongLoop < loop / 2 ? alongLoop : alongLoop - loop));
        }
        (installedArt ?? throw new InvalidOperationException(
            "Cinematic sprites require installed artwork."))
            .Draw(pose, objects, origin.ToSpritePosition(), Colouring);
    }
}
