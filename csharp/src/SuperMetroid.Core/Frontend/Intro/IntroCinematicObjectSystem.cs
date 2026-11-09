using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The narration pages' persistent objects: the portrait eye running its typed <see cref="IntroEyeAnimations"/>
/// program, the typewriter narration written from installed glyph records, and the text caret.
/// </summary>
internal sealed class IntroCinematicObjectSystem
{
    /// <summary>Queues alternating non-space typewriter sounds when audio is present.</summary>
    private readonly AudioQueues? audio;
    /// <summary>The narration text the typewriter types into and its glows recolour (#1467).</summary>
    private readonly IntroNarrationText text;
    /// <summary>Where the persistent portrait eye's program resumes when its frame expires.</summary>
    private AnimationProgramCursor<IntroEyeProgram> eyeProgram =
        AnimationProgramCursor<IntroEyeProgram>.Start(IntroEyeProgram.Blinking);
    /// <summary>Where the portrait eye's blink program resumes.</summary>
    internal AnimationProgramCursor<IntroEyeProgram> EyeProgram { get => eyeProgram; private set => eyeProgram = value; }
    /// <summary>Countdown before the eye shows its program's next frame.</summary>
    internal int EyeFrameHold { get; private set; } = 1;
    /// <summary>Countdown for initial marker, glyph, or final page hold.</summary>
    private int textFrameHold;
    /// <summary>Caller-installed narration artwork, rebound whenever the game is bound to its installed content.</summary>
    private IntroNarrationPresentation? narrationPresentation;
    /// <summary>The eye frame the portrait shows, as an installed picture over it (#1467); null before the first frame.</summary>
    internal IntroEyeFrame? EyeFrame { get; private set; }
    /// <summary>The page glyphs, compiled from the installed presentation when it is bound.</summary>
    private IntroNarrationCharacter[]? narrationProgram;
    /// <summary>The bound narration page's characters.</summary>
    private IntroNarrationCharacter[] NarrationProgram => narrationProgram ??
        throw new InvalidOperationException("No narration page is bound.");
    /// <summary>Page whose glyph stream and Japanese subtitle markers are active.</summary>
    private IntroNarrationPageId? narrationPage;
    /// <summary>Next semantic glyph in the active narration page.</summary>
    private int narrationCharacterIndex;
    /// <summary>Whether the page's subtitle creation marker has yet to execute.</summary>
    private bool narrationInitialMarkerPending;
    /// <summary>Whether page six entered its final 128-frame caret hold.</summary>
    private bool narrationFinalHoldStarted;
    /// <summary>The persistent text-caret sprite object, running its typed program (#641); <see cref="Caret"/> places it.</summary>
    private readonly IntroDiscoverySprite caretSprite = new(default, IntroShades.Drawn(IntroSpriteColouring.CrossFadeObjects), CinematicSpriteProgram.IntroTextCaret);
    /// <summary>Alternates typewriter audio calls while letters appear.</summary>
    private bool typewriterSoundToggle;
    /// <summary>Created by the first glyph to animate per-character glow effects.</summary>
    private CinematicTextGlowSystem? textGlow;
    /// <summary>Tracks subtitle page transitions and the Japanese input wait.</summary>
    private readonly IntroJapaneseSubtitles subtitles;

    /// <summary>Binds caret, narration, narration text, subtitle, and optional audio owners.</summary>
    public IntroCinematicObjectSystem(
        IntroNarrationText text,
        IntroJapaneseSubtitles subtitles,
        AudioQueues? audio = null,
        IntroNarrationPresentation? narrationPresentation = null)
    {
        this.subtitles = Ensure.NotNull(subtitles);
        this.text = Ensure.NotNull(text);
        this.audio = audio;
        this.narrationPresentation = narrationPresentation;
    }

    /// <summary>Set when the narration reaches the English page-one input marker.</summary>
    public bool PageOneAwaitingInput { get; private set; }

    /// <summary>Set when the narration reaches the English page-two input marker.</summary>
    public bool PageTwoAwaitingInput { get; private set; }

    /// <summary>Set when the narration completes English page three.</summary>
    public bool PageThreeAwaitingInput { get; private set; }

    /// <summary>Set when the narration completes English page four.</summary>
    public bool PageFourAwaitingInput { get; private set; }

    /// <summary>Set when the narration completes English page five.</summary>
    public bool PageFiveAwaitingInput { get; private set; }

    /// <summary>Set after page six's final 128-frame hold.</summary>
    public bool IntroFinishRequested { get; private set; }

    /// <summary>The pose the text caret's program shows; null before its first frame.</summary>
    internal CinematicSpritePose? CaretPose => caretSprite.Pose;

    /// <summary>Current screen position of the persistent text caret object; Y=248 deliberately hides it below the viewport.</summary>
    public Position16<ScreenPixels> Caret { get; private set; } =
        new(IntroCinematicDefinitions.ObjectSystem.CaretLeftX, IntroCinematicDefinitions.ObjectSystem.CaretFirstTextY);

