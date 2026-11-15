using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// A cinematic sprite object: its position and colouring, playing one of the typed cinematic sprite animations (#641,
/// #1600) where every cinematic sprite's update ran its animation.
/// </summary>
/// <remarks>
/// The actor's motion belongs to the owner that steps it (#1903): every cue goes to that owner, and an owner-less cue
/// fails rather than being skipped.
/// </remarks>
internal sealed class IntroDiscoverySprite
{
    /// <summary>
    /// Updates before the animation plays on to its next frame, one or more while it plays; setup may stagger the first
    /// frame (#1974).
    /// </summary>
    private int frameHold = 1;

    /// <summary>Whether the animation has come to rest for good, so it plays on no further until its owner starts another.</summary>
    private bool held;

    /// <summary>
    /// Creates an actor at a whole-pixel screen place with the drawn shade of the colouring it starts drawing in (#1512) and
    /// its animation.
    /// </summary>
    public IntroDiscoverySprite(
        Offset16<ScreenPixels> place,
        PictureShade colouring,
        CinematicSpriteSegment segment)
    {
        Position = FixedVector<ScreenPixels>.FromWholePixels(place);
        Colouring = colouring;
        Playback = new AnimationPlayback<CinematicSpriteSegment>(CinematicSpriteAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Cleared when the animation's end retires the actor.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Where the actor stands on the screen, in whole pixels and 65536ths, measured from the screen's top-left corner
    /// (#2000): a place left of or above the screen is negative.
    /// </summary>
    public FixedVector<ScreenPixels> Position { get; set; }

    /// <summary>
    /// The whole pixels of <see cref="Position"/> (#1974, #2000): a place left of or above the screen has a negative
    /// coordinate. Assigning replaces them and keeps both fractions.
    /// </summary>
    internal Offset16<ScreenPixels> ScreenPlace
    {
        get => Position.Whole;
        set => Position = Position.WithWhole(value);
    }

    /// <summary>The drawn shade of the colouring the current pose's recolourable parts draw in (#1512).</summary>
    public PictureShade Colouring { get; private set; }

    /// <summary>Applies a cinematic cue's change of the colours the actor draws in.</summary>
    public void SetColouring(PictureShade colouring) => Colouring = colouring;

    /// <summary>The pose the animation most recently showed; null before its first frame and after deletion.</summary>
    internal CinematicSpritePose? Pose { get; private set; }

    /// <summary>The actor's animation and where it stands in it (#1600).</summary>
    internal AnimationPlayback<CinematicSpriteSegment> Playback { get; private set; }

    /// <summary>Starts the animation of <paramref name="segment"/> on the sprite's next update, discarding the old frame countdown.</summary>
    public void Start(CinematicSpriteSegment segment)
    {
        Playback = new AnimationPlayback<CinematicSpriteSegment>(CinematicSpriteAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        frameHold = 1;
        held = false;
    }

    /// <summary>
    /// Seeds the private frame hold used by the sprite's next update
    /// call. Cinematic object setups write this hold directly when several actors
    /// share one animation but must begin on staggered frames.
    /// </summary>
    public void DelayFirstStep(int frames)
    {
        if (frames < 1)
            throw new ArgumentOutOfRangeException(nameof(frames), "A frame hold lasts at least one update.");
        frameHold = frames;
    }

    /// <summary>Retires the actor and clears its visible frame.</summary>
    public void Delete()
    {
        IsActive = false;
        Pose = null;
    }

    /// <summary>
    /// Advances the cinematic sprite by one update: when the frame expires, plays the animation on until it shows a
    /// frame, holds or ends. Owner cues go to <paramref name="ownerCue"/>; an animation that raises one with no owner
    /// fails.
    /// </summary>
    public void Step(Action<CinematicSpriteCueKind>? ownerCue = null)
    {
        if (!IsActive || held || --frameHold > 0)
            return;

        Animation<CinematicSpriteSegment> animation = Playback.Animation;
        AnimationPlace place = Playback.Place;
        AnimationShown<CinematicSpriteSegment, CinematicSpritePose> shown = CinematicSpriteAnimations.Segments.PlayToNextFrame(
            animation, ref place, cue => ApplyCue(cue, ownerCue));
        Playback = new AnimationPlayback<CinematicSpriteSegment>(animation, place);
        switch (shown)
        {
            case AnimationShown<CinematicSpriteSegment, CinematicSpritePose>.Frame frame:
                frameHold = frame.Duration;
                Pose = frame.Shown;
                return;
            case AnimationShown<CinematicSpriteSegment, CinematicSpritePose>.Held:
                // A held animation rests on its last frame for good: playing it on again would only hold again.
                held = true;
                return;
            case AnimationShown<CinematicSpriteSegment, CinematicSpritePose>.Ended:
                Delete();
                return;
            default:
                throw new InvalidOperationException($"A cinematic sprite animation ended an update with {shown}.");
        }
    }

    /// <summary>Hands the cue to the actor's owner.</summary>
    private void ApplyCue(CinematicSpriteCue cue, Action<CinematicSpriteCueKind>? ownerCue)
    {
        (ownerCue ?? throw new InvalidOperationException(
            $"Cinematic sprite animation in {Playback.Animation.Phases[Playback.Place.Phase].Segment} raised {cue.Kind} with no owner to carry it out."))(cue.Kind);
    }

    /// <summary>
    /// The width, in pixels, of the horizontal loop the actor's own motion carries it around, or null for an actor that
    /// travels no loop. A looping actor travels round the loop (<see cref="TravelLoop"/>), and the half of the loop past
    /// its middle lies left of the screen, where the actor comes round into view again.
    /// </summary>
    internal int? HorizontalLoopWidth { get; init; }

    /// <summary>
    /// Moves a looping actor right by the fraction of <paramref name="horizontalDelta"/> alone and carries it round its loop
    /// (<see cref="HorizontalLoopWidth"/>): its whole pixels stay within the loop, from zero up to the loop's width.
    /// </summary>
    /// <remarks>
    /// The looping field adds only the delta's fraction, with nothing in the whole pixel: a leftward delta, whose fraction is
    /// the rest of a pixel, carries the actor right instead.
    /// </remarks>
    /// <exception cref="InvalidOperationException">The actor travels no loop.</exception>
    internal void TravelLoop(SignedSixteenSixteen horizontalDelta)
    {
        int loop = HorizontalLoopWidth ?? throw new InvalidOperationException("A cinematic actor with no loop travelled round one.");
        SignedSixteenSixteen x = IntroCinematicMotion.AddFraction(Position.X, horizontalDelta);
        Position = Position.WithX(x.WithWholePixels(AlongLoop(x.WholePixels, loop)));
    }

    /// <summary>Where <paramref name="x"/> lies along a loop <paramref name="loop"/> pixels wide, from zero up to the width.</summary>
    private static int AlongLoop(int x, int loop) => ((x % loop) + loop) % loop;

    /// <summary>
    /// Draws the current frame at the screen place minus <paramref name="scroll"/>, wherever that lies: the display clips
    /// a frame off the screen (#1587).
    /// </summary>
    public void Draw(ScreenSprites objects, Offset16<ScreenPixels> scroll = default,
        IIntroCinematicSpritePresentation? installedArt = null)
    {
        // The blank pose draws nothing.
        if (!IsActive || Pose is not { } pose || pose == CinematicSpritePose.Blank)
            return;
        Offset16<ScreenPixels> placed = ScreenPlace + -scroll;
        if (HorizontalLoopWidth is int loop)
        {
            // The half of the loop past its middle lies left of the screen, at a negative screen X.
            int alongLoop = AlongLoop(placed.X, loop);
            placed = new Offset16<ScreenPixels>(alongLoop < loop / 2 ? alongLoop : alongLoop - loop, placed.Y);
        }
        (installedArt ?? throw new InvalidOperationException(
            "Cinematic sprites require installed artwork."))
            .Draw(pose, objects, placed, Colouring);
    }
}
