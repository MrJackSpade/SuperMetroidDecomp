using SuperMetroid.Core.Input;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The narration pages' persistent actors: the portrait eye running its typed <see cref="IntroEyeAnimations"/>
/// animation, the typewriter narration typing the installed text's letters, the text caret, and the Japanese text's
/// next-page arrow at each page's input wait.
/// </summary>
internal sealed class IntroNarrationActors
{
    /// <summary>Queues the alternating non-space typewriter sounds.</summary>
    private readonly AudioQueues audio;
    /// <summary>The narration text the typewriter types into and its glows recolour (#1467).</summary>
    private readonly IntroNarrationText text;
    /// <summary>The persistent portrait eye's animation and where it stands in it, played on when its frame expires (#1600).</summary>
    internal AnimationPlayback<IntroEyeSegment> EyePlayback { get; private set; } =
        new(IntroEyeAnimations.StartingWith(IntroEyeSegment.Blinking), AnimationPlace.StartOfAnimation);
    /// <summary>
    /// Countdown before the eye shows its animation's next frame; none once the eye rests on its final page's stare for
    /// good, when its animation plays on no further (#2196).
    /// </summary>
    internal int? EyeFrameHold { get; private set; } = 1;
    /// <summary>Countdown for initial marker, glyph, or final page hold.</summary>
    private int textFrameHold;
    /// <summary>The installed narration artwork, rebound whenever the game is bound to its installed content.</summary>
    private IntroNarrationPresentation? narrationPresentation;
    /// <summary>The eye frame the portrait shows, as an installed picture over it (#1467); null before the first frame.</summary>
    internal IntroEyeFrame? EyeFrame { get; private set; }
    /// <summary>The page glyphs, compiled from the installed presentation when it is bound.</summary>
    private IntroNarrationCharacter[]? narrationAnimation;
    /// <summary>The bound narration page's characters.</summary>
    private IntroNarrationCharacter[] NarrationAnimation => narrationAnimation ??
        throw new InvalidOperationException("No narration page is bound.");
    /// <summary>Page whose glyphs and Japanese subtitle markers are active.</summary>
    private IntroNarrationPageId? narrationPage;
    /// <summary>Next semantic glyph in the active narration page.</summary>
    private int narrationCharacterIndex;
    /// <summary>Whether the page's subtitle creation marker has yet to execute.</summary>
    private bool narrationInitialMarkerPending;
    /// <summary>Whether page six entered its final 128-frame caret hold.</summary>
    private bool narrationFinalHoldStarted;
    /// <summary>The persistent text-caret actor, running its typed animation (#641); <see cref="Caret"/> places it.</summary>
    private readonly CinematicActor<IntroNarrationAnimation, CinematicSpritePose, NoAnimationCue> caretSprite =
        new(default, IntroShades.Drawn(IntroSpriteColouring.CrossFadeActors), IntroNarrationAnimations.Narration, IntroNarrationAnimation.Caret);
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