    /// <summary>
    /// Places the intro text caret off screen. The retail game keeps
    /// the same object slot alive between narration pages and visibly moves it to Y=248
    /// while a gameplay or scientist illustration owns the screen.
    /// </summary>
    public void PlaceCaretOffScreen()
    {
        Caret = new(IntroCinematicDefinitions.ObjectSystem.CaretLeftX, IntroCinematicDefinitions.ObjectSystem.CaretInitialY);
    }

    /// <summary>
    /// Starts the letter-by-letter narration stream. The eye-blink
    /// stream and the portrait-border sprite are already active at page setup.
    /// </summary>
    public void StartEnglishPageOne()
    {
        StartNarration(
            IntroNarrationPageId.Page1);
    }

    /// <summary>
    /// Spawns page two's narration and restores the existing caret object.
    /// </summary>
    public void StartEnglishPageTwo()
    {
        StartNarration(
            IntroNarrationPageId.Page2);
        PageTwoAwaitingInput = false;
        ResetCaret();
    }

    /// <summary>
    /// Spawns page three's narration after the egg actor requests page three.
    /// </summary>
    public void StartEnglishPageThree()
    {
        StartNarration(
            IntroNarrationPageId.Page3);
        PageThreeAwaitingInput = false;
        ResetCaret();
    }

    /// <summary>Spawns the page-four text definition.</summary>
    public void StartEnglishPageFour()
    {
        StartNarration(
            IntroNarrationPageId.Page4);
        PageFourAwaitingInput = false;
        ResetCaret();
    }

    /// <summary>Spawns the page-five text definition.</summary>
    public void StartEnglishPageFive()
    {
        StartNarration(
            IntroNarrationPageId.Page5);
        PageFiveAwaitingInput = false;
        ResetCaret();
    }

    /// <summary>Clears pages one-through-five and starts the final text.</summary>
    public void StartEnglishPageSix()
    {
        text.ClearText();
        StartNarration(
            IntroNarrationPageId.Page6);
        IntroFinishRequested = false;
        ResetCaret();

        // The eye's frame behaviour observes the page-six cinematic function and permanently changes the eye
        // object to its closed/half-open/deadpan program.
        EyeProgram = AnimationProgramCursor<IntroEyeProgram>.Start(IntroEyeProgram.FinalPageClosing);
        EyeFrameHold = 1;
    }

    /// <summary>Runs the same post-state-function object order as the cinematic game state.</summary>
    /// <param name="newlyPressed">This frame's controller rising edges, read by the Japanese subtitles.</param>
    public void Step(ControllerButton newlyPressed)
    {
        if (narrationPage is not null && narrationProgram is null)
        {
            throw new InvalidOperationException(
                "Opening narration requires its installed presentation to be bound.");
        }
        caretSprite.Step();
        subtitles.Step(newlyPressed, MarkAwaitingInput);
        StepEye();
        if (narrationProgram is not null)
            StepInstalledNarration();
        textGlow?.Step(text);
    }

    /// <summary>Binds the installed narration content when the game is bound to its installed content.</summary>
    public void BindNarration(IntroNarrationPresentation? presentation)
    {
        narrationPresentation = presentation;
        narrationProgram = narrationPage is { } page && presentation is not null
            ? presentation.Compile(page)
            : null;
        if (narrationProgram is not null && narrationCharacterIndex > narrationProgram.Length)
        {
            throw new InvalidDataException(
                $"Saved narration cursor {narrationCharacterIndex} exceeds the rebound {narrationPage} program.");
        }
    }

    /// <summary>Compiles an installed page and primes its subtitle marker before the first glyph.</summary>
    private void StartNarration(IntroNarrationPageId page)
    {
        textFrameHold = IntroNarrationDefinitions.InitialMarkerDelayFrames;
        narrationCharacterIndex = 0;
        narrationInitialMarkerPending = true;
        narrationFinalHoldStarted = false;
        if (narrationPresentation is null)
            throw new InvalidOperationException(
                "Opening narration requires installed text presentation assets.");

        narrationPage = page;
        narrationProgram = narrationPresentation.Compile(page);
    }

