using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// A bank-$8B cinematic sprite object: its position, colouring and pre-instruction, running one of the typed
/// <see cref="CinematicSpriteProgram"/>s (#641) where the generic cinematic sprite handler ran its native list.
/// </summary>
/// <remarks>
/// The shared pre-instruction install is carried out here; every other cue goes to the owner that steps the actor, and
/// an owner-less cue fails rather than being skipped.
/// </remarks>
internal sealed class IntroDiscoverySprite
{
    /// <summary>Countdown before the next sprite instruction is decoded; setup may stagger the first frame.</summary>
    private WrappingCountdown instructionTimer = WrappingCountdown.Start(1);

    /// <summary>Creates an actor at a whole-pixel origin with the screen colours it starts drawing in and its program.</summary>
    public IntroDiscoverySprite(
        Position16<ScreenPixels> position,
        ScreenColourGroup colouring,
        CinematicSpriteProgram program)
    {
        Position = new(new(position.X, 0), new(position.Y, 0));
        Colouring = colouring;
        Program = AnimationProgramCursor<CinematicSpriteProgram>.Start(program);
    }

    /// <summary>Cleared when the list's delete opcode retires the actor.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>The native whole and subpixel words of the sprite's position on both axes.</summary>
    public FixedPosition<ScreenPixels> Position { get; set; }

    /// <summary>The whole-pixel words of <see cref="Position"/>; assigning replaces them and keeps both subpixel words.</summary>
    public Position16<ScreenPixels> WholePosition
    {
        get => Position.Whole;
        set => Position = new(Position.X.WithWhole(value.X), Position.Y.WithWhole(value.Y));
    }

    /// <summary>The screen colours the current pose's palette-inheriting parts draw in, one whole sprite set.</summary>
    public ScreenColourGroup Colouring { get; private set; }

    /// <summary>Applies a cinematic callback's change of the colours the actor draws in.</summary>
    public void SetColouring(ScreenColourGroup colouring) => Colouring = colouring;

    /// <summary>The pose the program most recently showed; null before its first frame and after deletion.</summary>
    internal CinematicSpritePose? Pose { get; private set; }

    /// <summary>Where the actor's program resumes, in place of <c>CinematicSpriteObject_InstListPointers</c>.</summary>
    internal AnimationProgramCursor<CinematicSpriteProgram> Program { get; private set; }

    /// <summary>The program the actor is running.</summary>
    internal CinematicSpriteProgram CurrentProgram => CinematicSpriteAnimations.Programs.ResidentProgram(Program);