    /// <summary>Binds caret, narration, narration text, subtitle, and audio owners.</summary>
    public IntroNarrationActors(
        IntroNarrationText text,
        IntroJapaneseSubtitles subtitles,
        AudioQueues audio,
        IntroNarrationPresentation narrationPresentation)
    {
        this.subtitles = Ensure.NotNull(subtitles);
        this.text = Ensure.NotNull(text);
        this.audio = Ensure.NotNull(audio);
        this.narrationPresentation = Ensure.NotNull(narrationPresentation);
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
    /// The pose the text caret shows: its animation's pose, or null while the caret is hidden or before its animation's first
    /// frame, when nothing is drawn.
    /// </summary>
    internal CinematicSpritePose? CaretPose => caretHidden ? null : caretSprite.Shown;

    /// <summary>
    /// Whether the caret is hidden while an illustration owns the screen; its animation keeps running, and the next page
    /// shows it again (#1625).
    /// </summary>
    private bool caretHidden;

    /// <summary>The Japanese text's next-page arrow while it shows (#1655); null with English text and between input waits.</summary>
    internal CinematicActor<IntroNarrationAnimation, CinematicSpritePose, NoAnimationCue>? NextPageArrow { get; private set; }

    /// <summary>
    /// Records that a page's input wait accepted the press that turns the page (#1655). The page-turn setup starts the
    /// page's cross-fade, and the next-page arrow, which watches for the cross-fade's start, deletes itself at its next
    /// update: the same frame, before anything is drawn.
    /// </summary>
    public void AcceptPageTurn() => pageTurnAccepted = true;

    /// <summary>The persistent text caret's place, in signed screen pixels from the screen's top-left corner.</summary>
    public Offset16<ScreenPixels> Caret { get; private set; } =
        new(IntroCinematicDefinitions.NarrationLayout.CaretLeftX, IntroCinematicDefinitions.NarrationLayout.CaretFirstTextY);

    /// <summary>
    /// Hides the intro text caret while a gameplay or scientist illustration owns the screen: the caret is not
    /// drawn until the next narration page restores it (#1625).
    /// </summary>
    public void HideCaret() => caretHidden = true;

    /// <summary>
    /// Starts the letter-by-letter narration. The eye-blink
    /// animation and the portrait-border sprite are already active at page setup.
    /// </summary>
    public void StartEnglishPageOne()
    {
        StartNarration(
            IntroNarrationPageId.Page1);
    }

    /// <summary>
    /// Spawns page two's narration and restores the existing caret.
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

        // The eye observes the page-six cinematic phase and permanently changes the eye
        // to its closed/half-open/deadpan animation.
        EyePlayback = new AnimationPlayback<IntroEyeSegment>(IntroEyeAnimations.StartingWith(IntroEyeSegment.FinalPageClosing), AnimationPlace.StartOfAnimation);
        EyeFrameHold = 1;
    }

    /// <summary>Runs the narration's actors in the cinematic game state's order.</summary>
    /// <param name="newlyPressed">This frame's controller rising edges, read by the Japanese subtitles.</param>
    public void Step(ControllerButton newlyPressed)
    {
        if (narrationPage is not null && narrationAnimation is null)
        {
            throw new InvalidOperationException(
                "Opening narration requires its installed presentation to be bound.");
        }
        caretSprite.Step();
        StepNextPageArrow();
        subtitles.Step(newlyPressed, MarkAwaitingInput);
        StepEye();
        if (narrationAnimation is not null)
            StepInstalledNarration();
        textGlow?.Step(text);
    }