    /// <summary>Advances marker, glyph, typewriter sound, caret, and final-hold timing.</summary>
    private void StepInstalledNarration()
    {
        if (--textFrameHold != 0)
            return;
        if (narrationInitialMarkerPending)
        {
            // Each page list opens with the page's subtitle-creating instruction.
            subtitles.BeginPage(narrationPage ?? throw new InvalidDataException(
                "Installed narration started without an active page identity."));
            narrationInitialMarkerPending = false;
            textFrameHold = IntroNarrationDefinitions.InitialMarkerDelayFrames;
            return;
        }

        IntroNarrationCharacter[] program = NarrationProgram;
        if (narrationCharacterIndex < program.Length)
        {
            IntroNarrationCharacter character = program[narrationCharacterIndex];
            IntroNarrationCharacter? next = narrationCharacterIndex + 1 < program.Length
                ? program[narrationCharacterIndex + 1]
                : null;
            (textGlow ??= new()).Spawn(character.Column, character.Row);
            if (next is { } following)
            {
                Caret = new(
                    CoordinateAxis.Wrap(following.Column *
                        IntroCinematicDefinitions.ObjectSystem.CharacterPixelSize),
                    CoordinateAxis.Wrap(following.Row *
                        IntroCinematicDefinitions.ObjectSystem.CharacterPixelSize -
                        IntroCinematicDefinitions.ObjectSystem.CharacterBaselineOffset));
            }
            else
            {
                Caret = new(
                    IntroCinematicDefinitions.ObjectSystem.CaretLeftX,
                    CoordinateAxis.Wrap((character.Row + 1) *
                        IntroCinematicDefinitions.ObjectSystem.CharacterPixelSize));
            }

            typewriterSoundToggle = !typewriterSoundToggle;
            if (!character.IsSpace && typewriterSoundToggle)
            {
                audio?.QueueSound(
                    IntroCinematicDefinitions.Objects.Typewriter,
                    priority: IntroCinematicDefinitions.Objects.MaximumQueuedSounds);
            }
            text.Type(character);
            narrationCharacterIndex++;
            textFrameHold = IntroNarrationDefinitions.CharacterDelayFrames;
            return;
        }

        if (narrationPage == IntroNarrationPageId.Page6 && !narrationFinalHoldStarted)
        {
            SetCaretBlinking();
            narrationFinalHoldStarted = true;
            textFrameHold = IntroNarrationDefinitions.FinalPageHoldFrames;
            return;
        }

        FinishInstalledNarration(narrationPage ?? throw new InvalidDataException(
            "Installed narration completed without an active page identity."));
        narrationProgram = null;
        narrationPage = null;
    }

    /// <summary>Starts the input wait or marks the sixth page complete after its hold.</summary>
    private void FinishInstalledNarration(IntroNarrationPageId page)
    {
        if (page == IntroNarrationPageId.Page6)
        {
            IntroFinishRequested = true;
            return;
        }
        // The page's blinking-markers-and-wait instruction first
        // sets the caret blinking. Japanese pages two to five then wait for the press that
        // shows their second subtitle, whose object selects the page's input wait.
        SetCaretBlinking();
        if (subtitles.FinishPage(page))
            MarkAwaitingInput(page);
    }

    /// <summary>Publishes the completed page's input marker to the outer intro state machine.</summary>
    private void MarkAwaitingInput(IntroNarrationPageId page)
    {
        switch (page)
        {
            case IntroNarrationPageId.Page1: PageOneAwaitingInput = true; break;
            case IntroNarrationPageId.Page2: PageTwoAwaitingInput = true; break;
            case IntroNarrationPageId.Page3: PageThreeAwaitingInput = true; break;
            case IntroNarrationPageId.Page4: PageFourAwaitingInput = true; break;
            case IntroNarrationPageId.Page5: PageFiveAwaitingInput = true; break;
            case IntroNarrationPageId.Page6:
            default: throw new ArgumentOutOfRangeException(nameof(page), page, null);
        }
    }

    /// <summary>
    /// Advances the portrait eye one cinematic update: when its frame expires, the program shows its next frame for
    /// that frame's duration and the portrait shows the frame's picture.
    /// </summary>
    private void StepEye()
    {
        if (--EyeFrameHold != 0)
            return;

        // The eye programs load no loop counter; the program set still takes one.
        WrappingCountdown loopCounter = default;
        AnimationProgramAdvance<IntroEyeFrame> advance = IntroEyeAnimations.Programs.Advance(
            ref eyeProgram, ref loopCounter, static (_, _) => null);
        if (advance is not AnimationProgramAdvance<IntroEyeFrame>.Show shown)
            throw new InvalidOperationException($"The intro eye program {EyeProgram.Program} ended an update with {advance}.");
        EyeFrameHold = shown.Duration;
        ShowEyeFrame(shown.Frame);
    }

    /// <summary>Shows <paramref name="frame"/> on the portrait.</summary>
    internal void ShowEyeFrame(IntroEyeFrame frame) => EyeFrame = frame;

    /// <summary>Moves the persistent caret onscreen and restarts its non-blinking sprite list.</summary>
    private void ResetCaret()
    {
        // Resting the intro text caret moves the persistent slot back from Y=248 before
        // restoring its non-blinking list. Position is state, not a renderer constant.
        Caret = new(IntroCinematicDefinitions.ObjectSystem.CaretLeftX, IntroCinematicDefinitions.ObjectSystem.CaretFirstTextY);
        caretSprite.Start(CinematicSpriteProgram.IntroTextCaret);
    }

    /// <summary>Points the existing caret slot at its blinking list and primes its timer.</summary>
    internal void SetCaretBlinking()
    {
        // The caret-blink instruction starts the existing slot's blinking program and primes timer
        // one, allowing the generic sprite handler to select the first frame now.
        caretSprite.Start(CinematicSpriteProgram.IntroTextCaretBlink);
    }

}