    /// <summary>The actor scratch word used by timer and motion opcodes.</summary>
    public int GeneralTimer
    {
        get;
        set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// <see cref="GeneralTimer"/> read as the loop countdown of the set-timer and decrement-timer-and-goto
    /// opcodes (<c>DEC</c>, <c>BNE</c>).
    /// </summary>
    private WrappingCountdown InstructionLoopCountdown
    {
        get => WrappingCountdown.FromRaw(GeneralTimer);
        set => GeneralTimer = value.RemainingFrames;
    }

    /// <summary>The pre-instruction the actor runs, or none.</summary>
    internal CinematicSpritePreInstruction? PreInstruction { get; private set; }

    /// <summary>The pre-instruction the actor's program installed, until an owner installs its own.</summary>
    internal CinematicSpritePreInstruction? ListPreInstruction { get; private set; }

    /// <summary>Starts <paramref name="program"/> on the next handler call, discarding the old frame countdown.</summary>
    public void Start(CinematicSpriteProgram program)
    {
        Program = AnimationProgramCursor<CinematicSpriteProgram>.Start(program);
        instructionTimer = WrappingCountdown.Start(1);
    }

    /// <summary>
    /// Seeds the private instruction countdown used by the next generic sprite-handler
    /// call. Cinematic object setup routines write this word directly when several actors
    /// share one list but must begin on staggered frames.
    /// </summary>
    /// <remarks>
    /// This is deliberately not represented by <see cref="GeneralTimer"/>. The latter is
    /// the separate <c>CinematicSpriteObject_GotoTimers</c> scratch word consumed by list
    /// opcodes $94C3/$94D6; conflating the two makes a delayed actor interpret its first
    /// frame immediately while merely changing an unrelated loop counter.
    /// </remarks>
    public void DelayFirstInstruction(int frames)
    {
        if (frames == 0)
            throw new ArgumentOutOfRangeException(nameof(frames), "A native instruction delay must be nonzero.");
        instructionTimer = WrappingCountdown.Start(frames);
    }

    /// <summary>Installs the pre-instruction the actor's owner selects, or none.</summary>
    internal void InstallPreInstruction(CinematicSpritePreInstruction? preInstruction)
    {
        PreInstruction = preInstruction;
        ListPreInstruction = null;
    }

    /// <summary>Retires the actor and clears its visible frame.</summary>
    public void Delete()
    {
        IsActive = false;
        Pose = null;
    }

    /// <summary>
    /// Advances one generic cinematic-sprite handler call: when the frame expires, runs the program until it shows a
    /// frame, holds or stops. Owner cues go to <paramref name="ownerCue"/>; a program that publishes one with no owner
    /// fails.
    /// </summary>
    public void Step(Action<CinematicSpriteCueKind>? ownerCue = null)
    {
        if (!IsActive)
            return;

        instructionTimer = instructionTimer.Decrement().Remaining;
        if (!instructionTimer.IsZero)
            return;

        AnimationProgramCursor<CinematicSpriteProgram> cursor = Program;
        // Counter steps count the actor's goto-timer word, as the native timer-load and countdown opcodes do. Only a
        // counter step changes it, so an owner's own use of the word between steps survives.
        int timerBefore = GeneralTimer;
        WrappingCountdown counter = InstructionLoopCountdown;
        AnimationProgramAdvance<CinematicSpritePose> advance = CinematicSpriteAnimations.Programs.Advance(ref cursor, ref counter,
            (cue, _) =>
            {
                ApplyCue(cue, ownerCue);
                return null;
            });
        Program = cursor;
        if (!counter.IsAt(timerBefore))
            InstructionLoopCountdown = counter;
        switch (advance)
        {
            case AnimationProgramAdvance<CinematicSpritePose>.Show shown:
                instructionTimer = WrappingCountdown.FromRaw(WordWrap.Unsigned(shown.Duration));
                Pose = shown.Frame;
                return;
            case AnimationProgramAdvance<CinematicSpritePose>.Hold:
                return;
            case AnimationProgramAdvance<CinematicSpritePose>.Stop:
                Delete();
                return;
            default:
                throw new InvalidOperationException($"A cinematic sprite program ended an update with {advance}.");
        }
    }

    /// <summary>Installs a pre-instruction, or hands any other cue to the actor's owner.</summary>
    private void ApplyCue(CinematicSpriteCue cue, Action<CinematicSpriteCueKind>? ownerCue)
    {
        if (cue.Kind == CinematicSpriteCueKind.SetPreInstruction)
        {
            PreInstruction = cue.PreInstruction;
            ListPreInstruction = cue.PreInstruction;
            return;
        }
        (ownerCue ?? throw new InvalidOperationException(
            $"Cinematic sprite program {CurrentProgram} published {cue.Kind} with no owner to carry it out."))(cue.Kind);
    }

    /// <summary>Draws the current frame at the whole position minus <paramref name="scroll"/>.</summary>
    public void Draw(ScreenSprites objects, Offset16<ScreenPixels> scroll = default,
        IIntroCinematicSpritePresentation? installedArt = null)
    {
        // The blank pose draws nothing.
        if (!IsActive || Pose is not { } pose || pose == CinematicSpritePose.Blank)
            return;
        Position16<ScreenPixels> origin = Position.Whole - scroll;
        int y = origin.Y;
        if (CoordinateAxis.Wrap(y + CinematicSpriteDrawDefinitions.OriginYBias) >=
            CinematicSpriteDrawDefinitions.BiasedOriginYLimit)
            return;
        // A negative origin still has visible component tiles, but their low-byte Y
        // arithmetic needs the opposite clipping branch to prevent bottom-edge wrap.
        bool originIsOnScreen =
            y < CinematicSpriteDrawDefinitions.OnScreenOriginYLimit;
        (installedArt ?? throw new InvalidOperationException(
            "Cinematic sprites require installed artwork."))
            .Draw(pose, objects, origin.ToSpritePosition(), Colouring, originIsOnScreen);
    }
}
