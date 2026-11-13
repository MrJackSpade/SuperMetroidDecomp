using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The narration pages' persistent objects: the portrait eye running its typed <see cref="IntroEyeAnimations"/>
/// program, the typewriter narration written from installed glyph records, the text caret, and the Japanese text's
/// next-page arrow at each page's input wait.
/// </summary>
internal sealed class IntroCinematicObjectSystem
{
    /// <summary>Queues alternating non-space typewriter sounds when audio is present.</summary>
    private readonly AudioQueues? audio;
    /// <summary>The narration text the typewriter types into and its glows recolour (#1467).</summary>
    private readonly IntroNarrationText text;
    /// <summary>The persistent portrait eye's animation and where it stands in it, played on when its frame expires (#1600).</summary>
    internal AnimationPlayback<IntroEyeProgram> EyePlayback { get; private set; } =
        new(IntroEyeAnimations.StartingWith(IntroEyeProgram.Blinking), AnimationPlace.StartOfAnimation);
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
    /// <summary>
    /// Whether the press that turns the page has been accepted since the arrow's last update: the page-turn setup starts
    /// the cross-fade, and the arrow deletes itself at its next update, in the same frame.
    /// </summary>
    private bool pageTurnAccepted;
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

    /// <summary>
    /// The pose the text caret shows: its program's pose, or null while the caret is hidden or before its program's first
    /// frame, when nothing is drawn.
    /// </summary>
    internal CinematicSpritePose? CaretPose => caretHidden ? null : caretSprite.Pose;

    /// <summary>
    /// Whether the caret is hidden while an illustration owns the screen; its program keeps running, and the next page
    /// shows it again (#1625).
    /// </summary>
    private bool caretHidden;

    /// <summary>The Japanese text's next-page arrow while it shows (#1655); null with English text and between input waits.</summary>
    internal IntroDiscoverySprite? NextPageArrow { get; private set; }

    /// <summary>
    /// Records that a page's input wait accepted the press that turns the page (#1655). The page-turn setup starts the
    /// page's cross-fade, and the next-page arrow, which watches for the cross-fade's start, deletes itself at its next
    /// update: the same frame, before anything is drawn.
    /// </summary>
    public void AcceptPageTurn() => pageTurnAccepted = true;

    /// <summary>Current screen position of the persistent text caret object.</summary>
    public Position16<ScreenPixels> Caret { get; private set; } =
        new(IntroCinematicDefinitions.ObjectSystem.CaretLeftX, IntroCinematicDefinitions.ObjectSystem.CaretFirstTextY);

    /// <summary>
    /// Takes the intro text caret off the screen while a gameplay or scientist illustration owns it: the caret is not
    /// drawn until the next narration page restores it (#1625).
    /// </summary>
    public void PlaceCaretOffScreen() => caretHidden = true;

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
        // object to its closed/half-open/deadpan animation.
        EyePlayback = new AnimationPlayback<IntroEyeProgram>(IntroEyeAnimations.StartingWith(IntroEyeProgram.FinalPageClosing), AnimationPlace.StartOfAnimation);
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
        StepNextPageArrow();
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
        // sets the caret blinking, and with Japanese text spawns the next-page arrow. Japanese pages two to five then
        // wait for the press that shows their second subtitle, whose object selects the page's input wait.
        SetCaretBlinking();
        if (subtitles.Enabled)
            SpawnNextPageArrow();
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
    /// Advances the portrait eye one cinematic update: when its frame expires, the animation shows its next frame for
    /// that frame's duration and the portrait shows the frame's picture.
    /// </summary>
    private void StepEye()
    {
        if (--EyeFrameHold != 0)
            return;

        // The eye animations play no counted phase; the play still takes a repeat counter.
        WrappingCountdown repeats = default;
        AnimationPlace place = EyePlayback.Place;
        AnimationShown<IntroEyeProgram, IntroEyeFrame> shown = IntroEyeAnimations.Segments.PlayToNextFrame(
            EyePlayback.Animation, ref place, ref repeats,
            static cue => throw new InvalidOperationException($"The intro eye animations raise no event; got {cue}."));
        EyePlayback = EyePlayback with { Place = place };
        if (shown is not AnimationShown<IntroEyeProgram, IntroEyeFrame>.Frame frame)
            throw new InvalidOperationException($"The intro eye animation ended an update with {shown}.");
        EyeFrameHold = frame.Duration;
        ShowEyeFrame(frame.Shown);
    }

    /// <summary>Shows <paramref name="frame"/> on the portrait.</summary>
    internal void ShowEyeFrame(IntroEyeFrame frame) => EyeFrame = frame;

    /// <summary>Shows the persistent caret again at the first text line and restarts its non-blinking sprite list.</summary>
    private void ResetCaret()
    {
        caretHidden = false;
        Caret = new(IntroCinematicDefinitions.ObjectSystem.CaretLeftX, IntroCinematicDefinitions.ObjectSystem.CaretFirstTextY);
        caretSprite.Start(CinematicSpriteProgram.IntroTextCaret);
    }

    /// <summary>
    /// Spawns the next-page arrow at the screen's middle below the text, its looping animation primed to show its first
    /// frame at its first update. The narration spawns it after this frame's sprite updates, so it first updates on the
    /// next frame.
    /// </summary>
    private void SpawnNextPageArrow()
    {
        if (NextPageArrow is not null)
            throw new InvalidOperationException("The narration spawned a next-page arrow while one was still showing.");
        NextPageArrow = new IntroDiscoverySprite(
            new(IntroCinematicDefinitions.ObjectSystem.NextPageArrowX, IntroCinematicDefinitions.ObjectSystem.NextPageArrowY),
            IntroShades.Drawn(IntroSpriteColouring.CrossFadeObjects), CinematicSpriteProgram.IntroNextPageArrow);
    }

    /// <summary>
    /// Runs the next-page arrow's update, its own checks first: the update after a page's second subtitle starts holding
    /// the input wait, it restarts on its delayed animation, which shows nothing for the hold before the arrow loops
    /// again; once the press that turns the page is accepted, it deletes itself.
    /// </summary>
    private void StepNextPageArrow()
    {
        bool turned = pageTurnAccepted;
        pageTurnAccepted = false;
        if (NextPageArrow is not { } arrow)
            return;
        if (subtitles.HoldFirstCounted)
            arrow.Start(CinematicSpriteProgram.IntroNextPageArrowDelayed);
        if (turned)
        {
            arrow.Delete();
            NextPageArrow = null;
            return;
        }
        arrow.Step();
    }

    /// <summary>Points the existing caret slot at its blinking list and primes its timer.</summary>
    internal void SetCaretBlinking()
    {
        // The caret-blink instruction starts the existing slot's blinking program and primes timer
        // one, allowing the generic sprite handler to select the first frame now.
        caretSprite.Start(CinematicSpriteProgram.IntroTextCaretBlink);
    }

}