    /// <summary>Binds the installed narration content when the game is bound to its installed content.</summary>
    public void BindNarration(IntroNarrationPresentation? presentation)
    {
        narrationPresentation = presentation;
        narrationAnimation = narrationPage is { } page && presentation is not null
            ? presentation.Compile(page)
            : null;
        if (narrationAnimation is not null && narrationCharacterIndex > narrationAnimation.Length)
        {
            throw new InvalidDataException(
                $"Saved narration cursor {narrationCharacterIndex} exceeds the rebound {narrationPage} narration.");
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
        narrationAnimation = narrationPresentation.Compile(page);
    }

    /// <summary>Advances marker, glyph, typewriter sound, caret, and final-hold timing.</summary>
    private void StepInstalledNarration()
    {
        if (--textFrameHold != 0)
            return;
        if (narrationInitialMarkerPending)
        {
            // Each page's animation opens with the page's subtitle-creating cue.
            subtitles.BeginPage(narrationPage ?? throw new InvalidDataException(
                "Installed narration started without an active page identity."));
            narrationInitialMarkerPending = false;
            textFrameHold = IntroNarrationDefinitions.InitialMarkerDelayFrames;
            return;
        }

        IntroNarrationCharacter[] animation = NarrationAnimation;
        if (narrationCharacterIndex < animation.Length)
        {
            IntroNarrationCharacter character = animation[narrationCharacterIndex];
            IntroNarrationCharacter? next = narrationCharacterIndex + 1 < animation.Length
                ? animation[narrationCharacterIndex + 1]
                : null;
            (textGlow ??= new()).Spawn(character.Column, character.Row);
            if (next is { } following)
            {
                // The caret stands a baseline above the next character's cell.
                Caret = new(
                    following.Column * IntroCinematicDefinitions.NarrationLayout.CharacterPixelSize,
                    following.Row * IntroCinematicDefinitions.NarrationLayout.CharacterPixelSize -
                        IntroCinematicDefinitions.NarrationLayout.CharacterBaselineOffset);
            }
            else
            {
                Caret = new(
                    IntroCinematicDefinitions.NarrationLayout.CaretLeftX,
                    (character.Row + 1) * IntroCinematicDefinitions.NarrationLayout.CharacterPixelSize);
            }

            typewriterSoundToggle = !typewriterSoundToggle;
            if (!character.IsSpace && typewriterSoundToggle)
            {
                audio.QueueSound(
                    IntroCinematicDefinitions.Sounds.Typewriter,
                    priority: IntroCinematicDefinitions.Sounds.SoundPriority);
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
        narrationAnimation = null;
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
        // Once the page is typed the caret starts blinking, and Japanese text shows the next-page arrow. Japanese pages
        // two to five then wait for the press that shows their second subtitle, which selects the page's input wait.
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
        if (EyeFrameHold is not { } hold)
            return;
        EyeFrameHold = --hold;
        if (hold != 0)
            return;

        AnimationPlace place = EyePlayback.Place;
        AnimationShown<IntroEyeSegment, IntroEyeFrame> shown = IntroEyeAnimations.Segments.PlayToNextFrame(
            EyePlayback.Animation, ref place,
            static cue => throw new InvalidOperationException($"The intro eye animations raise no event; got {cue}."));
        EyePlayback = EyePlayback with { Place = place };
        switch (shown)
        {
            case AnimationShown<IntroEyeSegment, IntroEyeFrame>.Frame frame:
                EyeFrameHold = frame.Duration;
                ShowEyeFrame(frame.Shown);
                return;
            case AnimationShown<IntroEyeSegment, IntroEyeFrame>.HeldFrame rested:
                // The final page's stare shows for the rest of the narration.
                EyeFrameHold = null;
                ShowEyeFrame(rested.Shown);
                return;
            default:
                throw new InvalidOperationException($"The intro eye animation ended an update with {shown}.");
        }
    }

    /// <summary>Shows <paramref name="frame"/> on the portrait.</summary>
    internal void ShowEyeFrame(IntroEyeFrame frame) => EyeFrame = frame;

    /// <summary>Shows the persistent caret again at the first text line and restarts its non-blinking sprite animation.</summary>
    private void ResetCaret()
    {
        caretHidden = false;
        Caret = new(IntroCinematicDefinitions.NarrationLayout.CaretLeftX, IntroCinematicDefinitions.NarrationLayout.CaretFirstTextY);
        caretSprite.Start(IntroNarrationAnimation.Caret);
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
        NextPageArrow = new(
            new(IntroCinematicDefinitions.NarrationLayout.NextPageArrowX, IntroCinematicDefinitions.NarrationLayout.NextPageArrowY),
            IntroShades.Drawn(IntroSpriteColouring.CrossFadeActors), IntroNarrationAnimations.Narration, IntroNarrationAnimation.NextPageArrow);
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
            arrow.Start(IntroNarrationAnimation.NextPageArrowDelayed);
        if (turned)
        {
            arrow.Delete();
            NextPageArrow = null;
            return;
        }
        arrow.Step();
    }

    /// <summary>Points the existing caret sprite at its blinking animation and primes its timer.</summary>
    internal void SetCaretBlinking()
    {
        // The caret-blink cue starts the existing caret sprite's blinking animation and primes timer
        // one, allowing the cinematic sprite's update to select the first frame now.
        caretSprite.Start(IntroNarrationAnimation.CaretBlinking);
    }

}
