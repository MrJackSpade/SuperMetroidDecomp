using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// A cinematic actor: its position and colouring, playing one of its family's typed animations (#641, #1600, #2197).
/// </summary>
/// <remarks>
/// The actor's motion belongs to the owner that steps it (#1903): every cue goes to that owner, and an owner-less cue
/// fails rather than being skipped. The cues are the family's own, so an owner is handed only the cues it acts on.
/// </remarks>
/// <typeparam name="TAnimation">The family's animation identity.</typeparam>
/// <typeparam name="TFrame">What the family's actors show.</typeparam>
/// <typeparam name="TCue">The cues the family's animations publish.</typeparam>
internal sealed class CinematicActor<TAnimation, TFrame, TCue> : ICinematicActor<TCue>
    where TAnimation : struct, Enum
    where TFrame : struct
    where TCue : notnull
{
    /// <summary>The family's animations, which the actor plays.</summary>
    private readonly CinematicAnimations<TAnimation, TFrame, TCue> animations;

    /// <summary>
    /// The frame's hold, counted once each update; the update it ends on plays the animation on to its next frame. Setup
    /// may stagger the first frame (#1974).
    /// </summary>
    private readonly UpdateCountdown frameHold = UpdateCountdown.LastingOneUpdate();

    /// <summary>Whether the animation has come to rest for good, so it plays on no further until its owner starts another.</summary>
    private bool held;

    /// <summary>
    /// Creates an actor at a whole-pixel screen place with the drawn shade of the colouring it starts drawing in (#1512),
    /// playing <paramref name="animation"/> of its family's <paramref name="animations"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">No actor of the family starts <paramref name="animation"/>.</exception>
    public CinematicActor(
        Offset16<ScreenPixels> place,
        PictureShade colouring,
        CinematicAnimations<TAnimation, TFrame, TCue> animations,
        TAnimation animation)
    {
        this.animations = Ensure.NotNull(animations);
        Position = FixedVector<ScreenPixels>.FromWholePixels(place);
        Colouring = colouring;
        Playback = new AnimationPlayback<TAnimation>(animations.StartingWith(animation), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Cleared when the animation's end retires the actor.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Where the actor stands on the screen, to a fraction of a pixel, measured from the screen's top-left corner
    /// (#2000): a place left of or above the screen is negative.
    /// </summary>
    public FixedVector<ScreenPixels> Position { get; set; }

    /// <summary>
    /// The whole pixels of <see cref="Position"/> (#1974, #2000): a place left of or above the screen has a negative
    /// coordinate. Assigning replaces them and keeps both fractions.
    /// </summary>
    public Offset16<ScreenPixels> ScreenPlace
    {
        get => Position.Whole;
        set => Position = Position.WithWhole(value);
    }

    /// <summary>The drawn shade of the colouring the current frame's recolourable parts draw in (#1512).</summary>
    private PictureShade Colouring { get; set; }

    /// <summary>Applies a cinematic cue's change of the colours the actor draws in.</summary>
    public void SetColouring(PictureShade colouring) => Colouring = colouring;

    /// <summary>What the animation most recently showed; null before its first frame and after deletion.</summary>
    internal TFrame? Shown { get; private set; }

    /// <summary>The actor's animation and where it stands in it (#1600).</summary>
    private AnimationPlayback<TAnimation> Playback { get; set; }

    /// <summary>Starts <paramref name="animation"/> on the actor's next update, discarding the old frame countdown.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No actor of the family starts <paramref name="animation"/>.</exception>
    public void Start(TAnimation animation)
    {
        Playback = new AnimationPlayback<TAnimation>(animations.StartingWith(animation), AnimationPlace.StartOfAnimation);
        frameHold.Restart(1);
        held = false;
    }

    /// <summary>
    /// Seeds the private frame hold the sprite's next update uses. Cinematic actor setups write this hold directly when several actors
    /// share one animation but must begin on staggered frames.
    /// </summary>
    public void DelayFirstStep(int frames)
    {
        if (frames < 1)
            throw new ArgumentOutOfRangeException(nameof(frames), "A frame hold lasts at least one update.");
        frameHold.Restart(frames);
    }

    /// <summary>Retires the actor and clears its visible frame.</summary>
    public void Delete()
    {
        IsActive = false;
        Shown = null;
    }

    /// <summary>
    /// Advances the actor by one update: when the frame expires, plays the animation on until it shows a frame, holds or
    /// ends. Owner cues go to <paramref name="ownerCue"/>; an animation that raises one with no owner fails.
    /// </summary>
    public void Step(Action<TCue>? ownerCue = null)
    {
        if (!IsActive || held || !frameHold.Step())
            return;

        Animation<TAnimation> animation = Playback.Animation;
        AnimationPlace place = Playback.Place;
        AnimationShown<TAnimation, TFrame> shown = animations.Segments.PlayToNextFrame(
            animation, ref place, cue => ApplyCue(cue, ownerCue));
        Playback = new AnimationPlayback<TAnimation>(animation, place);
        switch (shown)
        {
            case AnimationShown<TAnimation, TFrame>.Frame frame:
                frameHold.Restart(frame.Duration);
                Shown = frame.Shown;
                return;
            case AnimationShown<TAnimation, TFrame>.Hidden hidden:
                // A hidden beat shows no pose for its updates (#2558).
                frameHold.Restart(hidden.Duration);
                Shown = null;
                return;
            case AnimationShown<TAnimation, TFrame>.HeldFrame rested:
                // A held frame shows from now on: the animation plays on no further until its owner starts another.
                Shown = rested.Shown;
                held = true;
                return;
            case AnimationShown<TAnimation, TFrame>.Held:
                // A held animation rests on its last frame for good: playing it on again would only hold again.
                held = true;
                return;
            case AnimationShown<TAnimation, TFrame>.Ended:
                Delete();
                return;
            default:
                throw new InvalidOperationException($"A cinematic sprite animation ended an update with {shown}.");
        }
    }

    /// <summary>Hands the cue to the actor's owner.</summary>
    private void ApplyCue(TCue cue, Action<TCue>? ownerCue)
    {
        (ownerCue ?? throw new InvalidOperationException(
            $"Cinematic sprite animation in {Playback.PlayingSegment} raised {cue} with no owner to act on it."))(cue);
    }

    /// <summary>
    /// The width, in pixels, of the loop the actor drifts round, or null for an actor that drifts round none. The loop's
    /// first half is the screen and its second half an equally wide stretch of space left of the screen, so an actor that
    /// drifts off the screen's right edge (<see cref="DriftRoundLoop"/>) comes back into view across its left edge.
    /// </summary>
    internal int? DriftLoopWidth { get; init; }

    /// <summary>
    /// Moves a looping actor by <paramref name="horizontalDelta"/> and carries it round its loop
    /// (<see cref="DriftLoopWidth"/>): its whole pixels stay within the loop, from zero up to the loop's width.
    /// </summary>
    /// <exception cref="InvalidOperationException">The actor travels no loop.</exception>
    internal void DriftRoundLoop(SignedSixteenSixteen horizontalDelta)
    {
        int loop = DriftLoopWidth ?? throw new InvalidOperationException("A cinematic actor with no loop drifted round one.");
        SignedSixteenSixteen x = Position.X + horizontalDelta;
        Position = Position.WithX(x.WithWholePixels(AlongLoop(x.WholePixels, loop)));
    }

    /// <summary>Where <paramref name="x"/> lies along a loop <paramref name="loop"/> pixels wide, from zero up to the width.</summary>
    private static int AlongLoop(int x, int loop) => ((x % loop) + loop) % loop;

    /// <summary>
    /// What the actor shows and where on the screen, at its screen place wherever that lies (the display clips a frame off
    /// the screen, #1587); none for an actor that is deleted or has shown nothing yet.
    /// </summary>
    internal CinematicActorDrawing<TFrame>? Drawing()
    {
        if (!IsActive || Shown is not { } shown)
            return null;
        Offset16<ScreenPixels> placed = ScreenPlace;
        if (DriftLoopWidth is int loop)
        {
            // The loop's second half is the stretch of space left of the screen, drawn at a negative screen X.
            int alongLoop = AlongLoop(placed.X, loop);
            placed = new Offset16<ScreenPixels>(alongLoop < loop / 2 ? alongLoop : alongLoop - loop, placed.Y);
        }
        return new CinematicActorDrawing<TFrame>(shown, placed, Colouring);
    }
}
